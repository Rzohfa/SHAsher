using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace SHAsher
{
    public partial class MainView : Form
    {
        private List<byte> bytes = new List<byte>();
        private List<List<byte>> listOfBytes = new List<List<byte>>();
        private List<int> N = new List<int>();
        private List<byte[]> output = new List<byte[]>();
        private static Semaphore semaphore = new Semaphore(1, 1);
        long l = 0;
        String title = "ERROR!";
        MessageBoxButtons btn = MessageBoxButtons.OK;
        MessageBoxIcon icon = MessageBoxIcon.Error;
        int inputLen = 0;
        Stopwatch stopWatch = new Stopwatch();

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
            N.Add(bytes.Count / 64);
        }

        private void hashBtn_Click(object sender, EventArgs e)
        {
            // Preparing variables
            List<String> input = new List<string>();
            StreamReader inFile = null;
            StreamWriter outFile = null;
            bool doRest = true;
            String message;

            if (dllChooseCB.Text == "")
            {
                message = "You haven't chosen the DLL!";
                _ = MessageBox.Show(message, title, btn, icon);
                doRest = false;
            }

            // Trying to open input file
            if (doRest)
            {
                statusLabel.Text = "Opening files...";

                try
                {
                    inFile = new StreamReader(inFileBrowseTxt.Text);
                }
                catch (Exception exc)
                {
                    message = "Error opening input file!";
                    _ = MessageBox.Show(message, title, btn, icon);
                    doRest = false;
                    statusLabel.Text = "Status: Ready";
                }
            }

            // Trying to open output file
            if(doRest)
            {
                try
                {
                    outFile = new StreamWriter(outFileBrowseTxt.Text);
                }
                catch (Exception exc)
                {
                    message = "Error opening output file!";
                    _ = MessageBox.Show(message, title, btn, icon);
                    doRest = false;
                    inFile.Close();
                    statusLabel.Text = "Status: Ready";
                }
            }

            // If files are opened correctly, then preparing and hashing
            if (doRest)
            {
                statusLabel.Text = "Status: Preparing input";


                // Reading the input
                String line;
                int counter = 0;
                while ((line = inFile.ReadLine()) != null)
                {
                    input.Add(line);
                    counter++;
                }

                inFile.Close();
                N.Clear();

                // Preparing each input line to be hashed
                foreach (String inline in input)
                {
                    bytes.Clear();
                    l = 0;

                    convertToBytes(inline);
                    padMessage();

                    listOfBytes.Add(bytes);
                }

                inputLen = listOfBytes.Count();

                // Prepare ThreadPool
                ThreadPool.SetMinThreads((int)threadCounter.Value, 0);
                ThreadPool.SetMaxThreads((int)threadCounter.Value, 0);

                // Create/Prepare output list
                for (int i = 0; i < inputLen; i++)
                {
                    output.Add(new byte[64]);
                }

                statusLabel.Text = "Status: Hashing...";

                stopWatch.Restart();
                // Start threads
                if(dllChooseCB.Text == "C++")
                {
                    stopWatch.Start();
                    doThreads(true);
                    stopWatch.Stop();
                }
                else if(dllChooseCB.Text == "Assembler")
                {
                    stopWatch.Start();
                    doThreads(false);
                    stopWatch.Stop();
                }

                statusLabel.Text = "Status: Writing output...";

                // Output

                outFile.Close();

                statusLabel.Text = "Status: Ready";
            }
        }

        void doThreads(bool isCpp)
        {
            if (isCpp)
            {
                foreach(List<byte> arr in listOfBytes)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(threadJobCpp));
                }
            }
            else
            {
                foreach (List<byte> arr in listOfBytes)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(threadJobASM));
                }
            }
        }

        private void threadJobCpp(object callback)
        {
            // Get your string id
            int iteration;
            try
            {
                semaphore.WaitOne();
                iteration = inputLen;
                inputLen--;
            }
            finally
            {
                semaphore.Release();
            }
            iteration--;

            if (iteration > -1)
            {
                // Prepare message to be send to DLL
                byte[] bytesArr = new byte[bytes.Count];
                for (int i = 0; i < bytes.Count; i++)
                {
                    bytesArr[i] = listOfBytes[iteration][i];
                }

                // Create buffer in which C++ will store result
                byte[] hashedValue = new byte[64];

                // Calculate hash values
                //hash(bytesArr, N[N.Count()], hashedValue);

                // Save hash to output list
                output[iteration] = hashedValue;

                //string hashValue = Encoding.UTF8.GetString(hashedValue);

                // Send output to text box
                //outputTxt.Text = hashValue;
            }
        }

        private void threadJobASM(object callback)
        {

        }

        [DllImport("HighLevelHashLibrary.dll")]
        public static extern void hash(byte[] bytes, int N, byte[] buf);

        private void OutFileBrowseBtn_Click(object sender, EventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    if (Path.GetExtension(file) != ".txt")
                        _ = MessageBox.Show("Plik musi być rozszerzenia .txt", title, btn, icon);
                    else
                        outFileBrowseTxt.Text = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    outFileBrowseTxt.Text = null;
                    break;
            }
        }

        private void InFileBrowseBtn_Click(object sender, EventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    if (Path.GetExtension(file) != ".txt")
                        _ = MessageBox.Show("Plik musi być rozszerzenia .txt", title, btn, icon);
                    else
                        inFileBrowseTxt.Text = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    inFileBrowseTxt.Text = null;
                    break;
            }
        }
    }
}
