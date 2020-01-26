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
        private List<byte> bytes;
        private List<List<UInt32>> M;
        private List<List<List<UInt32>>> listOfM;
        private List<UInt32[]> listOfMArr;
        private List<int> N;
        int job = 0;
        private List<byte[]> outputCpp;
        private List<UInt32[]> outputAsm;
        private static Semaphore semaphore = new Semaphore(1, 1);
        private static Semaphore jobEnded;
        long l;
        String title = "ERROR!";
        MessageBoxButtons btn = MessageBoxButtons.OK;
        MessageBoxIcon icon = MessageBoxIcon.Error;
        int inputCounter;
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
                bytes.Add((byte)0x00);

            // Append 64 bit equal to message length (l as 64-bit)
            for (int i = 1; i <= 8; i++)
                bytes.Add((byte)(l >> (64 - i * 8)));

            // Count on how many 512-bit blocks input will split
            N.Add(bytes.Count / 64);
        }

        private void splitMessage(int it)
        {
            // Split padded message into N 512-bit blocks
            for (int i = 0, n = 0; n < N[it]; n++)
            {
                // saving blocks as sixteen 32-bit words
                List<UInt32> block = new List<UInt32>(16);
                for (int j = 0; j < 16; j++)
                {
                    UInt32 word = 0;
                    for (int k = 0; k < 4; k++, i++)
                    {
                        word <<= 8;
                        word |= bytes[i];
                    }

                    // saving word to block
                    block.Add(word);
                }

                // saving message block
                M.Add(block);
            }
        }

        private void hashBtn_Click(object sender, EventArgs e)
        {
            // Preparing variables
            listOfMArr = new List<uint[]>();
            listOfM = new List<List<List<UInt32>>>();
            List<String> input = new List<string>();
            StreamReader inFile = null;
            StreamWriter outFile = null;
            bool doRest = true;
            String message;
            inputCounter = 0;
            outputCpp = new List<byte[]>();
            outputAsm = new List<UInt32[]>();
            bytes = new List<byte>();
            N = new List<int>();

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
            if (doRest)
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
            try
            {
                if (doRest)
                {
                    job++;
                    statusLabel.Text = "Status: Preparing input";

                    // Reading the input
                    String line;
                    while ((line = inFile.ReadLine()) != null)
                    {
                        input.Add(line);
                    }

                    inputCounter = input.Count();
                    inFile.Close();
                    N.Clear();
                    int it = 0;

                    // Preparing each input line to be hashed
                    foreach (String inline in input)
                    {
                        bytes = new List<byte>();
                        M = new List<List<UInt32>>();
                        l = 0;

                        convertToBytes(inline);
                        padMessage();
                        splitMessage(it);

                        it++;
                        listOfM.Add(M);
                    }

                    input.Clear();

                    for (int i = 0; i < listOfM.Count(); i++)
                    {
                        listOfMArr.Add(new UInt32[listOfM[i].Count() * listOfM[0][0].Count()]);
                        for (int j = 0; j < listOfM[i].Count(); j++)
                            for (int k = 0; k < listOfM[i][j].Count(); k++)
                                listOfMArr[i][(j * listOfM[i].Count()) + k] = listOfM[i][j][k];
                    }

                    // Prepare ThreadPool
                    ThreadPool.SetMinThreads((int)threadCounter.Value, 0);
                    ThreadPool.SetMaxThreads((int)threadCounter.Value, 0);

                    bool isCpp = false;

                    if (dllChooseCB.Text == "C++")
                        isCpp = true;

                    // Create/Prepare output list
                    if (isCpp)
                        for (int i = 0; i < inputCounter; i++)
                            outputCpp.Add(new byte[64]);
                    else
                        for (int i = 0; i < inputCounter; i++)
                            outputAsm.Add(new UInt32[8]);

                    statusLabel.Text = "Status: Hashing...";

                    stopWatch.Restart();

                    jobEnded = new Semaphore(0, inputCounter);

                    // Start threads
                    stopWatch.Start();
                    doThreads(isCpp);

                    try
                    {
                        foreach (UInt32[] _output in listOfMArr)
                            jobEnded.WaitOne();
                    }
                    finally
                    {
                        stopWatch.Stop();
                    }

                    // Output
                    if (isCpp)
                        foreach (byte[] arr in outputCpp)
                            outFile.WriteLine(Encoding.Default.GetString(arr));
                    else
                    {
                        string hexString;
                        foreach (UInt32[] arr in outputAsm)
                        {
                            hexString = "";
                            foreach (UInt32 sign in arr)
                            {
                                string hexSubString = Convert.ToString(sign, 16);
                                for(int i=0;i<8-hexSubString.Length;i++)
                                    hexString += "0";
                                hexString += hexSubString;
                            }
                            outFile.WriteLine(hexString);
                        }
                    }

                    listOfMArr.Clear();
                    listOfM.Clear();
                    input.Clear();
                    outputCpp.Clear();
                    outputAsm.Clear();
                    bytes.Clear();
                    N.Clear();

                    outFile.WriteLine("Number of threads: " + threadCounter.Value.ToString());
                    outFile.WriteLine("Time taken [s]: " + stopWatch.Elapsed.TotalSeconds.ToString());

                    outFile.Close();

                    statusLabel.Text = "Status: Done & ready! [" + job.ToString() + "]";
                }
            }
            catch (Exception exc)
            {
                message = exc.ToString();
                _ = MessageBox.Show(message, title, btn, icon);
            }
        }

        void doThreads(bool isCpp)
        {
            if (isCpp)
            {
                foreach (byte[] _output in outputCpp)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(threadJobCpp));
                }
            }
            else
            {
                foreach (UInt32[] _output in outputAsm)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(threadJobASM));
                }
            }
        }

        private void threadJobCpp(object callback)
        {
            try
            {
                // Get your string id
                int iteration = 0;
                try
                {
                    semaphore.WaitOne();
                    iteration = inputCounter;
                    inputCounter--;
                }
                finally
                {
                    semaphore.Release();
                }
                iteration--;

                if (iteration > -1)
                {
                    hashCpp(listOfMArr[iteration], N[iteration], outputCpp[iteration]);

                    jobEnded.Release();
                }
            }
            catch (Exception exc)
            {
                _ = MessageBox.Show(exc.ToString(), title, btn, icon);
            }
        }

        private void threadJobASM(object callback)
        {
            try
            {
                // Get your string id
                int iteration = 0;
                try
                {
                    semaphore.WaitOne();
                    iteration = inputCounter;
                    inputCounter--;
                }
                finally
                {
                    semaphore.Release();
                }
                iteration--;

                if (iteration > -1)
                {
                    hashAsm(listOfMArr[iteration], N[iteration], outputAsm[iteration]);

                    jobEnded.Release();
                }
            }
            catch (Exception exc)
            {
                _ = MessageBox.Show(exc.ToString(), title, btn, icon);
            }
        }

        [DllImport("HashingLibraries.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hashCpp(UInt32[] bytes, int N, byte[] buf);

        [DllImport("HashingLibraries.dll")]
        public static extern void hashAsm(UInt32[] bytes, int N, UInt32[] buf);

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
