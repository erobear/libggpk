﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
//using LibGGPK;
using System.IO;
using System.Linq.Expressions;
//using LibDat;
//using LibDat.Files;
using Ionic.Zip;
using System.Text.RegularExpressions;

//4 bytes - Number of entries
//Entry 1
//Entry 2
//...
//Entry N
//8 bytes - BB BB BB BB BB BB BB BB
//String0 String1 String2 ...

namespace TestProject
{
	public class Program
	{
		static void Output(string msg)
		{
			Console.Write(msg);
		}

		public class DatFile
		{
			public string Name { get; set; }
		}

		[STAThread]
		public static void Main(string[] args)
		{
			MakeClass();
		   // ReadAndDumpStruct();
			//if (args.Length != 1)
			//{
			//	Console.WriteLine("Derp");
			//	Console.ReadLine();
			//	return;
			//}
			//DumpDat(args[0]);
			//Console.WriteLine(  "Press any key to continue...");
			//Console.ReadLine(); //new Program();
		}

		public static void MakeClass()
		{
			Regex patternPropName = new Regex("public (?<type>unsigned short|short|int|unsigned int|Int64|bool) (?<name>[^ ]+) { get; set; }");
			Regex patternClassName = new Regex("class (?<name>[^ ]+)");

			int size = 0;

			StringBuilder sbReader = new StringBuilder();
			StringBuilder sbWriter = new StringBuilder();
			StringBuilder sbHeader = new StringBuilder();

			while(true)
			{
				string input = Console.ReadLine().Replace("\t", "    "); ;
				if (input == "}")
					break;

				string trimmedInput = input.Trim();

				Match matchClass = patternClassName.Match(trimmedInput);
				if(matchClass.Success)
				{
					sbHeader.AppendFormat("using System;\nusing System.IO;\n\nnamespace LibDat.Files\n{{\n\tpublic class {0} : BaseDat\n", matchClass.Groups["name"].Value);
					sbReader.AppendFormat(
						"\n        public {0}()\n" +
						"        {{\n" +
						"\n" +
						"        }}\n\n" +
						"        public {0}(BinaryReader inStream)\n" +
						"        {{\n", matchClass.Groups["name"].Value);

					sbWriter.Append(
						"        public override void Save(BinaryWriter outStream)\n" +
						"        {\n");

					continue;
				}



				Match matchProp = patternPropName.Match(trimmedInput);
				if (matchProp.Success)
				{
					switch (matchProp.Groups["type"].Value)
					{
						case "int":
							sbReader.AppendFormat("           {0} = inStream.ReadInt32();\n", matchProp.Groups["name"].Value);
							sbWriter.AppendFormat("           outStream.Write({0});\n", matchProp.Groups["name"].Value);
							size += 4;
							break;
						case "unsigned int":
							sbReader.AppendFormat("           {0} = inStream.ReadUInt32();\n", matchProp.Groups["name"].Value);
							sbWriter.AppendFormat("           outStream.Write({0});\n", matchProp.Groups["name"].Value);
							size += 4;
							break;
						case "bool":
							sbReader.AppendFormat("           {0} = inStream.ReadBoolean();\n", matchProp.Groups["name"].Value);
							sbWriter.AppendFormat("           outStream.Write({0});\n", matchProp.Groups["name"].Value);
							size += 1;
							break;
						case "Int64":
							sbReader.AppendFormat("           {0} = inStream.ReadInt64();\n", matchProp.Groups["name"].Value);
							sbWriter.AppendFormat("           outStream.Write({0});\n", matchProp.Groups["name"].Value);
							size += 8;
							break;
						case "short":
							sbReader.AppendFormat("           {0} = inStream.ReadInt16();\n", matchProp.Groups["name"].Value);
							sbWriter.AppendFormat("           outStream.Write({0});\n", matchProp.Groups["name"].Value);
							size += 2;
							break;
						case "unsigned short":
							sbReader.AppendFormat("           {0} = inStream.ReadUInt16();\n", matchProp.Groups["name"].Value);
							sbWriter.AppendFormat("           outStream.Write({0});\n", matchProp.Groups["name"].Value);
							size += 2;
							break;
					}
					sbHeader.AppendLine(input);
					continue;
				}
				else
				{
					if (sbHeader.Length != 0)
					{
						if (!input.Contains("}"))
						{
							sbHeader.AppendLine(input);
						}
					}
				}
			}

			sbWriter.AppendFormat("        }}\n\n        public override int GetSize()\n        {{\n            return 0x{0:X};\n        }}\n    }}\n}}", size);
			sbReader.Append("        }\n\n");

			Clipboard.SetText(string.Format("{0}{1}{2}", sbHeader.ToString(), sbReader.ToString(), sbWriter.ToString()));
			Console.WriteLine("Updated clipboard text");
		}

		private static string ggpkPath = @"o:\Program Files (x86)\Grinding Gear Games\Path of Exile\content.ggpk";
		//private static Dictionary<string, FileRecord> RecordsByPath;
		//private static GGPK content;

		/*private static void InitGGPK()
		{
			if (!File.Exists(ggpkPath))
			{
				Microsoft.Win32.RegistryKey start = Microsoft.Win32.Registry.CurrentUser;
				Microsoft.Win32.RegistryKey programName = start.OpenSubKey("Software\\GrindingGearGames\\Path of Exile");
				if (programName != null)
				{
					string pathString = (string)programName.GetValue("InstallLocation");
					if (pathString != string.Empty && File.Exists(pathString + "Content.ggpk"))
					{
						ggpkPath = pathString + "Content.ggpk";
					}
				}
			}

			content = new GGPK();
			content.Read(ggpkPath, Output);
			
			RecordsByPath = new Dictionary<string, FileRecord>(content.RecordOffsets.Count);
			DirectoryTreeNode.TraverseTreePostorder(content.DirectoryRoot, null, n => RecordsByPath.Add(n.GetDirectoryPath() + n.Name, n as FileRecord));

			//DumpGGPK();
		}

		private static void DumpGGPK()
		{
			foreach (var item in RecordsByPath)
			{
				Console.WriteLine(item.Key + " -> " + item.Value.Name);
			}
		}
		*/























