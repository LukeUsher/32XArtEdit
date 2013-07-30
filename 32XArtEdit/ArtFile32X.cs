using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace _32XArtEdit
{
    /* This class handles reading and writing to/from a Sonic 1 32X Remix format sprite 
     * These art files are pretty much a fuckton of 8BPP images concatinated together
     * with a custom header for the 32X to read which contains a frames Width, Height, 
     * and if it should be dynamically rotated by the sprite engine to it's parent
     * object's angle.
     */

    // A single frame of a 32X art file
    class ArtFrame32X
    {
        public Byte width;         // Frame Width
        public Byte height;        // Frame Height
        public Byte rotatable;     // If 1, frame is rotated to objects angle
        public Byte format;        // Image format 0 = Uncompressed
        public Byte[] data;        // Raw data of the image 

        public ArtFrame32X()
        { 
            // Set default settings
            width = 32;
            height = 32;
            rotatable = 0;
            data = new Byte[width * height];
        }
    }

    class ArtFile32X
    {
        // Number of frames within this art file
        private Int32 frameCount; 
                                
        // Pointers to frame data (Relative from start of header)
        private UInt32[] framePointers;

        // Array of actual frame data
        private List<ArtFrame32X> frames;

        public ArtFile32X()
        {
            frameCount = 1;
            frames = new List<ArtFrame32X>();
            frames.Add(new ArtFrame32X());
        }

         // Function to load art file
        public bool Load(String filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, stream);

            // Readame count
            frameCount = reader.ReadInt32();

            // Allocate pointer array
            framePointers = new UInt32[frameCount];

            // Read frame pointers
            for (int i = 0; i < frameCount; i++)
            {
                // Pointer = (Frame Number) * (Size Of Frame + Header Size) + sizeof(Uint16)
                framePointers[i] = reader.ReadUInt32();
            }

            // Read frame data
            for (int i = 0; i < frameCount; i++)
            {
                ArtFrame32X newFrame = new ArtFrame32X();

                newFrame.width = reader.ReadByte();
                newFrame.height = reader.ReadByte();
                newFrame.rotatable = reader.ReadByte();
                newFrame.format = reader.ReadByte();


                newFrame.data = reader.ReadBytes(newFrame.width * newFrame.height);

                if (i == 0)
                {
                    frames[i] = newFrame;
                }
                else
                {
                    frames.Add(newFrame);
                }
            }

            stream.Close();

            return false;
        }

        // Function to save art file
        public bool Save(String filename)
        {
            FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Big, stream);

            // Write frame count
            writer.Write(frameCount);
            writer.Flush();

            long pointerTableOffset = writer.BaseStream.Position;

            // Allocate pointer array
            framePointers = new UInt32[frameCount];

            // Write dummy pointer table
            for (int i = 0; i < frameCount; i++)
            {
                // Write pointer
                framePointers[i] = 0;
                writer.Write(framePointers[i]);
            }

            // Write frame data
            for (int i = 0; i < frameCount; i++)
            {
                // Add this frame to the pointer table in RAM
                writer.Flush();
                framePointers[i] = (UInt32)writer.BaseStream.Position;

                // Write actual frame
                writer.Write(frames[i].width);
                writer.Write(frames[i].height);
                writer.Write(frames[i].rotatable);
                writer.Write(frames[i].format);
                writer.Write(frames[i].data);
            }

            // Seek to start of pointer table
            writer.Seek(4, SeekOrigin.Begin);

            // Write real pointer table
            for (int i = 0; i < frameCount; i++)
            {
                writer.Write(framePointers[i]);
            }

            stream.Close();

            return false;
        }

        public void AddFrame()
        {
            if (frameCount < UInt16.MaxValue)
            {
                frameCount++;
                frames.Add(new ArtFrame32X());
            }
            else
            {
                MessageBox.Show("Could not add a new frame, there is a hard limit of " + UInt16.MaxValue.ToString() + " frames");
            }
   
        }

        public void RemoveFrame(int frame)
        {
            if (frameCount > 1)
            {
                frameCount--;
                frames.RemoveAt(frame);
            }
            else
            {
                MessageBox.Show("There must be at least one frame.");
            }
        }


        public ArtFrame32X GetFrame(int frame)
        {
            // If frame exists, return it's data
            if(frame <= frameCount-1)
            {
                return frames[frame];
            }
            return frames[frameCount];
        }

        public void SetFrame(int frame, ArtFrame32X data)
        {
            if (frame <= frameCount-1)
            {
                 frames[frame] = data;
            }
        }

        public int GetFrameCount()
        {
            return frameCount-1;
        }
    }

}
