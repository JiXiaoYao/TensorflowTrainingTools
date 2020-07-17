using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TensorflowTrainingTools
{
    class CopyXml
    {
        static public void Copy(string ImagePath, string XmlModel, string OutputPath)
        {
            string[] images = Directory.GetFiles(ImagePath, "*.png|*.jpg");
            XmlDocument xmlDoc = new XmlDocument();                                                                           
            xmlDoc.Load(FilePath);
        }
    }
}
