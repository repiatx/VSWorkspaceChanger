using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VS_Workspacer
{
    public class Workspace
    {
        public String WorkspaceName { get; set; }
        public List<Extension> extensions;
        public bool isActive { get; set; }

        public Workspace()
        {
            WorkspaceName = "";
            extensions = new List<Extension>();
        }
        public Workspace(string _name,List<Extension> _extensions)
        {
            WorkspaceName = _name;
            extensions = _extensions;

        }
        public bool MoveAll(FileInfo _destinationFolder)
        {
            if (extensions == null || extensions.Count < 1)
                return false;


            foreach (Extension ext in extensions)
            {
                ext.extensionFolder.MoveTo(_destinationFolder.FullName);


            }
            return true;
        }

        public void AddExtension(Extension _extension)
        {
            extensions.Add(_extension);
        }


    }

    public class Extension
    {
        
        public String Name { get; set; }
        public String Icon { get; set; }
        public bool isActive { get; set; }

        public DirectoryInfo extensionFolder;
        //public List<FileInfo> files;

        public Extension()
        {

        }
        public Extension(string _path)
        {

            LoadExtension(_path);


        }
        public bool Move(string _destinationFolder)
        {
            if (extensionFolder==null)
                return false;

            if (extensionFolder.FullName == _destinationFolder)
                return false;



            try
            {
                extensionFolder.MoveTo(_destinationFolder);
                extensionFolder = new DirectoryInfo(_destinationFolder);
            }
            catch (Exception)
            {
               
            }
            
            
            return true;
        }

        public void LoadExtension(string _path)
        {
            if (File.Exists(_path + "\\package.json"))
            {
                extensionFolder = new DirectoryInfo(_path);
                using (StreamReader r = new StreamReader(_path + "\\package.json"))
                {
                    string json = r.ReadToEnd();
                    dynamic myarray = JsonConvert.DeserializeObject(json);
                    Name = myarray.name;
                    Icon = extensionFolder.FullName+"\\"+ myarray.icon;
                    Icon = Icon.Replace(@"/", @"\");
                    Console.WriteLine(myarray.name);
                }

                

            }
            else
            {
                return;
                throw new Exception("Cant Find packages.json");
            }
        }
       
    }
}