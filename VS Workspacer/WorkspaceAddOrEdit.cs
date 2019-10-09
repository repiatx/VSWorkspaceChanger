using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace VS_Workspacer
{
    public partial class WorkspaceAddOrEdit : Form
    {
        MainForm mf;
        bool isEdit = false;
        private int index = -1;
        public WorkspaceAddOrEdit(Object sender)
        {
            InitializeComponent();
            mf = (MainForm)sender;
            listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox_DrawItem);
        }
        public WorkspaceAddOrEdit(Object sender,int WorkspaceIndex)
        {
            InitializeComponent();
            isEdit = true;
            mf = (MainForm)sender;
            index = WorkspaceIndex;
            
            listBox1.DrawItem += new DrawItemEventHandler(this.listBox_DrawItem);
            
            //listBox1.DrawMode = DrawMode.OwnerDrawFixed;

        }
        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            if (((ListBox) sender).Name == "listBox2")
            {
                Debugger.Break();
            }

            var item =  ((Extension)((ListBox)sender).Items[e.Index]);
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            //var rect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 8, 16, 16);
            var rect = new Rectangle(e.Bounds.X, e.Bounds.Y+5, 24, 24);
            //assuming the icon is already added to project resources

            try
            {
                Bitmap bitmap = (Bitmap)Image.FromFile(item.Icon);
                e.Graphics.DrawIcon(Icon.FromHandle(bitmap.GetHicon()), rect);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            

            //e.Graphics.DrawIconUnstretched(new Icon("a.ico"), rect);
            e.Graphics.DrawString(
                item.Name.ToString(),
                e.Font, Brushes.Black,
                //new Rectangle(e.Bounds.X + 25, e.Bounds.Y + 10, e.Bounds.Width, e.Bounds.Height), 
                new Rectangle(e.Bounds.X+25, e.Bounds.Y+10, e.Bounds.Width, e.Bounds.Height),
                StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }
        private void WorkspaceAddOrEdit_Load(object sender, EventArgs e)
        {
            
            
            listBox1.Items.Clear();

            if (isEdit)
            {
                label1.Text = "Edit Workspace";
                button4.Text = "Save";
                if (mf.workspaces.Count > 0)
                {
                    var result = mf.extensions.Where(i => mf.workspaces[index].extensions.All(y => i.Name != y.Name)).ToArray();
                    listBox1.Items.AddRange(result);
                }

                else
                    listBox1.Items.AddRange(mf.extensions.ToArray());
                textBox1.Text = mf.workspaces[index].WorkspaceName;
                listBox2.Items.AddRange(mf.workspaces[index].extensions.ToArray());
            }
            else
            {
                listBox1.Items.AddRange(mf.extensions.ToArray());
            }
           


        

            listBox1.DisplayMember = "Name";
            listBox2.DisplayMember = "Name";

        }

        private void Button1_Click(object sender, EventArgs e)
        {

            
            if (listBox1.SelectedIndices.Count > -1)
            {

                List<Extension> objects = listBox1.SelectedItems.Cast<Extension>().ToList();


                //listBox1.SelectedItems
                foreach (Extension i in objects)
                {
                    listBox2.Items.Add(i);
                    listBox1.Items.Remove(i);
                }
                
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            
            if (listBox2.SelectedIndices.Count > -1)
            {
                List<Object> objects = listBox2.SelectedItems.Cast<Object>().ToList();


                foreach (Object i in objects)
                {
                    listBox1.Items.Add(i);
                    listBox2.Items.Remove(i);
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if(textBox1.Text !=null && textBox1.Text!="")
            {
                if (isEdit)
                {
                    mf.workspaces[index].WorkspaceName = textBox1.Text;
                    mf.workspaces[index].extensions.Clear();
                    mf.workspaces[index].extensions.AddRange(listBox2.Items.Cast<Extension>());
                    
                }
                else
                {
                    Workspace ws = new Workspace();
                    ws.WorkspaceName = textBox1.Text;
                    ws.extensions.AddRange(listBox2.Items.Cast<Extension>());
                    
                    mf.workspaces.Add(ws);
                }
                using (StreamWriter r = new StreamWriter("config.json"))
                {
                    dynamic arr = JsonConvert.SerializeObject(mf.workspaces);
                    r.Write(arr);
                }
                mf.LoadWorkspaces();
                this.Close();
            }
        }
    }
}
