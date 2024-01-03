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
            var sourceFolder = @"D:\\AOD Projects\\DSS\\MasterTemplates";

            // Destination folder for generated results
            var destinationFolder = @"C:\Users\DucDoanMinh\OneDrive - Add-On Products & Add-On Development\Desktop\tempTemplate";
            Directory.Delete(destinationFolder, true);
            Directory.CreateDirectory(destinationFolder);

            // Language list
            var listLanguages = new string[] { "English", "Danish", "German", "French", "Mandarin Chinese" /*"Polish"*/ };

            // Get all template xml files
            var sourceFiles = new DirectoryInfo(sourceFolder).GetFiles().Where(x => x.Extension == ".xml");
            foreach (var item in sourceFiles)
            {
                var text = File.ReadAllText(item.FullName);
                foreach (var lang in listLanguages)
                {
                    Console.WriteLine($"Begin writing {item.Name.Replace("English", lang)}");
                    var langText = text.Replace("English", lang);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(langText);

                    //if (doc.SelectSingleNode("//Version").InnerText != "4.3.0") continue;

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
                        case "Polish":
                            languageResource = new ResourceManager(typeof(Polish));
                            break;
                    }
                    ResourceSet resourceSet = languageResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

                    // Get nodes requiring translation
                    var listXPathToTranslate = new List<string> { 
                        "//Text", 
                        "//Placeholder", 
                        "//ShowAllRoomsText", 
                        "//ShowOnlyUsedMeetingRoomsText", 
                        "//ShowOnlyUnusedMeetingRoomsText", 
                        "//MessageInfo/*" 
                    };
                    IEnumerable<XmlNode> nodeList = new List<XmlNode>();
                    foreach (var i in listXPathToTranslate)
                    {
                        var textNodes = doc.SelectNodes(i);
                        nodeList = nodeList.Concat(textNodes.Cast<XmlNode>());
                    }

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
                    langText = XDocument.Parse(doc.OuterXml).ToString();
                    if (lang == "English") text = langText;

                    var fileName = $@"{destinationFolder}\{item.Name.Replace("English", lang).Replace("4.3.5", "4.3.6")}";
                    File.WriteAllText(fileName, langText);
                    Console.WriteLine($"Finished writing {fileName}");
                }
            }

            // Open destination folder after finishing
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = destinationFolder,
                UseShellExecute = true,
                Verb = "open"
            });

            // For changing template resource folder names

            //var sourceFolder2 = @"D:\\AOD Projects\\DSS\\DefaultTemplates";
            //foreach (var item in Directory.GetDirectories(sourceFolder2))
            //{
            //    Directory.Move(item, item.Replace("4.3.4", "4.3.5"));
            //}
        }
    }
}
