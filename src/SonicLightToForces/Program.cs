using System;
using HedgeLib.IO;
using System.IO;

namespace SonicLightToForces
{
    public enum LightType
    {
        Directional, Omni
    }
    public class Program
    {
        public static LightType lightType;
        public static float XPos;
        public static float YPos;
        public static float ZPos;
        public static float ColorR;
        public static float ColorG;
        public static float ColorB;
        //Omni Stuff
        public static uint Unknown1;
        public static uint Unknown2;
        public static uint Unknown3;
        public static float OmniInnerRange;
        public static float OmniOuterRange;

        public static void Main(string[] args)
        {
            // If the user dropped a light file onto the program...
            if (args.Length > 0)
            {
                string filePath = args[0];
                var fileInfo = new FileInfo(filePath);

                if (args[0].EndsWith(".light"))
                {
                    Console.WriteLine("SonicLightToForces.\n");
                    Console.WriteLine("Working With: " + fileInfo.Name);
                    LoadLight(filePath);
                    SaveLight(args[0]);
                    Console.WriteLine(fileInfo.Name + " Has been re-saved");
                    ReloadLight(args[0]);
                }
                else if (Directory.Exists(args[0]))
                {
                    foreach (var file in Directory.GetFiles(args[0], "*.light"))
                    {
                        Console.WriteLine($"Working with: {Path.GetFileName(file)}");
                        LoadLight(file);
                        SaveLight(file);
                        Console.WriteLine($"{Path.GetFileName(file)} Has been re-saved");
                        ReloadLight(file);
                    }
                }
                else
                {
                    Console.WriteLine("The file you dropped is not a .light file!");
                }
            }

            // Otherwise, show help
            else
            {
                Console.WriteLine("SonicLightToForces.\n");
                Console.WriteLine("Sonic Unleashed/Gens/LW/ To Forces .light converter.");
                Console.WriteLine("Made by SWS90 with help from Sajid\n");
                Console.WriteLine("Takes any .light file and multiplies the RGB color by 10,000");
                Console.WriteLine("");
                Console.WriteLine("Usage: Drag and drop any .light file to convert to Sonic Forces (multiply the RGB color by 10,000).\nOr a folder with .light files to convert multiple files.");
            }
            Console.WriteLine("");
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
                lightType = (LightType)reader.ReadUInt32();
                XPos = reader.ReadSingle();
                YPos = reader.ReadSingle();
                ZPos = reader.ReadSingle();
                ColorR = reader.ReadSingle();
                ColorG = reader.ReadSingle();
                ColorB = reader.ReadSingle();

                // Print out Light information
                Console.WriteLine($"Light type: {lightType}");
                Console.WriteLine("");
                Console.WriteLine($"ColorR is {ColorR}," + $" It should be {ColorR*10000} after re-saving");
                Console.WriteLine($"ColorG is {ColorG}," + $" It should be {ColorG*10000} after re-saving");
                Console.WriteLine($"ColorB is {ColorB}," + $" It should be {ColorB*10000} after re-saving");
                Console.WriteLine("");
                // Read Omni-Specific Values if present.
                if (lightType == LightType.Omni)
                {
                    Unknown1 = reader.ReadUInt32();
                    Unknown2 = reader.ReadUInt32();
                    Unknown3 = reader.ReadUInt32();
                    OmniInnerRange = reader.ReadSingle();
                    OmniOuterRange = reader.ReadSingle();
                }
            }
        }

        public static void SaveLight(string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                var writer = new ExtendedBinaryWriter(fileStream, true);

                // Write Header
                writer.Write(0);
                writer.Write(1);
                writer.Write(0);
                writer.Write(24);
                writer.Write(0);
                writer.Write(0);

                // Write Light Type
                bool isOmniLight = (lightType == LightType.Omni);
                writer.Write((isOmniLight) ? 1 : 0);

                // Write Light XYZ Position and RGB Color
                writer.Write(XPos);
                writer.Write(YPos);
                writer.Write(ZPos);
                writer.Write(ColorR*10000);
                writer.Write(ColorG*10000);
                writer.Write(ColorB*10000);
                
                // Write Omni-Specific Values
                if (isOmniLight)
                {
                    writer.Write(Unknown1);
                    writer.Write(Unknown2);
                    writer.Write(Unknown3);
                    writer.Write(OmniInnerRange);
                    writer.Write(OmniOuterRange);
                }

                // Write Offset Table
                writer.Write(0);

                // Fill-In Header Values
                fileStream.Position = 0;
                writer.Write((uint)fileStream.Length);

                uint finalTablePosition = (uint)fileStream.Length - 4;
                fileStream.Position = 8;
                writer.Write(finalTablePosition - 0x18);

                fileStream.Position = 16;
                writer.Write(finalTablePosition);
            }
        }
        public static void ReloadLight(string filePath)
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
                lightType = (LightType)reader.ReadUInt32();
                XPos = reader.ReadSingle();
                YPos = reader.ReadSingle();
                ZPos = reader.ReadSingle();
                ColorR = reader.ReadSingle();
                ColorG = reader.ReadSingle();
                ColorB = reader.ReadSingle();

                // Print out Light information
                Console.WriteLine("");
                Console.WriteLine($"ColorR is now {ColorR}," + $" after re-saving");
                Console.WriteLine($"ColorG is now {ColorG}," + $" after re-saving");
                Console.WriteLine($"ColorB is now {ColorB}," + $" after re-saving"); 
                
                // Read Omni-Specific Values if present.
                if (lightType == LightType.Omni)
                {
                    uint Unknown1 = reader.ReadUInt32();
                    uint Unknown2 = reader.ReadUInt32();
                    uint Unknown3 = reader.ReadUInt32();
                    OmniInnerRange = reader.ReadSingle();
                    OmniOuterRange = reader.ReadSingle();
                }
            }
        }
    }
}