		private static void DumpDat(string filePath)
		{
			byte[] fileBytes = File.ReadAllBytes(filePath);
			StringBuilder sb = new StringBuilder();
			using (MemoryStream ms = new MemoryStream(fileBytes))
			{
				using (BinaryReader br = new BinaryReader(ms, System.Text.Encoding.Unicode))
				{
					int entryCount = br.ReadInt32();
					int dataTableStart = -1;

					while (true)
					{
						if (br.ReadByte() == 0xbb &&
							br.ReadByte() == 0xbb &&
							br.ReadByte() == 0xbb &&
							br.ReadByte() == 0xbb &&
							br.ReadByte() == 0xbb &&
							br.ReadByte() == 0xbb &&
							br.ReadByte() == 0xbb &&
							br.ReadByte() == 0xbb)
						{
							dataTableStart = (int)(br.BaseStream.Position - 8);
							break;
						}
					}


					br.BaseStream.Seek(4, SeekOrigin.Begin);

					int entrySize = dataTableStart / entryCount;
					Console.WriteLine("0x{0:X2}", entrySize);
					for (int i = 0; i < entrySize/4; i++)
					{
						sb.Append("Unknown" + i + "\t\t");
					}
					sb.AppendLine();

					//sb.AppendLine(Path.GetFileNameWithoutExtension(filePath));
					for (int i = 0; i < entryCount; i++)
					{
						byte[] data = br.ReadBytes(entrySize);
						for (int j = 0; j < data.Length; j++)
						{
							if (j != 0 && j%4 == 0)
								sb.Append("\t");

							sb.AppendFormat("{0:X2} ", data[j]);
						}
						sb.AppendLine();
					}

					br.BaseStream.Seek(dataTableStart, SeekOrigin.Begin);
					File.WriteAllBytes(filePath + "_data.bin", br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position)));
				}
			}

			File.WriteAllText(filePath + "_bytes.txt", sb.ToString());
		}

		private static void DumpProps(string[] vars)
		{
			foreach (var @var in vars)
			{
				string lowerVar = var.ToLower();

				if (lowerVar.Contains("flag"))
					Console.WriteLine("		public bool {0} {{ get; set; }}", var);
				else if (lowerVar.Contains("64"))
					Console.WriteLine("		public Int64 {0} {{ get; set; }}", var);
				else
				{
					if (lowerVar.Contains("index"))
						Console.WriteLine("		[StringIndex]");
					else if (lowerVar.Contains("data"))
						Console.WriteLine("		[DataIndex]");

					Console.WriteLine("		public int {0} {{ get; set; }}", var);
				}
			}
		}

		private static void DumpRead(string[] vars, string className)
		{
			Console.WriteLine();
			Console.WriteLine("		public {0}(BinaryReader inStream)", className);
			Console.WriteLine("		{");
			foreach (var @var in vars)
			{
				string lowerVar = var.ToLower();

				if (lowerVar.Contains("flag"))
					Console.WriteLine("			{0} = inStream.ReadBoolean();", var);
				else if (lowerVar.Contains("64"))
					Console.WriteLine("			{0} = inStream.ReadInt64();", var);
				else
					Console.WriteLine("			{0} = inStream.ReadInt32();", var);
			}
			Console.WriteLine("		}");
		}

		private static void DumpWrite(string[] vars)
		{
			Console.WriteLine();
			Console.WriteLine("		public override void Save(BinaryWriter outStream)");
			Console.WriteLine("		{");
			foreach (var @var in vars)
			{
				string lowerVar = var.ToLower();

				Console.WriteLine("			outStream.Write({0});", var);
			}
			Console.WriteLine("		}");
		}

		private static void DumpSize(string[] vars)
		{
			int size = 0;

			foreach (var @var in vars)
			{
				string lowerVar = var.ToLower();

				if (lowerVar.Contains("flag"))
					size += 1;
				else if (lowerVar.Contains("64"))
					size += 8;
				else
					size += 4;
			}

			Console.WriteLine();
			Console.WriteLine("		public override int GetSize()");
			Console.WriteLine("		{");
			Console.WriteLine("			return 0x{0:X};", size);
			Console.WriteLine("		}");
		}

		private static void ReadAndDumpStruct()
		{
			string className = Console.ReadLine().Trim();
			string input = Console.ReadLine().Trim();
			string previousInput = input;
			Console.Clear();
			do
			{
				previousInput = input;
				input = input.Replace("\t\t", "\t");
			} while (previousInput != input);

			string[] varNames = input.Split(new char[] { '\t' });

			Console.WriteLine("using System.IO;");
			Console.WriteLine();
			Console.WriteLine("namespace LibDat.Files");
			Console.WriteLine("{");
			Console.WriteLine("	public class {0} : BaseDat", className);
			Console.WriteLine("	{");
			DumpProps(varNames);
			DumpRead(varNames, className);
			DumpWrite(varNames);
			DumpSize(varNames);
			Console.WriteLine("	}");
			Console.WriteLine("}");

			Console.ReadKey();
		}
	}
}