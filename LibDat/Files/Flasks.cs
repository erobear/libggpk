﻿using System;
using System.IO;

namespace LibDat.Files
{
	public class Flasks : BaseDat
	{
		public Int64 Unknown0 { get; set; }
		[StringIndex]
		public int Index0 { get; set; }
		public int Unknown1 { get; set; }
		public int Unknown2 { get; set; }
		public int Unknown3 { get; set; }
		public int Unknown4 { get; set; }
		public Int64 Unknown5 { get; set; }
		public int Data0Length { get; set; }
		[UInt32Index]
		public int Data0 { get; set; }

		public Flasks(BinaryReader inStream)
		{
			Unknown0 = inStream.ReadInt64();
			Index0 = inStream.ReadInt32();
			Unknown1 = inStream.ReadInt32();
			Unknown2 = inStream.ReadInt32();
			Unknown3 = inStream.ReadInt32();
			Unknown4 = inStream.ReadInt32();
			Unknown5 = inStream.ReadInt64();
			Data0Length = inStream.ReadInt32();
			Data0 = inStream.ReadInt32();
		}

		public override void Save(BinaryWriter outStream)
		{
			outStream.Write(Unknown0);
			outStream.Write(Index0);
			outStream.Write(Unknown1);
			outStream.Write(Unknown2);
			outStream.Write(Unknown3);
			outStream.Write(Unknown4);
			outStream.Write(Unknown5);
			outStream.Write(Data0Length);
			outStream.Write(Data0);
		}

		public override int GetSize()
		{
			return 0x2C;
		}
	}
}