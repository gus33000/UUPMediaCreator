using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Cabinet
{
    public class CabinetFile
    {
        private CFHEADER header;
        private List<CFFOLDER> folders;
        private List<(CFFILE file, string fileName)> files;
        private string InputFile;
        private ushort cbCFHeader = 0;
        private byte cbCFFolder = 0;
        private byte cbCFData = 0;

        public IReadOnlyCollection<string> Files
        {
            get
            {
                return files.Select(x => x.fileName).ToList();
            }
        }

        public CabinetFile(string Path)
        {
            InputFile = Path;

            using (var cabinetFileStream = File.OpenRead(InputFile))
            {
                var cabinetBinaryReader = new BinaryReader(cabinetFileStream);

                header = cabinetFileStream.ReadStruct<CFHEADER>();

                if (StructuralComparisons.StructuralComparer.Compare(header.signature, new byte[] { (byte)'M', (byte)'S', (byte)'C', (byte)'F' }) != 0)
                {
                    throw new Exception("Bad Cabinet: Invalid Signature");
                }

                if ((header.flags & CFHEADER.Options.ReservePresent) != 0)
                {
                    cbCFHeader = cabinetBinaryReader.ReadUInt16();
                    cbCFFolder = cabinetBinaryReader.ReadByte();
                    cbCFData = cabinetBinaryReader.ReadByte();
                    cabinetBinaryReader.BaseStream.Seek(cbCFHeader, SeekOrigin.Current);
                }

                if ((header.flags & CFHEADER.Options.PreviousCabinet) != 0)
                {
                    var prevCab = cabinetBinaryReader.BaseStream.ReadString();
                    var prevDisk = cabinetBinaryReader.BaseStream.ReadString();

                    throw new Exception("Unsupported Cabinet: Multi Part");
                }

                if ((header.flags & CFHEADER.Options.NextCabinet) != 0)
                {
                    var prevCab = cabinetBinaryReader.BaseStream.ReadString();
                    var prevDisk = cabinetBinaryReader.BaseStream.ReadString();

                    throw new Exception("Unsupported Cabinet: Multi Part");
                }

                folders = new List<CFFOLDER>();
                for (int i = 0; i < header.cFolders; i++)
                {
                    CFFOLDER folder = cabinetFileStream.ReadStruct<CFFOLDER>();
                    cabinetBinaryReader.BaseStream.Seek(cbCFFolder, SeekOrigin.Current);
                    folders.Add(folder);

                    if (folder.typeCompress != CFFOLDER.CFTYPECOMPRESS.TYPE_LZX &&
                        folder.typeCompress != CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP &&
                        folder.typeCompress != CFFOLDER.CFTYPECOMPRESS.TYPE_NONE)
                    {
                        throw new Exception("Unsupported Cabinet: Only LZX, MSZip and Store is currently supported");
                    }

                    if (folder.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                    {
                        //Console.WriteLine("LZX Detected with Window Size of: " + folder.typeCompressOption);
                        if (folder.typeCompressOption < 15 || folder.typeCompressOption > 21)
                        {
                            throw new Exception("Unsupported Cabinet: LZX variable does not fall in supported ranges");
                        }
                    }
                }

                if (cabinetBinaryReader.BaseStream.Position != header.coffFiles)
                {
                    throw new Exception("Bad Cabinet: First File Block does not match header");
                }

                files = new List<(CFFILE file, string fileName)>();
                for (int i = 0; i < header.cFiles; i++)
                {
                    CFFILE file = cabinetBinaryReader.BaseStream.ReadStruct<CFFILE>();
                    string name = "";
                    if (file.IsFileNameUTF8())
                        name = cabinetBinaryReader.BaseStream.ReadUTF8tring();
                    else
                        name = cabinetBinaryReader.BaseStream.ReadString();
                    files.Add((file, name));
                }
            }
        }

        public void ExtractAllFiles(string OutputDirectory, Action<int, string> progressCallBack = null)
        {
            // Cleanup existing files
            foreach ((CFFILE file, string name) in files)
            {
                string destination = Path.Combine(OutputDirectory, name);
                if (!Directory.Exists(Path.GetDirectoryName(destination)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                }

                if (File.Exists(destination))
                    File.Delete(destination);
            }

            // Do the extraction
            for (int i1 = 0; i1 < folders.Count; i1++)
            {
                CFFOLDER folder = folders[i1];

                LzxDecoder lzx = null;
                if (folder.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                {
                    lzx = new LzxDecoder(folder.typeCompressOption);
                }

                List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> datas = new List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)>();
                List<(CFFILE file, string fileName, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)> fileBlockMap = new List<(CFFILE file, string fileName, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)>();

                // Build Data Map
                using (var cabinetFileStream = File.OpenRead(InputFile))
                {
                    var cabinetBinaryReader = new BinaryReader(cabinetFileStream);
                    cabinetFileStream.Seek(folder.firstDataBlockOffset, SeekOrigin.Begin);

                    int offset = 0;
                    for (int i = 0; i < folder.dataBlockCount; i++)
                    {
                        CFDATA CabinetData = cabinetBinaryReader.BaseStream.ReadStruct<CFDATA>();
                        cabinetBinaryReader.BaseStream.Seek(cbCFData, SeekOrigin.Current);
                        datas.Add((CabinetData, (int)cabinetBinaryReader.BaseStream.Position, offset, offset + CabinetData.cbUncomp - 1, i));
                        cabinetBinaryReader.BaseStream.Seek(CabinetData.cbData, SeekOrigin.Current);
                        offset += CabinetData.cbUncomp;
                    }

                    // Build Block Map
                    foreach ((CFFILE file, string name) in files)
                    {
                        if (file.iFolder != i1)
                            continue;

                        var FirstBlock = datas.First(x => x.beginFolderOffset <= file.uoffFolderStart &&
                                                            file.uoffFolderStart <= x.endFolderOffset);
                        var LastBlock = datas.First(x => x.beginFolderOffset <= (file.uoffFolderStart + file.cbFile - 1) &&
                                                            (file.uoffFolderStart + file.cbFile - 1) <= x.endFolderOffset);

                        int fileBeginFolderOffset = (int)file.uoffFolderStart;
                        int fileEndFolderOffset = (int)file.uoffFolderStart + (int)file.cbFile - 1;

                        int start = (int)file.uoffFolderStart - FirstBlock.beginFolderOffset;
                        int end = fileEndFolderOffset - LastBlock.beginFolderOffset;

                        fileBlockMap.Add((file, name, FirstBlock.index, start, LastBlock.index, end));

                        //Console.WriteLine($"[{FirstBlock.index}({start})..{LastBlock.index}({end})] {name}");
                    }

                    int fcount = 0;

                    for (int i = 0; i < folder.dataBlockCount; i++)
                    {
                        //Console.WriteLine($"Begin Reading Block[{i}]");

                        var CabinetData = datas[i];

                        cabinetBinaryReader.BaseStream.Seek(CabinetData.dataOffsetCabinet, SeekOrigin.Begin);

                        byte[] uncompressedDataBlock = new byte[CabinetData.dataStruct.cbUncomp];
                        byte[] compressedDataBlock = null;

                        if (folder.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
                        {
                            var magic = cabinetBinaryReader.ReadBytes(2);

                            if (StructuralComparisons.StructuralComparer.Compare(magic, new byte[] { (byte)'C', (byte)'K' }) != 0)
                            {
                                throw new Exception("Bad Cabinet: Invalid Signature for MSZIP block");
                            }

                            compressedDataBlock = cabinetBinaryReader.ReadBytes(CabinetData.dataStruct.cbData - 2);
                        }
                        else
                        {
                            compressedDataBlock = cabinetBinaryReader.ReadBytes(CabinetData.dataStruct.cbData);
                        }

                        using (var uncompressedDataBlockStream = new MemoryStream(uncompressedDataBlock))
                        using (var compressedDataBlockStream = new MemoryStream(compressedDataBlock))
                        {
                            ExpandBlock(uncompressedDataBlockStream, compressedDataBlockStream, folder.typeCompress, lzx);
                        }

                        //Console.WriteLine($"End Reading Block[{i}]");

                        foreach ((CFFILE file, string name) in files)
                        {
                            if (file.iFolder != i1)
                                continue;

                            var mapping = fileBlockMap.First(x => x.fileName == name);

                            // This block contains this file
                            if (mapping.startingBlock <= i && i <= mapping.endingBlock)
                            {
                                //Console.WriteLine("Expanding " + mapping.fileName);

                                int start = 0;
                                int end = CabinetData.dataStruct.cbUncomp - 1;

                                bool IsFirstBlock = mapping.startingBlock == i;
                                bool IsLastBlock = mapping.endingBlock == i;

                                if (IsFirstBlock)
                                {
                                    progressCallBack?.Invoke((int)Math.Round((double)fcount / (double)header.cFiles * 100d), mapping.fileName);
                                    start = mapping.startingBlockOffset;
                                }

                                if (IsLastBlock)
                                {
                                    end = mapping.endingBlockOffset;
                                    fcount++;
                                }

                                int count = end - start + 1;

                                string destination = Path.Combine(OutputDirectory, name);
                                using (var uncompressedDataStream = File.Open(destination, FileMode.OpenOrCreate))
                                {
                                    uncompressedDataStream.Seek(0, SeekOrigin.End);
                                    uncompressedDataStream.Write(uncompressedDataBlock, start, count);
                                }

                                if (IsLastBlock)
                                {
                                    File.SetAttributes(destination, file.GetFileAttributes());
                                    var dt = file.GetDateTime();
                                    File.SetCreationTimeUtc(destination, dt);
                                    File.SetLastWriteTimeUtc(destination, dt);
                                    File.SetLastAccessTimeUtc(destination, dt);
                                }
                            }
                        }
                    }
                }
            }
        }

        public byte[] ReadFile(string FileName)
        {
            var destination = ExtractFile(FileName);
            var result = File.ReadAllBytes(destination);
            File.Delete(destination);
            return result;
        }

        public string ExtractFile(string FileName)
        {
            var destination = Path.GetTempFileName();

            // Do the extraction
            for (int i1 = 0; i1 < folders.Count; i1++)
            {
                CFFOLDER folder = folders[i1];

                LzxDecoder lzx = null;
                if (folder.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                {
                    lzx = new LzxDecoder(folder.typeCompressOption);
                }

                List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> datas = new List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)>();
                List<(CFFILE file, string fileName, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)> fileBlockMap = new List<(CFFILE file, string fileName, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)>();

                // Build Data Map
                using (var cabinetFileStream = File.OpenRead(InputFile))
                {
                    var cabinetBinaryReader = new BinaryReader(cabinetFileStream);
                    cabinetFileStream.Seek(folder.firstDataBlockOffset, SeekOrigin.Begin);

                    int offset = 0;
                    for (int i = 0; i < folder.dataBlockCount; i++)
                    {
                        CFDATA CabinetData = cabinetBinaryReader.BaseStream.ReadStruct<CFDATA>();
                        cabinetBinaryReader.BaseStream.Seek(cbCFData, SeekOrigin.Current);
                        datas.Add((CabinetData, (int)cabinetBinaryReader.BaseStream.Position, offset, offset + CabinetData.cbUncomp - 1, i));
                        cabinetBinaryReader.BaseStream.Seek(CabinetData.cbData, SeekOrigin.Current);
                        offset += CabinetData.cbUncomp;
                    }

                    // Build Block Map
                    foreach ((CFFILE file, string name) in files)
                    {
                        if (file.iFolder != i1)
                            continue;

                        if (name.ToLower() != FileName.ToLower())
                            continue;

                        var FirstBlock = datas.First(x => x.beginFolderOffset <= file.uoffFolderStart &&
                                                            file.uoffFolderStart <= x.endFolderOffset);
                        var LastBlock = datas.First(x => x.beginFolderOffset <= (file.uoffFolderStart + file.cbFile - 1) &&
                                                            (file.uoffFolderStart + file.cbFile - 1) <= x.endFolderOffset);

                        int fileBeginFolderOffset = (int)file.uoffFolderStart;
                        int fileEndFolderOffset = (int)file.uoffFolderStart + (int)file.cbFile - 1;

                        int start = (int)file.uoffFolderStart - FirstBlock.beginFolderOffset;
                        int end = fileEndFolderOffset - LastBlock.beginFolderOffset;

                        fileBlockMap.Add((file, name, FirstBlock.index, start, LastBlock.index, end));

                        //Console.WriteLine($"[{FirstBlock.index}({start})..{LastBlock.index}({end})] {name}");
                    }

                    int fcount = 0;

                    for (int i = 0; i < folder.dataBlockCount; i++)
                    {
                        //Console.WriteLine($"Begin Reading Block[{i}]");

                        var CabinetData = datas[i];

                        cabinetBinaryReader.BaseStream.Seek(CabinetData.dataOffsetCabinet, SeekOrigin.Begin);

                        byte[] uncompressedDataBlock = new byte[CabinetData.dataStruct.cbUncomp];
                        byte[] compressedDataBlock = null;

                        if (folder.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
                        {
                            var magic = cabinetBinaryReader.ReadBytes(2);

                            if (StructuralComparisons.StructuralComparer.Compare(magic, new byte[] { (byte)'C', (byte)'K' }) != 0)
                            {
                                throw new Exception("Bad Cabinet: Invalid Signature for MSZIP block");
                            }

                            compressedDataBlock = cabinetBinaryReader.ReadBytes(CabinetData.dataStruct.cbData - 2);
                        }
                        else
                        {
                            compressedDataBlock = cabinetBinaryReader.ReadBytes(CabinetData.dataStruct.cbData);
                        }

                        using (var uncompressedDataBlockStream = new MemoryStream(uncompressedDataBlock))
                        using (var compressedDataBlockStream = new MemoryStream(compressedDataBlock))
                        {
                            ExpandBlock(uncompressedDataBlockStream, compressedDataBlockStream, folder.typeCompress, lzx);
                        }

                        //Console.WriteLine($"End Reading Block[{i}]");

                        foreach ((CFFILE file, string name) in files)
                        {
                            if (file.iFolder != i1)
                                continue;

                            if (name.ToLower() != FileName.ToLower())
                                continue;

                            var mapping = fileBlockMap.First(x => x.fileName == name);

                            // This block contains this file
                            if (mapping.startingBlock <= i && i <= mapping.endingBlock)
                            {
                                //Console.WriteLine("Expanding " + mapping.fileName);

                                int start = 0;
                                int end = CabinetData.dataStruct.cbUncomp - 1;

                                bool IsFirstBlock = mapping.startingBlock == i;
                                bool IsLastBlock = mapping.endingBlock == i;

                                if (IsFirstBlock)
                                {
                                    start = mapping.startingBlockOffset;
                                }

                                if (IsLastBlock)
                                {
                                    end = mapping.endingBlockOffset;
                                    fcount++;
                                }

                                int count = end - start + 1;

                                using (var uncompressedDataStream = File.Open(destination, FileMode.OpenOrCreate))
                                {
                                    uncompressedDataStream.Seek(0, SeekOrigin.End);
                                    uncompressedDataStream.Write(uncompressedDataBlock, start, count);
                                }
                            }
                        }
                    }
                }
            }

            return destination;
        }

        private void ExpandBlock(Stream uncompressedDataBlockStream, Stream compressedDataBlockStream, CFFOLDER.CFTYPECOMPRESS compressionType, LzxDecoder lzx)
        {
            switch (compressionType)
            {
                case CFFOLDER.CFTYPECOMPRESS.TYPE_LZX:
                    {
                        lzx.Decompress(compressedDataBlockStream, (int)compressedDataBlockStream.Length, uncompressedDataBlockStream, (int)uncompressedDataBlockStream.Length);
                        break;
                    }

                case CFFOLDER.CFTYPECOMPRESS.TYPE_NONE:
                    {
                        compressedDataBlockStream.CopyTo(uncompressedDataBlockStream);
                        break;
                    }
                case CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP:
                    {
                        using (var mszip = new DeflateStream(compressedDataBlockStream, CompressionMode.Decompress))
                        {
                            mszip.CopyTo(uncompressedDataBlockStream);
                        }
                        break;
                    }
            }
        }
    }
}