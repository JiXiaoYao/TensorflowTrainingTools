using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace TensorflowTrainingTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Helper helper { set; get; }
        public string WorkPath { set; get; }
        public MainWindow()
        {
            InitializeComponent();
            helper = new Helper(@"C:\Users\16668\Desktop\workPath", "python");
            //helper.ReleaseScript();
            //helper.CreateCache();
            //helper.CreatePbtxt();
            //helper.XmlToCSV();
            //helper.CsvToTFRecord();
            //helper.DeleteCache();
            CopyXml.Copy(@"C:\Users\16668\Desktop\screenshot", @"C:\Users\16668\Desktop\workPath\imageData","");
        }
        public void Draw(System.Windows.Controls.Image imShow, string ImagePath, string XmlPath)
        {
            var boxes = helper.GetLabelFromXml(XmlPath);
            Image bitmap = Image.FromFile(ImagePath);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Font font = new Font("微软雅黑", (float)(bitmap.Width * 0.005) * 2, GraphicsUnit.Pixel);
                foreach (var box in boxes)
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(204, 255, 0), (float)(bitmap.Width * 0.005)), new System.Drawing.Rectangle(box.xmin, box.ymin, box.xmax - box.xmin, box.ymax - box.ymin));
                    var size = g.MeasureString(box.Name, font);
                    g.DrawLine(new Pen(Color.FromArgb(204, 255, 0), (float)(bitmap.Width * 0.005)), box.xmin, box.ymin - (float)(bitmap.Width * 0.005), box.xmin + size.Width, box.ymin - (float)(bitmap.Width * 0.005));
                }
                foreach (var box in boxes)
                    g.DrawString(box.Name, font, new SolidBrush(Color.Black), new PointF(box.xmin, box.ymin - (float)(bitmap.Width * 0.01)));
            }
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            System.Windows.Media.Imaging.BitmapImage Bimage = new System.Windows.Media.Imaging.BitmapImage();
            Bimage.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);
            Bimage.StreamSource = memoryStream;
            Bimage.EndInit();
            imShow.Source = Bimage;
        }

        private void DataProcessing_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                ChangeShow("正在创建缓存...");
                helper.CreateCache();
                ChangeShow("正在创建标签(pbtxt)...");
                helper.CreatePbtxt();
                ChangeShow("正在将XML转为CSV...");
                helper.XmlToCSV();
                ChangeShow("正在转储TFRecord");
                helper.CsvToTFRecord();
                ChangeShow("正在删除缓存");
                helper.DeleteCache();
                ChangeShow("数据预处理完成");
                Process explorer = new Process();
                explorer.StartInfo.FileName = "explorer";
                explorer.StartInfo.Arguments = " " + helper.BasePath + "\\annotations";
                explorer.Start();
            }));
            thread.Start();
            //python script/model_main.py --alsologtostderr --model_dir=training/ --pipeline_config_path=training/ssd_inception_v2_coco.config
            //tfv1Python\python.exe script/model_main.py --alsologtostderr --model_dir=training/ --pipeline_config_path=training/ssd_inception_v2_coco.config
        }
        public void ChangeShow(string msg)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                // Infomation.Content = msg;
            });
        }

        private void LoadFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dilog = new System.Windows.Forms.FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            if (dilog.ShowDialog() == System.Windows.Forms.DialogResult.OK || dilog.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
            {
                WorkPath = dilog.SelectedPath;
                List<string> files = new List<string>();
                foreach (var file in Directory.GetFiles(WorkPath + "/imageData", "*.png"))
                    if (File.Exists(Regex.Match(file, @".+(?=png)").Value + "xml"))
                        files.Add(Path.GetFileName(file));
                files.Sort(FeatureHelper.Compare);
                foreach (string file in files)
                    FileList.Items.Add(file);
            }
        }
        public bool FileListHasDown = false;
        private void FileList_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FileListHasDown)
            {
                FileListHasDown = false;
                string fileName = (string)FileList.SelectedItem;
            }
        }


        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FileListHasDown = false;
        }

        private void FileList_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //   FileListHasDown = true;
        }

        private void FileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string fileName = (string)FileList.SelectedItem;
            string name = Regex.Match(fileName, @".+(?=png)").Value + "xml";
            Draw(ImageShow, WorkPath + $"/imageData/{fileName}", WorkPath + $"/imageData/{name}");
        }

        private void ChooseImgFilePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dilog = new System.Windows.Forms.FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            if (dilog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rawImagePath.Text = dilog.SelectedPath;
            }
        }


        private void ChooseXmlModel_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                // openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "xml files (*.xml)|*.xml";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    XmlModelPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void TitleBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseThisWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChooseOutPutPath_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "xml files (*.xml)|*.xml";
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    XmlPngOutPutPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void GenerateXml(object sender, RoutedEventArgs e)
        {

        }
    }

}
