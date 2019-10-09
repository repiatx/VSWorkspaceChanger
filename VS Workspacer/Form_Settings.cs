using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VS_Workspacer
{
    public partial class Form_Settings : Form
    {
        MainForm mf;
        public Form_Settings(MainForm _mf)
        {
            InitializeComponent();
            mf = _mf;
        }

        private void Form_Settings_Load(object sender, EventArgs e)
        {

            textBox1.Text = mf.extensionsPath;
            textBox2.Text = mf.disabledExtensionsPath;

            
            button1.Click += (s, d) => { showFolderDialog(textBox1);};
            button2.Click += (s, d) => { showFolderDialog(textBox2); };

        }

     

        void showFolderDialog(TextBox tb)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.SelectedPath = tb.Text;
            var result = fd.ShowDialog();
            if (result == DialogResult.OK)
            {
                if(Directory.Exists(fd.SelectedPath))
                    tb.Text = fd.SelectedPath;
            }

            fd.Dispose();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                addTrailing(textBox1);
                addTrailing(textBox2);
                if (Directory.Exists(textBox1.Text) && Directory.Exists(textBox2.Text))
                {
                    mf.changeExtensionsPath(textBox1.Text);
                    mf.changeDisabledExtensionsPath(textBox2.Text);
                    mf.LoadWorkspaces();
                    mf.LoadExtension();
                }

                this.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
           

        }

        private void Button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void addTrailing(TextBox tb)
        {
            string lastDigit = tb.Text.Substring(tb.Text.Length - 1, 1);
            string lastTwoDigit = tb.Text.Substring(tb.Text.Length - 2, 2);
            if ( lastTwoDigit== @"\\")
                tb.Text = tb.Text.Remove(tb.Text.Length - 2, 1);
            
            if (lastDigit!= @"\")
            {
                tb.Text += @"\";
            }
            
        }
    }
}
