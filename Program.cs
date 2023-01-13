using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TranslateTemplates
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            // Source folder for reading all master templates
            var sourceFolder = @"D:\\AOD Projects\\From4.3SR2RC3_To4.3SR2RC4\\MasterTemplates";

            // Destination folder for generated results
            var destinationFolder = @"C:\Users\DucDoanMinh\OneDrive - Add-On Products & Add-On Development\Desktop\tempTemplate";

            // Language list
            var listLanguages = new string[] { "English", "Danish", "German", "French", "Mandarin Chinese" };

            // Get all template xml files
            var sourceFiles = new DirectoryInfo(sourceFolder).GetFiles().Where(x => x.Extension == ".xml");
            foreach (var item in sourceFiles)
            {
                foreach (var lang in listLanguages)
                {
                    var text = File.ReadAllText(item.FullName);
                    Console.WriteLine($"Begin writing {item.Name.Replace("English", lang)}");
                    text = text.Replace("English", lang);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(text);

                    // Get language resource file
                    var languageResource = new ResourceManager(typeof(English));
                    switch (lang)
                    {
                        case "English":
                            languageResource = new ResourceManager(typeof(English));
                            break;
                        case "Danish":
                            languageResource = new ResourceManager(typeof(Danish));
                            break;
                        case "German":
                            languageResource = new ResourceManager(typeof(German));
                            break;
                        case "French":
                            languageResource = new ResourceManager(typeof(French));
                            break;
                        case "Mandarin Chinese":
                            languageResource = new ResourceManager(typeof(MandarinChinese));
                            break;
                    }
                    ResourceSet resourceSet = languageResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

                    // Get nodes requiring translation
                    var textNodes = doc.SelectNodes("//Text");
                    var messageInfoNodes = doc.SelectNodes("//MessageInfo/*");
                    var nodeList = textNodes.Cast<XmlNode>().Concat<XmlNode>(messageInfoNodes.Cast<XmlNode>());

                    // Begin translate
                    foreach (DictionaryEntry entry in resourceSet)
                    {
                        foreach (XmlNode node in nodeList)
                        {
                            if (node.InnerText == entry.Key.ToString())
                                node.InnerText = entry.Value.ToString();
                        }
                    }

                    // Write output file
                    text = XDocument.Parse(doc.OuterXml).ToString();

                    File.WriteAllText($@"{destinationFolder}\{item.Name.Replace("English", lang)}", text);
                    Console.WriteLine($"Finished writing {item.Name.Replace("English", lang)}");
                }
            }

            // Open destination folder after finishing
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = destinationFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
