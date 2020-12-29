using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ACNHPoker
{
    class floorSlot : Button
    {
        public string itemName { get; set; }
        public string flag1 { get; set; }
        public string flag2 { get; set; }
        public UInt16 itemID { get; set; } // flag 1 + flag 2 + itemID
        public UInt32 itemData { get; set; }

        public UInt32 part2 { get; set; } // FDFF0000
        public UInt32 part2Data { get; set; } // 01 + 00 + itemID
        public UInt32 part3 { get; set; } // FDFF0000
        public UInt32 part3Data { get; set; } // 00 + 01 + itemID
        public UInt32 part4 { get; set; } // FDFF0000
        public UInt32 part4Data { get; set; } // 01 + 01 + itemID

        private string image1Path = "";
        private string image2Path = "";
        private string image3Path = "";
        private string image4Path = "";

        public int mapX { get; set; }
        public int mapY { get; set; }

        private const string RecipeOverlayFolder = @"img\";
        private const string RecipeOverlayFile = @"PaperRecipe.png";
        private const string RecipeOverlayPath = RecipeOverlayFolder + RecipeOverlayFile;

        private Image recipe;

        private string containItemPath = "";

        public Boolean locked = false;

        public floorSlot()
        {
            if (File.Exists(RecipeOverlayPath))
                recipe = Image.FromFile(RecipeOverlayPath);

            itemName = "";
            flag1 = "00";
            flag2 = "00";
            itemID = 0xFFFE;
            itemData = 0x00000000;
            mapX = -1;
            mapY = -1;
        }

        public void setup(string Name, UInt16 ID, UInt32 Data, UInt32 P2, UInt32 P2Data, UInt32 P3, UInt32 P3Data, UInt32 P4, UInt32 P4Data, string Path1, string Path2, string Path3, string Path4, string containPath = "", string flagA = "00", string flagB = "00")
        {
            itemName = Name;
            itemID = ID;
            flag1 = flagA;
            flag2 = flagB;
            itemData = Data;

            part2 = P2;
            part2Data = P2Data;
            part3 = P3;
            part3Data = P3Data;
            part4 = P4;
            part4Data = P4Data;

            image1Path = Path1;
            image2Path = Path2;
            image3Path = Path3;
            image4Path = Path4;

            containItemPath = containPath;

            this.refresh(false);
        }

        public void refresh(Boolean large)
        {
            this.ForeColor = System.Drawing.Color.White;
            this.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.Text = "";
            this.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.Image = null;
            this.locked = false;

            UInt32 P2Id = part2Data & 0x0000FFFF;
            UInt32 P3Id = part3Data & 0x0000FFFF;
            UInt32 P4Id = part4Data & 0x0000FFFF;

            if (itemID != 0xFFFE && (itemID == P2Id && P2Id == P3Id && P3Id == P4Id)) // Filled Slot
            {
                if (flag2 != "20")
                {
                    this.BackColor = Color.Gray;
                    locked = true;
                }

                if (flag2 == "04")
                {
                    this.BackColor = Color.DarkKhaki;
                }
                this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
                this.Image = displayItemImage(large, false);
            }
            else if (itemID == 0xFFFE && part2 == 0xFFFE && part3 == 0xFFFE && part4 == 0xFFFE) // Empty
            {
                //this.BackColor = Color.LightSalmon;
            }
            else // seperate
            {
                locked = true;
                this.BackColor = Color.Gray;
                this.Image = displayItemImage(large, true);
            }
        }

        public void reset()
        {
            itemName = "";
            flag1 = "00";
            flag2 = "00";
            itemID = 0xFFFE;
            itemData = 0x00000000;
            //mapX = -1;
            //mapY = -1;

            part2 = 0x0000FFFE;
            part2Data = 0x00000000;
            part3 = 0x0000FFFE;
            part3Data = 0x00000000;
            part4 = 0x0000FFFE;
            part4Data = 0x00000000;

            image1Path = "";
            image2Path = "";
            image3Path = "";
            image4Path = "";

            containItemPath = "";

            this.refresh(false);
        }

        public Image displayItemImage(Boolean large, Boolean separate)
        {
            if (separate)
            {
                Size size;
                Double recipeMultiplier;
                Double wallMultiplier;

                if (large)
                {
                    size = new Size(128, 128);
                    recipeMultiplier = 0.3;
                    wallMultiplier = 0.45;
                }
                else
                {
                    size = new Size(64, 64);
                    recipeMultiplier = 0.35;
                    wallMultiplier = 0.6;
                }

                if (large)
                {
                    return null;
                }
                else
                {
                    Image background = (new Bitmap(75, 75));
                    Image topleft = null;
                    Image topright = null;
                    Image bottomleft = null;
                    Image bottomright = null;

                    if (image1Path != "")
                    {
                        topleft = (new Bitmap(Image.FromFile(image1Path), new Size((this.Width) / 2, (this.Height) / 2)));
                    }
                    else if (itemID != 0xFFFE)
                    {
                        topleft = (new Bitmap(ACNHPoker.Properties.Resources.ACLeaf2.ToBitmap(), new Size((this.Width) / 2, (this.Height) / 2)));
                    }
                    if (image2Path != "")
                    {
                        bottomleft = (new Bitmap(Image.FromFile(image2Path), new Size((this.Width) / 2, (this.Height) / 2)));
                    }
                    else if (part2 != 0x0000FFFE)
                    {
                        bottomleft = (new Bitmap(ACNHPoker.Properties.Resources.ACLeaf2.ToBitmap(), new Size((this.Width) / 2, (this.Height) / 2)));
                    }
                    if (image3Path != "")
                    {
                        topright = (new Bitmap(Image.FromFile(image3Path), new Size((this.Width) / 2, (this.Height) / 2)));
                    }
                    else if (part3 != 0x0000FFFE)
                    {
                        topright = (new Bitmap(ACNHPoker.Properties.Resources.ACLeaf2.ToBitmap(), new Size((this.Width) / 2, (this.Height) / 2)));
                    }
                    if (image4Path != "")
                    {
                        bottomright = (new Bitmap(Image.FromFile(image4Path), new Size((this.Width) / 2, (this.Height) / 2)));
                    }
                    else if (part4 != 0x0000FFFE)
                    {
                        bottomright = (new Bitmap(ACNHPoker.Properties.Resources.ACLeaf2.ToBitmap(), new Size((this.Width) / 2, (this.Height) / 2)));
                    }

                    Image img = PlaceImages(background, topleft, topright, bottomleft, bottomright, 1);
                    return (Image)(new Bitmap(img, size));
                }
            }
            else
            {
                Size size;
                Double recipeMultiplier;
                Double wallMultiplier;

                if (large)
                {
                    size = new Size(128, 128);
                    recipeMultiplier = 0.3;
                    wallMultiplier = 0.45;
                }
                else
                {
                    size = new Size(64, 64);
                    recipeMultiplier = 0.5;
                    wallMultiplier = 0.6;
                }

                    if (image1Path == "" & itemID != 0xFFFE)
                    {
                        return (Image)(new Bitmap(ACNHPoker.Properties.Resources.Leaf, size));
                    }
                    else if (itemID == 0x16A2) // recipe
                    {
                        Image background = Image.FromFile(image1Path);
                        int imageSize = (int)(background.Width * recipeMultiplier);
                        Image icon = (new Bitmap(recipe, new Size(imageSize, imageSize)));

                        Image img = PlaceImageOverImage(background, icon, background.Width - (imageSize - 5), background.Width - (imageSize - 5), 1);
                        return (Image)(new Bitmap(img, size));
                    }
                    else if (itemID == 0x315A || itemID == 0x1618) // Wall-Mount
                    {
                        if (File.Exists(containItemPath))
                        {
                            Image background = Image.FromFile(image1Path);
                            int imageSize = (int)(background.Width * wallMultiplier);
                            Image icon = (new Bitmap(Image.FromFile(containItemPath), new Size(imageSize, imageSize)));

                            Image img = PlaceImageOverImage(background, icon, background.Width - (imageSize - 5), background.Width - (imageSize - 5), 1);
                            return (Image)(new Bitmap(img, size));
                        }
                        else
                        {
                            Image img = Image.FromFile(image1Path);
                            return (Image)(new Bitmap(img, size));
                        }
                    }
                    else if (image1Path != "")
                    {
                        Image img = Image.FromFile(image1Path);
                        return (Image)(new Bitmap(img, size));
                    }
                    else
                    {
                        return null;
                    }
            }
        }

        public Boolean isEmpty()
        {
            if (itemID == 0xFFFE && part2 == 0xFFFE && part3 == 0xFFFE && part4 == 0xFFFE)
            {
                return true;
            }
            return false;
        }

        private static Image PlaceImageOverImage(Image bottom, Image top, int x, int y, float alpha)
        {
            Image output = bottom;
            using (Graphics graphics = Graphics.FromImage(output))
            {
                var cm = new ColorMatrix();
                cm.Matrix33 = alpha;

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                graphics.DrawImage(top, new Rectangle(x, y, top.Width, top.Height), 0, 0, top.Width, top.Height, GraphicsUnit.Pixel, ia);
            }

            return output;
        }

        private static Image PlaceImages(Image bottom, Image topleft, Image topright, Image bottomleft, Image bottomright, float alpha)
        {
            Image output = bottom;
            using (Graphics graphics = Graphics.FromImage(output))
            {
                var cm = new ColorMatrix();
                cm.Matrix33 = alpha;

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                if (topleft != null)
                    graphics.DrawImage(topleft, new Rectangle(0, 0, topleft.Width, topleft.Height), 0, 0, topleft.Width, topleft.Height, GraphicsUnit.Pixel, ia);
                if (topright != null)
                    graphics.DrawImage(topright, new Rectangle(38, 0, topright.Width, topright.Height), 0, 0, topright.Width, topright.Height, GraphicsUnit.Pixel, ia);
                if (bottomleft != null)
                    graphics.DrawImage(bottomleft, new Rectangle(0, 38, bottomleft.Width, bottomleft.Height), 0, 0, bottomleft.Width, bottomleft.Height, GraphicsUnit.Pixel, ia);
                if (bottomright != null)
                    graphics.DrawImage(bottomright, new Rectangle(38, 38, bottomright.Width, bottomright.Height), 0, 0, bottomright.Width, bottomright.Height, GraphicsUnit.Pixel, ia);
            }

            return output;
        }
    }
}
