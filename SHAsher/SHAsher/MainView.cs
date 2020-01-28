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


///////////////////////////////////////////////////////////
/* 
 * Autor: Dominik Wolny
 * Informatyka
 * Semestr: 5
 * Grupa dziekanska: 1-2
 * 
 * Temat: Hashowanie algorytmem SHA-256
 */
///////////////////////////////////////////////////////////

namespace SHAsher
{
    public partial class MainView : Form
    {
        // Creating global variables
        private List<byte> bytes;                                   // List of bytes, used for first part of preparing input
        private List<List<UInt32>> M;                               // Message to be hashed, product of all preparing functions
        private List<List<List<UInt32>>> listOfM;                   // List of messages M
        private List<UInt32[]> listOfMArr;                          // List of messages saved as array, becaused DLL accepts only array adresses as arguments
        private List<int> N;                                        // List of numbers of 512-bit blocs 
        int job = 0;                                                // Number of current job for easier identification in UI
        private List<byte[]> outputCpp;                             // Output of C++ DLL function
        private List<UInt32[]> outputAsm;                           // Output of Assembly DLL function
        private static Semaphore semaphore = new Semaphore(1, 1);   // Semaphore used for restricting threads from accessing and editing global variable
        private static Semaphore jobEnded;                          // Semaphore used for informing GUI whether hashing was completed
        long l;                                                     // Variable representing length of message M in bits
        String title = "ERROR!";                                    // Global title of error popup MessageBox window
        MessageBoxButtons btn = MessageBoxButtons.OK;               // Global button scheme of error popup MessageBox window 
        MessageBoxIcon icon = MessageBoxIcon.Error;                 // Global icon of error popup MessageBox window 
        int inputCounter;                                           // Variable storing amount of input to be hashed, thread use it to identify what to hash
        Stopwatch stopWatch = new Stopwatch();                      // StopWatch used for measuring time taken for hashing

        public MainView()
        {
            InitializeComponent();                                  // Class constructor which displays GUI
        }

