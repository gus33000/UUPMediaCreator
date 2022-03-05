/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MediaCreationLib.BootlegEditions
{
    public static class AssemblyManifestHandler
    {
        public static void RemoveNonLTSBPackages(string manifestPath)
        {
            string content = File.ReadAllText(manifestPath);
            Assembly assembly = Deserialize(content);
            assembly.Package.Update.RemoveAll(x => x.Name.Contains("Not-Supported-On-LTSB"));
            File.WriteAllText(manifestPath, Serialize(assembly));
        }

        internal static void RemoveWOW64Package(string manifestPath, string v)
        {
            string content = File.ReadAllText(manifestPath);
            Assembly assembly = Deserialize(content);
            assembly.Package.Update.RemoveAll(x => x.Name.Contains(v, System.StringComparison.CurrentCultureIgnoreCase));
            File.WriteAllText(manifestPath, Serialize(assembly));
        }

        public static Assembly Deserialize(string Xml)
        {
            if (Xml == null)
            {
                return null;
            }

            XmlSerializer xmlSerializer = new(typeof(Assembly));

            using StringReader stringReader = new(Xml);
            return (Assembly)xmlSerializer.Deserialize(stringReader);
        }

        public static string Serialize(Assembly assembly)
        {
            if (assembly == null)
            {
                return string.Empty;
            }

            XmlSerializer xmlSerializer = new(typeof(Assembly));

            using StringWriter stringWriter = new();
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                NewLineOnAttributes = false,
                Encoding = Encoding.UTF8
            });
            xmlSerializer.Serialize(xmlWriter, assembly);
            return stringWriter.ToString()
                .Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "xmlns=\"urn:schemas-microsoft-com:asm.v3\"")
                .Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "xmlns=\"urn:schemas-microsoft-com:asm.v3\"")
                .Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"")
                .Replace("encoding=\"utf-8\"", "encoding=\"utf-8\" standalone=\"yes\"")
                .Replace(" xmlns=\"urn:schemas-microsoft-com:asm.v3\">", ">");
        }
    }
}