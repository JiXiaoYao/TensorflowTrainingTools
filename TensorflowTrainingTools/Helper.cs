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
        /// <summary>
        /// 创建数据缓存
        /// </summary>
        public void CreateCache()
        {
            string ProcessingPath = BasePath + "/imageData";                                                                      // 设置目录
            if (Directory.Exists(ProcessingPath + "/Cache"))                                                                      // 如果缓存目录存在，就清除
                Directory.Delete(ProcessingPath + "/Cache", true);
            Directory.CreateDirectory(ProcessingPath + "/Cache");                                                                 // 创建缓存目录
            Directory.CreateDirectory(ProcessingPath + "/Cache/Train");                                                           // 创建训练数据目录
            Directory.CreateDirectory(ProcessingPath + "/Cache/Test");                                                            // 创建测试数据目录
            string[] files = Directory.GetFiles(ProcessingPath);                                                                  // 获取图片目录下所有文件名
            Dictionary<string, List<string[]>> Datas = new Dictionary<string, List<string[]>>();                                  // 分类文件名字典
            foreach (var file in files)                                                                                           // 遍历文件名
            {
                string fileName = Path.GetFileName(file);                                                                         // 从路径中取出文件名
                string MainName = Regex.Match(fileName, @"^.+?(?=[0-9]+?\.[a-zA-Z]+?$)").Value;                                   // 获取主体名，比如说Choose1.png，Choose 就是主体名
                string Name = Regex.Match(fileName, @"^.+(?=\.[a-zA-Z]+?$)").Value;                                               // 获取文件名
                string ExtensionName = Regex.Match(fileName, @"(?<=\.)[a-zA-Z]+?$").Value;                                        // 获取文件扩展名
                if (Regex.IsMatch(ExtensionName.ToLower(), @"png|jpg|jpeg|bmp|tif|psd|webp"))                                     // 判断扩展名是否为图片扩展名
                {
                    if (!Datas.ContainsKey(MainName))                                                                             // 如果字典中没有该类图片
                        Datas.Add(MainName, new List<string[]>());                                                                // 则添加
                    if (File.Exists(ProcessingPath + "/" + $"{Name}.xml"))                                                        // 判断同名的XML配置文件是否存在
                    {
                                                                                                                                  // 如果存在则加入字典数组
                    }
                }
            }
            foreach (var key in Datas.Keys.ToArray())                                                                             // 遍历字典
                Datas[key] = RandomSort(Datas[key]);                                                                              // 乱序
            foreach (var key in Datas.Keys.ToArray())                                                                             // 遍历字典
                for (int i = 0; i < Datas[key].Count; i++)                                                                        // 遍历该类型下的数组
                {
                    string relativePath = "";
                    if (i < Datas[key].Count * 0.9)                                                                               // 前90%用作训练数据，后10%用作测试数据
                        relativePath = "Cache/Train";
                    else
                        relativePath = "Cache/Test";
                                                                                                                                  // 复制图片与XML配置文件
                    File.Copy($"{ProcessingPath}/{Datas[key][i][1]}", $"{ProcessingPath}/{relativePath}/{Datas[key][i][1]}");
                }
        }
        /// <summary>
        /// 创建pbtxt
        /// </summary>
        public void CreatePbtxt()
        {
            string[] XmlList = Directory.GetFiles(BasePath + "/imageData", "*.xml");                                              // 读取所有的xml文件的路径
            List<string> Names = new List<string>();                                                                              // 标签名数组
            foreach (var xml in XmlList)                                                                                          // 遍历路径
            {
                XmlDocument xmlDoc = new XmlDocument();                                                                           // 实例化XML对象
                xmlDoc.Load(xml);                                                                                                 // 加载XML文件
                                                                                                                                  // 遍历XML中名为object的节点
                {
                    var name = obj.SelectSingleNode("name");                                                                      // 获取名字
                    if (!Names.Contains(name.InnerText))                                                                          // 如果该标签名不在数组中
                        Names.Add(name.InnerText);                                                                                // 则添加
                }
            }
            Names.Sort();                                                                                                         // 对名称数组进行排序
            string pbtxt = "";                                                                                                    // pbtxt内容
            for (int i = 0; i < Names.Count; i++)                                                                                 // 遍历标签名数组，创建pbtxt
            {
                pbtxt += "item {\n";
                pbtxt += $"    id: {i + 1}\n";
                pbtxt += $"    name: '{Names[i]}'\n";
                pbtxt += "}\n";
            }
            if (!Directory.Exists(BasePath + "/annotations"))                                                                     // 判断annotations目录是否不存在
                Directory.CreateDirectory(BasePath + "/annotations");                                                             // 则创建
            File.WriteAllText(BasePath + "/annotations/label_map.pbtxt", pbtxt);                                                  // 写入文件
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