        private void convertToBytes(string input)
        {
            // Converting input to Hex and unifying format
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

        // Function used for padding input message (5.1.1 in specification)
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

        // Function for parsing input message (5.2.1 in specification)
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
            // Initializing global variables
            listOfMArr = new List<uint[]>();
            listOfM = new List<List<List<UInt32>>>();
            inputCounter = 0;
            outputCpp = new List<byte[]>();
            outputAsm = new List<UInt32[]>();
            bytes = new List<byte>();
            N = new List<int>();

            // Creating local variables
            List<String> input = new List<string>();    // List of input messages
            StreamReader inFile = null;                 // Object storing input file
            StreamWriter outFile = null;                // Object storing output file
            bool doRest = true;                         // Boolean ensuring that if at some part program crashes it won't be working any further
            String message;                             // Variable storing error messages

            // Ensuring that either C++ or Assembly is chosen in GUI
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

            // If files are opened correctly and either C++ or Assembly was chosen, then preparing and hashing
            try
            {
                if (doRest)
                {
                    // Incrementing job ID
                    job++;

                    // Informing user that Preparing input is in progress
                    statusLabel.Text = "Status: Preparing input";

                    // Reading the input
                    String line;
                    while ((line = inFile.ReadLine()) != null)
                    {
                        input.Add(line);
                    }

                    // Setting global variable storing number of input messages
                    inputCounter = input.Count();

                    // Closing input file as it's no longer needed
                    inFile.Close();

                    // Clearing list of N values to ensure it stores correct values
                    N.Clear();

                    // Local variable used by splitting function
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

                    // Clearing input list as it's no longer needed
                    input.Clear();

                    // Preparing input for sending to hashing functions, as they accept only address to input array
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

                    // Boolean for identification which option (C++/Assembly) was chosen
                    bool isCpp = false;

                    // Setting above boolean
                    if (dllChooseCB.Text == "C++")
                        isCpp = true;

                    // Prepare output list
                    if (isCpp)
                        for (int i = 0; i < inputCounter; i++)
                            outputCpp.Add(new byte[64]);
                    else
                        for (int i = 0; i < inputCounter; i++)
                            outputAsm.Add(new UInt32[8]);

                    // Informing user that Hashing is in progress
                    statusLabel.Text = "Status: Hashing...";

                    // Zeroing stopwatch
                    stopWatch.Restart();

                    // Assigning value to semaphore ensuring that GUI will wait for threads to end their job.
                    jobEnded = new Semaphore(0, inputCounter);

                    // Start threads and time measuring
                    stopWatch.Start();
                    doThreads(isCpp);

                    // Waiting for threads to end their job, than stopping stopwatch
                    try
                    {
                        foreach (UInt32[] _output in listOfMArr)
                            jobEnded.WaitOne();
                    }
                    finally
                    {
                        stopWatch.Stop();
                    }

                    // Writing output
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

                    // Clearing variables to save system memory
                    listOfMArr.Clear();
                    listOfM.Clear();
                    input.Clear();
                    outputCpp.Clear();
                    outputAsm.Clear();
                    bytes.Clear();
                    N.Clear();

                    // Writing some job info into output file (how much time hashing has taken, and how many threads were used)
                    outFile.WriteLine("Number of threads: " + threadCounter.Value.ToString());
                    outFile.WriteLine("Time taken [s]: " + stopWatch.Elapsed.TotalSeconds.ToString());

                    // Closing output file as it's no longer needed
                    outFile.Close();

                    // Informing user that the hashing was done
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
            // Queuing threads
            if (isCpp)
                foreach (byte[] _output in outputCpp)       
                    ThreadPool.QueueUserWorkItem(new WaitCallback(threadJobCpp));   // Queuing C++ threads
            else
                foreach (UInt32[] _output in outputAsm)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(threadJobASM));   // Queuing Assembly threads
        }

        private void threadJobCpp(object callback)
        {
            try
            {
                // Get your string id using semaphore so that only one thread edits global variable and thus there are no errors
                int iteration = 0;
                try
                {
                    semaphore.WaitOne();
                    inputCounter--;     // Decrementing global variable so that next thread does next job. Doing it before assignment because initial value stores number one bigger than last index
                    iteration = inputCounter;
                }
                finally
                {
                    semaphore.Release();
                }

                // Ensuring that string ID is valid
                if (iteration > -1)
                {
                    // Hashing
                    hashCpp(listOfMArr[iteration], N[iteration], outputCpp[iteration]);
                    jobEnded.Release();
                }
            }
            catch (Exception exc)
            {
                // Showing thread execution error
                _ = MessageBox.Show(exc.ToString(), title, btn, icon);
                jobEnded.Release(); // Releasing semaphore so that error won't show up all the time
            }
        }

        private void threadJobASM(object callback)
        {
            try
            {
                // Get your string id using semaphore so that only one thread edits global variable and thus there are no errors
                int iteration = 0;
                try
                {
                    semaphore.WaitOne();
                    inputCounter--;     // Decrementing global variable so that next thread does next job. Doing it before assignment because initial value stores number one bigger than last index
                    iteration = inputCounter;
                }
                finally
                {
                    semaphore.Release();
                }

                // Ensuring that string ID is valid
                if (iteration > -1)
                {
                    // Hashing
                    hashAsm(listOfMArr[iteration], N[iteration], outputAsm[iteration]);

                    jobEnded.Release();
                }
            }
            catch (Exception exc)
            {
                // Showing thread execution error
                _ = MessageBox.Show(exc.ToString(), title, btn, icon);
                jobEnded.Release(); // Releasing semaphore so that error won't show up all the time
            }
        }

        // Importing C++ DLL function 
        [DllImport("HashingLibraries.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hashCpp(UInt32[] bytes, int N, byte[] buf);

        // Importing Assembly DLL function 
        [DllImport("HashingLibraries.dll")]
        public static extern void hashAsm(UInt32[] bytes, int N, UInt32[] buf);

        // Dialog window for choosing output file
        private void OutFileBrowseBtn_Click(object sender, EventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    if (Path.GetExtension(file) != ".txt")
                        _ = MessageBox.Show("Plik musi być rozszerzenia .txt", title, btn, icon);   // Ensuring that only text files are loaded
                    else
                        outFileBrowseTxt.Text = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    outFileBrowseTxt.Text = null;
                    break;
            }
        }

        // Dialog window for choosing input file
        private void InFileBrowseBtn_Click(object sender, EventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    if (Path.GetExtension(file) != ".txt")
                        _ = MessageBox.Show("Plik musi być rozszerzenia .txt", title, btn, icon);   // Ensuring that only text files are loaded
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
