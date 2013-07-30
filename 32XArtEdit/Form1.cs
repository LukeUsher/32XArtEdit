using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace _32XArtEdit
{
    public partial class Form1 : Form
    {
        private Point selectedColour;
        private Palette32X palette32X;
        private ArtFile32X art32X;
        private ArtFrame32X currentFrameData;
        private int currentFrame;

        private bool mouseHeld;

        public Form1()
        {
            InitializeComponent();
            palette32X = new Palette32X();
            art32X = new ArtFile32X();
            currentFrameData = new ArtFrame32X();
            currentFrame = 0;
            mouseHeld = false;
            UpdateUI();
        }

        private void panelPalette_MouseDown(object sender, MouseEventArgs e)
        {
            selectedColour = new Point(e.X / 16, e.Y / 16);
            panelPalette.Invalidate();
        }

        private void panelPalette_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            for (int y = 0; y <= 31; y++)
            {
                for (int x = 0; x <= 7; x++)
                {
                    // Draw palette data
                    e.Graphics.FillRectangle(new SolidBrush(palette32X.GetColour(y * 8 + x)), x * 16, y * 16, 16, 16);
                }
            }
            // Highlight selected palette entry
            e.Graphics.DrawRectangle(new Pen(Color.Yellow, 2), selectedColour.X * 16, selectedColour.Y * 16, 16, 16);
        }

        private void panelPalette_DoubleClick(object sender, EventArgs e)
        {
            colorDialog1.FullOpen = true;
            colorDialog1.Color = palette32X.GetColour(selectedColour.Y * 8 + selectedColour.X);
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                palette32X.SetColour(selectedColour.Y * 8 + selectedColour.X, colorDialog1.Color);
                panelPalette.Invalidate();
            }
        }

        private void loadPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "32X Palette Files | *.p32";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                palette32X.Load(openFileDialog1.FileName);
                panelPalette.Invalidate();
            }
        }

        private void savePaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "32X Palette Files | *.p32";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                palette32X.Save(saveFileDialog1.FileName);
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            art32X.AddFrame();
            // Save current frame
            art32X.SetFrame(currentFrame, currentFrameData);

            currentFrame++;

            // Load new frame
            currentFrameData = art32X.GetFrame(currentFrame);

            UpdateUI();

            panelFrame.Invalidate();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            art32X.RemoveFrame(currentFrame);

            if (currentFrame != 0)
            {
                currentFrame--;
            }

            // Load new frame
            currentFrameData = art32X.GetFrame(currentFrame);

            UpdateUI();

            panelFrame.Invalidate();
        }

       
        private void lblFrame_TextChanged(object sender, EventArgs e)
        {
           // UpdateUI();
        }

        private void UpdateUI()
        {
            if (currentFrame < art32X.GetFrameCount())
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
            }
            if (currentFrame == 0)
            {
                btnPrevious.Enabled = false;
            }
            else
            {
                btnPrevious.Enabled = true;
            }
            panelFrame.Width = 16 * currentFrameData.width;
            panelFrame.Height = 16 * currentFrameData.height;

            txtWidth.Text = currentFrameData.width.ToString();
            txtHeight.Text = currentFrameData.height.ToString();
            if (currentFrameData.rotatable == 1)
            {
                chkRotate.Checked = true;
            }
            else
            {
                chkRotate.Checked = false;
            }

            lblFrame.Text = (currentFrame + 1).ToString("X2") + "/" + (art32X.GetFrameCount() + 1).ToString("X2");
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // Save current frame
            art32X.SetFrame(currentFrame, currentFrameData);

            currentFrame++;

            // Load new frame
            currentFrameData = art32X.GetFrame(currentFrame);

            UpdateUI();

            panelFrame.Invalidate();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // Save current frame
            art32X.SetFrame(currentFrame, currentFrameData);

            currentFrame--;

            // Load new frame
            currentFrameData = art32X.GetFrame(currentFrame);

            UpdateUI();

            panelFrame.Invalidate();
        }

        private void panelFrame_Paint(object sender, PaintEventArgs e)
        {
            for (int y = 0; y <= currentFrameData.height-1; y++)
            {
                for (int x = 0; x <= currentFrameData.width - 1; x++)
                {
                    // Read palette offset in ArtFrame
                    int pixel = currentFrameData.data[y * currentFrameData.width + x];

                    // Draw art data
                    e.Graphics.FillRectangle(new SolidBrush(palette32X.GetColour(pixel)), x * 16, y * 16, 16, 16);
                }
            }
        }

        private void panelFrame_MouseDown(object sender, MouseEventArgs e)
        {
            Point selectedPixel = new Point(e.X / 16, e.Y / 16);
            mouseHeld = true;

            if (selectedPixel.X < currentFrameData.width && selectedPixel.Y < currentFrameData.height)
            {
                if (e.Button == MouseButtons.Left)
                {
                    currentFrameData.data[(selectedPixel.Y * currentFrameData.width) + selectedPixel.X] = (byte)(selectedColour.Y * 8 + selectedColour.X);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    currentFrameData.data[(selectedPixel.Y * currentFrameData.width) + selectedPixel.X] = 0;
                }
            }
            panelFrame.Invalidate();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFrame = 0;
            int count = art32X.GetFrameCount();
            
            for (int i = 0; i < count; i++)
            {
                art32X.RemoveFrame(0);
            }

            UpdateUI();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "32X Art Files | *.a32";

            // Save current frame
            art32X.SetFrame(currentFrame, currentFrameData);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                art32X.Save(saveFileDialog1.FileName);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "32X Art Files | *.a32";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                art32X.Load(openFileDialog1.FileName);
            }

            currentFrameData = art32X.GetFrame(currentFrame);
            UpdateUI();
            panelFrame.Invalidate();
        }

        private void panelFrame_MouseMove(object sender, MouseEventArgs e)
        {
            Point selectedPixel = new Point(e.X / 16, e.Y / 16);

            if (selectedPixel.X < 0 || selectedPixel.Y < 0)
            {
                return;
            }

            if (selectedPixel.X < currentFrameData.width && selectedPixel.Y < currentFrameData.height)
            {
                if (mouseHeld)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        currentFrameData.data[(selectedPixel.Y * currentFrameData.width) + selectedPixel.X] = (byte)(selectedColour.Y * 8 + selectedColour.X);
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        currentFrameData.data[(selectedPixel.Y * currentFrameData.width) + selectedPixel.X] = 0;
                    }
                }

                lblXPos.Text = selectedPixel.X.ToString();
                lblYPos.Text = selectedPixel.Y.ToString();

                panelFrame.Invalidate();
            }
        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {
            if (txtWidth.TextLength > 0)
            {
                int width = Int32.Parse(txtWidth.Text);
                if (width > 255)
                {
                    width = 255;
                    txtWidth.Text = width.ToString();
                }

                currentFrameData.width = Byte.Parse(txtWidth.Text);

                // Create new array
                Byte[] data = new Byte[(int)currentFrameData.width * (int)currentFrameData.height];

                for (int i = 0; i < (int)currentFrameData.width * (int)currentFrameData.height; i++)
                {
                    if (i < currentFrameData.data.Length)
                    {
                        data[i] = currentFrameData.data[i];
                    }
                    else
                    {
                        break;
                    }
                }

                // Overwrite old array
                currentFrameData.data = data;

                panelFrame.Invalidate();
                UpdateUI();
            }
        }

        private void txtHeight_TextChanged(object sender, EventArgs e)
        {
            if (txtWidth.TextLength > 0)
            {
                int height = Int32.Parse(txtHeight.Text);
                if (height > 255)
                {
                    height = 255;
                    txtHeight.Text = height.ToString();
                }
                currentFrameData.height = Byte.Parse(txtHeight.Text);

                // Create new array
                Byte[] data = new Byte[(int)currentFrameData.width * (int)currentFrameData.height];

                for (int i = 0; i < (int)currentFrameData.width * (int)currentFrameData.height; i++)
                {
                    if (i < currentFrameData.data.Length)
                    {
                        data[i] = currentFrameData.data[i];
                    }
                    else
                    {
                        break;
                    }
                }


                // Overwrite old array
                currentFrameData.data = data;

                panelFrame.Invalidate();
                UpdateUI();
            }
        }

        private void chkRotate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRotate.Checked)
            {
                currentFrameData.rotatable = 1;
            }
            else
            {
                currentFrameData.rotatable = 0;
            }
            UpdateUI();
        }

        private void panelFrame_MouseUp(object sender, MouseEventArgs e)
        {
            mouseHeld = false;
        }

        private void importBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Bitmap Images|*.bmp";
            openFileDialog1.FileName = (currentFrame+1).ToString("X2");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Open bitmap
                Bitmap bitmap = new Bitmap(openFileDialog1.FileName);

                // Allocate data for the frame
                currentFrameData.width = (Byte)bitmap.Width;
                currentFrameData.height = (Byte)bitmap.Height;
                currentFrameData.data = new Byte[currentFrameData.width * currentFrameData.height];
   

                // Hashtable of mapped colours
                Hashtable colourTable = new Hashtable();

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        // Get pixel colour from bitmap
                        Color colour = bitmap.GetPixel(x, y);

                        // Keep transparency as entry #0
                        if (colour.R == 255 && colour.B == 255 && colour.G == 0)
                        {
                            currentFrameData.data[y * bitmap.Width + x] = 0;
                        }
                        else
                        {
                            // Search palette for the colour
                            int found = 0;
                            for (int i = 1; i < 256; i++)
                            {
                                if (palette32X.GetColour(i) == colour)
                                {
                                    found = i;
                                    // Force array to exit
                                    i = 256;        
                                }
                            }

                            // If colour was not found
                            if (found == 0)
                            {
                                // Add the colour to palette
                                // Search the palette for the next empty colour
                                int next = 0;
                                for (int i = 1; i < 256; i++)
                                {
                                    if (palette32X.GetColour(i) == Color.Magenta)
                                    {
                                        next = i;
                                        i = 256;
                                    }
                                }
                                palette32X.SetColour(next, colour);

                                // If hashtable doesn't contain colour, add it
                                if (!colourTable.ContainsKey(colour))
                                {
                                    colourTable.Add(colour, (Byte)next);
                                }     
                            }
                            // If the colour WAS found
                            else
                            {
                                // If hashtable doesn't contain colour, add it
                                if (!colourTable.ContainsKey(colour))
                                {
                                    colourTable.Add(colour, (Byte)found);
                                }                                
                            }

                            // Set data
                            currentFrameData.data[y * bitmap.Width + x] = (Byte)colourTable[colour];
                        }
                    }
                }

                // Store frame
                art32X.SetFrame(currentFrame, currentFrameData);

                panelFrame.Invalidate();
                UpdateUI();
            }
        }

        private void exportBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Bitmap images | *.bmp";
            saveFileDialog1.FileName = (currentFrame+1).ToString("X2");
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                int width = currentFrameData.width;
                int height = currentFrameData.height;

                Bitmap bitmap = new Bitmap(width, height);

                for(int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int pixel = currentFrameData.data[y * currentFrameData.width + x];

                        bitmap.SetPixel(x, y, palette32X.GetColour(pixel));
                    }
                }

                bitmap.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }
        }
    }    
}
