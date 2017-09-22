using System;
using System.IO;
using System.Xml;
using NLog;

namespace DirectoryCleaner
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {

            bool firstRun = false;

            XmlDocument xmlDocument = new XmlDocument();

            try
            {
                xmlDocument.Load("DirectoryCleaner.config");
            }
            catch (FileNotFoundException) // this is the default behavior
            {
                firstRun = true;
                CreateNewConfigFile(xmlDocument);
                logger.Warn(
                    "Configuration program has been written. Modify the 'DirectoryCleaner.config' file before running the program again!");
                logger.Warn("Otherwise all the files on your desktop will be deleted.");
                Console.WriteLine(
                    "Configuration program has been written. Modify the 'DirectoryCleaner.config' file before running the program again!");
                Console.WriteLine("Otherwise all the files on your desktop will be deleted.");
            }
            catch (SystemException ex) // this is when something actually goes wrong
            {
                logger.Error(ex, "Something went wrong trying to load the 'DirectoryCleaner.config' file: " + ex);
            }

            if (!firstRun)
            {
                XmlNamespaceManager xmlnm = new XmlNamespaceManager(xmlDocument.NameTable);
                xmlnm.AddNamespace("ns", "http://www.w3.org/2005/Atom");

                ParseXML(xmlDocument, xmlnm);
            }

        }

        private static void CreateNewConfigFile(XmlDocument xmlDocument)
        {
            try
            {
                string fileName = "DirectoryCleaner.config";
                File.Create(fileName).Dispose();
                CreateConfigFile();
                xmlDocument.Load("DirectoryCleaner.config");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong creating new config file: " + ex);
            }
        }


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

        public static void CreateConfigFile()
        {
            XmlWriterSettings mySettings = new XmlWriterSettings();
            mySettings.NewLineOnAttributes = true;
            mySettings.Encoding = System.Text.Encoding.UTF8;
            mySettings.Indent = true;
            mySettings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create("DirectoryCleaner.config", mySettings))
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                writer.WriteStartDocument();
                writer.WriteStartElement("feed", "http://www.w3.org/2005/Atom");
                writer.WriteStartElement("entry");
                writer.WriteStartElement("Directories");
                writer.WriteElementString("Directory", homeDir);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

        }

        private static void CleanDirectory(string directoryPath)
        {

            try
            {
                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Something went wrong trying to delete the file: " + ex);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went wrong: " + ex);
            }
        }
    }
}



