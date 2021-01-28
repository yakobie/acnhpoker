using System;
using System.Collections.Generic;
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

        public UInt16 itemDurability
        {
            get
            {
                return (UInt16)((itemData >> 16) & 0xFFFF);
            }
            set
            {
                itemData = (itemData & 0xFFFF) + ((UInt32)value << 16);
            }
        }
        public UInt16 itemQuantity
        {
            get
            {
                return (UInt16)(itemData & 0xFFFF);
            }
            set
            {
                itemData = (itemData & 0xFFFF0000) + value;
            }
        }

        public UInt16 flowerQuantity
        {
            get
            {
                return (UInt16)(itemData & 0xFF);
            }
            set
            {
                itemData = (itemData & 0xFFFFFF00) + value;
            }
        }

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

            this.Invoke((MethodInvoker)delegate
            {
                this.Text = "";
            });

            this.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.Image = null;
            this.locked = false;

            UInt32 P2Id = part2Data & 0x0000FFFF;
            UInt32 P3Id = part3Data & 0x0000FFFF;
            UInt32 P4Id = part4Data & 0x0000FFFF;

            if (itemID != 0xFFFE && (itemID == P2Id && P2Id == P3Id && P3Id == P4Id)) // Filled Slot
            {

                if (flag2 != "20" || flag1 != "00")
                {
                    locked = true;
                }
                //this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
                this.Image = displayItemImage(large, false);
            }
            else if (itemID == 0xFFFE && part2 == 0xFFFE && part3 == 0xFFFE && part4 == 0xFFFE) // Empty
            {
                //this.BackColor = Color.LightSalmon;
            }
            else if (itemID != 0xFFFE && flag1 != "00") // wrapped
            {
                this.Image = displayItemImage(large, false);
            }
            else // seperate
            {
                locked = true;

                if (flag1 != "00")
                {
                    locked = true;
                }

                this.Image = displayItemImage(large, true);
            }
        }

        public void setBackColor(bool Layer1 = true)
        {
            if (Layer1)
            {
                //this.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
                this.BackColor = miniMap.GetBackgroundColor(mapX, mapY);
            }
            else
            {
                //this.BackColor = Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
                this.BackColor = miniMap.GetBackgroundColor(mapX, mapY, false);
            }

            if (flag1 != "00") //Wrapped
            {
                if (itemID == 0x16A1) //Inside Bottle
                {
                    this.BackColor = Color.LightSalmon;
                }
                else if (itemID == 0x16A2) // Recipe
                {
                    this.BackColor = Color.LightSalmon;
                }
                else
                {
                    this.BackColor = Color.LightSalmon;
                }
            }
            else if (flag2 == "04" || flag2 == "24") //Bury
            {
                this.BackColor = Color.DarkKhaki;
            }
            else if (ItemAttr.hasDurability(itemID)) //Tools
            {
                this.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
                this.Text = "Dur: " + itemDurability.ToString();
                return;
            }
            else if (ItemAttr.isFlower(itemID)) //Flowers
            {
                this.TextAlign = System.Drawing.ContentAlignment.BottomRight;
                this.ForeColor = System.Drawing.Color.Yellow;
                this.Text = (flowerQuantity + 1).ToString();
                return;
            }
            else if (ItemAttr.hasQuantity(itemID)) // Materials
            {
                this.TextAlign = System.Drawing.ContentAlignment.BottomRight;
                this.Text = (itemQuantity + 1).ToString();
                return;
            }
            else if (ItemAttr.hasGenetics(itemID))
            {
                if (itemData.ToString("X").Contains("83E0") || (itemData.ToString("X").Contains("8642"))) // Flower
                {
                    this.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    this.Text = "✶";
                    return;
                }

                this.TextAlign = System.Drawing.ContentAlignment.TopRight;
                string value = itemData.ToString("X");
                int length = value.Length;
                string firstByte;
                string secondByte;
                if (length < 2)
                {
                    firstByte = "0";
                    secondByte = value;
                }
                else
                {
                    firstByte = value.Substring(length - 2, 1);
                    secondByte = value.Substring(length - 1, 1);
                }
                this.Text = HexToBinary(secondByte) + "-" + HexToBinary(firstByte);
                //System.Diagnostics.Debug.Print(secondByte + " " + firstByte + " " + HexToBinary(secondByte) + " " + HexToBinary(firstByte));
            }
            else if (locked)
            {
                //this.BackColor = Color.Gray;
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
        public string HexToBinary(string hex)
        {
            return hexCharacterToBinary[hex.ToLower()];
        }

        private static readonly Dictionary<string, string> hexCharacterToBinary = new Dictionary<string, string> {
            { "0", "0-0" },
            { "1", "1-0" },
            { "2", "X-0" },//
            { "3", "2-0" },
            { "4", "0-1" },
            { "5", "1-1" },
            { "6", "X-1" },//
            { "7", "2-1" },
            { "8", "0-X" },//
            { "9", "1-X" },//
            { "a", "X-X" },//
            { "b", "2-X" },//
            { "c", "0-2" },
            { "d", "1-2" },
            { "e", "X-2" },//
            { "f", "2-2" }
        };

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
