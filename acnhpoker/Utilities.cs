using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace acnhpoker
{
    class Utilities
    {
        UInt32 ItemSlotBase = 0xAC4723D0;
        UInt32 ItemSlot21Base = 0xAC472318;

        UInt32 MasterRecyclingBase = 0xAB968B78;
        UInt32 MasterRecycling21Base = 0xAB968C18;

        UInt32 MasterHouseBase = 0xAC472494;
        UInt32 MasterHouse21Base = 0xAC472534;

        public const UInt32 TurnipPurchasePriceAddr = 0xAB2B0B38;
        public const UInt32 TurnipSellPriceAddr = TurnipPurchasePriceAddr + 0xC;

        public const UInt32 TownNameddress = 0xAB075DC8;

        public const UInt32 staminaAddress = 0xB4A11180;

        public const UInt32 reactionAddress = 0xAC47D2C4;

        static readonly string[] peekType = new string[3] { "peekMain", "peek", "peekAbsolute" };

        public Utilities()
        {
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public string GetItemSlotAddress(int slot)
        {
            if(slot <= 20)
            {
                return "0x" + (ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8)).ToString("X");
            } 
            else
            {
                return "0x" + (ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8)).ToString("X");
            }
        }

        public string GetItemCountAddress(int slot)
        {
            if(slot <= 20)
            {
                return "0x" + (ItemSlotBase + ((Clamp(slot, 1, 20) - 1) * 0x8) + 0x4).ToString("X");
            }
            else
            {
                return "0x" + (ItemSlot21Base + ((Clamp(slot, 21, 40) - 21) * 0x8) + 0x4).ToString("X");
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

        public byte[] GetInventoryBank(Socket socket, int slot)
        {
            try
            {
                //byte[] msg = Encoding.UTF8.GetBytes("peek " + GetItemSlotAddress(slot) + " 160\r\n");
                string msg = String.Format("peek {0:X8} {1}\r\n", GetItemSlotAddress(slot), 160);
                //Debug.Print(msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));


                byte[] b = new byte[500];
                socket.Receive(b);

                return b;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public bool SpawnItem(Socket socket, int slot, String value, String amount)
        {
            try
            {
                string msg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemSlotAddress(slot), flip(precedingZeros(value,8)));
                SendString(socket, Encoding.UTF8.GetBytes(msg));

                string countMsg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemCountAddress(slot), flip(precedingZeros(amount,8)));
                SendString(socket, Encoding.UTF8.GetBytes(countMsg));

                Debug.Print("Slot : " + slot + " | ID : " + value + " | Amount : " + amount);
                Debug.Print("Spawn Item : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(amount, 8)));
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public bool SpawnRecipe(Socket socket, int slot, String value, String recipeValue)
        {
            try
            {
                string msg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemSlotAddress(slot), flip(precedingZeros(value,8)));
                SendString(socket, Encoding.UTF8.GetBytes(msg));

                string countMsg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemCountAddress(slot), flip(precedingZeros(recipeValue, 8)));
                SendString(socket, Encoding.UTF8.GetBytes(countMsg));

                Debug.Print("Slot : " + slot + " | ID : " + value + " | RecipeValue : " + recipeValue);
                Debug.Print("Spawn recipe : poke " + GetItemSlotAddress(slot) + " 0x" + flip(precedingZeros(value, 8)) + " 0x" + flip(precedingZeros(recipeValue, 8)));
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public bool SpawnFlower(Socket socket, int slot, String value, String flowerValue)
        {
            try
            {
                string msg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemSlotAddress(slot), flip(precedingZeros(value,8)));
                SendString(socket, Encoding.UTF8.GetBytes(msg));

                string countMsg = String.Format("poke {0:X8} 0x{1}\r\n", GetItemCountAddress(slot), flip(precedingZeros(flowerValue, 8)));
                SendString(socket, Encoding.UTF8.GetBytes(countMsg));

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

        public string removeZeros(string ID)
        {
            return ID.Substring(4, 4);
        }

        public void DeleteSlot(Socket s, int slot)
        {
            SpawnItem(s, slot, "FFFE", "0");
        }

        public static UInt32[] GetTurnipPrices(Socket socket)
        {
            UInt32[] result = new UInt32[13];
            ReadUInt32Array(socket, TurnipPurchasePriceAddr, result, 4, 12);
            ReadUInt32Array(socket, TurnipSellPriceAddr, result, 4 * 12, 0);
            return result;
        }

        public static bool ChangeTurnipPrices(Socket socket, UInt32[] prices)
        {
            SendUInt32Array(socket, TurnipPurchasePriceAddr, prices, 4, 12);
            SendUInt32Array(socket, TurnipSellPriceAddr, prices, 4 * 12);
            return false;
        }

        public void setAddress(int player)
        {
            if (player == 1)
            {
                ItemSlotBase = 0xAC4723D0;
                ItemSlot21Base = 0xAC472318;
            }
            else if (player == 2)
            {
                ItemSlotBase = 0xAC4DFA90;
                ItemSlot21Base = 0xAC4DF9D8;
            }
            else if (player == 3)
            {
                ItemSlotBase = 0xAC54D150;
                ItemSlot21Base = 0xAC54D098;
            }
            else if (player == 4)
            {
                ItemSlotBase = 0xAC5BA810;
                ItemSlot21Base = 0xAC5BA758;
            }
            else if (player == 9) //Recycling
            {
                ItemSlotBase = 0xAB968B78;
                ItemSlot21Base = 0xAB968C18;
            }
            else if (player == 10) //House 1
            {
                ItemSlotBase = 0xAC472494;
                ItemSlot21Base = 0xAC472534;
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

        public byte[] peekAddress(Socket socket, string address)
        {
            try
            {
                string msg = String.Format("peek {0:X8} {1}\r\n", address, 500);
                Debug.Print("Peek : " + msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));

                byte[] b = new byte[4096];
                socket.Receive(b);

                return b;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public void pokeAddress(Socket socket, string address, string value)
        {
            try
            {
                string msg = String.Format("poke {0:X8} {1}\r\n", address, value);
                Debug.Print("Poke : " + msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }


        public void setStamina(Socket socket, string value)
        {
            pokeAddress(socket, "0x" + staminaAddress.ToString("X"), value);
        }

        public void setFlag1(Socket socket, int slot, string flag)
        {
            pokeAddress(socket, GetItemFlag1Address(slot), "0x" + flag);
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

        private static string ReadToIntermediateString(Socket socket, long address, int size)
        {
            try
            {

                string msg = String.Format("peek 0x{0:X8} {1}\r\n", address, size);
                Debug.Print(msg);
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

        public static byte[] GetTownID(Socket socket)
        {
            return ReadByteArray(socket, TownNameddress, 0x1C);
        }

        public static byte[] getReaction(Socket socket)
        {
            try
            {
                string msg = String.Format("peek 0x{0:X8} {1}\r\n", reactionAddress.ToString("x"), 8);
                Debug.Print("Peek Reaction : " + msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));

                byte[] b = new byte[4096];
                socket.Receive(b);

                return b;
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
                return null;
            }
        }

        public static void setReaction(Socket socket, string reaction1, string reaction2)
        {
            try
            {
                string msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", reactionAddress.ToString("x"), reaction1);
                Debug.Print("Poke Reaction: " + msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));

                msg = String.Format("poke 0x{0:X8} 0x{1}\r\n", (reactionAddress + 4).ToString("x"), reaction2);
                Debug.Print("Poke Reaction: " + msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }
        }

    }
}
