using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Cabinet
{
    public class CabinetFile
    {
        private readonly byte[] CabinetMagic = new byte[] { (byte)'M', (byte)'S', (byte)'C', (byte)'F' };

        private class CabinetHeader
        {
            /// <summary>
            /// The raw header structure of the cabinet file
            /// </summary>
            public CFHEADER CabinetFileHeader { get; set; }

            /// <summary>
            /// Additional Application-specific data for the header
            /// </summary>
            public byte[] AdditionalApplicationData { get; set; }

            /// <summary>
            /// Additional Application-specific data size for the volumes
            /// </summary>
            public byte VolumeAdditionalApplicationDataSize { get; set; }

            /// <summary>
            /// Additional Application-specific data size for the data
            /// </summary>
            public byte DataAdditionalApplicationDataSize { get; set; }
        }

        private class CabinetVolume
        {
            /// <summary>
            /// The raw folder structure of the volume
            /// </summary>
            public CFFOLDER CabinetFileVolume { get; set; }

            /// <summary>
            /// Additional Application-specific data for the volume
            /// </summary>
            public byte[] AdditionalApplicationData { get; set; }
        }

        private class CabinetVolumeFile
        {
            /// <summary>
            /// The raw folder structure of the file in the volume
            /// </summary>
            public CFFILE CabinetFileVolumeFile { get; set; }

            /// <summary>
            /// The file name
            /// </summary>
            public string FileName { get; set; }
        }

        private class CabinetData
        {
            /// <summary>
            /// The raw folder structure of the data in the volume
            /// </summary>
            public CFDATA CabinetFileData { get; set; }

            /// <summary>
            /// Additional Application-specific data for the data
            /// </summary>
            public byte[] AdditionalApplicationData { get; set; }

            /// <summary>
            /// The offset to the data payload described by this object in the cabinet file
            /// </summary>
            public uint CabinetFileDataPayloadOffset { get; set; }
        }

        private readonly CabinetHeader cabinetHeader;
        private readonly IReadOnlyCollection<CabinetVolume> volumes;
        private readonly IReadOnlyCollection<CabinetVolumeFile> files;
        private readonly string InputFile;

        public CabinetFile(string Path)
        {
            InputFile = Path;

            using (var cabinetFileStream = File.OpenRead(InputFile))
            {
                cabinetHeader = ReadHeader(cabinetFileStream);
                volumes = ReadVolumes(cabinetFileStream);
                files = ReadVolumeFiles(cabinetFileStream);
            }
        }

        public IReadOnlyCollection<string> Files => files.Select(x => x.FileName).ToList();

        #region Reading Metadata
        private CabinetHeader ReadHeader(Stream cabinetStream)
        {
            ushort cbCFHeader = 0;
            byte cbCFFolder = 0;
            byte cbCFData = 0;
            byte[] AdditionalData = new byte[0];

            var cabinetBinaryReader = new BinaryReader(cabinetStream);

            CFHEADER header = cabinetBinaryReader.BaseStream.ReadStruct<CFHEADER>();

            if (StructuralComparisons.StructuralComparer.Compare(header.signature, CabinetMagic) != 0)
            {
                throw new Exception($"Bad Cabinet: Invalid Signature: {header.signature:X}");
            }

            if ((header.flags & CFHEADER.Options.ReservePresent) != 0)
            {
                cbCFHeader = cabinetBinaryReader.ReadUInt16();
                cbCFFolder = cabinetBinaryReader.ReadByte();
                cbCFData = cabinetBinaryReader.ReadByte();
                AdditionalData = cabinetBinaryReader.ReadBytes(cbCFHeader);
            }

            if ((header.flags & CFHEADER.Options.PreviousCabinet) != 0)
            {
                var prevCab = cabinetBinaryReader.BaseStream.ReadString();
                var prevDisk = cabinetBinaryReader.BaseStream.ReadString();

                throw new Exception($"Unsupported Cabinet: Multi Part: {prevCab}/{prevDisk}");
            }

            if ((header.flags & CFHEADER.Options.NextCabinet) != 0)
            {
                var prevCab = cabinetBinaryReader.BaseStream.ReadString();
                var prevDisk = cabinetBinaryReader.BaseStream.ReadString();

                throw new Exception($"Unsupported Cabinet: Multi Part: {prevCab}/{prevDisk}");
            }

            return new CabinetHeader()
            {
                CabinetFileHeader = header,
                AdditionalApplicationData = AdditionalData,
                VolumeAdditionalApplicationDataSize = cbCFFolder,
                DataAdditionalApplicationDataSize = cbCFData
            };
        }

        private IReadOnlyCollection<CabinetVolume> ReadVolumes(Stream cabinetStream)
        {
            var cabinetBinaryReader = new BinaryReader(cabinetStream);

            List<CabinetVolume> volumes = new List<CabinetVolume>();
            for (int i = 0; i < cabinetHeader.CabinetFileHeader.cFolders; i++)
            {
                CabinetVolume volume = new CabinetVolume()
                {
                    CabinetFileVolume = cabinetStream.ReadStruct<CFFOLDER>(),
                    AdditionalApplicationData = cabinetHeader.VolumeAdditionalApplicationDataSize > 0 ? cabinetBinaryReader.ReadBytes(cabinetHeader.VolumeAdditionalApplicationDataSize) : new byte[0]
                };

                volumes.Add(volume);

                if (volume.CabinetFileVolume.typeCompress != CFFOLDER.CFTYPECOMPRESS.TYPE_LZX &&
                    volume.CabinetFileVolume.typeCompress != CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP &&
                    volume.CabinetFileVolume.typeCompress != CFFOLDER.CFTYPECOMPRESS.TYPE_NONE)
                {
                    throw new Exception("Unsupported Cabinet: Only LZX, MSZip and Store is currently supported");
                }

                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                {
                    if (volume.CabinetFileVolume.typeCompressOption < 15 || volume.CabinetFileVolume.typeCompressOption > 21)
                    {
                        throw new Exception("Unsupported Cabinet: LZX variable does not fall in supported ranges");
                    }
                }
            }

            return volumes;
        }

        private IReadOnlyCollection<CabinetVolumeFile> ReadVolumeFiles(Stream cabinetStream)
        {
            var cabinetBinaryReader = new BinaryReader(cabinetStream);

            if (cabinetBinaryReader.BaseStream.Position != cabinetHeader.CabinetFileHeader.coffFiles)
            {
                throw new Exception("Bad Cabinet: First File Block does not match header");
            }

            List<CabinetVolumeFile> files = new List<CabinetVolumeFile>();
            for (int i = 0; i < cabinetHeader.CabinetFileHeader.cFiles; i++)
            {
                CFFILE file = cabinetBinaryReader.BaseStream.ReadStruct<CFFILE>();

                string name = "";
                if (file.IsFileNameUTF8())
                    name = cabinetBinaryReader.BaseStream.ReadUTF8tring();
                else
                    name = cabinetBinaryReader.BaseStream.ReadString();

                files.Add(new CabinetVolumeFile()
                {
                    CabinetFileVolumeFile = file,
                    FileName = name.Replace('\\', Path.DirectorySeparatorChar)
                });
            }

            return files;
        }
        #endregion

        public void ExtractAllFiles(string OutputDirectory, Action<int, string> progressCallBack = null)
        {
            // Cleanup existing files
            foreach (CabinetVolumeFile file in files)
            {
                string destination = Path.Combine(OutputDirectory, file.FileName);
                if (!Directory.Exists(Path.GetDirectoryName(destination)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                }

                if (File.Exists(destination))
                    File.Delete(destination);
            }

            // Do the extraction
            for (int volumeIndex = 0; volumeIndex < volumes.Count; volumeIndex++)
            {
                CabinetVolume volume = volumes.ElementAt(volumeIndex);

                LzxDecoder lzx = null;
                Inflater inf = null;
                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                {
                    lzx = new LzxDecoder(volume.CabinetFileVolume.typeCompressOption);
                }

                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
                {
                    inf = new Inflater(true);
                }

                List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> datas = new List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)>();
                List<(CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)> fileBlockMap = new List<(CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)>();

                // Build Data Map
                using (var cabinetFileStream = File.OpenRead(InputFile))
                {
                    var cabinetBinaryReader = new BinaryReader(cabinetFileStream);
                    cabinetFileStream.Seek(volume.CabinetFileVolume.firstDataBlockOffset, SeekOrigin.Begin);

                    int offset = 0;
                    for (int i = 0; i < volume.CabinetFileVolume.dataBlockCount; i++)
                    {
                        CFDATA CabinetData = cabinetBinaryReader.BaseStream.ReadStruct<CFDATA>();
                        cabinetBinaryReader.BaseStream.Seek(cabinetHeader.DataAdditionalApplicationDataSize, SeekOrigin.Current);
                        datas.Add((CabinetData, (int)cabinetBinaryReader.BaseStream.Position, offset, offset + CabinetData.cbUncomp - 1, i));
                        cabinetBinaryReader.BaseStream.Seek(CabinetData.cbData, SeekOrigin.Current);
                        offset += CabinetData.cbUncomp;
                    }

                    // Build Block Map
                    foreach (CabinetVolumeFile file in files)
                    {
                        if (file.CabinetFileVolumeFile.iFolder != volumeIndex)
                            continue;

                        var FirstBlock = datas.First(x => x.beginFolderOffset <= file.CabinetFileVolumeFile.uoffFolderStart &&
                                                            file.CabinetFileVolumeFile.uoffFolderStart <= x.endFolderOffset);
                        var LastBlock = datas.First(x => x.beginFolderOffset <= (file.CabinetFileVolumeFile.uoffFolderStart + file.CabinetFileVolumeFile.cbFile - 1) &&
                                                            (file.CabinetFileVolumeFile.uoffFolderStart + file.CabinetFileVolumeFile.cbFile - 1) <= x.endFolderOffset);

                        int fileBeginFolderOffset = (int)file.CabinetFileVolumeFile.uoffFolderStart;
                        int fileEndFolderOffset = (int)file.CabinetFileVolumeFile.uoffFolderStart + (int)file.CabinetFileVolumeFile.cbFile - 1;

                        int start = (int)file.CabinetFileVolumeFile.uoffFolderStart - FirstBlock.beginFolderOffset;
                        int end = fileEndFolderOffset - LastBlock.beginFolderOffset;

                        fileBlockMap.Add((file, FirstBlock.index, start, LastBlock.index, end));
                    }

                    int fcount = 0;

                    for (int i = 0; i < volume.CabinetFileVolume.dataBlockCount; i++)
                    {
                        var CabinetData = datas[i];

                        cabinetBinaryReader.BaseStream.Seek(CabinetData.dataOffsetCabinet, SeekOrigin.Begin);

                        byte[] uncompressedDataBlock = new byte[CabinetData.dataStruct.cbUncomp];
                        byte[] compressedDataBlock = null;

                        if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
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
                            ExpandBlock(uncompressedDataBlockStream, compressedDataBlockStream, volume.CabinetFileVolume.typeCompress, lzx, inf);
                        }

                        foreach (CabinetVolumeFile file in files)
                        {
                            if (file.CabinetFileVolumeFile.iFolder != volumeIndex)
                                continue;

                            var mapping = fileBlockMap.First(x => x.file.FileName == file.FileName);

                            // This block contains this file
                            if (mapping.startingBlock <= i && i <= mapping.endingBlock)
                            {
                                int start = 0;
                                int end = CabinetData.dataStruct.cbUncomp - 1;

                                bool IsFirstBlock = mapping.startingBlock == i;
                                bool IsLastBlock = mapping.endingBlock == i;

                                if (IsFirstBlock)
                                {
                                    progressCallBack?.Invoke((int)Math.Round((double)fcount / (double)cabinetHeader.CabinetFileHeader.cFiles * 100d), mapping.file.FileName);
                                    start = mapping.startingBlockOffset;
                                }

                                if (IsLastBlock)
                                {
                                    end = mapping.endingBlockOffset;
                                    fcount++;
                                }

                                int count = end - start + 1;

                                string destination = Path.Combine(OutputDirectory, file.FileName);
                                using (var uncompressedDataStream = File.Open(destination, FileMode.OpenOrCreate))
                                {
                                    uncompressedDataStream.Seek(0, SeekOrigin.End);
                                    uncompressedDataStream.Write(uncompressedDataBlock, start, count);
                                }

                                if (IsLastBlock)
                                {
                                    File.SetAttributes(destination, file.CabinetFileVolumeFile.GetFileAttributes());
                                    var dt = file.CabinetFileVolumeFile.GetDateTime();
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
            for (int volumeIndex = 0; volumeIndex < volumes.Count; volumeIndex++)
            {
                CabinetVolume volume = volumes.ElementAt(volumeIndex);

                LzxDecoder lzx = null;
                Inflater inf = null;
                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                {
                    lzx = new LzxDecoder(volume.CabinetFileVolume.typeCompressOption);
                }

                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
                {
                    inf = new Inflater(true);
                }

                List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> datas = new List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)>();
                List<(CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)> fileBlockMap = new List<(CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)>();

                // Build Data Map
                using (var cabinetFileStream = File.OpenRead(InputFile))
                {
                    var cabinetBinaryReader = new BinaryReader(cabinetFileStream);
                    cabinetFileStream.Seek(volume.CabinetFileVolume.firstDataBlockOffset, SeekOrigin.Begin);

                    int offset = 0;
                    for (int i = 0; i < volume.CabinetFileVolume.dataBlockCount; i++)
                    {
                        CFDATA CabinetData = cabinetBinaryReader.BaseStream.ReadStruct<CFDATA>();
                        cabinetBinaryReader.BaseStream.Seek(cabinetHeader.DataAdditionalApplicationDataSize, SeekOrigin.Current);
                        datas.Add((CabinetData, (int)cabinetBinaryReader.BaseStream.Position, offset, offset + CabinetData.cbUncomp - 1, i));
                        cabinetBinaryReader.BaseStream.Seek(CabinetData.cbData, SeekOrigin.Current);
                        offset += CabinetData.cbUncomp;
                    }

                    // Build Block Map
                    foreach (CabinetVolumeFile file in files)
                    {
                        if (file.CabinetFileVolumeFile.iFolder != volumeIndex)
                            continue;

                        var FirstBlock = datas.First(x => x.beginFolderOffset <= file.CabinetFileVolumeFile.uoffFolderStart &&
                                                            file.CabinetFileVolumeFile.uoffFolderStart <= x.endFolderOffset);
                        var LastBlock = datas.First(x => x.beginFolderOffset <= (file.CabinetFileVolumeFile.uoffFolderStart + file.CabinetFileVolumeFile.cbFile - 1) &&
                                                            (file.CabinetFileVolumeFile.uoffFolderStart + file.CabinetFileVolumeFile.cbFile - 1) <= x.endFolderOffset);

                        int fileBeginFolderOffset = (int)file.CabinetFileVolumeFile.uoffFolderStart;
                        int fileEndFolderOffset = (int)file.CabinetFileVolumeFile.uoffFolderStart + (int)file.CabinetFileVolumeFile.cbFile - 1;

                        int start = (int)file.CabinetFileVolumeFile.uoffFolderStart - FirstBlock.beginFolderOffset;
                        int end = fileEndFolderOffset - LastBlock.beginFolderOffset;

                        fileBlockMap.Add((file, FirstBlock.index, start, LastBlock.index, end));
                    }

                    int fcount = 0;

                    for (int i = 0; i < volume.CabinetFileVolume.dataBlockCount; i++)
                    {
                        var CabinetData = datas[i];

                        cabinetBinaryReader.BaseStream.Seek(CabinetData.dataOffsetCabinet, SeekOrigin.Begin);

                        byte[] uncompressedDataBlock = new byte[CabinetData.dataStruct.cbUncomp];
                        byte[] compressedDataBlock = null;

                        if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
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
                            ExpandBlock(uncompressedDataBlockStream, compressedDataBlockStream, volume.CabinetFileVolume.typeCompress, lzx, inf);
                        }

                        foreach (CabinetVolumeFile file in files)
                        {
                            if (file.CabinetFileVolumeFile.iFolder != volumeIndex)
                                continue;

                            if (file.FileName.ToLower() != FileName.ToLower())
                                continue;

                            var mapping = fileBlockMap.First(x => x.file.FileName == file.FileName);

                            // This block contains this file
                            if (mapping.startingBlock <= i && i <= mapping.endingBlock)
                            {
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

        private void ExpandBlock(Stream uncompressedDataBlockStream, Stream compressedDataBlockStream, CFFOLDER.CFTYPECOMPRESS compressionType, LzxDecoder lzx, Inflater inf)
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
                        using (var br = new BinaryReader(compressedDataBlockStream))
                        using (var bw = new BinaryWriter(uncompressedDataBlockStream))
                        {
                            byte[] inBuffer = br.ReadBytes((int)br.BaseStream.Length);
                            inf.SetInput(inBuffer);
                            byte[] outBuffer = new byte[bw.BaseStream.Length];
                            int ret = inf.Inflate(outBuffer);
                            if (ret == 0)
                            {
                                throw new Exception("Inflate failed");
                            }
                            bw.Write(outBuffer);
                            inf.Reset();
                        }
                        break;
                    }
            }
        }
    }
}