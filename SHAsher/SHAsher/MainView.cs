using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SHAsher
{
    public partial class MainView : Form
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void hashBtn_Click(object sender, EventArgs e)
        {
            //outputTxt.Text = inputTxt.Text;
            List<byte> bytes = new List<byte>();
            List<List<UInt32>> M = new List<List<UInt32>>();
            int N = 0;
            long l = 0;

            // Converting input to Hex
            byte[] byteArr = Encoding.Default.GetBytes(inputTxt.Text);
            string hexString = BitConverter.ToString(byteArr);
            hexString = hexString.Replace("-", "");
            hexString = hexString.ToLower();

            // Saving hex input to bytes
            for (int i = 0, j = 0; i < hexString.Length / 2; i++, j += 2)
            {
                byte b = (byte) int.Parse(hexString.Substring(j, 2), System.Globalization.NumberStyles.HexNumber);
                bytes.Add(b);
                l += 8;
            }

            // Calculating k value
            int k = 0;
            while ((k + 1 + l) % 512 != 448)
                k++;

            // Padding the message

            // append single one and 7 zeroes as 128 byte (1000 0000)
            bytes.Add((byte)0x80);
            k -= 7;

            // append zeroes rest of zeroes as bytes (0000 0000)
            for (int i = 0; i < k / 8; i++)
                bytes.Add((byte)0);

            // append 64 bit equal to message length (l as 64-bit)
            for (int i = 0; i < 9; i++)
                bytes.Add((byte)(l >> (64 - i * 8)));

            // Splitting the message into N 512-bit blocks
            for (int i = 0; N < bytes.Count / 64; N++)
            {
                // saving blocks as sixteen 32-bit words
                List<UInt32> block = new List<UInt32>();
                for (int j = 0; j < 16; j++)
                {
                    UInt32 word = 0;
                    for (k = 0; k < 4; k++, i++)
                    {
                        word <<= 8;
                        word |= bytes[i];
                    }

                    // saving word to block
                    block[j] = word;
                }

                // saving message block
                M.Add(block);
            }

            // Preparing data for output to DLL

            UInt32[] M_Pass = new UInt32[16 * N];
            for (int i=0;i<N;i++)
            {
                for(int j=0;j<16;j++)
                {
                    M_Pass[16 * i + j] = M[i][j];
                }
            }

            // Output to DLL

            //there will be DLL

            outputTxt.Text = hexString;


        }
    }
}
