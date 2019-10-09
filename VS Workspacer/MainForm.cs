using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        public String extensionsPath = Properties.Settings.Default.extensionsPath=="" || Properties.Settings.Default.extensionsPath == null ?  System.AppDomain.CurrentDomain.BaseDirectory + "extensions\\": Properties.Settings.Default.extensionsPath;
        public String disabledExtensionsPath = Properties.Settings.Default.disabledExtensionsPath=="" || Properties.Settings.Default.disabledExtensionsPath== null ? System.AppDomain.CurrentDomain.BaseDirectory + "extensions\\disabled\\" : Properties.Settings.Default.disabledExtensionsPath;


        public List<Workspace> workspaces = new List<Workspace>();
        public List<Extension> extensions = new List<Extension>();

        public void changeExtensionsPath(string _path)
        {
            extensionsPath = _path;
            Properties.Settings.Default.extensionsPath = _path;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Upgrade();
        }
        public  void changeDisabledExtensionsPath(string _path)
        {
            disabledExtensionsPath = _path;
            Properties.Settings.Default.disabledExtensionsPath = _path;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Upgrade();
        }
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //CheckForIllegalCrossThreadCalls = false;
            LoadExtension();
            LoadWorkspaces();
            comboBox1.SelectedIndexChanged += (s, d) => {
                
                if (comboBox1.SelectedIndex != -1) 
                    button1.Enabled = true;
                else 
                    button1.Enabled = false;
            };




        }

        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            var res = MessageBox.Show(
                "You need to close VSCode before changing. Are you sure about that? Have you saved You files?","Be Carefull!!!",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning);

            if (res == DialogResult.Cancel)
                return;

            string filePath = "";
            Process[] process = Process.GetProcessesByName("Code");
            try
            {
                process.ToList().ForEach(x =>
                {
                    filePath = filePath == "" ? x.GetMainModuleFileName() : filePath;

                    x.Kill();
                    while (!x.HasExited)
                    {
                        Thread.Sleep(100);
                    }

                });
            }
            catch (Exception)
            {

            }

           

            




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
                    x.Move(disabledExtensionsPath + x.extensionFolder.Name);
                    x.isActive = false;
                }
            });
            int l = 0;
            workspaces.ForEach(x => { x.isActive = l == comboBox1.SelectedIndex ? true : false;
                l++;
            });
            

            using (StreamWriter r = new StreamWriter("config.json"))
            {
                dynamic arr = JsonConvert.SerializeObject(workspaces);
                r.Write(arr);
            }
            if(filePath != "")
                Process.Start(filePath);
            button1.Enabled = true;
        }
        static readonly object _object = new object();
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
            if (extensions == null || extensions.Count <= 0)
                return;
            lock (_object)
            {
                workspaces.Clear();
                if (File.Exists("config.json"))
                {
                    using (StreamReader reader = new StreamReader("config.json"))
                    {
                        string json = reader.ReadToEnd();
                        if (json == null || json == "")
                            return;
                        dynamic arr = JsonConvert.DeserializeObject(json);
                    
                        foreach (var i in arr)
                        {
                            Workspace ws = new Workspace();
                            
                            ws.WorkspaceName = i.WorkspaceName;
                            ws.isActive = (bool)i.isActive ? i.isActive:false;
                            foreach (var k in i.extensions)
                            {
                                //String _path = k.extensionFolder.FullPath;
                                String name = k.Name;
                                Extension kutu;
                                if ( (kutu= extensions.Where(x =>
                                {
                                    try
                                    {
                                        return x.Name == name;
                                    }
                                    catch (Exception)
                                    {
                                        return false;
                                    }

                                }).FirstOrDefault()).Name!=null)
                                {
                                    kutu.isActive = k.isActive;
                                    ws.extensions.Add(kutu);
                                }
                                
                                //kutu.LoadExtension(_path);
                                
                            }

                            workspaces.Add(ws);

                        }

                    }

                    Invoke((MethodInvoker) delegate {
                        comboBox1.DataSource = new BindingSource(workspaces, null);
                        comboBox1.DisplayMember = "WorkspaceName";
                        comboBox1.SelectedIndex = workspaces.FindIndex(x => x.isActive==true);
                    });




                }
                else
                {
                    File.Create("config.json");
                }
            }

        }
        private void LoadExtensionsProcess()
        {
            if (!Directory.Exists(extensionsPath))
            {
                MessageBox.Show("Extension And DisabledExtension Path must be set!","Information",MessageBoxButtons.OK);
                return;
            }

            lock (_object)
            {

                    if (!Directory.Exists(disabledExtensionsPath))
                    {
                        var result = MessageBox.Show("Extension And DisabledExtension Path must be set. It looks like disabledExtensionPath didn't set correctly. Do you want to Create Folder Now?","Information",MessageBoxButtons.OKCancel,MessageBoxIcon.Information);
                        if (result == DialogResult.OK)
                        {
                            String ss = extensionsPath + "disabled\\";
                            Directory.CreateDirectory(extensionsPath + "disabled\\");
                            changeDisabledExtensionsPath(ss);
                            LoadExtension();
                            return;
                        }
                        else
                            return;
                    }
                



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

                root = new DirectoryInfo(disabledExtensionsPath);
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
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            WorkspaceAddOrEdit f2 = new WorkspaceAddOrEdit(this);
            f2.ShowDialog();
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedIndex != -1)
                {
                    WorkspaceAddOrEdit f2 = new WorkspaceAddOrEdit(this, comboBox1.SelectedIndex);
                    f2.ShowDialog();
                }

            }
            catch (Exception)
            {
            }

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

        private void Button3_Click(object sender, EventArgs e)
        {
            Form_Settings fm = new Form_Settings(this);
            fm.ShowDialog();
        }


    }


}

internal static class Extensions
{
    [DllImport("Kernel32.dll")]
    private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

    public static string GetMainModuleFileName(this Process process, int buffer = 1024)
    {
        var fileNameBuilder = new StringBuilder(buffer);
        uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
        return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
            fileNameBuilder.ToString() :
            null;
    }
}