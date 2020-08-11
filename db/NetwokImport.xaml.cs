using MiniSDN.Dataplane;
using MiniSDN.Properties;
using MiniSDN.ui;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MiniSDN.db
{
    /// <summary>
    /// Interaction logic for NetwokImport.xaml
    /// </summary>
    public partial class NetwokImport : UserControl
    {
        public MainWindow MainWindow { set; get; }
        public List<ImportedSensor> ImportedSensorSensors = new List<ImportedSensor>();

        public UiImportTopology UiImportTopology { get; set; }
        public NetwokImport()
        {
            InitializeComponent();
        }

        private void brn_import_Click(object sender, RoutedEventArgs e)
        {
            
            //导入拓扑图时点击对应的import按钮将会生成对应数据库节点信息的拓扑图，主要包括节点ID,坐标和感知半径
            //注：通信半径=感知半径*2
            NetworkTopolgy.ImportNetwok(this);
            PublicParamerters.NetworkName = lbl_network_name.Content.ToString();
            PublicParamerters.SensingRangeRadius = ImportedSensorSensors[0].R;
            // now add them to feild.

            foreach (ImportedSensor imsensor in ImportedSensorSensors)
            {
                Sensor node = new Sensor(imsensor.NodeID);
                node.MainWindow = MainWindow;
                Point p = new Point(imsensor.Pox, imsensor.Poy);
                node.Position = p;
                node.VisualizedRadius = imsensor.R;
                MainWindow.myNetWork.Add(node);
                MainWindow.Canvas_SensingFeild.Children.Add(node);


                node.ShowID(Settings.Default.ShowID);
                node.ShowSensingRange(Settings.Default.ShowSensingRange);
                node.ShowComunicationRange(Settings.Default.ShowComunicationRange);
                node.ShowBattery(Settings.Default.ShowBattry);
            }
           

            try
            {
                UiImportTopology.Close();
            }
            catch
            {

            }
            

           

        }
    }
}
