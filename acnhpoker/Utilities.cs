using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace acnhpoker
{
    class Utilities
    {
        public static UInt32 masterAddress = 0xABA526A8; //0xAC4723D;

        public UInt32 ItemSlotBase = masterAddress; // 0xAC4723D0
        public UInt32 ItemSlot21Base = masterAddress - 0xB8; // 0xAC472318



        public static readonly UInt32 MasterRecyclingBase = masterAddress - 0xB09908; // 0xAB968B78
        public static readonly UInt32 MasterRecycling21Base = MasterRecyclingBase + 0xA0; // 0xAB968C18

        public static readonly UInt32 TurnipPurchasePriceAddr = masterAddress - 0x11C19F8;//0x11C1898; // 0xAB2B0B38;
        public static readonly UInt32 TurnipSellPriceAddr = TurnipPurchasePriceAddr + 0xC; // 0xAB2B0B44

        public static readonly UInt32 TownNameddress = masterAddress - 0x92D9C8; //AB124CE0 //masterAddress - 0x13FC608;// 0xAB075DC8; 0x2AA1A1A4

        public static readonly UInt32 staminaAddress = masterAddress + 0x859EC30; //B3FF12D8; 0x859EDB0; // 0xB4A11180;

        public static readonly UInt32 weatherSeed = masterAddress - 0x13FC980; // AA655D28; 0x13FC820; //0xAB075BB0;



        public static readonly UInt32 player1SlotBase = masterAddress;
        public static readonly UInt32 player1Slot21Base = player1SlotBase - 0xB8;
        public static readonly UInt32 reactionAddress = player1SlotBase + 0xAEF4; // 0xAC47D2C4;
        public static readonly UInt32 MasterHouseBase = player1SlotBase + 0xC4; // 0xAC472494;
        public static readonly UInt32 MasterHouse21Base = MasterHouseBase + 0xA0; // 0xAC472534‬

        public static readonly UInt32 player2SlotBase = player1SlotBase + 0x6D6D0;//player1SlotBase + 0x6D6C0; // 0xAC4DFA90
        public static readonly UInt32 player2Slot21Base = player2SlotBase - 0xB8;

        public static readonly UInt32 player3SlotBase = player2SlotBase + 0x6D6D0;//player2SlotBase + 0x6D6C0; // 0xAC54D150
        public static readonly UInt32 player3Slot21Base = player3SlotBase - 0xB8;

        public static readonly UInt32 player4SlotBase = player3SlotBase + 0x6D6D0;//player3SlotBase + 0x6D6C0; // 0xAC5BA810
        public static readonly UInt32 player4Slot21Base = player4SlotBase - 0xB8;



        public const UInt32 InsectAppearPointer = 0x466293F8;
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

        public static readonly UInt32 freezeTimeAddress = 0x0024CF30;
        public static readonly UInt32 readTimeAddress = 0x0B973978;

        public static readonly UInt32 wSpeedAddress = 0x00FB725C;
        public static readonly UInt32 CollisionAddress = 0x00F39150;
        public static readonly UInt32 aSpeedAddress = 0x034A7EC0;

        //static readonly string[] peekType = new string[3] { "peekMain", "peek", "peekAbsolute" };

        public Utilities()
        {
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public string GetItemSlotAddress(int slot)
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

        public uint GetItemSlotUIntAddress(int slot)
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

        public string GetItemCountAddress(int slot)
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

        public uint GetItemCountUIntAddress(int slot)
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

        public string GetItemFlag1Address(int slot)
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

        public byte[] transform(byte[] data)
        {
            string bank = "";
            for (int i = 0; i < data.Length; i++)
            {
                bank += precedingZeros(((UInt16)data[i]).ToString("X"), 2);
            }
            //Debug.Print(bank);

            return Encoding.ASCII.GetBytes(bank);
        }

        public byte[] stringToByte(string Bank)
        {
            byte[] save = new byte[Bank.Length / 2];

            for (int i = 0; i < Bank.Length / 2; i++)
            {

                string data = String.Concat(Bank[(i * 2)].ToString(), Bank[((i * 2) + 1)].ToString());
                //Debug.Print(i.ToString() + " " + data);
                save[i] = Convert.ToByte(data, 16);
            }

            return save;
        }

        public byte[] GetInventoryBank(Socket socket, USBBot bot, int slot)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("peek {0:X8} {1}\r\n", GetItemSlotAddress(slot), 160);
                    Debug.Print(msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
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
                    byte[] b = new byte[1000];
                    socket.Receive(b);

                    return b;
                }
                else
                {
                    byte[] b = transform(bot.ReadBytes(GetItemSlotUIntAddress(slot), 160));

                    return b;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.\nIf you are using USB connection, try restart your switch as well.");
                return null;
            }
        }

        public bool SpawnItem(Socket socket, USBBot bot, int slot, String value, String amount)
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

                Debug.Print("Slot : " + slot + " | ID : " + value + " | Amount : " + amount);
                Debug.Print("Spawn Item : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(amount, 8)));
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public bool SpawnRecipe(Socket socket, USBBot bot, int slot, String value, String recipeValue)
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

                Debug.Print("Slot : " + slot + " | ID : " + value + " | RecipeValue : " + recipeValue);
                Debug.Print("Spawn recipe : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(recipeValue, 8)));
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public bool SpawnFlower(Socket socket, USBBot bot, int slot, String value, String flowerValue)
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

                Debug.Print("Slot : " + slot + " | ID : " + value + " | FlowerValue : " + flowerValue);
                Debug.Print("Spawn Flower : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(flowerValue, 8)));
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public string flip(string value)
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

        public string precedingZeros(string value, int length)
        {
            string n0 = String.Concat(Enumerable.Repeat("0", length - value.Length));
            string result = String.Concat(n0, value);
            return result;
        }

        public string turn2bytes(string value)
        {
            if (value.Length < 4)
                return precedingZeros(value, 4);
            else
                return value.Substring(value.Length - 4, 4);
        }

        public void DeleteSlot(Socket s, USBBot bot, int slot)
        {
            SpawnItem(s, bot, slot, "FFFE", "0");
        }

        public void OverwriteAll(Socket socket, USBBot bot, byte[] buffer1, byte[] buffer2)
        {
            if (bot == null)
            {
                SendByteArray(socket, GetItemSlotUIntAddress(1), buffer1, 160);
                SendByteArray(socket, GetItemSlotUIntAddress(21), buffer2, 160);
            }
            else
            {
                bot.WriteBytes(buffer1, GetItemSlotUIntAddress(1));
                bot.WriteBytes(buffer2, GetItemSlotUIntAddress(21));
            }
        }

        public UInt32[] GetTurnipPrices(Socket socket, USBBot bot)
        {
            UInt32[] result = new UInt32[13];
            if (bot == null)
            {
                ReadUInt32Array(socket, TurnipPurchasePriceAddr, result, 4, 12);
                ReadUInt32Array(socket, TurnipSellPriceAddr, result, 4 * 12, 0);
            }
            else
            {
                byte[] data = transform(bot.ReadBytes(TurnipPurchasePriceAddr, 100));
                string bank = Encoding.ASCII.GetString(data);

                result[12] = Convert.ToUInt32(flip(bank.Substring(0, 8)), 16);

                for (int i = 0; i < 12; i++)
                {
                    result[i] = Convert.ToUInt32(flip(bank.Substring(24 + (8 * i), 8)), 16);
                }
            }
            return result;
        }

        public bool ChangeTurnipPrices(Socket socket, USBBot bot, UInt32[] prices)
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

        public void setAddress(int player)
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
            else if (player == 9) //Recycling
            {
                ItemSlotBase = MasterRecyclingBase;
                ItemSlot21Base = MasterRecycling21Base;
            }
            else if (player == 10) //House 1
            {
                ItemSlotBase = MasterHouseBase;
                ItemSlot21Base = MasterHouse21Base;
            }

        }

        public void gotoRecyclingPage(uint page)
        {
            ItemSlotBase = MasterRecyclingBase + ((page - 1) * 0x140);
            ItemSlot21Base = MasterRecycling21Base + ((page - 1) * 0x140);
        }

        public void gotoHousePage(uint page)
        {
            ItemSlotBase = MasterHouseBase + ((page - 1) * 0x140);
            ItemSlot21Base = MasterHouse21Base + ((page - 1) * 0x140);
        }

        public byte[] peekAddress(Socket socket, USBBot bot, string address, int size)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("peek {0:X8} {1}\r\n", address, size);
                    Debug.Print("Peek : " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    byte[] b = new byte[330];
                    socket.Receive(b);

                    return b;
                }
                else
                {
                    byte[] b = transform(bot.ReadBytes(Convert.ToUInt32(address, 16), size));

                    return b;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public void pokeAddress(Socket socket, USBBot bot, string address, string value)
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

        public byte[] peekMainAddress(Socket socket, USBBot bot, string address, int size)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("peekMain 0x{0:X8} {1}\r\n", address, size);
                    Debug.Print("PeekMain : " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    byte[] b = new byte[330];
                    socket.Receive(b);

                    return b;
                }
                else
                {
                    byte[] b = transform(bot.ReadBytes(Convert.ToUInt32(address, 16), size));

                    return b;
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public void pokeMainAddress(Socket socket, USBBot bot, string address, string value)
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


        public void setStamina(Socket socket, USBBot bot, string value)
        {
            pokeAddress(socket, bot, "0x" + staminaAddress.ToString("X"), value);
        }

        public void setFlag1(Socket socket, USBBot bot, int slot, string flag)
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
            }

            return null;
        }
        public static bool SendByteArray(Socket socket, long initAddr, byte[] buffer, int size)
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
                    Debug.Print(msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                    sent += bytesToSend;
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
                return ReadByteArray(socket, TownNameddress, 0x1C);
            }
            else
            {
                return bot.ReadBytes(TownNameddress, 0x1C);
            }
        }

        public static byte[] GetWeatherSeed(Socket socket, USBBot bot)
        {
            if (bot == null)
            {
                return ReadByteArray(socket, weatherSeed, 0x4);
            }
            else
            {
                return bot.ReadBytes(weatherSeed, 0x4);
            }
        }

        public byte[] getReaction(Socket socket, USBBot bot)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("peek 0x{0:X8} {1}\r\n", reactionAddress.ToString("X"), 8);
                    //Debug.Print("Peek Reaction : " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    byte[] b = new byte[4096];
                    socket.Receive(b);

                    return b;
                }
                else
                {
                    byte[] b = transform(bot.ReadBytes(reactionAddress, 8));

                    return b;
                }

            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public void setReaction(Socket socket, USBBot bot, string reaction1, string reaction2)
        {
            try
            {
                if (bot == null)
                {
                    string msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", reactionAddress.ToString("x"), reaction1);
                    Debug.Print("Poke Reaction: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));

                    msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", (reactionAddress + 4).ToString("x"), reaction2);
                    Debug.Print("Poke Reaction: " + msg);
                    SendString(socket, Encoding.UTF8.GetBytes(msg));
                }
                else
                {
                    bot.WriteBytes(stringToByte(reaction1), reactionAddress);

                    bot.WriteBytes(stringToByte(reaction2), reactionAddress + 4);
                }
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }

        public static void SendSpawnRate(Socket socket, USBBot bot, byte[] buffer, int index, int type)
        {
            if (bot == null)
            {
                if (type == 0)
                {
                    SendByteArray(socket, InsectAppearPointer + InsectDataSize * index + 0x2, buffer, 12 * 6 * 2);
                }
                else if (type == 1)
                {
                    SendByteArray(socket, FishRiverAppearPointer + FishDataSize * index + 0x2, buffer, 12 * 3 * 2);
                }
                else if (type == 2)
                {
                    SendByteArray(socket, FishSeaAppearPointer + FishDataSize * index + 0x2, buffer, 12 * 3 * 2);
                }
                else if (type == 3)
                {
                    SendByteArray(socket, CreatureSeaAppearPointer + SeaCreatureDataSize * index + 0x2, buffer, 12 * 3 * 2);
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
                    return ReadByteArray(socket, InsectAppearPointer, InsectDataSize * InsectNumRecords);
                }
                else if (mode == 1)
                {
                    return ReadByteArray(socket, FishRiverAppearPointer, FishDataSize * FishRiverNumRecords);
                }
                else if (mode == 2)
                {
                    return ReadByteArray(socket, FishSeaAppearPointer, FishDataSize * FishSeaNumRecords);
                }
                else if (mode == 3)
                {
                    return ReadByteArray(socket, CreatureSeaAppearPointer, SeaCreatureDataSize * SeaCreatureNumRecords);
                }
                return null;
            }
            else
            {
                if (mode == 0)
                {
                    return ReadLargeBytes(bot, InsectAppearPointer, InsectDataSize * InsectNumRecords);
                }
                else if (mode == 1)
                {
                    return ReadLargeBytes(bot, FishRiverAppearPointer, FishDataSize * FishRiverNumRecords);
                }
                else if (mode == 2)
                {
                    return ReadLargeBytes(bot, FishSeaAppearPointer, FishDataSize * FishSeaNumRecords);
                }
                else if (mode == 3)
                {
                    return ReadLargeBytes(bot, CreatureSeaAppearPointer, SeaCreatureDataSize * SeaCreatureNumRecords);
                }
                return null;
            }
        }

        private static byte[] ReadLargeBytes(USBBot bot, uint address, int size)
        {
            // Read in small chunks
            byte[] result = new byte[size];
            const int maxBytesToReceive = 156;
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
    }
}
