using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows;

namespace ACNHPoker
{
    class miniMap
    {
        private byte[][] ItemMapData;
        private int[][] tilesType;

        private byte[] AcreMapByte;

        private const int numOfColumn = 0x70;
        private const int numOfRow = 0x60;
        private const int columnSize = 0xC00;
        private const int numOfTiles = 0x2A00;

        private int mapSize = 2;



        private byte[] AcreData = ACNHPoker.Properties.Resources.acre;
        private static Color[][] floorBackgroundColor;

        public const int MapTileCount16x16 = 16 * 16 * 7 * 6;
        public const int TerrainTileSize = 0xE;

        public const int AllTerrainSize = MapTileCount16x16 * TerrainTileSize;

        private const int BuildingSize = 0x14;
        private const int AllBuildingSize = 46 * BuildingSize;

        public const int AcreWidth = 7 + (2 * 1);
        private const int AcreHeight = 6 + (2 * 1);
        private const int AcreMax = AcreWidth * AcreHeight;
        private const int AllAcreSize = AcreMax * 2;
        private const int AcrePlusAdditionalParams = AllAcreSize + 2 + 2 + 4 + 4;
        public miniMap(byte[] ItemMapByte, byte[] acreMapByte, int size = 2)
        {
            AcreMapByte = acreMapByte;

            ItemMapData = new byte[numOfColumn][];
            mapSize = size;
            for (int i = 0; i < numOfColumn; i++)
            {
                ItemMapData[i] = new byte[columnSize];
                Buffer.BlockCopy(ItemMapByte, i * columnSize, ItemMapData[i], 0x0, columnSize);
            }
            transformItemMap();
        }

        public Bitmap drawBackground()
        {
            try
            {
                byte[] AllAcre = new byte[AcreMax];
                byte plazeX;
                byte plazeY;

                for (int i = 0; i < AcreMax; i++)
                {
                    AllAcre[i] = AcreMapByte[i * 2];
                }

                plazeX = AcreMapByte[AcreMax * 2 + 4];
                plazeY = AcreMapByte[AcreMax * 2 + 8];

                byte[] AcreWOOutside = new byte[7 * 6];

                for (int i = 1; i <= 6; i++)
                {
                    Buffer.BlockCopy(AllAcre, i * 9 + 1, AcreWOOutside, (i - 1) * 7, 7);
                }

                buildBackgroundColor(AcreWOOutside);

                Bitmap[] AcreImage = new Bitmap[7 * 6];

                for (int i = 0; i < AcreImage.Length; i++)
                {
                    AcreImage[i] = DrawAcre(GetAcreData(AcreWOOutside[i]));
                    //AcreImage[i].Save(i + ".bmp");
                }

                return toFullMap(AcreImage);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Bitmap myBitmap = new Bitmap(16 * 7 * mapSize, 16 * 6 * mapSize);
                Graphics g = Graphics.FromImage(myBitmap);
                g.Clear(Color.White);
                return myBitmap;
            }
        }

        private byte[] GetAcreData(byte Acre)
        {
            byte[] data = new byte[0x100];
            Buffer.BlockCopy(AcreData, Acre * 0x100, data, 0, 0x100);
            return data;
        }

        public Bitmap DrawAcre(byte[] OneAcre)
        {
            Bitmap myBitmap;
            myBitmap = new Bitmap(16 * mapSize, 16 * mapSize);

            using (Graphics gr = Graphics.FromImage(myBitmap))
            {
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        PutPixel(gr, j * mapSize, i * mapSize, Pixel[OneAcre[i * 0x10 + j]]);
                    }
                }
            }

