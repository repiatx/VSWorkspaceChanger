using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using VS_Workspacer;

namespace VS_Workspacer
{
    public partial class MainForm : Form
    {
        private String extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory + "extensions\\";
        
        private String disabledExtensionPath = System.AppDomain.CurrentDomain.BaseDirectory + "extensions\\disabled\\";
        public List<Workspace> workspaces = new List<Workspace>();
        public List<Extension> extensions = new List<Extension>();
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //CheckForIllegalCrossThreadCalls = false;
            LoadExtension();
            LoadWorkspaces();




        }

        private void Button1_Click(object sender, EventArgs e)
        {


            var result =
                extensions.Where(x => workspaces[comboBox1.SelectedIndex].extensions.All(y => y.Name != x.Name)).ToList();

            workspaces[comboBox1.SelectedIndex].extensions.ForEach(x =>
            {
                if (x.isActive == false)
                {
                    x.Move(extensionsPath + x.extensionFolder.Name);
                    x.isActive = true;
                }
            });
            result.ForEach(x =>
            {
                if( x.isActive == true)
                {
                    x.Move(disabledExtensionPath + "\\" + x.extensionFolder.Name);
                    x.isActive = false;
                }
            });

           

            using (StreamWriter r = new StreamWriter("config.json"))
            {
                dynamic arr = JsonConvert.SerializeObject(workspaces);
                r.Write(arr);
            }
        }

        public void LoadExtension()
        {
            Thread extensionThread = new Thread(new ThreadStart(LoadExtensionsProcess));
            extensionThread.Start();

        }
        public void LoadWorkspaces()
        {
            Thread workspaceThread = new Thread(new ThreadStart(LoadWorkspacesProcess));
            workspaceThread.Start();
        }

        private void LoadWorkspacesProcess()
        {
            workspaces.Clear();
            if (File.Exists("config.json"))
            {
                using (StreamReader reader= new StreamReader("config.json"))
                {
                    string json = reader.ReadToEnd();
                    if (json == null || json == "")
                        return;
                    dynamic arr = JsonConvert.DeserializeObject(json);

                    foreach (var i in arr)
                    {
                        Workspace ws = new Workspace();
                        ws.WorkspaceName = i.WorkspaceName;
                        foreach (var k in i.extensions)
                        {
                            String _path = k.extensionFolder.FullPath;
                            Extension kutu = new Extension();
                            kutu.LoadExtension(_path);
                            kutu.isActive = k.isActive;
                            ws.extensions.Add(kutu);
                        }

                        workspaces.Add(ws);
                    }
                    
                }

                CallThis(comboBox1, () =>
                {
                    comboBox1.DataSource = new BindingSource(workspaces, null);
                    comboBox1.DisplayMember = "WorkspaceName";
                });


            }

        }
        private void LoadExtensionsProcess()
        {
            extensions.Clear();
            DirectoryInfo root = new DirectoryInfo(extensionsPath);
            root.GetDirectories().ToList().ForEach(x =>
            {
                Extension ext = new Extension(x.FullName);
                if (ext.Name != "" && ext.Name != null)
                {
                    ext.isActive = true;
                    extensions.Add(ext);
                }


            });

            root = new DirectoryInfo(disabledExtensionPath);
            root.GetDirectories().ToList().ForEach(x =>
            {
                Extension ext = new Extension(x.FullName);
                if (ext.Name != "" && ext.Name != null)
                {
                    ext.isActive = false;
                    extensions.Add(ext);
                }


            });


        }

        private void Button2_Click(object sender, EventArgs e)
        {
            WorkspaceAddOrEdit f2 = new WorkspaceAddOrEdit(this);
            f2.ShowDialog();
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            WorkspaceAddOrEdit f2 = new WorkspaceAddOrEdit(this,comboBox1.SelectedIndex);
            f2.ShowDialog();
        }






        delegate void InvokeDelegate(Control o, Action f);
        public void CallThis(Control o, Action f)
        {
            if (o.InvokeRequired)
            {
                InvokeDelegate del = new InvokeDelegate(CallThis);
                o.Invoke(del, new object[] { o, f });

            }
            else
                f();
        }
    }

}
