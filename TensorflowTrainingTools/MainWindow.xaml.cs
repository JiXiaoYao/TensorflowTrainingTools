using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TensorflowTrainingTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Helper helper { set; get; }
        public MainWindow()
        {
            InitializeComponent();
            helper = new Helper(@"C:\Users\16668\Desktop\workPath", "python");
            //helper.CreateCache();
            //helper.CreatePbtxt();
            //helper.XmlToCSV();
            //helper.CsvToTFRecord();
            //helper.DeleteCache();
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
                Infomation.Content = msg;
            });
        }
    }
}
