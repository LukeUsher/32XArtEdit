using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace _32XArtEdit
{
    class Palette32X
    {
        private UInt16[] data;

        public Palette32X()
        {
            // 32X Palette is always 256 colours, 16bpp.
            // 1 bit for BG/FG flag, 15 bits for colour data
            data = new UInt16[256];

            for (int i = 0; i < 256; i++)
            {
                SetColour(i, Color.Magenta);
            }
        }

        public void SetColour(int entry, Color colour)
        {
            // Forbid changing of first entry
            if (entry!=0)
            {
                data[entry] = (UInt16)((((colour.R / 8) & 0x1F)) | ((colour.G / 8) & 0x1F) << 5 | ((colour.B / 8) & 0x1F) << 10 | 0x8000);
            }
        }

        public Color GetColour(int entry)
        {
            if(entry == 0)
            {
                return Color.Magenta;
            }

            int R = ((data[entry] & 0x001f) * 8);
            int G = (((data[entry] & 0x03e0) >> 5) * 8);
            int B = (((data[entry] & 0x7C00) >> 10) * 8);
  
            Color colour = Color.FromArgb(R, G, B);
            return colour;
        }

        public void Load(String filename)
        {

            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, stream);

            for (int i = 0; i < 256; i++)
            {
                data[i] = reader.ReadUInt16();
            }

            stream.Close();
        }

        public void Save(String filename)
        {
            FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Big, stream);

            for (int i = 0; i < 256; i++)
            {
                writer.Write(data[i]);
            }

            stream.Close();
        }
    }
}


