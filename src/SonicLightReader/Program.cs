using System;
using System.IO;
using HedgeLib.IO;

namespace SonicLightReader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // If the user dropped a light file onto the program, read it
            if (args.Length > 0)
            {
                string filePath = args[0];
                var fileInfo = new FileInfo(filePath);

                if (args[0].EndsWith(".light"))
                {
                    Console.WriteLine("SonicLightReader.\n");
                    Console.WriteLine("Light name: {0}", fileInfo.Name);
                    LoadLight(filePath);
                }
                else
                {
                    Console.WriteLine("The file you dropped is not a .light file!");
                }
            }

            // Otherwise, show help
            else
            {
                Console.WriteLine("Sonic Unleashed/Gens/LW/Forces .light reader.");
                Console.WriteLine("Made by SWS90 with help from Radfordhound.");
                Console.WriteLine("Reads the following for .light files:");
                Console.WriteLine("Directional: X,Y,Z Position, and RGB Color.");
                Console.WriteLine("Omni: X,Y,Z Position, RGB Color, Inner and Outer Range.");

                Console.WriteLine();
                Console.WriteLine("Usage: Drag and drop any .light file to get it's infomation.");
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

                // Print out Light information
                Console.WriteLine($"Light type: {value_LightType}");
                Console.WriteLine($"PosX: {XPos}");
                Console.WriteLine($"PosY: {YPos}");
                Console.WriteLine($"PosZ: {ZPos}");
                Console.WriteLine($"ColorR: {ColorR}");
                Console.WriteLine($"ColorG: {ColorG}");
                Console.WriteLine($"ColorB: {ColorB}");

                // Read Omni-Specific Values and print them, if present.
                bool isOmniLight = (value_LightType == 1);
                if (isOmniLight)
                {
                    uint Unknown1 = reader.ReadUInt32();
                    uint Unknown2 = reader.ReadUInt32();
                    uint Unknown3 = reader.ReadUInt32();
                    float OmniInnerRange = reader.ReadSingle();
                    float OmniOuterRange = reader.ReadSingle();

                    Console.WriteLine($"OmniLightInnerRange: {OmniInnerRange}");
                    Console.WriteLine($"OmniLightOuterRange: {OmniOuterRange}");
                }
            }
        }
    }
}