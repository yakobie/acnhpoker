using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ACNHPoker
{
    class Utilities
    {
        public static UInt32 masterAddress = 0xABC25840; //

        public static UInt32 ItemSlotBase = masterAddress; //
        public static UInt32 ItemSlot21Base = masterAddress - 0xB8; //



        public static readonly UInt32 MasterRecyclingBase = masterAddress - 0xB5BE38; //
        public static readonly UInt32 MasterRecycling21Base = MasterRecyclingBase + 0xA0; //

        public static readonly UInt32 TurnipPurchasePriceAddr = masterAddress - 0x125A568; //
        public static readonly UInt32 TurnipSellPriceAddr = TurnipPurchasePriceAddr + 0xC; //

        public static readonly UInt32 VillagerAddress = masterAddress - 0x16778E8; //
        public static readonly UInt32 VillagerSize = 0x13230; //
        public static readonly UInt32 VillagerOldSize = 0x12AB0; //
        public static readonly UInt32 VillagerMemorySize = 0x5F0; // TODO

        public static readonly UInt32 VillagerMemoryTinySize = 0x47; //

        public static readonly UInt32 VillagerPlayerOffset = 0x5F0; //

        public static readonly UInt32 VillagerMoveoutOffset = 0x1267A; //0x11EFA;
        public static readonly UInt32 VillagerForceMoveoutOffset = 0x126Ac;//0x11F2C;
        public static readonly UInt32 VillagerFriendshipOffset = 0x46;
        public static readonly UInt32 VillagerCatchphraseOffset = 0x10794;//0x10014;

        public static readonly UInt32 VillagerHouseAddress = 0xAA9C7580; 
        public static readonly UInt32 VillagerHouseSize = 0x1D4; //
        public static readonly UInt32 VillagerHouseBufferDiff = 0xB20770; //
        public static readonly UInt32 VillagerHouseOwnerOffset = 0x1C4; //

        public static readonly UInt32 MysIslandVillagerAddress = 0x3E619AFC; //
        public static readonly UInt32 MysIslandVillagerSpecies = MysIslandVillagerAddress + 0x110;



        public static readonly UInt32 TownNameddress = 0x29D63FD8; //masterAddress - 0x15DE1A0; //? // TODO

        public static readonly UInt32 weatherSeed = masterAddress - 0xA01F9CFC; //



        public static readonly UInt32 player1SlotBase = masterAddress; //
        public static readonly UInt32 player1Slot21Base = player1SlotBase - 0xB8; //
        public static readonly UInt32 player1HouseBase = player1SlotBase + 0xC4; //
        public static readonly UInt32 player1House21Base = player1HouseBase + 0xA0; //

        public static readonly UInt32 playerOffset = 0x76390; //
        public static readonly UInt32 playerReactionAddress = player1SlotBase + 0xAFB4; //

        public static readonly UInt32 player2SlotBase = player1SlotBase + playerOffset; //
        public static readonly UInt32 player2Slot21Base = player2SlotBase - 0xB8;
        public static readonly UInt32 player2HouseBase = player2SlotBase + 0xC4; //
        public static readonly UInt32 player2House21Base = player2HouseBase + 0xA0; //

        public static readonly UInt32 player3SlotBase = player2SlotBase + playerOffset; //
        public static readonly UInt32 player3Slot21Base = player3SlotBase - 0xB8;
        public static readonly UInt32 player3HouseBase = player3SlotBase + 0xC4; //
        public static readonly UInt32 player3House21Base = player3HouseBase + 0xA0; //

        public static readonly UInt32 player4SlotBase = player3SlotBase + playerOffset; //
        public static readonly UInt32 player4Slot21Base = player4SlotBase - 0xB8;
        public static readonly UInt32 player4HouseBase = player4SlotBase + 0xC4; //
        public static readonly UInt32 player4House21Base = player4HouseBase + 0xA0; //

        public static readonly UInt32 player5SlotBase = player4SlotBase + playerOffset; //
        public static readonly UInt32 player5Slot21Base = player5SlotBase - 0xB8;
        public static readonly UInt32 player5HouseBase = player5SlotBase + 0xC4; //
        public static readonly UInt32 player5House21Base = player5HouseBase + 0xA0; //

        public static readonly UInt32 player6SlotBase = player5SlotBase + playerOffset; //
        public static readonly UInt32 player6Slot21Base = player6SlotBase - 0xB8;
        public static readonly UInt32 player6HouseBase = player6SlotBase + 0xC4; //
        public static readonly UInt32 player6House21Base = player6HouseBase + 0xA0; //

        public static readonly UInt32 player7SlotBase = player6SlotBase + playerOffset; //
        public static readonly UInt32 player7Slot21Base = player7SlotBase - 0xB8;
        public static readonly UInt32 player7HouseBase = player7SlotBase + 0xC4; //
        public static readonly UInt32 player7House21Base = player7HouseBase + 0xA0; //

        public static readonly UInt32 player8SlotBase = player7SlotBase + playerOffset; //
        public static readonly UInt32 player8Slot21Base = player8SlotBase - 0xB8;
        public static readonly UInt32 player8HouseBase = player8SlotBase + 0xC4; //
        public static readonly UInt32 player8House21Base = player8HouseBase + 0xA0; //

        // ---- Critter
        public const UInt32 InsectAppearPointer = 0x4668F858; //
        public const Int32 InsectDataSize = 2 * (1 + 6 * 12 + 5);
        public const Int32 InsectNumRecords = 166;

        public const Int32 FishDataSize = 2 * (1 + 3 * 12 + 3);

        public const UInt32 FishRiverAppearPointer = InsectAppearPointer + 0x3F778;
        public const Int32 FishRiverNumRecords = 100;

        public const UInt32 FishSeaAppearPointer = InsectAppearPointer + 0x55618;
        public const Int32 FishSeaNumRecords = 76;

        public const UInt32 CreatureSeaAppearPointer = InsectAppearPointer - 0x3DCE4;
        public const Int32 SeaCreatureDataSize = 84;
        public const Int32 SeaCreatureNumRecords = 41 * 2;
        // ----

        // ---- Main
        public static readonly UInt32 staminaAddress = 0xB4A1EBE0;

        public static readonly UInt32 freezeTimeAddress = 0x0025AB50; //
        public static readonly string freezeTimeValue = "D503201F";
        public static readonly string unfreezeTimeValue = "F9203260";

        public static readonly UInt32 readTimeAddress = 0x0B9A29C0; //

        public static readonly UInt32 wSpeedAddress = 0x0105BABC; //0x0105B8DC; //
        public static readonly string wSpeedX1 = "BD51D661";
        public static readonly string wSpeedX2 = "1E201001";
        public static readonly string wSpeedX3 = "1E211001";
        public static readonly string wSpeedX4 = "1E221001";

        public static readonly UInt32 CollisionAddress = 0x00FD5380; //0x00FD51D0; //
        public static readonly string CollisionDisable = "12800014";
        public static readonly string CollisionEnable = "B955E014";

        public static readonly UInt32 aSpeedAddress = 0x0360F610; //0x0360F610; //
        public static readonly string aSpeedX1 = "3F800000";
        public static readonly string aSpeedX2 = "40000000";
        public static readonly string aSpeedX5 = "40A00000";
        public static readonly string aSpeedX50 = "42480000";
        public static readonly string aSpeedX01 = "3DCCCCCD";

        public static Form1 formControl;

        public Utilities()
        {
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static string GetItemSlotAddress(int slot)
        {
            if (slot <= 20)
            {
                return "0x" + (ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8)).ToString("X");
            }
            else
            {
                return "0x" + (ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8)).ToString("X");
            }
        }

        public static uint GetItemSlotUIntAddress(int slot)
        {
            if (slot <= 20)
            {
                return (uint)(ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8));
            }
            else
            {
                return (uint)(ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8));
            }
        }

        public static string GetItemCountAddress(int slot)
        {
            if (slot <= 20)
            {
                return "0x" + (ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8) + 0x4).ToString("X");
            }
            else
            {
                return "0x" + (ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8) + 0x4).ToString("X");
            }
        }

        public static uint GetItemCountUIntAddress(int slot)
        {
            if (slot <= 20)
            {
                return (uint)(ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8) + 0x4);
            }
            else
            {
                return (uint)(ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8) + 0x4);
            }
        }

        public static string GetItemFlag1Address(int slot)
        {
            if (slot <= 20)
            {
                return "0x" + (0x3 + ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8)).ToString("X");
            }
            else
            {
                return "0x" + (0x3 + ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8)).ToString("X");
            }
        }

        public static byte[] stringToByte(string Bank)
        {
            if (Bank.Length <= 1)
            {
                byte[] small = new byte[1];
                small[0] = Convert.ToByte(Bank, 16);
                return small;
            }

            byte[] save = new byte[Bank.Length / 2];

            for (int i = 0; i < Bank.Length / 2; i++)
            {

                string data = String.Concat(Bank[(i * 2)].ToString(), Bank[((i * 2) + 1)].ToString());
                //Debug.Print(i.ToString() + " " + data);
                save[i] = Convert.ToByte(data, 16);
            }

            return save;
        }

        public static string ByteToHexString(byte[] b)
        {
            String hexString = BitConverter.ToString(b);
            hexString = hexString.Replace("-", "");

            return hexString;
        }

        public static byte[] ByteTrim(byte[] input)
        {
            int newLength = 1;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == 0x0)
                {
                    newLength = i;
                    break;
                }
            }

            byte[] newArray = new byte[newLength];
            Array.Copy(input, newArray, newArray.Length);

            return newArray;
        }

        public static byte[] GetInventoryBank(Socket socket, USBBot bot, int slot)
        {
            try
            {
                if (bot == null)
                {
                    /*
                    string msg = String.Format("peek {0:X8} {1}\r\n", GetItemSlotAddress(slot), 160);
                    Debug.Print(msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                    */

                    /*
                    byte[] msg = Encoding.UTF8.GetBytes("peek" + GetItemSlotAddress(slot) + " 160\r\n");
                    socket.Send(msg);
                    byte[] b = new byte[500];
                    */
                    /*
                    msg = Encoding.UTF8.GetBytes("click X\r\n");
                    socket.Send(msg);
                    msg = Encoding.UTF8.GetBytes("click X\r\n");
                    socket.Send(msg);
                    */
                    /*
                    byte[] b = new byte[325];
                    socket.Receive(b);

                    */
                    Debug.Print("[Sys] Poke : Inventory " + GetItemSlotUIntAddress(slot).ToString("X") + " " + slot);

                    byte[] b = ReadByteArray(socket, GetItemSlotUIntAddress(slot), 160);

                    if (b == null)
                    {
                        MessageBox.Show("Wait something is wrong here!? \n\n GetItemSlotUIntAddress(" + slot + ")");
                    }

                    return b;
                }
                else
                {
                    Debug.Print("[Usb] Poke : Inventory " + GetItemSlotUIntAddress(slot).ToString("X") + " " + slot);

                    byte[] b = bot.ReadBytes(GetItemSlotUIntAddress(slot), 160);

                    if (b == null)
                    {
                        MessageBox.Show("Wait something is wrong here!? \n\n GetItemSlotUIntAddress(" + slot + ")");
                    }

                    return b;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.\nIf you are using USB connection, try restart your switch as well.");
                return null;
            }
        }

        public static bool SpawnItem(Socket socket, USBBot bot, int slot, String value, String amount)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemSlotAddress(slot), flip(precedingZeros(value, 8)));
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    string countMsg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemCountAddress(slot), flip(precedingZeros(amount, 8)));
                    SendString(socket, Encoding.UTF8.GetBytes(countMsg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(flip(precedingZeros(value, 8))), GetItemSlotUIntAddress(slot));

                    bot.WriteBytes(stringToByte(flip(precedingZeros(amount, 8))), GetItemCountUIntAddress(slot));
                }

                //Debug.Print("Slot : " + slot + " | ID : " + value + " | Amount : " + amount);
                //Debug.Print("Spawn Item : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(amount, 8)));
                return true;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public static bool SpawnRecipe(Socket socket, USBBot bot, int slot, String value, String recipeValue)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemSlotAddress(slot), flip(precedingZeros(value, 8)));
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    string countMsg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemCountAddress(slot), flip(precedingZeros(recipeValue, 8)));
                    SendString(socket, Encoding.UTF8.GetBytes(countMsg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(flip(precedingZeros(value, 8))), GetItemSlotUIntAddress(slot));

                    bot.WriteBytes(stringToByte(flip(precedingZeros(recipeValue, 8))), GetItemCountUIntAddress(slot));
                }

                //Debug.Print("Slot : " + slot + " | ID : " + value + " | RecipeValue : " + recipeValue);
                //Debug.Print("Spawn recipe : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(recipeValue, 8)));
                return true;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public static bool SpawnFlower(Socket socket, USBBot bot, int slot, String value, String flowerValue)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemSlotAddress(slot), flip(precedingZeros(value, 8)));
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    string countMsg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemCountAddress(slot), flip(precedingZeros(flowerValue, 8)));
                    SendString(socket, Encoding.UTF8.GetBytes(countMsg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(flip(precedingZeros(value, 8))), GetItemSlotUIntAddress(slot));

                    bot.WriteBytes(stringToByte(flip(precedingZeros(flowerValue, 8))), GetItemCountUIntAddress(slot));
                }

                //Debug.Print("Slot : " + slot + " | ID : " + value + " | FlowerValue : " + flowerValue);
                //Debug.Print("Spawn Flower : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(flowerValue, 8)));
                return true;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public static string flip(string value)
        {
            if (value.Length == 4)
            {
                string first = value.Substring(0, 2);
                string second = value.Substring(2, 2);
                string postFlip = second + first;
                return postFlip;
            }
            else if (value.Length == 8)
            {
                string first = value.Substring(0, 2);
                string second = value.Substring(2, 2);
                string third = value.Substring(4, 2);
                string fourth = value.Substring(6, 2);
                string postFlip = fourth + third + second + first;
                return postFlip;
            }
            else
            {
                return value;
            }
        }

        public static string precedingZeros(string value, int length)
        {
            string n0 = String.Concat(Enumerable.Repeat("0", length - value.Length));
            string result = String.Concat(n0, value);
            return result;
        }

        public static string turn2bytes(string value)
        {
            if (value.Length < 4)
                return precedingZeros(value, 4);
            else
                return value.Substring(value.Length - 4, 4);
        }

        public static void DeleteSlot(Socket s, USBBot bot, int slot)
        {
            SpawnItem(s, bot, slot, "FFFE", "0");
        }

        public static void OverwriteAll(Socket socket, USBBot bot, byte[] buffer1, byte[] buffer2, ref int counter)
        {
            if (bot == null)
            {
                SendByteArray(socket, GetItemSlotUIntAddress(1), buffer1, 160, ref counter);
                SendByteArray(socket, GetItemSlotUIntAddress(21), buffer2, 160, ref counter);
            }
            else
            {
                bot.WriteBytes(buffer1, GetItemSlotUIntAddress(1));
                bot.WriteBytes(buffer2, GetItemSlotUIntAddress(21));
            }
        }

        public static UInt32[] GetTurnipPrices(Socket socket, USBBot bot)
        {
            UInt32[] result = new UInt32[13];
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : TurnipPurchasePrice " + TurnipPurchasePriceAddr.ToString("X"));

                ReadUInt32Array(socket, TurnipPurchasePriceAddr, result, 4, 12);

                Debug.Print("[Sys] Poke : TurnipSellPriceAddr " + TurnipSellPriceAddr.ToString("X"));

                ReadUInt32Array(socket, TurnipSellPriceAddr, result, 4 * 12, 0);
            }
            else
            {
                Debug.Print("[Usb] Poke : TurnipPrice " + TurnipPurchasePriceAddr.ToString("X") + " " + TurnipSellPriceAddr.ToString("X"));

                byte[] b = bot.ReadBytes(TurnipPurchasePriceAddr, 57);

                result[12] = b[0];

                for (int i = 0; i < 12; i++)
                {
                    result[i] = b[12 + (i * 4)];
                }
            }
            return result;
        }

        public static bool ChangeTurnipPrices(Socket socket, USBBot bot, UInt32[] prices)
        {
            if (bot == null)
            {
                SendUInt32Array(socket, TurnipPurchasePriceAddr, prices, 4, 12);
                SendUInt32Array(socket, TurnipSellPriceAddr, prices, 4 * 12);
            }
            else
            {
                byte[] BuyPrice = stringToByte(flip(precedingZeros(prices[12].ToString("X"), 8)));
                bot.WriteBytes(BuyPrice, TurnipPurchasePriceAddr);

                for (int i = 0; i < 12; i++)
                {
                    bot.WriteBytes(stringToByte(flip(precedingZeros(prices[i].ToString("X"), 8))), (uint)(TurnipSellPriceAddr + (4 * i)));
                }
            }
            return false;
        }

        public static void setAddress(int player)
        {
            if (player == 1)
            {
                ItemSlotBase = player1SlotBase;
                ItemSlot21Base = player1Slot21Base;
            }
            else if (player == 2)
            {
                ItemSlotBase = player2SlotBase;
                ItemSlot21Base = player2Slot21Base;
            }
            else if (player == 3)
            {
                ItemSlotBase = player3SlotBase;
                ItemSlot21Base = player3Slot21Base;
            }
            else if (player == 4)
            {
                ItemSlotBase = player4SlotBase;
                ItemSlot21Base = player4Slot21Base;
            }
            else if (player == 5)
            {
                ItemSlotBase = player5SlotBase;
                ItemSlot21Base = player5Slot21Base;
            }
            else if (player == 6)
            {
                ItemSlotBase = player6SlotBase;
                ItemSlot21Base = player6Slot21Base;
            }
            else if (player == 7)
            {
                ItemSlotBase = player7SlotBase;
                ItemSlot21Base = player7Slot21Base;
            }
            else if (player == 8)
            {
                ItemSlotBase = player8SlotBase;
                ItemSlot21Base = player8Slot21Base;
            }
            else if (player == 9) //Recycling
            {
                ItemSlotBase = MasterRecyclingBase;
                ItemSlot21Base = MasterRecycling21Base;
            }
            else if (player == 11) //House 1
            {
                ItemSlotBase = player1HouseBase;
                ItemSlot21Base = player1House21Base;
            }
            else if (player == 12) //House 2
            {
                ItemSlotBase = player2HouseBase;
                ItemSlot21Base = player2House21Base;
            }
            else if (player == 13) //House 3
            {
                ItemSlotBase = player3HouseBase;
                ItemSlot21Base = player3House21Base;
            }
            else if (player == 14) //House 4
            {
                ItemSlotBase = player4HouseBase;
                ItemSlot21Base = player4House21Base;
            }
            else if (player == 15) //House 5
            {
                ItemSlotBase = player5HouseBase;
                ItemSlot21Base = player5House21Base;
            }
            else if (player == 16) //House 6
            {
                ItemSlotBase = player6HouseBase;
                ItemSlot21Base = player6House21Base;
            }
            else if (player == 17) //House 7
            {
                ItemSlotBase = player7HouseBase;
                ItemSlot21Base = player7House21Base;
            }
            else if (player == 18) //House 8
            {
                ItemSlotBase = player8HouseBase;
                ItemSlot21Base = player8House21Base;
            }
        }

        public static void gotoRecyclingPage(uint page)
        {
            ItemSlotBase = MasterRecyclingBase + ((page - 1) * 0x140);
            ItemSlot21Base = MasterRecycling21Base + ((page - 1) * 0x140);
        }

        public static void gotoHousePage(uint page, int player)
        {
            switch (player)
            {
                case 1:
                    ItemSlotBase = player1HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player1House21Base + ((page - 1) * 0x140);
                    break;
                case 2:
                    ItemSlotBase = player2HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player2House21Base + ((page - 1) * 0x140);
                    break;
                case 3:
                    ItemSlotBase = player3HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player3House21Base + ((page - 1) * 0x140);
                    break;
                case 4:
                    ItemSlotBase = player4HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player4House21Base + ((page - 1) * 0x140);
                    break;
                case 5:
                    ItemSlotBase = player5HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player5House21Base + ((page - 1) * 0x140);
                    break;
                case 6:
                    ItemSlotBase = player6HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player6House21Base + ((page - 1) * 0x140);
                    break;
                case 7:
                    ItemSlotBase = player7HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player7House21Base + ((page - 1) * 0x140);
                    break;
                case 8:
                    ItemSlotBase = player8HouseBase + ((page - 1) * 0x140);
                    ItemSlot21Base = player8House21Base + ((page - 1) * 0x140);
                    break;
            }
        }

        public static byte[] peekAddress(Socket socket, USBBot bot, long address, int size)
        {
            try
            {
                if (bot == null)
                {
                    /*
                    string msg = String.Format("peek {0:X8} {1}\r\n", address, size);
                    Debug.Print("Peek : " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    byte[] b = new byte[330];
                    socket.Receive(b);
                    */
                    Debug.Print("[Sys] Poke : Address " + address.ToString("X") + " " + size);

                    byte[] b = ReadByteArray(socket, address, size);

                    if (b == null)
                    {
                        MessageBox.Show("Wait something is wrong here!? \n\n peek " + address.ToString("X") + " " + size);
                    }

                    return b;
                }
                else
                {
                    Debug.Print("[Usb] Poke : Address " + address.ToString("X") + " " + size);

                    byte[] b = bot.ReadBytes((uint)address, size);

                    if (b == null)
                    {
                        MessageBox.Show("Wait something is wrong here!? \n\n peek " + address);
                    }

                    return b;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public static void pokeAddress(Socket socket, USBBot bot, string address, string value)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke {0:X8} {1}\r\n", address, "0x" + value);
                    Debug.Print("Poke : " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(value), Convert.ToUInt32(address, 16));
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }

        public static void pokeMainAddress(Socket socket, USBBot bot, string address, string value)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("pokeMain 0x{0:X8} 0x{1}\r\n", address, flip(value));
                    Debug.Print("PokeMain : " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(value), Convert.ToUInt32(address, 16));
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }


        public static void setStamina(Socket socket, USBBot bot, string value)
        {
            pokeAddress(socket, bot, "0x" + staminaAddress.ToString("X"), value);
        }

        public static void setFlag1(Socket socket, USBBot bot, int slot, string flag)
        {
            pokeAddress(socket, bot, GetItemFlag1Address(slot), flag);
        }

        public static byte[] ReadByteArray(Socket socket, long initAddr, int size)
        {
            try
            {
                // Read in small chunks
                byte[] result = new byte[size];
                const int maxBytesToReceive = 504;
                int received = 0;
                int bytesToReceive;
                while (received < size)
                {
                    bytesToReceive = (size - received > maxBytesToReceive) ? maxBytesToReceive : size - received;
                    string bufferRepr = ReadToIntermediateString(socket, initAddr + received, bytesToReceive);
                    if (bufferRepr == null)
                    {
                        formControl.ClearRefresh();
                        return null;
                    }
                    for (int i = 0; i < bytesToReceive; i++)
                    {
                        result[received + i] = Convert.ToByte(bufferRepr.Substring(i * 2, 2), 16);
                    }
                    received += bytesToReceive;
                }
                return result;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                formControl.ClearRefresh();
            }

            return null;
        }
        public static byte[] ReadByteArray(Socket socket, long initAddr, int size, ref int counter)
        {
            try
            {
                // Read in small chunks
                byte[] result = new byte[size];
                const int maxBytesToReceive = 504;
                int received = 0;
                int bytesToReceive;
                while (received < size)
                {
                    bytesToReceive = (size - received > maxBytesToReceive) ? maxBytesToReceive : size - received;
                    string bufferRepr = ReadToIntermediateString(socket, initAddr + received, bytesToReceive);
                    for (int i = 0; i < bytesToReceive; i++)
                    {
                        result[received + i] = Convert.ToByte(bufferRepr.Substring(i * 2, 2), 16);
                    }
                    received += bytesToReceive;
                    counter++;
                }
                return result;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                formControl.ClearRefresh();
            }

            return null;
        }
        public static bool SendByteArray(Socket socket, long initAddr, byte[] buffer, int size, ref int counter)
        {
            try
            {
                // Send in small chunks
                const int maxBytesTosend = 500;
                int sent = 0;
                int bytesToSend = 0;
                StringBuilder dataTemp = new StringBuilder();
                string msg;
                while (sent < size)
                {
                    dataTemp.Clear();
                    bytesToSend = (size - sent > maxBytesTosend) ? maxBytesTosend : size - sent;
                    for (int i = 0; i < bytesToSend; i++)
                    {
                        dataTemp.Append(String.Format("{0:X2}", buffer[sent + i]));
                    }
                    msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", initAddr + sent, dataTemp.ToString());
                    //Debug.Print(msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                    sent += bytesToSend;
                    counter++;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        private static string ReadToIntermediateString(Socket socket, long address, int size)
        {
            try
            {

                string msg = String.Format("peek 0x{0:X8} {1}\r\n", address, size);
                //Debug.Print(msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));
                byte[] b = new byte[size * 2 + 64];
                int first_rec = ReceiveString(socket, b);
                Debug.Print(String.Format("Received {0} Bytes", first_rec));
                return Encoding.ASCII.GetString(b, 0, size * 2);
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                formControl.ClearRefresh();
            }

            return null;
        }

        public static void ReadUInt32Array(Socket socket, long initAddr, UInt32[] buffer, int size, int offset = 0)
        {
            try
            {
                // Read in small chunks
                const int maxBytesToReceive = 127 * 4;  // Absolutely needs to be multiple of 4
                int received = 0;
                int bytesToReceive;
                while (received < size)
                {
                    bytesToReceive = (size - received > maxBytesToReceive) ? maxBytesToReceive : size - received;
                    string bufferRepr = ReadToIntermediateString(socket, initAddr + received, bytesToReceive);
                    for (int i = 0; i < (bytesToReceive / 4); i++)
                    {
                        buffer[offset + (received / 4) + i] = Convert.ToUInt32(bufferRepr.Substring(i * 8 + 6, 2) +
                                                    bufferRepr.Substring(i * 8 + 4, 2) +
                                                    bufferRepr.Substring(i * 8 + 2, 2) +
                                                    bufferRepr.Substring(i * 8, 2), 16);
                    }
                    received += bytesToReceive;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                formControl.ClearRefresh();
            }
        }

        public static bool SendUInt32Array(Socket socket, long initAddr, UInt32[] buffer, int size, int offset = 0)
        {
            try
            {
                // Send in small chunks
                const int maxUInt32Tosend = 125;
                size /= 4;
                int sent = 0;
                int UInt32ToSend = 0;
                StringBuilder dataTemp = new StringBuilder();
                string msg;
                while (sent < size)
                {
                    dataTemp.Clear();
                    UInt32ToSend = (size - sent > maxUInt32Tosend) ? maxUInt32Tosend : size - sent;
                    for (int i = 0; i < UInt32ToSend; i++)
                    {
                        dataTemp.Append(String.Format("{0:X2}{1:X2}{2:X2}{3:X2}",
                            (buffer[offset + sent + i] & 0xFF), (buffer[offset + sent + i] & 0xFF00) >> 8,
                            (buffer[offset + sent + i] & 0xFF0000) >> 16, (buffer[offset + sent + i] & 0xFF000000) >> 24));
                    }
                    msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", initAddr + sent * 4, dataTemp.ToString());
                    Debug.Print(msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                    sent += UInt32ToSend;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public static void SendString(Socket socket, byte[] buffer, int offset = 0, int size = 0, int timeout = 100)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes is already sent
            if (size == 0)
                for (int i = offset; i < buffer.Length; i++)
                    if (buffer[i] == 0xA)
                    {
                        size = i + 1 - offset;
                        break;
                    }
            if (size == 0) size = buffer.Length - offset;
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (sent < size);
        }

        public static int ReceiveString(Socket socket, byte[] buffer, int offset = 0, int size = 0, int timeout = 100)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;  // how many bytes is already received
            if (size == 0) size = buffer.Length - offset;
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (received < size && buffer[received - 1] != 0xA);
            return received;
        }

        public static byte[] GetTownID(Socket socket, USBBot bot)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : TownID " + TownNameddress.ToString("X"));

                byte[] b = ReadByteArray(socket, TownNameddress, 0x1C);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n TownNameddress");
                }
                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : TownID " + TownNameddress.ToString("X"));

                byte[] b = bot.ReadBytes(TownNameddress, 0x1C);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n TownNameddress");
                }
                return b;
            }
        }

        public static byte[] GetWeatherSeed(Socket socket, USBBot bot)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : WeatherSeed " + weatherSeed.ToString("X"));

                byte[] b = ReadByteArray(socket, weatherSeed, 0x4);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n WeatherSeed");
                }
                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : WeatherSeed " + weatherSeed.ToString("X"));

                byte[]b =  bot.ReadBytes(weatherSeed, 0x4);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n WeatherSeed");
                }
                return b;
            }
        }

        public static byte[] getReaction(Socket socket, USBBot bot, int player)
        {
            try
            {
                if (bot == null)
                {
                    /*
                    string msg = String.Format("peek 0x{0:X8} {1}\r\n", reactionAddress.ToString("X"), 8);
                    //Debug.Print("Peek Reaction : " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    byte[] b = new byte[4096];
                    socket.Receive(b);
                    */
                    Debug.Print("[Sys] Poke : Reaction " + (playerReactionAddress + (player * playerOffset)).ToString("X"));

                    byte[] b = ReadByteArray(socket, (playerReactionAddress + (player * playerOffset)), 8);

                    if (b == null)
                    {
                        MessageBox.Show("Wait something is wrong here!? \n\n Reaction ");
                    }

                    return b;
                }
                else
                {
                    Debug.Print("[Usb] Poke : Reaction " + (playerReactionAddress + (player * playerOffset)).ToString("X"));

                    byte[] b = bot.ReadBytes((uint)(playerReactionAddress + (player * playerOffset)), 8);

                    if (b == null)
                    {
                        MessageBox.Show("Wait something is wrong here!? \n\n Reaction");
                    }

                    return b;
                }

            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public static void setReaction(Socket socket, USBBot bot, int player, string reaction1, string reaction2)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", (playerReactionAddress + (player * playerOffset)).ToString("x"), reaction1);
                    Debug.Print("Poke Reaction: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", ((playerReactionAddress + (player * playerOffset)) + 4).ToString("x"), reaction2);
                    Debug.Print("Poke Reaction: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(reaction1), (uint)(playerReactionAddress + (player * playerOffset)));

                    bot.WriteBytes(stringToByte(reaction2), (uint)((playerReactionAddress + (player * playerOffset)) + 4));
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }

        public static void SendSpawnRate(Socket socket, USBBot bot, byte[] buffer, int index, int type, ref int counter)
        {
            if (bot == null)
            {
                if (type == 0)
                {
                    SendByteArray(socket, InsectAppearPointer + InsectDataSize * index + 0x2, buffer, 12 * 6 * 2, ref counter);
                }
                else if (type == 1)
                {
                    SendByteArray(socket, FishRiverAppearPointer + FishDataSize * index + 0x2, buffer, 12 * 3 * 2, ref counter);
                }
                else if (type == 2)
                {
                    SendByteArray(socket, FishSeaAppearPointer + FishDataSize * index + 0x2, buffer, 12 * 3 * 2, ref counter);
                }
                else if (type == 3)
                {
                    SendByteArray(socket, CreatureSeaAppearPointer + SeaCreatureDataSize * index + 0x2, buffer, 12 * 3 * 2, ref counter);
                }
            }
            else
            {
                if (type == 0)
                {
                    bot.WriteBytes(buffer, (uint)(InsectAppearPointer + InsectDataSize * index + 0x2));
                }
                else if (type == 1)
                {
                    bot.WriteBytes(buffer, (uint)(FishRiverAppearPointer + FishDataSize * index + 0x2));
                }
                else if (type == 2)
                {
                    bot.WriteBytes(buffer, (uint)(FishSeaAppearPointer + FishDataSize * index + 0x2));
                }
                else if (type == 3)
                {
                    bot.WriteBytes(buffer, (uint)(CreatureSeaAppearPointer + SeaCreatureDataSize * index + 0x2));
                }
            }
        }

        public static byte[] GetCritterData(Socket socket, USBBot bot, int mode)
        {
            if (bot == null)
            {
                if (mode == 0)
                {
                    Debug.Print("[Sys] Poke : Insect " + InsectAppearPointer.ToString("X") + " " + InsectDataSize * InsectNumRecords);
                    return ReadByteArray(socket, InsectAppearPointer, InsectDataSize * InsectNumRecords);
                }
                else if (mode == 1)
                {
                    Debug.Print("[Sys] Poke : FishRiver " + FishRiverAppearPointer.ToString("X") + " " + FishDataSize * FishRiverNumRecords);
                    return ReadByteArray(socket, FishRiverAppearPointer, FishDataSize * FishRiverNumRecords);
                }
                else if (mode == 2)
                {
                    Debug.Print("[Sys] Poke : FishSea " + FishSeaAppearPointer.ToString("X") + " " + FishDataSize * FishSeaNumRecords);
                    return ReadByteArray(socket, FishSeaAppearPointer, FishDataSize * FishSeaNumRecords);
                }
                else if (mode == 3)
                {
                    Debug.Print("[Sys] Poke : CreatureSea " + CreatureSeaAppearPointer.ToString("X") + " " + SeaCreatureDataSize * SeaCreatureNumRecords);
                    return ReadByteArray(socket, CreatureSeaAppearPointer, SeaCreatureDataSize * SeaCreatureNumRecords);
                }
                return null;
            }
            else
            {
                if (mode == 0)
                {
                    Debug.Print("[Usb] Poke : Insect " + InsectAppearPointer.ToString("X") + " " + InsectDataSize * InsectNumRecords);
                    return ReadLargeBytes(bot, InsectAppearPointer, InsectDataSize * InsectNumRecords);
                }
                else if (mode == 1)
                {
                    Debug.Print("[Usb] Poke : FishRiver " + FishRiverAppearPointer.ToString("X") + " " + FishDataSize * FishRiverNumRecords);
                    return ReadLargeBytes(bot, FishRiverAppearPointer, FishDataSize * FishRiverNumRecords);
                }
                else if (mode == 2)
                {
                    Debug.Print("[Usb] Poke : FishSea " + FishSeaAppearPointer.ToString("X") + " " + FishDataSize * FishSeaNumRecords);
                    return ReadLargeBytes(bot, FishSeaAppearPointer, FishDataSize * FishSeaNumRecords);
                }
                else if (mode == 3)
                {
                    Debug.Print("[Usb] Poke : CreatureSea " + CreatureSeaAppearPointer.ToString("X") + " " + SeaCreatureDataSize * SeaCreatureNumRecords);
                    return ReadLargeBytes(bot, CreatureSeaAppearPointer, SeaCreatureDataSize * SeaCreatureNumRecords);
                }
                return null;
            }
        }

        private static byte[] ReadLargeBytes(USBBot bot, uint address, int size)
        {
            // Read in small chunks
            byte[] result = new byte[size];
            const int maxBytesToReceive = 300;
            int received = 0;
            int bytesToReceive;
            while (received < size)
            {
                bytesToReceive = (size - received > maxBytesToReceive) ? maxBytesToReceive : size - received;
                byte[] buffer = bot.ReadBytes((uint)(address + received), bytesToReceive);
                for (int i = 0; i < bytesToReceive; i++)
                {
                    result[received + i] = buffer[i];
                }
                received += bytesToReceive;
            }
            return result;
        }

        private static byte[] ReadLargeBytes(USBBot bot, uint address, int size, ref int counter)
        {
            // Read in small chunks
            byte[] result = new byte[size];
            const int maxBytesToReceive = 300;
            int received = 0;
            int bytesToReceive;
            while (received < size)
            {
                bytesToReceive = (size - received > maxBytesToReceive) ? maxBytesToReceive : size - received;
                byte[] buffer = bot.ReadBytes((uint)(address + received), bytesToReceive);
                for (int i = 0; i < bytesToReceive; i++)
                {
                    result[received + i] = buffer[i];
                }
                received += bytesToReceive;
                counter++;
            }
            return result;
        }

        private static void WriteLargeBytes(USBBot bot, long initAddr, byte[] buffer, int size, ref int counter)
        {

            const int maxBytesTosend = 300;
            int sent = 0;
            int bytesToSend = 0;
            byte[] temp;
            while (sent < size)
            {
                bytesToSend = (size - sent > maxBytesTosend) ? maxBytesTosend : size - sent;
                temp = new byte[bytesToSend];
                for (int i = 0; i < bytesToSend; i++)
                {
                    temp[i] = buffer[sent + i];
                }
                /*
                for (int i = 0; i < bytesToSend; i++)
                {
                    dataTemp.Append(String.Format("{0:X2}", buffer[sent + i]));
                }
                msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", initAddr + sent, dataTemp.ToString());
                */
                //Debug.Print(msg);
                //SendString(socket, Encoding.UTF8.GetBytes(msg));
                bot.WriteBytes(temp, (uint)(initAddr + sent));
                sent += bytesToSend;
                counter++;
            }
        }

        public static byte[] GetVillager(Socket socket, USBBot bot, int num, int size, ref int counter)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : Villager " + (VillagerAddress + (num * VillagerSize)).ToString("X") + " " + num + " " + size);

                byte[] b = ReadByteArray(socket, VillagerAddress + (num * VillagerSize), size, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Villager");
                }

                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : Villager " + (VillagerAddress + (num * VillagerSize)).ToString("X") + " " + num + " " + size);

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerAddress + (num * VillagerSize)), size, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Villager");
                }

                return b;
            }
        }

        public static void LoadVillager(Socket socket, USBBot bot, int num, byte[] villager, ref int counter)
        {
            if (bot == null)
            {
                SendByteArray(socket, VillagerAddress + (num * VillagerSize), villager, (int)VillagerSize, ref counter);
            }
            else
            {
                WriteLargeBytes(bot, VillagerAddress + (num * VillagerSize), villager, (int)VillagerSize, ref counter);
            }
        }

        public static byte[] GetMoveout(Socket socket, USBBot bot, int num, int size, ref int counter)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : Moveout " + (VillagerAddress + (num * VillagerSize) + VillagerMoveoutOffset).ToString("X") + " " + size);

                byte[] b = ReadByteArray(socket, VillagerAddress + (num * VillagerSize) + VillagerMoveoutOffset, size, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Moveout");
                }

                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : Moveout " + (VillagerAddress + (num * VillagerSize) + VillagerMoveoutOffset).ToString("X") + " " + size);

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerAddress + (num * VillagerSize) + VillagerMoveoutOffset), size, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Moveout");
                }

                return b;
            }
        }
        public static byte[] GetHouse(Socket socket, USBBot bot, int num, ref int counter, uint diff = 0)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : House " + (VillagerHouseAddress + (num * VillagerHouseSize) + diff).ToString("X") + " " + (int)VillagerHouseSize);

                byte[] b = ReadByteArray(socket, VillagerHouseAddress + (num * VillagerHouseSize) + diff, (int)VillagerHouseSize, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n House");
                }

                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : House " + (VillagerHouseAddress + (num * VillagerHouseSize) + diff).ToString("X") + " " + (int)VillagerHouseSize);

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerHouseAddress + (num * VillagerHouseSize) + diff), (int)VillagerHouseSize);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n House");
                }

                return b;
            }
        }

        public static void LoadHouse(Socket socket, USBBot bot, int num, byte[] house, ref int counter, uint diff = 0)
        {
            if (bot == null)
            {
                SendByteArray(socket, VillagerHouseAddress + (num * VillagerHouseSize) + diff, house, (int)VillagerHouseSize, ref counter);
            }
            else
            {
                WriteLargeBytes(bot, VillagerHouseAddress + (num * VillagerHouseSize) + diff, house, (int)VillagerHouseSize, ref counter);
            }
        }

        public static byte GetHouseOwner(Socket socket, USBBot bot, int num, ref int counter)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : HouseOwner " + (VillagerHouseAddress + (num * VillagerHouseSize) + VillagerHouseOwnerOffset).ToString("X"));

                byte[] b = ReadByteArray(socket, VillagerHouseAddress + (num * VillagerHouseSize) + VillagerHouseOwnerOffset, 1, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n HouseOwner");
                }

                return b[0];
            }
            else
            {
                Debug.Print("[Usb] Poke : HouseOwner " + (VillagerHouseAddress + (num * VillagerHouseSize) + VillagerHouseOwnerOffset).ToString("X"));

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerHouseAddress + (num * VillagerHouseSize) + VillagerHouseOwnerOffset), 1, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n HouseOwner");
                }

                return b[0];
            }
        }

        public static byte[] GetCatchphrase(Socket socket, USBBot bot, int num, ref int counter)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : Catchphrase " + (VillagerAddress + (num * VillagerSize) + VillagerCatchphraseOffset).ToString("X"));

                byte[] b = ReadByteArray(socket, VillagerAddress + (num * VillagerSize) + VillagerCatchphraseOffset, 0x2C, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Catchphrase");
                }

                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : Catchphrase " + (VillagerAddress + (num * VillagerSize) + VillagerCatchphraseOffset).ToString("X"));

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerAddress + (num * VillagerSize) + VillagerCatchphraseOffset), 0x2C, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Catchphrase");
                }

                return b;
            }
        }

        public static void SetCatchphrase(Socket socket, USBBot bot, int num, byte[] pharse)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", (VillagerAddress + (num * VillagerSize) + VillagerCatchphraseOffset).ToString("X"), ByteToHexString(pharse));
                    Debug.Print("Poke Catchphrase: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    bot.WriteBytes(pharse, (uint)(VillagerAddress + (num * VillagerSize) + VillagerCatchphraseOffset));
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }

        public static byte GetVillagerFlag(Socket socket, USBBot bot, int num, uint offset)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : VillagerFlag " + (VillagerAddress + (num * VillagerSize) + offset).ToString("X"));

                byte[] b = ReadByteArray(socket, VillagerAddress + (num * VillagerSize) + offset, 1);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n VillagerFlag");
                }

                return b[0];
            }
            else
            {
                Debug.Print("[Usb] Poke : VillagerFlag " + (VillagerAddress + (num * VillagerSize) + offset).ToString("X"));

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerAddress + (num * VillagerSize) + offset), 1);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n VillagerFlag");
                }

                return b[0];
            }
        }

        public static byte GetVillagerHouseFlag(Socket socket, USBBot bot, int num, uint offset, ref int counter)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : VillagerHouseFlag " + (VillagerHouseAddress + (num * VillagerHouseSize) + offset).ToString("X"));

                byte[] b = ReadByteArray(socket, VillagerHouseAddress + (num * VillagerHouseSize) + offset, 1, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n VillagerHouseFlag");
                }

                return b[0];
            }
            else
            {
                Debug.Print("[Usb] Poke : VillagerHouseFlag " + (VillagerHouseAddress + (num * VillagerHouseSize) + offset).ToString("X"));

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerHouseAddress + (num * VillagerHouseSize) + offset), 1, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n VillagerHouseFlag");
                }

                return b[0];
            }
        }

        public static int FindHouseIndex(int VillagerNum, int[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == VillagerNum)
                    return i;
            }
            return -1;
        }

        public static void SetMoveout(Socket socket, USBBot bot, int num, string MoveoutFlag = "2", string ForceMoveoutFlag = "1")
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", (VillagerAddress + (num * VillagerSize) + VillagerMoveoutOffset).ToString("X"), MoveoutFlag);
                    Debug.Print("Poke Moveout: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", (VillagerAddress + (num * VillagerSize) + VillagerForceMoveoutOffset).ToString("X"), ForceMoveoutFlag);
                    Debug.Print("Poke ForceMoveout: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(MoveoutFlag), (uint)(VillagerAddress + (num * VillagerSize) + VillagerMoveoutOffset));

                    bot.WriteBytes(stringToByte(ForceMoveoutFlag), (uint)(VillagerAddress + (num * VillagerSize) + VillagerForceMoveoutOffset));
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }

        public static void SetFriendship(Socket socket, USBBot bot, int num, int player, string FriendshipFlag = "FF")
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", (VillagerAddress + (num * VillagerSize) + (player * VillagerPlayerOffset) + VillagerFriendshipOffset).ToString("X"), FriendshipFlag);
                    Debug.Print("Poke Friendship: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(FriendshipFlag), (uint)(VillagerAddress + (num * VillagerSize) + (player * VillagerPlayerOffset) + VillagerFriendshipOffset));
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }

        public static byte[] GetPlayerDataVillager(Socket socket, USBBot bot, int num, int player, int size, ref int counter)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : Villager " + player + " " + (VillagerAddress + (num * VillagerSize) + (player * VillagerPlayerOffset)).ToString("X") + " " + num + " " + size);

                byte[] b = ReadByteArray(socket, VillagerAddress + (num * VillagerSize) + (player * VillagerPlayerOffset), size, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Villager");
                }

                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : Villager " + player + " " + (VillagerAddress + (num * VillagerSize) + (player * VillagerPlayerOffset)).ToString("X") + " " + num + " " + size);

                byte[] b = ReadLargeBytes(bot, (uint)(VillagerAddress + (num * VillagerSize) + (player * VillagerPlayerOffset)), size, ref counter);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n Villager");
                }

                return b;
            }
        }

        public static void SetMysVillager(Socket socket, USBBot bot, byte[] buffer, byte[] species, ref int counter)
        {
            if (bot == null)
            {
                SendByteArray(socket, MysIslandVillagerAddress, buffer, buffer.Length, ref counter);
                SendByteArray(socket, MysIslandVillagerSpecies, species, species.Length, ref counter);
            }
            else
            {
                bot.WriteBytes(buffer, MysIslandVillagerAddress);
                bot.WriteBytes(species, MysIslandVillagerSpecies);
            }
        }

        public static byte[] GetMysVillagerName(Socket socket, USBBot bot)
        {
            if (bot == null)
            {
                Debug.Print("[Sys] Poke : MysVillager " + MysIslandVillagerAddress.ToString("X"));

                byte[] b = ReadByteArray(socket, MysIslandVillagerAddress, 8);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n MysVillagerName");
                }
                return b;
            }
            else
            {
                Debug.Print("[Usb] Poke : MysVillager " + MysIslandVillagerAddress.ToString("X"));

                byte[] b = bot.ReadBytes(MysIslandVillagerAddress, 8);

                if (b == null)
                {
                    MessageBox.Show("Wait something is wrong here!? \n\n MysVillagerName");
                }
                return b;
            }
        }

        public static string GetVillagerInternalName(byte Species, byte Variant)
        {
            int s = Convert.ToInt32(Species);
            int v = Convert.ToInt32(Variant);
            return $"{(VillagerSpecies)Species}{Variant:00}";
        }
        public static string GetVillagerRealName(byte Species, byte Variant)
        {
            string internalName = GetVillagerInternalName(Species, Variant);
            if (RealName.ContainsKey(internalName))
                return RealName[internalName];
            else
                return "ERROR";
        }

        public static string GetVillagerRealName(string IName)
        {
            if (RealName.ContainsKey(IName))
                return RealName[IName];
            else
                return "ERROR";
        }

        public static string GetVillagerImage(string name)
        {
            string path = @"img\Villager\" + name + ".png";
            if (File.Exists(path))
                return path; //file found
            else
                return @"img\Villager\QuestionMark.png"; //file not found
        }

        public static string TrimFromZero(string input) => TrimFromFirst(input, '\0');

        private static string TrimFromFirst(string input, char c)
        {
            int index = input.IndexOf(c);
            return index < 0 ? input : input.Substring(0, index);
        }

        public static string GetString(byte[] data, int offset, int maxLength)
        {
            var str = Encoding.Unicode.GetString(data, offset, maxLength * 2);
            return TrimFromZero(str);
        }


        public static byte[] GetBytes(string value, int maxLength)
        {
            if (value.Length > maxLength)
                value = value.Substring(0, maxLength);
            else if (value.Length < maxLength)
                value = value.PadRight(maxLength, '\0');
            return Encoding.Unicode.GetBytes(value);
        }

        public static byte[] Slice(byte[] src, int offset, int length)
        {
            byte[] data = new byte[length];
            Buffer.BlockCopy(src, offset, data, 0, data.Length);
            return data;
        }

        public enum VillagerPersonality : byte
        {
            Lazy,
            Jock,
            Cranky,
            Smug,
            Normal,
            Peppy,
            Snooty,
            Uchi,
            None,
        }
        public enum VillagerSpecies
        {
            ant = 0x0,
            bea = 0x1,
            brd = 0x2,
            bul = 0x3,
            cat = 0x4,
            cbr = 0x5,
            chn = 0x6,
            cow = 0x7,
            crd = 0x8,
            der = 0x9,
            dog = 0xA,
            duk = 0xB,
            elp = 0xC,
            flg = 0xD,
            goa = 0xE,
            gor = 0xF,
            ham = 0x10,
            hip = 0x11,
            hrs = 0x12,
            kal = 0x13,
            kgr = 0x14,
            lon = 0x15,
            mnk = 0x16,
            mus = 0x17,
            ocp = 0x18,
            ost = 0x19,
            pbr = 0x1A,
            pgn = 0x1B,
            pig = 0x1C,
            rbt = 0x1D,
            rhn = 0x1E,
            shp = 0x1F,
            squ = 0x20,
            tig = 0x21,
            wol = 0x22,
            non = 0x23,
        }

        public static Dictionary<string, byte> CheckSpecies = new Dictionary<string, byte>
        {
            {"ant", 0x0},
            {"bea", 0x1},
            {"brd", 0x2},
            {"bul", 0x3},
            {"cat", 0x4},
            {"cbr", 0x5},
            {"chn", 0x6},
            {"cow", 0x7},
            {"crd", 0x8},
            {"der", 0x9},
            {"dog", 0xA},
            {"duk", 0xB},
            {"elp", 0xC},
            {"flg", 0xD},
            {"goa", 0xE},
            {"gor", 0xF},
            {"ham", 0x10},
            {"hip", 0x11},
            {"hrs", 0x12},
            {"kal", 0x13},
            {"kgr", 0x14},
            {"lon", 0x15},
            {"mnk", 0x16},
            {"mus", 0x17},
            {"ocp", 0x18},
            {"ost", 0x19},
            {"pbr", 0x1A},
            {"pgn", 0x1B},
            {"pig", 0x1C},
            {"rbt", 0x1D},
            {"rhn", 0x1E},
            {"shp", 0x1F},
            {"squ", 0x20},
            {"tig", 0x21},
            {"wol", 0x22},
            {"non", 0x23},
        };

        public static Dictionary<string, string> RealName = new Dictionary<string, string>
            {
                {"ant00", "Cyrano"},
                {"ant01", "Antonio"},
                {"ant02", "Pango"},
                {"ant03", "Anabelle"},
                {"ant06", "Snooty"},
                {"ant08", "Annalisa"},
                {"ant09", "Olaf"},
                {"bea00", "Teddy"},
                {"bea01", "Pinky"},
                {"bea02", "Curt"},
                {"bea03", "Chow"},
                {"bea05", "Nate"},
                {"bea06", "Groucho"},
                {"bea07", "Tutu"},
                {"bea08", "Ursala"},
                {"bea09", "Grizzly"},
                {"bea10", "Paula"},
                {"bea11", "Ike"},
                {"bea12", "Charlise"},
                {"bea13", "Beardo"},
                {"bea14", "Klaus"},
                {"bea15", "Megan"},
                {"brd00", "Jay"},
                {"brd01", "Robin"},
                {"brd02", "Anchovy"},
                {"brd03", "Twiggy"},
                {"brd04", "Jitters"},
                {"brd05", "Piper"},
                {"brd06", "Admiral"},
                {"brd08", "Midge"},
                {"brd11", "Jacob"},
                {"brd15", "Lucha"},
                {"brd16", "Jacques"},
                {"brd17", "Peck"},
                {"brd18", "Sparro"},
                {"bul00", "Angus"},
                {"bul01", "Rodeo"},
                {"bul03", "Stu"},
                {"bul05", "T-Bone"},
                {"bul07", "Coach"},
                {"bul08", "Vic"},
                {"cat00", "Bob"},
                {"cat01", "Mitzi"},
                {"cat02", "Rosie"},
                {"cat03", "Olivia"},
                {"cat04", "Kiki"},
                {"cat05", "Tangy"},
                {"cat06", "Punchy"},
                {"cat07", "Purrl"},
                {"cat08", "Moe"},
                {"cat09", "Kabuki"},
                {"cat10", "Kid Cat"},
                {"cat11", "Monique"},
                {"cat12", "Tabby"},
                {"cat13", "Stinky"},
                {"cat14", "Kitty"},
                {"cat15", "Tom"},
                {"cat16", "Merry"},
                {"cat17", "Felicity"},
                {"cat18", "Lolly"},
                {"cat19", "Ankha"},
                {"cat20", "Rudy"},
                {"cat21", "Katt"},
                {"cat23", "Raymond"},
                {"cbr00", "Bluebear"},
                {"cbr01", "Maple"},
                {"cbr02", "Poncho"},
                {"cbr03", "Pudge"},
                {"cbr04", "Kody"},
                {"cbr05", "Stitches"},
                {"cbr06", "Vladimir"},
                {"cbr07", "Murphy"},
                {"cbr09", "Olive"},
                {"cbr10", "Cheri"},
                {"cbr13", "June"},
                {"cbr14", "Pekoe"},
                {"cbr15", "Chester"},
                {"cbr16", "Barold"},
                {"cbr17", "Tammy"},
                {"cbr18", "Marty"},
                {"cbr19", "Judy"},
                {"chn00", "Goose"},
                {"chn01", "Benedict"},
                {"chn02", "Egbert"},
                {"chn05", "Ava"},
                {"chn09", "Becky"},
                {"chn10", "Plucky"},
                {"chn11", "Knox"},
                {"chn12", "Broffina"},
                {"chn13", "Ken"},
                {"cow00", "Patty"},
                {"cow01", "Tipper"},
                {"cow06", "Norma"},
                {"cow07", "Naomi"},
                {"crd00", "Alfonso"},
                {"crd01", "Alli"},
                {"crd02", "Boots"},
                {"crd04", "Del"},
                {"crd06", "Sly"},
                {"crd07", "Gayle"},
                {"crd08", "Drago"},
                {"der00", "Fauna"},
                {"der01", "Bam"},
                {"der02", "Zell"},
                {"der03", "Bruce"},
                {"der04", "Deirdre"},
                {"der05", "Lopez"},
                {"der06", "Fuchsia"},
                {"der07", "Beau"},
                {"der08", "Diana"},
                {"der09", "Erik"},
                {"der10", "Chelsea"},
                {"dog00", "Goldie"},
                {"dog01", "Butch"},
                {"dog02", "Lucky"},
                {"dog03", "Biskit"},
                {"dog04", "Bones"},
                {"dog05", "Portia"},
                {"dog06", "Walker"},
                {"dog07", "Daisy"},
                {"dog08", "Cookie"},
                {"dog09", "Maddie"},
                {"dog10", "Bea"},
                {"dog14", "Mac"},
                {"dog15", "Marcel"},
                {"dog16", "Benjamin"},
                {"dog17", "Cherry"},
                {"dog18", "Shep"},
                {"duk00", "Bill"},
                {"duk01", "Joey"},
                {"duk02", "Pate"},
                {"duk03", "Maelle"},
                {"duk04", "Deena"},
                {"duk05", "Pompom"},
                {"duk06", "Mallary"},
                {"duk07", "Freckles"},
                {"duk08", "Derwin"},
                {"duk09", "Drake"},
                {"duk10", "Scoot"},
                {"duk11", "Weber"},
                {"duk12", "Miranda"},
                {"duk13", "Ketchup"},
                {"duk15", "Gloria"},
                {"duk16", "Molly"},
                {"duk17", "Quillson"},
                {"elp00", "Opal"},
                {"elp01", "Dizzy"},
                {"elp02", "Big Top"},
                {"elp03", "Eloise"},
                {"elp04", "Margie"},
                {"elp05", "Paolo"},
                {"elp06", "Axel"},
                {"elp07", "Ellie"},
                {"elp09", "Tucker"},
                {"elp10", "Tia"},
                {"elp11", "Chai"},
                {"elp12", "Cyd"},
                {"flg00", "Lily"},
                {"flg01", "Ribbot"},
                {"flg02", "Frobert"},
                {"flg03", "Camofrog"},
                {"flg04", "Drift"},
                {"flg05", "Wart Jr."},
                {"flg06", "Puddles"},
                {"flg07", "Jeremiah"},
                {"flg09", "Tad"},
                {"flg10", "Cousteau"},
                {"flg11", "Huck"},
                {"flg12", "Prince"},
                {"flg13", "Jambette"},
                {"flg15", "Raddle"},
                {"flg16", "Gigi"},
                {"flg17", "Croque"},
                {"flg18", "Diva"},
                {"flg19", "Henry"},
                {"goa00", "Chevre"},
                {"goa01", "Nan"},
                {"goa02", "Billy"},
                {"goa04", "Gruff"},
                {"goa06", "Velma"},
                {"goa07", "Kidd"},
                {"goa08", "Pashmina"},
                {"goa09", "Sherb"},
                {"gor00", "Cesar"},
                {"gor01", "Peewee"},
                {"gor02", "Boone"},
                {"gor04", "Louie"},
                {"gor05", "Boyd"},
                {"gor07", "Violet"},
                {"gor08", "Al"},
                {"gor09", "Rocket"},
                {"gor10", "Hans"},
                {"gor11", "Rilla"},
                {"ham00", "Hamlet"},
                {"ham01", "Apple"},
                {"ham02", "Graham"},
                {"ham03", "Rodney"},
                {"ham04", "Soleil"},
                {"ham05", "Clay"},
                {"ham06", "Flurry"},
                {"ham07", "Hamphrey"},
                {"hip00", "Rocco"},
                {"hip02", "Bubbles"},
                {"hip03", "Bertha"},
                {"hip04", "Biff"},
                {"hip05", "Bitty"},
                {"hip08", "Harry"},
                {"hip09", "Hippeux"},
                {"hrs00", "Buck"},
                {"hrs01", "Victoria"},
                {"hrs02", "Savannah"},
                {"hrs03", "Elmer"},
                {"hrs04", "Roscoe"},
                {"hrs05", "Winnie"},
                {"hrs06", "Ed"},
                {"hrs07", "Cleo"},
                {"hrs08", "Peaches"},
                {"hrs09", "Annalise"},
                {"hrs10", "Clyde"},
                {"hrs11", "Colton"},
                {"hrs12", "Papi"},
                {"hrs13", "Julian"},
                {"hrs16", "Reneigh"},
                {"kal00", "Yuka"},
                {"kal01", "Alice"},
                {"kal02", "Melba"},
                {"kal03", "Sydney"},
                {"kal04", "Gonzo"},
                {"kal05", "Ozzie"},
                {"kal08", "Canberra"},
                {"kal09", "Lyman"},
                {"kal10", "Eugene"},
                {"kgr00", "Kitt"},
                {"kgr01", "Mathilda"},
                {"kgr02", "Carrie"},
                {"kgr05", "Astrid"},
                {"kgr06", "Sylvia"},
                {"kgr08", "Walt"},
                {"kgr09", "Rooney"},
                {"kgr10", "Marcie"},
                {"lon00", "Bud"},
                {"lon01", "Elvis"},
                {"lon02", "Rex"},
                {"lon04", "Leopold"},
                {"lon06", "Mott"},
                {"lon07", "Rory"},
                {"lon08", "Lionel"},
                {"mnk01", "Nana"},
                {"mnk02", "Simon"},
                {"mnk03", "Tammi"},
                {"mnk04", "Monty"},
                {"mnk05", "Elise"},
                {"mnk06", "Flip"},
                {"mnk07", "Shari"},
                {"mnk08", "Deli"},
                {"mus00", "Dora"},
                {"mus01", "Limberg"},
                {"mus02", "Bella"},
                {"mus03", "Bree"},
                {"mus04", "Samson"},
                {"mus05", "Rod"},
                {"mus08", "Candi"},
                {"mus09", "Rizzo"},
                {"mus10", "Anicotti"},
                {"mus12", "Broccolo"},
                {"mus14", "Moose"},
                {"mus15", "Bettina"},
                {"mus16", "Greta"},
                {"mus17", "Penelope"},
                {"mus18", "Chadder"},
                {"ocp00", "Octavian"},
                {"ocp01", "Marina"},
                {"ocp02", "Zucker"},
                {"ost00", "Queenie"},
                {"ost01", "Gladys"},
                {"ost02", "Sandy"},
                {"ost03", "Sprocket"},
                {"ost05", "Julia"},
                {"ost06", "Cranston"},
                {"ost07", "Phil"},
                {"ost08", "Blanche"},
                {"ost09", "Flora"},
                {"ost10", "Phoebe"},
                {"pbr00", "Apollo"},
                {"pbr01", "Amelia"},
                {"pbr02", "Pierce"},
                {"pbr03", "Buzz"},
                {"pbr05", "Avery"},
                {"pbr06", "Frank"},
                {"pbr07", "Sterling"},
                {"pbr08", "Keaton"},
                {"pbr09", "Celia"},
                {"pgn00", "Aurora"},
                {"pgn01", "Roald"},
                {"pgn02", "Cube"},
                {"pgn03", "Hopper"},
                {"pgn04", "Friga"},
                {"pgn05", "Gwen"},
                {"pgn06", "Puck"},
                {"pgn09", "Wade"},
                {"pgn10", "Boomer"},
                {"pgn11", "Iggly"},
                {"pgn12", "Tex"},
                {"pgn13", "Flo"},
                {"pgn14", "Sprinkle"},
                {"pig00", "Curly"},
                {"pig01", "Truffles"},
                {"pig02", "Rasher"},
                {"pig03", "Hugh"},
                {"pig04", "Lucy"},
                {"pig05", "Spork"},
                {"pig08", "Cobb"},
                {"pig09", "Boris"},
                {"pig10", "Maggie"},
                {"pig11", "Peggy"},
                {"pig13", "Gala"},
                {"pig14", "Chops"},
                {"pig15", "Kevin"},
                {"pig16", "Pancetti"},
                {"pig17", "Agnes"},
                {"rbt00", "Bunnie"},
                {"rbt01", "Dotty"},
                {"rbt02", "Coco"},
                {"rbt03", "Snake"},
                {"rbt04", "Gaston"},
                {"rbt05", "Gabi"},
                {"rbt06", "Pippy"},
                {"rbt07", "Tiffany"},
                {"rbt08", "Genji"},
                {"rbt09", "Ruby"},
                {"rbt10", "Doc"},
                {"rbt11", "Claude"},
                {"rbt12", "Francine"},
                {"rbt13", "Chrissy"},
                {"rbt14", "Hopkins"},
                {"rbt15", "O'Hare"},
                {"rbt16", "Carmen"},
                {"rbt17", "Bonbon"},
                {"rbt18", "Cole"},
                {"rbt19", "Mira"},
                {"rbt20", "Toby"},
                {"rhn00", "Tank"},
                {"rhn01", "Rhonda"},
                {"rhn02", "Spike"},
                {"rhn04", "Hornsby"},
                {"rhn07", "Merengue"},
                {"rhn08", "Renée"},
                {"shp00", "Vesta"},
                {"shp01", "Baabara"},
                {"shp02", "Eunice"},
                {"shp03", "Stella"},
                {"shp04", "Cashmere"},
                {"shp07", "Willow"},
                {"shp08", "Curlos"},
                {"shp09", "Wendy"},
                {"shp10", "Timbra"},
                {"shp11", "Frita"},
                {"shp12", "Muffy"},
                {"shp13", "Pietro"},
                {"shp14", "Étoile"},
                {"shp15", "Dom"},
                {"squ00", "Peanut"},
                {"squ01", "Blaire"},
                {"squ02", "Filbert"},
                {"squ03", "Pecan"},
                {"squ04", "Nibbles"},
                {"squ05", "Agent S"},
                {"squ06", "Caroline"},
                {"squ07", "Sally"},
                {"squ08", "Static"},
                {"squ09", "Mint"},
                {"squ10", "Ricky"},
                {"squ11", "Cally"},
                {"squ13", "Tasha"},
                {"squ14", "Sylvana"},
                {"squ15", "Poppy"},
                {"squ16", "Sheldon"},
                {"squ17", "Marshal"},
                {"squ18", "Hazel"},
                {"tig00", "Rolf"},
                {"tig01", "Rowan"},
                {"tig02", "Tybalt"},
                {"tig03", "Bangle"},
                {"tig04", "Leonardo"},
                {"tig05", "Claudia"},
                {"tig06", "Bianca"},
                {"wol00", "Chief"},
                {"wol01", "Lobo"},
                {"wol02", "Wolfgang"},
                {"wol03", "Whitney"},
                {"wol04", "Dobie"},
                {"wol05", "Freya"},
                {"wol06", "Fang"},
                {"wol08", "Vivian"},
                {"wol09", "Skye"},
                {"wol10", "Kyle"},
                {"wol12", "Audie"},
                {"non00", "Empty" }
            };
    }
}
