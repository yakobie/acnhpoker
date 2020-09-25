using System;
using System.Threading;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class Form1 : Form
    {
        private void PokeBtn_Click(object sender, EventArgs e)
        {
            Utilities.pokeAddress(s, bot, "0x" + debugAddress.Text, debugAmount.Text);
        }
        private void PeekBtn_Click(object sender, EventArgs e)
        {
            byte[] AddressBank = Utilities.peekAddress(s, bot, Convert.ToInt64(debugAddress.Text, 16), 160);

            byte[] firstBytes = new byte[4];
            byte[] secondBytes = new byte[4];
            byte[] thirdBytes = new byte[4];
            byte[] fourthBytes = new byte[4];
            byte[] FullBytes = new byte[16];

            Buffer.BlockCopy(AddressBank, 0x0, firstBytes, 0x0, 0x4);
            Buffer.BlockCopy(AddressBank, 0x4, secondBytes, 0x0, 0x4);
            Buffer.BlockCopy(AddressBank, 0x8, thirdBytes, 0x0, 0x4);
            Buffer.BlockCopy(AddressBank, 0x16, fourthBytes, 0x0, 0x4);
            Buffer.BlockCopy(AddressBank, 0x0, FullBytes, 0x0, 0x16);

            string firstResult = Utilities.ByteToHexString(firstBytes);
            string secondResult = Utilities.ByteToHexString(secondBytes);
            string thirdResult = Utilities.ByteToHexString(thirdBytes);
            string fourthResult = Utilities.ByteToHexString(fourthBytes);
            string FullResult = Utilities.ByteToHexString(FullBytes);

            Result1.Text = Utilities.flip(firstResult);
            Result2.Text = Utilities.flip(secondResult);
            Result3.Text = Utilities.flip(thirdResult);
            Result4.Text = Utilities.flip(fourthResult);
            FullAddress.Text = FullResult;
        }

        private void PeekMainBtn_Click(object sender, EventArgs e)
        {

        }

        private void PokeMainBtn_Click(object sender, EventArgs e)
        {

        }

        UInt32 loopID = 0x00002200;
        UInt32 loopData = 0x00000000;

        private void debugBtn_Click(object sender, EventArgs e)
        {
            //showWait();

            byte[] b1 = new byte[160];
            byte[] b2 = new byte[160];
            //byte[] ID = Utilities.stringToByte(Utilities.flip(Utilities.precedingZeros(loopID.ToString("X"), 8)));
            byte[] Data = Utilities.stringToByte(Utilities.flip(Utilities.precedingZeros(loopData.ToString("X"), 8)));

            //Debug.Print(Utilities.precedingZeros(itemID, 8));
            //Debug.Print(Utilities.precedingZeros(itemAmount, 8));

            for (int i = 0; i < b1.Length; i += 8)
            {
                byte[] ID = Utilities.stringToByte(Utilities.flip(Utilities.precedingZeros(loopID.ToString("X"), 8)));
                for (int j = 0; j < 4; j++)
                {
                    b1[i + j] = ID[j];
                    b1[i + j + 4] = Data[j];
                }
                loopID++;
            }

            for (int i = 0; i < b2.Length; i += 8)
            {
                byte[] ID = Utilities.stringToByte(Utilities.flip(Utilities.precedingZeros(loopID.ToString("X"), 8)));
                for (int j = 0; j < 4; j++)
                {
                    b2[i + j] = ID[j];
                    b2[i + j + 4] = Data[j];
                }
                loopID++;
            }

            //string result1 = Encoding.ASCII.GetString(Utilities.transform(b1));
            //string result2 = Encoding.ASCII.GetString(Utilities.transform(b2));
            //Debug.Print(result1 + "\n" + result2);

            Utilities.OverwriteAll(s, bot, b1, b2, ref counter);

            /*
            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                Invoke((MethodInvoker)delegate
                {
                    btn.setup(GetNameFromID(Utilities.turn2bytes(itemID), itemSource), Convert.ToUInt16("0x" + Utilities.turn2bytes(itemID), 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(Utilities.turn2bytes(itemID), itemSource, Convert.ToUInt32("0x" + itemAmount, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                });
            }
            */

            //Thread.Sleep(1000);
            System.Media.SystemSounds.Asterisk.Play();
            //hideWait();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Result1.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.masterAddress).ToString("X");

            Result2.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.TownNameddress).ToString("X");

            Result3.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.reactionAddress).ToString("X");

            Result4.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.weatherSeed).ToString("X");

            Result5.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.MasterRecyclingBase).ToString("X");

            Result6.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.MasterHouseBase).ToString("X");

            Result7.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.InsectAppearPointer).ToString("X");

            Result8.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.FishRiverAppearPointer).ToString("X");

            Result9.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.FishSeaAppearPointer).ToString("X");
        }

        private void ChaseBtn_Click(object sender, EventArgs e)
        {
            UInt32 startAddress = 0xAB000000;
            UInt32 endAddress = 0xAD000000;

            UInt32 diff = ((endAddress - startAddress) / 10);

            //Debug.Print(diff.ToString("X") + " " + (startAddress + diff).ToString("X"));


            //Debug.Print(Encoding.Default.GetString(Utilities.peekAddress(s, "0x" + (startAddress).ToString("X"))));


            //Debug.Print(Encoding.Default.GetString(output));

            //Debug.Print("Searching...");

            Thread SearchThread1 = new Thread(delegate () { SearchAddress(startAddress, startAddress + diff); });
            SearchThread1.Start();

            Thread SearchThread2 = new Thread(delegate () { SearchAddress(startAddress + diff * 1, startAddress + diff * 2); });
            SearchThread2.Start();

            Thread SearchThread3 = new Thread(delegate () { SearchAddress(startAddress + diff * 2, startAddress + diff * 3); });
            SearchThread3.Start();

            Thread SearchThread4 = new Thread(delegate () { SearchAddress(startAddress + diff * 3, startAddress + diff * 4); });
            SearchThread4.Start();

            Thread SearchThread5 = new Thread(delegate () { SearchAddress(startAddress + diff * 4, startAddress + diff * 5); });
            SearchThread5.Start();

            Thread SearchThread6 = new Thread(delegate () { SearchAddress(startAddress + diff * 5, startAddress + diff * 6); });
            SearchThread6.Start();

            Thread SearchThread7 = new Thread(delegate () { SearchAddress(startAddress + diff * 6, startAddress + diff * 7); });
            SearchThread7.Start();

            Thread SearchThread8 = new Thread(delegate () { SearchAddress(startAddress + diff * 7, startAddress + diff * 8); });
            SearchThread8.Start();

            Thread SearchThread9 = new Thread(delegate () { SearchAddress(startAddress + diff * 8, startAddress + diff * 9); });
            SearchThread9.Start();

            Thread SearchThread10 = new Thread(delegate () { SearchAddress(startAddress + diff * 9, endAddress); });
            SearchThread10.Start();
        }

        private void SearchAddress(UInt32 startAddress, UInt32 endAddress)
        {
            /*
            Debug.Print("Thread Start " + startAddress.ToString("X") + " " + endAddress.ToString("X"));

            byte[] result = Encoding.UTF8.GetBytes("400A000001000000C409000001000000");

            BoyerMoore boi = new BoyerMoore(result);

            for (UInt32 i = 0x0; startAddress + i <= endAddress; i += 500)
            {
                if (offsetFound)
                {
                    return;
                }

                byte[] b = Utilities.peekAddress(s, bot, "0x" + (startAddress + i).ToString("X"), 160);
                Debug.Print(Encoding.UTF8.GetString(b));
                int NUM = boi.Search(b);

                if (NUM >= 0)
                {
                    Debug.Print(">> 0x" + (startAddress + i + NUM / 2).ToString("X") + " << DONE : 0x" + (NUM / 2).ToString("X"));
                    offsetFound = true;
                    return;
                }
            }
            */
        }
    }
}
