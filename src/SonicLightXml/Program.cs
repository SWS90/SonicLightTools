using System;
using System.IO;
using HedgeLib.IO;
using System.Xml.Linq;

namespace SonicLightXml
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // If the user dropped a .light or light.xml file onto the program, read it.
            if (args.Length > 0)
            {
                string filePath = args[0];
                var fileInfo = new FileInfo(filePath);

                if (args[0].EndsWith(".light"))
                {
                    LoadLight(filePath);
                    Environment.Exit(0); //Close the command line window.
                }
                else if (args[0].EndsWith(".light.xml"))
                {
                    SaveLight(filePath);
                    Environment.Exit(0); //Close the command line window.
                }
                else
                {
                    Console.WriteLine("The file you dropped is not a .light or .light.xml file!");
                }
            }

            // Otherwise, show help
            else
            {
                Console.WriteLine("Sonic Unleashed/Gens/LW/Forces LightXmlTool.");
                Console.WriteLine("Made by SWS90 with help from Radfordhound.");
                Console.WriteLine("Reads the following for .light files:");
                Console.WriteLine("Directional: X,Y,Z Position, and RGB Color.");
                Console.WriteLine("Omni: X,Y,Z Position, RGB Color, Inner and Outer Range.");

                Console.WriteLine();
                Console.WriteLine("Usage: Drag and drop any .light file to convert to .light.xml,");
                Console.WriteLine("or drag and drop any .light.xml file to convert back to .light.");
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to close.");
            Console.ReadLine();
        }

        public static void LoadLight(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                var reader = new ExtendedBinaryReader(fileStream, true);

                uint fileSize = reader.ReadUInt32();
                uint rootNodeType = reader.ReadUInt32();
                uint finalTableOffset = reader.ReadUInt32();
                uint rootNodeOffset = reader.ReadUInt32();
                uint finalTableOffsetAbs = reader.ReadUInt32();
                uint padding = reader.ReadUInt32();
                uint value_LightType = reader.ReadUInt32();
                float XPos = reader.ReadSingle();
                float YPos = reader.ReadSingle();
                float ZPos = reader.ReadSingle();
                float ColorR = reader.ReadSingle();
                float ColorG = reader.ReadSingle();
                float ColorB = reader.ReadSingle();

                // Generate XML
                var rootElement = new XElement("SonicLightXml");
                rootElement.Add(new XElement("LightType", value_LightType));

                var lightPosition = new XElement("Position");
                lightPosition.Add(new XElement("X", XPos));
                lightPosition.Add(new XElement("Y", YPos));
                lightPosition.Add(new XElement("Z", ZPos));
                rootElement.Add(lightPosition);

                var lightColor = new XElement("Color");
                lightColor.Add(new XElement("R", ColorR));
                lightColor.Add(new XElement("G", ColorG));
                lightColor.Add(new XElement("B", ColorB));
                rootElement.Add(lightColor);

                // Omni-Specific Values
                if (value_LightType == 1)
                {
                    uint Unknown1 = reader.ReadUInt32();
                    uint Unknown2 = reader.ReadUInt32();
                    uint Unknown3 = reader.ReadUInt32();
                    float OmniInnerRange = reader.ReadSingle();
                    float OmniOuterRange = reader.ReadSingle();

                    var OmniLightRange = new XElement("OmniLightRange");
                    OmniLightRange.Add(new XElement("OmniLightInnerRange", OmniInnerRange));
                    OmniLightRange.Add(new XElement("OmniLightOuterRange", OmniOuterRange));
                    rootElement.Add(OmniLightRange);
                }

                // Save the Generated XML File
                var xml = new XDocument(rootElement);
                xml.Save($"{filePath}.xml");
            }
        }

        public static void SaveLight(string filePath)
        {
            // Get file path without the .xml at the end
            var fileInfo = new FileInfo(filePath);
            string shortName = filePath.Substring(0,
                filePath.Length - fileInfo.Extension.Length);

            var xml = XDocument.Load(filePath);
            var root = xml.Root;

            var lightTypeElem = root.Element("LightType");
            var lightPosElem = root.Element("Position");
            var lightColorElem = root.Element("Color");

            using (var fileStream = File.Create($"{shortName}"))
            {
                var writer = new ExtendedBinaryWriter(fileStream, true);
                
                // Write Header
                writer.Write(0);
                writer.Write(1);
                writer.Write(0);
                writer.Write(24);
                writer.Write(0);
                writer.Write(0);

                // Light Type
                bool isOmniLight = (lightTypeElem != null && lightTypeElem.Value == "1");
                writer.Write((isOmniLight) ? 1 : 0);

                // Position
                WriteXMLFloat(writer, lightPosElem?.Element("X"));
                WriteXMLFloat(writer, lightPosElem?.Element("Y"));
                WriteXMLFloat(writer, lightPosElem?.Element("Z"));

                // Color
                WriteXMLFloat(writer, lightColorElem?.Element("R"));
                WriteXMLFloat(writer, lightColorElem?.Element("G"));
                WriteXMLFloat(writer, lightColorElem?.Element("B"));

                // Omni-Specific Stuff
                if (isOmniLight)
                {
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);

                    var omniElem = root.Element("OmniLightRange");
                    WriteXMLFloat(writer, omniElem?.Element("OmniLightInnerRange"));
                    WriteXMLFloat(writer, omniElem?.Element("OmniLightOuterRange"));
                }

                // Fill-In Header
                writer.Write(0);
                fileStream.Position = 0;
                writer.Write((uint)fileStream.Length);

                fileStream.Position = 8;
                uint finalTablePosition = (uint)fileStream.Length - 4;
                writer.Write(finalTablePosition - 0x18);

                fileStream.Position = 16;
                writer.Write(finalTablePosition);
            }
        }

        public static void WriteXMLFloat(BinaryWriter writer, XElement elem)
        {
            // Gets a float value from the XML and writes it
            // Writes 0 if the given element was missing from the XML
            writer.Write((elem != null) ? float.Parse(elem.Value) : 0f);
        }
    }
}