using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DirectoryCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            bool firstRun = false;
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load("DirectoryCleaner.Config");
            }
            catch
            {
                firstRun = true;
                CreateNewConfigFile(xmlDocument);
            }

            if (!firstRun)
            {
                XmlNamespaceManager xmlnm = new XmlNamespaceManager(xmlDocument.NameTable);
                xmlnm.AddNamespace("ns", "http://www.w3.org/2005/Atom");

                ParseXML(xmlDocument, xmlnm);
            }
            else
            {
                Console.WriteLine("Configuration program has been written. Modify DirectoryCleaner.Config before running the program again!");
            }


        }

        private static void CreateNewConfigFile(XmlDocument xmlDocument)
        {
            string fileName = "DirectoryCleaner.Config";
            File.Create(fileName).Dispose();
            WriteMyXML();
            xmlDocument.Load("DirectoryCleaner.Config");
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>

        /// <summary>
        ///  here is the function name
        /// </summary>
        /// <param name="xmlFile">An xmldocument object fo the file you want to parse</param>
        /// <param name="xmlnm">your xml name space</param>
        public static void ParseXML(XmlDocument xmlFile, XmlNamespaceManager xmlnm)
        {
            XmlNodeList nodes = xmlFile.SelectNodes("//ns:Directories", xmlnm);
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "Directories")
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        CleanDirectory(childNode.InnerText);
                    }
                }
            }
        }

        public static void WriteMyXML()
        {
            XmlWriterSettings mySettings = new XmlWriterSettings();
            mySettings.NewLineOnAttributes = true;
            mySettings.Encoding = System.Text.Encoding.UTF8;
            mySettings.Indent = true;
            mySettings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create("DirectoryCleaner.Config", mySettings))
            {
                string homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                writer.WriteStartDocument();
                writer.WriteStartElement("feed", "http://www.w3.org/2005/Atom");
                writer.WriteStartElement("entry");
                writer.WriteStartElement("Directories");
                writer.WriteElementString("Directory", homeDir );
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }




        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        private static void CleanDirectory(string directoryPath)
        {
           Array.ForEach(Directory.GetFiles(directoryPath), File.Delete); 
        }

    }
}


