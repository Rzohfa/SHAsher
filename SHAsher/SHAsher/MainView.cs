using System;
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
            outputTxt.Text = inputTxt.Text;
        }
    }
}
