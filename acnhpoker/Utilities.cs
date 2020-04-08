using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace acnhpoker
{
    class Utilities
    {
        UInt32 ItemSlotBase = 0xAC3B90C0;

        public Utilities()
        {
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public string GetItemSlotAddress(int slot)
        {
            
            return "0x" + (ItemSlotBase + (( Clamp(slot, 1, 20) - 1 ) * 0x8)).ToString("X");
        }

        public bool SpawnItem(Socket socket, int slot, String value, int amount)
        {
            try
            {
                byte[] msg = Encoding.UTF8.GetBytes("poke " + GetItemSlotAddress(slot) + " " + FormatItemId(value) + "\r\n");
                Debug.Print("poke " + GetItemSlotAddress(slot) + " " + FormatItemId(value));
                socket.Send(msg);
            }
            catch
            {
                MessageBox.Show("Exception, try restarting the program or reconnecting to the switch.");
            }

            return false;
        }

        public string FormatItemId(String itemId)
        {
            string n0 = String.Concat(Enumerable.Repeat("0", 4 - itemId.Length));
            string preFlip = String.Concat(n0, itemId);
            string firstHalf = preFlip.Substring(0, 2);
            string secondHalf = preFlip.Substring(2, 2);
            string postFlip = "0x" + secondHalf + firstHalf;
            //probaby a better way to do this lol

            return postFlip;
        }

    }
}
