using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace TensorflowTrainingTools
{
    class Helper
    {
        public string BasePath { set; get; }
        public Helper(string path)
        {
            BasePath = Path.GetFullPath(path);
            if (BasePath.Last() == '\\')
                BasePath = BasePath.Substring(0, BasePath.Length - 2);
            if (BasePath.Last() == '/')
                BasePath = BasePath.Substring(0, BasePath.Length - 2);
        }
        public void CreateCache()
        {
            string ProcessingPath = BasePath + "/imageData";
            if (Directory.Exists(ProcessingPath + "/Cache"))
                Directory.Delete(ProcessingPath + "/Cache", true);
            Directory.CreateDirectory(ProcessingPath + "/Cache");
            Directory.CreateDirectory(ProcessingPath + "/Cache/Train");
            Directory.CreateDirectory(ProcessingPath + "/Cache/Test");
            string[] files = Directory.GetFiles(ProcessingPath);
            Dictionary<string, List<string[]>> Datas = new Dictionary<string, List<string[]>>();
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string MainName = Regex.Match(fileName, @"^.+?(?=[0-9]+?\.[a-zA-Z]+?$)").Value;
                string Name = Regex.Match(fileName, @"^.+(?=\.[a-zA-Z]+?$)").Value;
                string ExtensionName = Regex.Match(fileName, @"(?<=\.)[a-zA-Z]+?$").Value;
                if (Regex.IsMatch(ExtensionName.ToLower(), @"png|jpg|jpeg|bmp|tif|psd|webp"))
                {
                    if (!Datas.ContainsKey(MainName))
                        Datas.Add(MainName, new List<string[]>());
                    if (File.Exists(ProcessingPath + "/" + $"{Name}.xml"))
                    {
                        Datas[MainName].Add(new string[] { $"{Name}.{ExtensionName}", $"{Name}.xml" });
                    }
                }
            }
            foreach (var key in Datas.Keys.ToArray())
                Datas[key] = RandomSort(Datas[key]);
            foreach (var key in Datas.Keys.ToArray())
                for (int i = 0; i < Datas[key].Count; i++)
                {
                    string relativePath = "";
                    if (i < Datas[key].Count * 0.9)
                        relativePath = "Cache/Train";
                    else
                        relativePath = "Cache/Test";
                    File.Copy($"{ProcessingPath}/{Datas[key][i][0]}", $"{ProcessingPath}/{relativePath}/{Datas[key][i][0]}");
                    File.Copy($"{ProcessingPath}/{Datas[key][i][1]}", $"{ProcessingPath}/{relativePath}/{Datas[key][i][1]}");
                }
        }
        public void CreatePbtxt()
        {
            string[] XmlList = Directory.GetFiles(BasePath + "/imageData", "*.xml");
            List<string> Names = new List<string>();
            foreach (var xml in XmlList)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xml);
                foreach (XmlNode obj in xmlDoc.SelectSingleNode("annotation").SelectNodes("object"))
                {
                    var name = obj.SelectSingleNode("name");
                    if (!Names.Contains(name.InnerText))
                        Names.Add(name.InnerText);
                }
            }
            string pbtxt = "";
            for (int i = 0; i < Names.Count; i++)
            {
                pbtxt += "item {\n";
                pbtxt += $"    id: {i + 1}\n";
                pbtxt += $"    name: '{Names[i]}'\n";
                pbtxt += "}\n";
            }
            if (!Directory.Exists(BasePath + "/annotations"))
                Directory.CreateDirectory(BasePath + "/annotations");
            File.WriteAllText(BasePath + "/annotations/label_map.pbtxt", pbtxt);
        }
        #region ToolFunction
        private List<T> RandomSort<T>(List<T> list)
        {
            var random = new Random();
            var newList = new List<T>();
            foreach (var item in list)
            {
                newList.Insert(random.Next(newList.Count), item);
            }
            return newList;
        }
        #endregion
    }
}
