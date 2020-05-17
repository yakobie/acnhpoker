using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acnhpoker
{
    class inventorySlot : System.Windows.Forms.Button
    {
        private string itemName;
        private UInt16 itemID;
        private UInt32 itemData;
        private string imagePath = "";
        private Boolean init = false;

        private string flag1;
        private string flag2;

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
                return (UInt16)(itemData & 0xF);
            }
            set
            {
                itemData = (itemData & 0xFFFFFFF0) + value;
            }
        }

        public string displayItemID()
        {
            return "0x" + itemID.ToString("X");
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

        public Image displayItemImage()
        {
            if (imagePath == "" & itemID != 0xFFFE)
            {
                return (Image)(new Bitmap(Properties.Resources.ACLeaf.ToBitmap(), new Size(64, 64)));
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

        public void setFlag1(string flag)
        {
            flag1 = flag;
        }
        public void setFlag2(string flag)
        {
            flag2 = flag;
        }
        public string getFlag1()
        {
            return flag1;
        }
        public string getFlag2()
        {
            return flag2;
        }

        public void reset()
        {
            itemName = "";
            itemID = 0x0;
            flag1 = "";
            flag2 = "";
            itemData = 0x0;
            imagePath = "";
            this.Image = null;
            this.Text = "";
            init = false;
        }

        public void setup(string Name, UInt16 ID, UInt32 Data, string Path, string flagA = "00", string flagB = "00")
        {
            itemName = Name;
            itemID = ID;
            flag1 = flagA;
            flag2 = flagB;
            itemData = Data;
            imagePath = Path;
            this.refresh();
            init = true;
        }

        public void setup(inventorySlot btn)
        {
            itemName = btn.itemName;
            itemID = btn.itemID;
            flag1 = btn.flag1;
            flag2 = btn.flag2;
            itemData = btn.itemData;
            imagePath = btn.imagePath;
            this.refresh();
            init = true;
        }

        public void refresh()
        {
            this.ForeColor = System.Drawing.Color.White;
            this.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.Text = "";

            if (itemID != 0xFFFE) //Empty
            {
                this.Image = displayItemImage();

                if (flag1 != "00") //Wrapped
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
                else if (itemID == 0x16A2) // Recipe
                {
                    this.Text = "";
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
                else if (itemData > 0)
                {
                    if (displayItemData().Contains("83E0") || displayItemData().Contains("8642")) // Flower
                    {
                        this.TextAlign = System.Drawing.ContentAlignment.TopRight;
                        this.Text = "✶";
                    }
                    else
                    {
                        this.Text = "Var: " + itemData.ToString();
                    }
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
