using System.IO;
using System.Text;
using System.Xml;
using System;

namespace BuildTool
{
    public static class Tabify
    {
        // Tabify XML document
        public static void Xml(string filename)
        {
            DoTabify(filename, false);
        }

        // Tabify Xaml document
        public static void Xaml(string filename)
        {
            DoTabify(filename, true);
        }

        // Tabify
        private static void DoTabify(string filename, bool xaml = false)
        {
            // XmlDocument container
            XmlDocument xmlDocument = new XmlDocument();

            // We want to make sure that decimal and hex character references are not lost
            string xmlString = File.ReadAllText(filename);
            xmlString = xmlString.Replace("&", "&");

            // Xml Reader settings 
            XmlReaderSettings xmlReadSettings = new XmlReaderSettings()
            {
                CheckCharacters = false,     // We have some invalid characters we want to ignore
            };

            // Use XML reader to load content to XmlDocument container
            using (XmlReader xmReader = XmlReader.Create(new StringReader(xmlString), xmlReadSettings))
            {
                xmReader.MoveToContent();
                xmlDocument.Load(xmReader);
            }

            // Customize how our XML will look, we want tabs, UTF8 encoding and new line on attributes
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,                              // Indent elements
                IndentChars = "\t",                         // Indent with tabs
                CheckCharacters = false,                    // Ignore invalid characters
                NewLineChars = Environment.NewLine,         // Set newline character
                NewLineHandling = NewLineHandling.None,     // Normalize line breaks
                Encoding = new UTF8Encoding()               // UTF8 encoding
            };

            // We do not want the xml declaration for xaml files
            if (xaml)
                xmlWriterSettings.OmitXmlDeclaration = true;    // For XAML this must be true!!!!

            StringBuilder xmlStringBuilder = new StringBuilder();

            // Write xml to file using saved settings
            using (XmlWriter xmlWriter = XmlWriter.Create(xmlStringBuilder, xmlWriterSettings))
            {
                xmlWriter.Flush();
                xmlDocument.WriteContentTo(xmlWriter);
            }

            // Restore decimal and hex character references
            xmlString = xmlStringBuilder.ToString().Replace("&", "&");
            File.WriteAllText(filename, xmlString);
        }
    }
}