using System;
using System.Collections.Generic;
using System.Drawing;

namespace ACNHPoker
{
    public class inventorySlot : System.Windows.Forms.Button
    {
        private string itemName;
        private UInt16 itemID = 0xFFFE;
        private UInt32 itemData = 0x0;
        private string imagePath = "";
        //private Boolean init = false;
        private Boolean hide = false;

        private string flag1 = "00";
        private string flag2 = "00";

        private string containItemName = "";
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

        public inventorySlot()
        {

        }

        public string displayItemID()
        {
            return "0x" + String.Format("{0:X4}", itemID);
        }

        public string fillItemID()
        {
            return itemID.ToString("X");
        }

        public string displayItemData()
        {
            return "0x" + itemData.ToString("X");
        }
        public string fillItemData()
        {
            return itemData.ToString("X");
        }

        public string displayItemName()
        {
            return itemName;
        }

        public string getPath()
        {
            return imagePath;
        }

        public Boolean isEmpty()
        {
            if (itemID == 0x0 ^ itemID == 0xFFFE)
            {
                return true;
            }
            return false;
        }

        public Image displayItemImage(Boolean large)
        {
            if (large)
            {
                if (imagePath == "" & itemID != 0xFFFE)
                {
                    return (Image)(new Bitmap(ACNHPoker.Properties.Resources.ACLeaf.ToBitmap(), new Size(128, 128)));
                }
                else if (imagePath != "")
                {
                    Image img = Image.FromFile(imagePath);
                    return (Image)(new Bitmap(img, new Size(128, 128)));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (imagePath == "" & itemID != 0xFFFE)
                {
                    return (Image)(new Bitmap(ACNHPoker.Properties.Resources.ACLeaf.ToBitmap(), new Size(64, 64)));
                }
                else if (imagePath != "")
                {
                    Image img = Image.FromFile(imagePath);
                    return (Image)(new Bitmap(img, new Size(64, 64)));
                }
                else
                {
                    return null;
                }
            }
        }

        public void setFlag1(string flag)
        {
            flag1 = flag;
        }
        public void setFlag2(string flag)
        {
            flag2 = flag;
        }
        public string getFlag1() // Wrapping
        {
            return flag1;
        }
        public string getFlag2() // Direction ?
        {
            return flag2;
        }
        public void setHide(Boolean flag)
        {
            hide = flag;
        }
        public string getContainItemName()
        {
            return containItemName;
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
            /*
            { "0", "0000" },
            { "1", "0001" },
            { "2", "0010" },
            { "3", "0011" },
            { "4", "0100" },
            { "5", "0101" },
            { "6", "0110" },
            { "7", "0111" },
            { "8", "1000" },
            { "9", "1001" },
            { "a", "1010" },
            { "b", "1011" },
            { "c", "1100" },
            { "d", "1101" },
            { "e", "1110" },
            { "f", "1111" }
            */
        };

        public string HexToBinary(string hex)
        {
            return hexCharacterToBinary[hex.ToLower()];
        }

        public void reset()
        {
            itemName = "";
            itemID = 0xFFFE;
            flag1 = "00";
            flag2 = "00";
            itemData = 0x0;
            imagePath = "";
            hide = false;
            containItemName = "";
            this.Image = null;
            this.Text = "";
            //init = false;
        }

        public void setup(string Name, UInt16 ID, UInt32 Data, string Path, string containName = "", string flagA = "00", string flagB = "00")
        {
            itemName = Name;
            itemID = ID;
            flag1 = flagA;
            flag2 = flagB;
            itemData = Data;
            imagePath = Path;
            containItemName = containName;
            this.refresh(false);
            //init = true;
        }

        public void setup(string Name, UInt16 ID, UInt32 Data, string Path, Boolean large, string containName = "", string flagA = "00", string flagB = "00")
        {
            itemName = Name;
            itemID = ID;
            flag1 = flagA;
            flag2 = flagB;
            itemData = Data;
            imagePath = Path;
            containItemName = containName;
            this.refresh(large);
            //init = true;
        }

        public void setup(inventorySlot btn)
        {
            itemName = btn.itemName;
            itemID = btn.itemID;
            flag1 = btn.flag1;
            flag2 = btn.flag2;
            itemData = btn.itemData;
            imagePath = btn.imagePath;
            containItemName = btn.containItemName;
            this.refresh(true);
            //init = true;
        }

        public void refresh(Boolean large)
        {
            this.ForeColor = System.Drawing.Color.White;
            this.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.Text = "";

            if (itemID != 0xFFFE) //Empty
            {
                this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
                this.Image = displayItemImage(large);
                if (hide)
                {
                    return;
                }
                else if (flag1 != "00") //Wrapped
                {
                    if (itemID == 0x16A1) //Inside Bottle
                    {
                        this.Text = "Bottle";
                        this.ForeColor = System.Drawing.Color.LightGreen;
                        this.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    }
                    else
                    {
                        this.Text = "Wrap";
                        this.ForeColor = System.Drawing.Color.LightSalmon;
                    }
                    return;
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
                    if (displayItemData().Contains("83E0") || displayItemData().Contains("8642")) // Flower
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
                else if (itemID == 0x16A2) // Recipe
                {
                    this.Text = "";
                    return;
                }
                else if (itemID == 0x1095) // Villager Delivery
                {
                    this.Text = "Delivery";
                    this.ForeColor = System.Drawing.Color.Red;
                    this.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    return;
                }
                else if (itemID == 0x0A13) // Fossil
                {
                    this.Text = "Fossil";
                    this.ForeColor = System.Drawing.Color.Blue;
                    this.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    return;
                }
                else if (itemID == 0x114A) // Money Tree
                {
                    this.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
                    if (containItemName.Length > 10)
                    {
                        this.Text = containItemName.Substring(0, 8) + "...";
                    }
                    else
                    {
                        this.Text = containItemName;
                    }
                    this.ForeColor = System.Drawing.Color.White;
                    this.TextAlign = System.Drawing.ContentAlignment.BottomRight;
                }
                else if (itemData > 0)
                {
                    this.ForeColor = System.Drawing.Color.Gold;
                    this.TextAlign = System.Drawing.ContentAlignment.BottomRight;
                    this.Text = "Var: " + itemData.ToString("X");
                }
            }
            else
            {
                this.Image = null;
                this.Text = "";
            }
        }

    }
}