            return myBitmap;
        }

        private Bitmap toFullMap(Bitmap[] AcreImage)
        {
            Bitmap myBitmap;
            myBitmap = new Bitmap(16 * 7 * mapSize, 16 * 6 * mapSize);

            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                var cm = new ColorMatrix();
                cm.Matrix33 = 1;

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                int ImageNum = 0;

                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        graphics.DrawImage(AcreImage[ImageNum], new Rectangle(j * 16 * mapSize, i * 16 * mapSize, AcreImage[ImageNum].Width, AcreImage[ImageNum].Height), 0, 0, AcreImage[ImageNum].Width, AcreImage[ImageNum].Height, GraphicsUnit.Pixel, ia);
                        ImageNum++;
                    }
                }
            }

            return myBitmap;
        }

        public Bitmap combineMap(Bitmap bottom, Bitmap top)
        {
            Bitmap myBitmap = bottom;

            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                var cm = new ColorMatrix();
                cm.Matrix33 = 1;

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                graphics.DrawImage(top, new Rectangle(0, 0, top.Width, top.Height), 0, 0, top.Width, top.Height, GraphicsUnit.Pixel, ia);
            }
            

            return myBitmap;
        }

        public Image refreshItemMap(byte[] itemData)
        {
            ItemMapData = null;
            tilesType = null;

            ItemMapData = new byte[numOfColumn][];

            for (int i = 0; i < numOfColumn; i++)
            {
                ItemMapData[i] = new byte[columnSize];
                Buffer.BlockCopy(itemData, i * columnSize, ItemMapData[i], 0x0, columnSize);
            }
            transformItemMap();

            Image itemMap = drawItemMap();
            Image myBitmap = drawBackground();

            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                var cm = new ColorMatrix();
                cm.Matrix33 = 1;

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                graphics.DrawImage(itemMap, new Rectangle(0, 0, itemMap.Width, itemMap.Height), 0, 0, itemMap.Width, itemMap.Height, GraphicsUnit.Pixel, ia);
            }

            return myBitmap;
        }

        public Bitmap drawSelectSquare(int x, int y)
        {
            Bitmap square = new Bitmap(7 * mapSize + 2, 7 * mapSize + 2);
            Pen p = new Pen(Color.Red, 2 * mapSize);
            using (Graphics g = Graphics.FromImage(square))
            {
                g.Clear(Color.Transparent);
                g.DrawRectangle(p, new Rectangle(0, 0, square.Width, square.Height));
            }


            Bitmap myBitmap;
            myBitmap = new Bitmap(16 * 7 * mapSize, 16 * 6 * mapSize);

            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                graphics.Clear(Color.Transparent);

                var cm = new ColorMatrix();
                cm.Matrix33 = 1;

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                graphics.DrawImage(square, new Rectangle(x * mapSize - square.Width / 2 + mapSize / 2, y * mapSize - square.Height / 2 + mapSize / 2, square.Width, square.Height), 0, 0, square.Width, square.Height, GraphicsUnit.Pixel, ia);
            }
            return myBitmap;
        }

        public Bitmap drawMarker(int x, int y)
        {
            Bitmap marker = new Bitmap(ACNHPoker.Properties.Resources.marker);

            Bitmap myBitmap;
            myBitmap = new Bitmap(16 * 7 * mapSize, 16 * 6 * mapSize);

            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                graphics.Clear(Color.Transparent);

                var cm = new ColorMatrix();
                cm.Matrix33 = 1;

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                graphics.DrawImage(marker, new Rectangle(x * mapSize - marker.Width / 2 + mapSize / 2, y * mapSize - marker.Height + mapSize / 2, marker.Width, marker.Height), 0, 0, marker.Width, marker.Height, GraphicsUnit.Pixel, ia);
            }
            return myBitmap;
        }

        public Bitmap drawPreview(int row, int column, int x, int y, bool right)
        {
            int[][] previewtilesType = null;

            if (right)
            {
                previewtilesType = new int[numOfColumn][];

                for (int i = 0; i < numOfColumn; i++)
                {
                    previewtilesType[i] = new int[numOfRow];

                    for (int j = 0; j < numOfRow; j++)
                    {
                        if (i == x && j == y)
                            previewtilesType[i][j] = 2;
                        else if (i >= x && i < x + column && j >= y && j < y + row)
                            previewtilesType[i][j] = 1;
                        else
                            previewtilesType[i][j] = 0;
                    }
                }
            }
            else
            {
                previewtilesType = new int[numOfColumn][];

                for (int i = numOfColumn - 1; i >= 0; i--)
                {
                    previewtilesType[i] = new int[numOfRow];

                    for (int j = 0; j < numOfRow; j++)
                    {
                        if (i == x && j == y)
                            previewtilesType[i][j] = 2;
                        else if (i <= x && i > x - column && j >= y && j < y + row)
                            previewtilesType[i][j] = 1;
                        else
                            previewtilesType[i][j] = 0;
                    }
                }
            }



            Bitmap myBitmap;

            myBitmap = new Bitmap(numOfColumn * mapSize, numOfRow * mapSize);

            using (Graphics gr = Graphics.FromImage(myBitmap))
            {
                gr.SmoothingMode = SmoothingMode.None;

                for (int i = 0; i < numOfColumn; i++)
                {
                    for (int j = 0; j < numOfRow; j++)
                    {
                        if (previewtilesType[i][j] == 1)
                            PutPixel(gr, i * mapSize, j * mapSize, Color.FromArgb(200, Color.Red));
                        else if (previewtilesType[i][j] == 2)
                            PutPixel(gr, i * mapSize, j * mapSize, Color.LightSkyBlue);

                    }
                }
            }

            return myBitmap;
        }

        private void transformItemMap()
        {
            tilesType = new int[numOfColumn][];

            for (int i = 0; i < numOfColumn; i++)
            {
                tilesType[i] = new int[numOfRow];

                for (int j = 0; j < numOfRow; j++)
                {
                    byte[] tempPart1 = new byte[0x8];
                    byte[] tempPart2 = new byte[0x8];
                    byte[] tempPart3 = new byte[0x8];
                    byte[] tempPart4 = new byte[0x8];
                    byte[] IDByte = new byte[0x2];

                    Buffer.BlockCopy(ItemMapData[i], j * 0x10, tempPart1, 0x0, 0x8);
                    Buffer.BlockCopy(ItemMapData[i], j * 0x10 + 0x8, tempPart2, 0x0, 0x8);
                    Buffer.BlockCopy(ItemMapData[i], j * 0x10 + 0x600, tempPart3, 0x0, 0x8);
                    Buffer.BlockCopy(ItemMapData[i], j * 0x10 + 0x600 + 0x8, tempPart4, 0x0, 0x8);
                    Buffer.BlockCopy(ItemMapData[i], j * 0x10, IDByte, 0x0, 0x2);

                    string strPart1 = Utilities.ByteToHexString(tempPart1);
                    string strPart2 = Utilities.ByteToHexString(tempPart2);
                    string strPart3 = Utilities.ByteToHexString(tempPart3);
                    string strPart4 = Utilities.ByteToHexString(tempPart4);
                    ushort itemID = Convert.ToUInt16(Utilities.flip(Utilities.ByteToHexString(IDByte)), 16);

                    if (strPart1.Equals("FEFF000000000000") && strPart2.Equals("FEFF000000000000") && strPart3.Equals("FEFF000000000000") && strPart4.Equals("FEFF000000000000"))
                    {
                        tilesType[i][j] = 0; // Empty
                    }
                    else if (ItemAttr.isTree(itemID))
                    {
                        tilesType[i][j] = 1; // Tree
                    }
                    else if (ItemAttr.hasGenetics(itemID))
                    {
                        tilesType[i][j] = 2; // Flower
                    }
                    else if (ItemAttr.isShell(itemID))
                    {
                        tilesType[i][j] = 3; // Shell
                    }
                    else if (ItemAttr.isWeed(itemID))
                    {
                        tilesType[i][j] = 4; // Weed
                    }
                    else if (ItemAttr.isFence(itemID))
                    {
                        tilesType[i][j] = 5; // Fence
                    }
                    else if (ItemAttr.isStone(itemID))
                    {
                        tilesType[i][j] = 6; // Stone
                    }
                    else if (ItemAttr.hasQuantity(itemID))
                    {
                        tilesType[i][j] = 9; // Material
                    }
                    else if (itemID == 0x16A2)
                    {
                        tilesType[i][j] = 10; // Recipe
                    }
                    else
                    {
                        tilesType[i][j] = 69;
                    }
                }
            }
        }

        public Bitmap drawItemMap()
        {
            try
            {
                Bitmap myBitmap;

                myBitmap = new Bitmap(numOfColumn * mapSize, numOfRow * mapSize);

                using (Graphics gr = Graphics.FromImage(myBitmap))
                {
                    gr.SmoothingMode = SmoothingMode.None;

                    for (int i = 0; i < numOfColumn; i++)
                    {
                        for (int j = 0; j < numOfRow; j++)
                        {
                            if (tilesType[i][j] == 0)
                            {
                                //PutPixel(gr, i * mapSize, j* mapSize, Color.White);
                            }
                            else if (tilesType[i][j] == 1)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.GreenYellow);
                            }
                            else if (tilesType[i][j] == 2)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.Pink);
                            }
                            else if (tilesType[i][j] == 3)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.Blue);
                            }
                            else if (tilesType[i][j] == 4)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.MediumSeaGreen);
                            }
                            else if (tilesType[i][j] == 5)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.Purple);
                            }
                            else if (tilesType[i][j] == 6)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.Black);
                            }
                            else if (tilesType[i][j] == 9)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.Yellow);
                            }
                            else if (tilesType[i][j] == 10)
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.Orange);
                            }
                            else
                            {
                                PutPixel(gr, i * mapSize, j * mapSize, Color.Gray);
                            }
                        }
                    }
                }

                return myBitmap;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Bitmap myBitmap = new Bitmap(16 * 7 * mapSize, 16 * 6 * mapSize);
                Graphics g = Graphics.FromImage(myBitmap);
                g.Clear(Color.Transparent);
                return myBitmap;
            }
        }

        private void buildBackgroundColor(byte[] AcreWOOutside)
        {
            floorBackgroundColor = new Color[numOfRow][];

            for (int i = 0; i < numOfRow; i++)
            {
                floorBackgroundColor[i] = new Color[numOfColumn];
                for (int j = 0; j < numOfColumn; j++)
                {
                    floorBackgroundColor[i][j] = Color.Tomato;
                }
            }

            for (int i = 0; i < 6; i++) // y
            {
                for (int j = 0; j < 7; j++) // x
                {
                    byte[] Acre = GetAcreData(AcreWOOutside[i * 7 + j]);

                    for (int m = 0; m < 16; m++) // y
                    {
                        for (int n = 0; n < 16; n++) // x
                        {
                            if (floorBackgroundColor[i * 16 + m][j * 16 + n] != Color.Tomato)
                                Debug.Print(i + " " + j + " " + m + " " + n);
                            floorBackgroundColor[i * 16 + m][j * 16 + n] = Pixel[Acre[m * 16 + n]];
                        }
                    }
                }
            }
        }

        public static Color GetBackgroundColor(int x, int y, bool Layer1 = true)
        {
            if (floorBackgroundColor == null)
                return Color.White;
            else if (Layer1)
                return floorBackgroundColor[y][x];
            else
            {
                Color newColor = Color.FromArgb(150, floorBackgroundColor[y][x]);
                return newColor;
            }
        }

        private void PutPixel(Graphics g, int x, int y, Color c)
        {
            Bitmap Bmp = new Bitmap(mapSize, mapSize);

            using (Graphics gfx = Graphics.FromImage(Bmp))
            using (SolidBrush brush = new SolidBrush(c))
            {
                gfx.SmoothingMode = SmoothingMode.None;
                gfx.FillRectangle(brush, 0, 0, mapSize, mapSize);
            }

            g.DrawImageUnscaled(Bmp, x, y);
        }

        private Dictionary<byte, Color> Pixel = new Dictionary<byte, Color>
        {
                {0x00, Color.FromArgb(70, 116, 71)}, // Grass

                {0x04, Color.FromArgb(228, 216, 156)}, // Sand
                {0x05, Color.FromArgb(128, 200, 175)}, // Sea
                {0x06, Color.FromArgb(187, 121, 109)}, // Studio Bridge
                {0x07, Color.FromArgb(70, 116, 71)}, // Grass

                {0x0C, Color.FromArgb(21, 147, 229)}, // River Mouth

                {0x0F, Color.FromArgb(21, 147, 229)}, // River Mouth - River Edge

                {0x16, Color.FromArgb(187, 121, 109)}, // Studio Bridge
                {0x1D, Color.FromArgb(255, 244, 193)}, // Beach - Sea Edge

                {0x29, Color.FromArgb(109, 113, 124)}, // Walkable Rock
                {0x2A, Color.FromArgb(78, 83, 96)}, // High Rock
                {0x2B, Color.FromArgb(169, 255, 255)}, // Rock pool
                {0x2C, Color.FromArgb(144, 115, 104)}, // Beach - Grass Edge
                {0x2D, Color.FromArgb(187, 121, 109)}, // Pier
                {0x2E, Color.FromArgb(169, 255, 255)}, // Sea - Beach Edge

        };
    }
}
