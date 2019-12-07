using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SHAsher
{
    public partial class MainView : Form
    {
        private List<byte> bytes = new List<byte>();
        long l = 0;
        int N = 0;

        public MainView()
        {
            InitializeComponent();
        }

        private void convertToBytes(string input)
        {
            // Converting input to Hex
            byte[] byteArr = Encoding.Default.GetBytes(input);
            string hexString = BitConverter.ToString(byteArr);
            hexString = hexString.Replace("-", "");
            hexString = hexString.ToLower();

            // Saving hex input to bytes
            for (int i = 0, j = 0; i < hexString.Length / 2; i++, j += 2)
            {
                byte b = (byte)int.Parse(hexString.Substring(j, 2), System.Globalization.NumberStyles.HexNumber);
                bytes.Add(b);
                l += 8;
            }
        }

        private void padMessage()
        {
            // Calculating k value
            int k = 0;
            while ((k + 1 + l) % 512 != 448)
                k++;

            // Padding the message

            // Append single one and 7 zeroes as 128 byte (1000 0000)
            bytes.Add((byte)0x80);
            k -= 7;

            // Append zeroes rest of zeroes as bytes (0000 0000)
            for (int i = 0; i < k / 8; i++)
                bytes.Add((byte)0x0);

            // Append 64 bit equal to message length (l as 64-bit)
            for (int i = 1; i <= 8; i++)
                bytes.Add((byte)(l >> (64 - i * 8)));

            // Count on how many 512-bit blocks input will split
            N = bytes.Count / 64;
        }

        private void hashBtn_Click(object sender, EventArgs e)
        {
            bytes.Clear();
            N = 0;
            l = 0;

            convertToBytes(inputTxt.Text);
            padMessage();
            
            // Prepare message to be send to DLL
            byte[] bytesArr = new byte[bytes.Count];
            for (int i=0; i<bytes.Count; i++)
            {
                bytesArr[i] = bytes[i];
            }

            // Create buffer in which C++ will store result
            byte[] hashedValue = new byte[64];

            // Calculate hash values
            hash(bytesArr, N, hashedValue);

            // Read hash values
            string hashValue = Encoding.UTF8.GetString(hashedValue);

            // Send output to text box
            outputTxt.Text = hashValue;
        }

        [DllImport("HighLevelHashLibrary.dll")]
        public static extern void hash(byte[] bytes, int N, byte[] buf);
    }
}
