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
        }
        public WorkspaceAddOrEdit(Object sender,int WorkspaceIndex)
        {
            InitializeComponent();
            isEdit = true;
            mf = (MainForm)sender;
            index = WorkspaceIndex;

        }

        private void WorkspaceAddOrEdit_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();


            if (mf.workspaces.Count > 0)
            {
                var result = mf.extensions.Where( i => mf.workspaces[index].extensions.All(y => i.Name != y.Name)).ToArray();
                listBox1.Items.AddRange(result);
            }

            else
                listBox1.Items.AddRange(mf.extensions.ToArray());


            if (isEdit)
            {
                textBox1.Text = mf.workspaces[index].WorkspaceName;
                listBox2.Items.AddRange(mf.workspaces[index].extensions.ToArray());



            }

            listBox1.DisplayMember = "Name";
            listBox2.DisplayMember = "Name";

        }

        private void Button1_Click(object sender, EventArgs e)
        {

            
            if (listBox1.SelectedIndices.Count > -1)
            {

                List<Object> objects = listBox1.SelectedItems.Cast<Object>().ToList();


                //listBox1.SelectedItems
                foreach (Object i in objects)
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
