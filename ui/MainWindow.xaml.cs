using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MiniSDN.Dataplane;
using MiniSDN.db;
using MiniSDN.Intilization;
using MiniSDN.Coverage;
using MiniSDN.Properties;
using System.Windows.Media;
using System.Windows.Threading;
using MiniSDN.Forwarding;
using MiniSDN.ExpermentsResults.Energy_consumptions;
using MiniSDN.ExpermentsResults.Lifetime;
using MiniSDN.ControlPlane.NOS.TC;
using MiniSDN.ControlPlane.NOS.TC.subgrapgh;
using MiniSDN.DataPlane.NeighborsDiscovery;
using MiniSDN.ControlPlane.NOS.FlowEngin;
using MiniSDN.ui.conts;
using MiniSDN.Charts.Intilization;
using MiniSDN.ControlPlane.NOS.Visualizating;
using System.Threading;
using System.Windows.Input;
using MiniSDN.Dataplane.PacketRouter;

namespace MiniSDN.ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string PacketRate { get; set; }
        public Int32 stopSimlationWhen = 1000000000; // s by defult.
        public DispatcherTimer TimerCounter = new DispatcherTimer();
        public DispatcherTimer RandomSelectSourceNodesTimer = new DispatcherTimer();
        public static double Swith;// sensing feild width.
        public static double Sheigh;// sensing feild height.
        //若使用ORR路由协议，则该定时器定时计算最优nmax且更新节点的转发集
        public DispatcherTimer ORRTimer = new DispatcherTimer();

        /// <summary>
        /// the area of sensing feild.
        /// </summary>
        public static double SensingFeildArea
        {
            get
            {
                return Swith * Sheigh;
            }
        }

        public List<Vertex> MyGraph = new List<Vertex>();
        public List<Sensor> myNetWork = new List<Sensor>();
        public int index = 1;

        bool isCoverageSelected = false;


        public MainWindow()
        {
            InitializeComponent();
            // sensing feild
            Swith = Canvas_SensingFeild.Width - 218;
            Sheigh = Canvas_SensingFeild.Height - 218;
            PublicParamerters.SensingFeildArea = SensingFeildArea;
            PublicParamerters.MainWindow = this;
            // battery levels colors:
            FillColors();

            PublicParamerters.RandomColors = RandomColorsGenerator.RandomColor(100); // 100 diffrent colores.

            /*
            List<UplinkFlowEnery> list = DistrubtionsTests.TestHvalue(5, 1);
            ListControl HList = new ui.conts.ListControl();
            HList.dg_date.ItemsSource = list;
            UiShowLists win = new UiShowLists();
            win.stack_items.Children.Add(HList);
            win.Show();
            win.WindowState = WindowState.Maximized;*/


            _show_id.IsChecked = Settings.Default.ShowID;
            _show_battrey.IsChecked = Settings.Default.ShowBattry;
            _show_sen_range.IsChecked= Settings.Default.ShowSensingRange;
            _show_com_range.IsChecked= Settings.Default.ShowComunicationRange;
            _Show_Routing_Paths.IsChecked = Settings.Default.ShowRoutingPaths;
            _Show_Packets_animations.IsChecked = Settings.Default.ShowAnimation;
        }

        private void TimerCounter_Tick(object sender, EventArgs e)
        {
            //
            if (PublicParamerters.SimulationTime <= stopSimlationWhen + PublicParamerters.MacStartUp)
            {
                Dispatcher.Invoke(() => PublicParamerters.SimulationTime += 1, DispatcherPriority.Send);
                //每秒都会对主窗口最左上方的文字进行更新，显示实验进行的时间，单位：秒
                Dispatcher.Invoke(() => Title = "MiniSDN:" + PublicParamerters.SimulationTime.ToString(), DispatcherPriority.Send);
                MainWindowUpdataMessage();
            }
            else
            {
                TimerCounter.Stop();
                RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(0);
                RandomSelectSourceNodesTimer.Stop();
                top_menu.IsEnabled = true;
            }
        }

        private void RandomSelectNodes_Tick(object sender, EventArgs e)
        {
            // start sending after the nodes are intilized all.
            if (PublicParamerters.SimulationTime > PublicParamerters.MacStartUp)
            {

                /*
                int index = 1 + Convert.ToInt16(UnformRandomNumberGenerator.GetUniform(PublicParamerters.NumberofNodes - 2));
                if (index != PublicParamerters.SinkNode.ID)
                {
                    //随机选择一个节点，该点将执行生成数据包的函数                  
                    myNetWork[index].GenerateDataPacket();

                }
                */


                //按顺序生成数据包，原始版本为随机选择一个节点生成数据包
                if (index == 0)
                    index++;
                myNetWork[index].GenerateDataPacket();
                index = (index + 1) % myNetWork.Count;
               

            }
        }

        private void FillColors()
        {

            // POWER LEVEL:
            lvl_0.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col0));
            lvl_1_9.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col1_9));
            lvl_10_19.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col10_19));
            lvl_20_29.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col20_29));
            lvl_30_39.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col30_39));
            lvl_40_49.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col40_49));
            lvl_50_59.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col50_59));
            lvl_60_69.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col60_69));
            lvl_70_79.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col70_79));
            lvl_80_89.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col80_89));
            lvl_90_100.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col90_100));

            // MAC fuctions:
            lbl_node_state_check.Fill = NodeStateColoring.ActiveColor;
            lbl_node_state_sleep.Fill = NodeStateColoring.SleepColor;
        }


        private void BtnFile(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string Header = item.Header.ToString();
            switch (Header)
            {
                case "_Multiple Nodes"://手动生成新的拓扑图，原始版本功能未完善，现已完成
                    {
                        UiAddNodes ui = new UiAddNodes();
                        ui.MainWindow = this;
                        ui.Show();
                        break;
                    }

                case "_Export Topology"://导出拓扑图
                    {
                        UiExportTopology top = new UiExportTopology(myNetWork);
                        top.Show();
                        break;
                    }

                case "_Import Topology"://导入一个拓扑图，即将数据库中的节点信息导入构成网络拓扑图
                    {
                        UiImportTopology top = new UiImportTopology(this);
                        top.Show();
                        break;
                    }
            }

        }

       


        public void DisplaySimulationParameters(int rootNodeId, string deblpaymentMethod)
        {
            PublicParamerters.SinkNode = myNetWork[rootNodeId];
            PublicParamerters.SinkNode.Ellipse_battryIndicator.Width = 16;
            PublicParamerters.SinkNode.Ellipse_battryIndicator.Height = 16;
            PublicParamerters.SinkNode.Ellipse_battryIndicator.Fill = Brushes.OrangeRed;
            PublicParamerters.SinkNode.Ellipse_MAC.Fill = Brushes.OrangeRed;

            //显示各参数内容
            PublicParamerters.SinkNode.lbl_Sensing_ID.Foreground = Brushes.Blue;
            PublicParamerters.SinkNode.lbl_Sensing_ID.FontWeight = FontWeights.Bold;
            lbl_sink_id.Content = rootNodeId;
            lbl_coverage.Content = deblpaymentMethod;
            lbl_network_size.Content = myNetWork.Count;
            lbl_sensing_range.Content = PublicParamerters.SinkNode.VisualizedRadius;
            lbl_communication_range.Content = (PublicParamerters.SinkNode.VisualizedRadius * 2);
            lbl_Transmitter_Electronics.Content = PublicParamerters.E_elec;
            lbl_fes.Content = PublicParamerters.Efs;
            lbl_Transmit_Amplifier.Content = PublicParamerters.Emp;
            lbl_data_length_control.Content = PublicParamerters.ControlDataLength;
            lbl_data_length_routing.Content = PublicParamerters.RoutingDataLength;
            lbl_density.Content = PublicParamerters.Density;
            Settings.Default.IsIntialized = true;

            TimerCounter.Interval=TimeSpan.FromSeconds(1); // START count the running time.
            TimerCounter.Start(); // START count the running time.
            TimerCounter.Tick += TimerCounter_Tick;//每秒都会更新主窗口的显示信息

            //:主窗口右侧各参数显示
           // prog_total_energy.Maximum = Convert.ToDouble(myNetWork.Count) * PublicParamerters.BatteryIntialEnergy;
           // prog_total_energy.Value = 0;



            lbl_x_active_time.Content = Settings.Default.ActivePeriod + ",";
            lbl_x_queue_time.Content = Settings.Default.CheckQueueTime + ".";
            lbl_x_sleep_time.Content = Settings.Default.SleepPeriod + ",";
            lbl_x_start_up_time.Content = Settings.Default.MacStartUp + ",";
            lbl_intial_energy.Content = Settings.Default.BatteryIntialEnergy;

            lbl_par_D.Content = Settings.Default.ExpoDCnt;
            lbl_par_Dir.Content = Settings.Default.ExpoECnt;
            lbl_par_H.Content = Settings.Default.ExpoHCnt;
            lbl_par_L.Content = Settings.Default.ExpoLCnt;
            lbl_par_R.Content = Settings.Default.ExpoRCnt;
            lbl_update_percentage.Content = Settings.Default.UpdateLossPercentage;
        }

        public void HideSimulationParameters()
        {
            menSimuTim.IsEnabled = true;
            stopSimlationWhen = 1000000;

            rounds = 0;
            lbl_rounds.Content = "0";
            PublicParamerters.SinkNode = null;
            lbl_sink_id.Content = "nil";
            lbl_coverage.Content = "nil";
            lbl_network_size.Content = "unknown";
            lbl_sensing_range.Content = "unknown";
            lbl_communication_range.Content = "unknown";
            lbl_Transmitter_Electronics.Content = "unknown";
            lbl_fes.Content = "unknown";
            lbl_Transmit_Amplifier.Content = "unknown";
            lbl_data_length_control.Content = "unknown";
            lbl_data_length_routing.Content = "unknown";
            lbl_density.Content = "0";
            // lbl_control_range.Content = "0";
            //  lbl_zone_width.Content = "0";
            Settings.Default.IsIntialized = false;

            //
            RandomSelectSourceNodesTimer.Stop();
            TimerCounter.Stop();


            lbl_x_active_time.Content = "0";
            lbl_x_queue_time.Content = "0";
            lbl_x_sleep_time.Content = "0";
            lbl_x_start_up_time.Content = "0";
            lbl_intial_energy.Content = "0";

            lbl_par_D.Content = "0";
            lbl_par_Dir.Content = "0";
            lbl_par_H.Content = "0";
            lbl_par_L.Content = "0";
            lbl_par_R.Content = "0";

            lbl_total_delay.Content = "0";
            lbl_total_delay_by_waiting_in_queue.Content = "0";
            lbl_total_delay_by_waiting_in_queue_percentage.Content = "0";
            lbl_total_delay_by_no_ack.Content = "0";
            lbl_total_delay_by_no_ack_percentage.Content = "0";
            lbl_total_delay_by_data_packet.Content = "0";
            lbl_total_delay_by_data_packet_percentage.Content = "0";
            lbl_total_delay_by_preamble_packet.Content = "0";
            lbl_total_delay_by_preamble_packet_percentage.Content = "0";
            lbl_average_delay.Content = "0";

            lbl_Number_of_Delivered_Packet.Content = "0";
            lbl_Number_of_Droped_Packet.Content = "0";
            lbl_Droped_because_of_Cannot_Send.Content = "0";
            lbl_Droped_because_of_TTL.Content = "0";
            lbl_Droped_because_of_No_Energy.Content = "0";
            lbl_num_of_gen_packets.Content = "0";
            lbl_nymber_inQueu.Content = "0";
            lbl_Redundant_packets.Content = "0";
            lbl_sucess_ratio.Content = "0";
            lbl_droped_ratio.Content = "0";
            lbl_Wasted_Energy_percentage.Content = "0";
            lbl_update_percentage.Content = "0";
            lbl_number_of_control_packets.Content = "0";
            PublicParamerters.NumberofControlPackets = 0;
            PublicParamerters.EnergyComsumedForControlPackets = 0;
            PublicParamerters.SimulationTime = 0;
        }

        public void MainWindowUpdataMessage()
        {
            //Energy 相关显示

            //网络总能量
            Dispatcher.Invoke(() => lbl_total_energy.Content = PublicParamerters.TotalEnergy, DispatcherPriority.Send);

            //生命周期内消耗的能量
            Dispatcher.Invoke(() => lbl_total_consumed_energy.Content = Math.Round(PublicParamerters.TotalEnergyConsumptionJoule,4), DispatcherPriority.Send);

            //总能量的使用率
            Dispatcher.Invoke(() => lbl_total_consumed_energy_percentage.Content = PublicParamerters.Total_Energy_Consumption_Percentage,DispatcherPriority.Send );

            //数据包消耗的总能量占比
            Dispatcher.Invoke(() => lbl_total_data_packet_consumed_energy_percentage.Content = PublicParamerters.Total_Data_Packet_Consumption_Percentage,DispatcherPriority.Send);

            //数据包发送能耗占比
            Dispatcher.Invoke(() => lbl_data_consumed_send.Content = PublicParamerters.Data_Packet_Consumption_Send_Percentage, DispatcherPriority.Send);

            //数据包接收能耗占比
            Dispatcher.Invoke(() => lbl_data_consumed_receive.Content = PublicParamerters.Data_Packet_Consumption_Receive_Percentage,DispatcherPriority.Send );

            //preamble包消耗的总能量占比
            Dispatcher.Invoke(() => lbl_total_preamble_packet_consumed_energy_percentage.Content = PublicParamerters.Total_Preamble_Packet_Consumption_Percentage,DispatcherPriority.Send);

            //preamble包发送能耗占比
            Dispatcher.Invoke(() => lbl_preamble_consumed_send.Content = PublicParamerters.Preamble_Packet_Consumption_Send_Percentage,DispatcherPriority.Send);

            //preamble包接收能耗占比
            Dispatcher.Invoke(() => lbl_preamle_consumed_receive.Content = PublicParamerters.Preamble_Packet_Consumption_Receive_Percentage,DispatcherPriority.Send );

            //ACK包消耗的总能量占比
            Dispatcher.Invoke(() => lbl_total_ack_packet_consumed_energy_percentage.Content = PublicParamerters.Total_ACK_Packet_Consumption_Percentage,DispatcherPriority.Send);

            //ACK包发送能耗占比
            Dispatcher.Invoke(() => lbl_ack_consumed_send.Content = PublicParamerters.ACK_Packet_Consumption_Send_Percentage,DispatcherPriority.Send);

            //ACK包接收能耗占比
            Dispatcher.Invoke(() => lbl_ack_consumed_receive.Content = PublicParamerters.ACK_Packet_Consumption_Receive_Percentage,DispatcherPriority.Send);
            
            //冗余传输消耗的能量占总消耗能量的比率
            Dispatcher.Invoke(() => lbl_Wasted_Energy_percentage.Content = PublicParamerters.WastedEnergyPercentage);
           
            
            //Packet相关显示

            //总生成包
            Dispatcher.Invoke(() => lbl_num_of_gen_packets.Content = PublicParamerters.NumberofGeneratedPackets, DispatcherPriority.Normal);

            //总队列包
            Dispatcher.Invoke(() => lbl_nymber_inQueu.Content = PublicParamerters.NumberofInAllQueuePackets.ToString());

            //总成功传输包
            Dispatcher.Invoke(() => lbl_Number_of_Delivered_Packet.Content = PublicParamerters.NumberofDeliveredPacket, DispatcherPriority.Send);

            //总控制包
            Dispatcher.Invoke(() => lbl_number_of_control_packets.Content = PublicParamerters.NumberofControlPackets, DispatcherPriority.Normal);

            //总丢弃包
            Dispatcher.Invoke(() => lbl_Number_of_Droped_Packet.Content = PublicParamerters.NumberofDropedPacket, DispatcherPriority.Send);
           
            //发不出去的而丢弃的数据包
            Dispatcher.Invoke(() => lbl_Droped_because_of_Cannot_Send.Content = PublicParamerters.DropedbecauseofCannotSend, DispatcherPriority.Send);

            //TTL丢弃的数据包
            Dispatcher.Invoke(() => lbl_Droped_because_of_TTL.Content = PublicParamerters.DropedbecauseofTTL,DispatcherPriority.Send);

            //节点能量耗尽丢弃的队列中的包
            Dispatcher.Invoke(() => lbl_Droped_because_of_No_Energy.Content = PublicParamerters.DropedbecauseofNoEnergy,DispatcherPriority.Send);

            //成功率（成功传输包/总生成包）
            Dispatcher.Invoke(() => lbl_sucess_ratio.Content = PublicParamerters.DeliveredRatio, DispatcherPriority.Send);

            //丢包率 （丢弃包/总生成包）
            Dispatcher.Invoke(() =>lbl_droped_ratio.Content = PublicParamerters.DropedRatio, DispatcherPriority.Send);

            // 新成功率     成功传输包/（总生成包-总队列包）==========成功传输包/（成功传输包+丢弃包）
            Dispatcher.Invoke(() => lbl_new_delivered_ratio.Content = PublicParamerters.NewDeliveredRatio,DispatcherPriority.Send);
            //新丢包率      丢弃包/（总生成包-总队列包）==========丢弃包/（成功传输包+丢弃包）
            Dispatcher.Invoke(() => lbl_new_droped_ratio.Content = PublicParamerters.NewDropedRatio,DispatcherPriority.Send);
            //冗余传输
            Dispatcher.Invoke(() => lbl_Redundant_packets.Content = PublicParamerters.TotalReduntantTransmission,DispatcherPriority.Send);



            //delay相关显示,单位：S

            //总delay
            Dispatcher.Invoke(() => lbl_total_delay.Content = Math.Round( PublicParamerters.TotalDelayMs/1000,2), DispatcherPriority.Send);

            //平均时延 端到端 end-end
            Dispatcher.Invoke(() => lbl_average_delay.Content = PublicParamerters.Total_Average_Delay, DispatcherPriority.Send);



            //因为等待节点醒来而产生的总时延
            Dispatcher.Invoke(() => lbl_total_delay_by_waiting_in_queue.Content =Math.Round( PublicParamerters.TotalDelay_IN_Queue/1000,0), DispatcherPriority.Send);
            Dispatcher.Invoke(() => lbl_total_delay_by_waiting_in_queue_percentage.Content = PublicParamerters.Total_Delay_by_Waiting_In_Queue_Percentage, DispatcherPriority.Send);

            //因为没有ACK回复产生的总时延
            Dispatcher.Invoke(() => lbl_total_delay_by_no_ack.Content =Math.Round( PublicParamerters.TotalDelay_NO_ACK/1000,0), DispatcherPriority.Send);
            Dispatcher.Invoke(() => lbl_total_delay_by_no_ack_percentage.Content = PublicParamerters.Total_Delay_by_No_ACK_Percentage, DispatcherPriority.Send);

            //因为发送data包产生的总时延
            Dispatcher.Invoke(() => lbl_total_delay_by_data_packet.Content =Math.Round( PublicParamerters.TotalDelay_DataPackets/1000,2), DispatcherPriority.Send);
            Dispatcher.Invoke(() => lbl_total_delay_by_data_packet_percentage.Content = PublicParamerters.Total_Delay_by_Data_Packet_Percentage, DispatcherPriority.Send);

            //因为发送preamb包产生的总时延
            Dispatcher.Invoke(() => lbl_total_delay_by_preamble_packet.Content = Math.Round(PublicParamerters.TotalDelay_PreamblePackets/1000,2), DispatcherPriority.Send);
            Dispatcher.Invoke(() => lbl_total_delay_by_preamble_packet_percentage.Content = PublicParamerters.Total_Delay_by_Preamble_Packet_Percentage, DispatcherPriority.Send);



        }


        private void EngageMacAndRadioProcol()
        {
            foreach (Sensor sen in myNetWork)
            {
                sen.Mac = new BoXMAC(sen);//实现节点醒睡模式
                sen.BatRangesList = PublicParamerters.getRanges();
                sen.Myradar = new Intilization.Radar(sen);
            }
        }



        private void SetAllNodesIntialEnergy()
        {

            foreach (Sensor sen in myNetWork)
            {

                if (sen.ID == 0) sen.BatteryIntialEnergy = PublicParamerters.BatteryIntialEnergyForSink; // the value will not be change
                else
                    sen.BatteryIntialEnergy = PublicParamerters.BatteryIntialEnergy;


                sen.ResidualEnergy = sen.BatteryIntialEnergy;
                
                sen.Prog_batteryCapacityNotation.Value = PublicParamerters.BatteryIntialEnergy;
                sen.Prog_batteryCapacityNotation.Maximum = PublicParamerters.BatteryIntialEnergy;

            }

        }

        public void RandomDeplayment(int sinkIndex)
        {
            //为每个节点重新分配初始能量 
            SetAllNodesIntialEnergy();


            PublicParamerters.NumberofNodes = myNetWork.Count;
            int rootNodeId = sinkIndex;
            PublicParamerters.SinkNode = myNetWork[rootNodeId];//设置sink节点
            NeighborsDiscovery overlappingNodesFinder = new NeighborsDiscovery(myNetWork);
            overlappingNodesFinder.GetOverlappingForAllNodes();//邻居发现，通信距离=感知距离*2，通信距离内的节点为邻居节点


            string PowersString = "γL=" + Settings.Default.ExpoLCnt + ",γR=" + Settings.Default.ExpoRCnt + ", γH=" + Settings.Default.ExpoHCnt + ",γD" + Settings.Default.ExpoDCnt;
            PublicParamerters.PowersString = PublicParamerters.NetworkName + ",  " + PowersString;
            lbl_PowersString.Content = PublicParamerters.PowersString;//此语句是一条显示信息，参数为其他算法路由参数

            isCoverageSelected = true;
            PublicParamerters.Density = Density.GetDensity(myNetWork);//获取网络密度
            DisplaySimulationParameters(rootNodeId, "Random");//在主窗口右侧显示仿真参数

            

            TopologyConstractor.BuildToplogy(Canvas_SensingFeild, myNetWork);//节点间动画显示相关

            HopsToSinkComputation.ComputeHopsToSink(PublicParamerters.SinkNode);//跳数初始化

            if (Settings.Default.RoutingAlgorithm == "ORR")
            {
                ORRCompute(); //ORR相关参数的初始化，函数执行完之后每个节点的forward中的节点都将加入MiniFlowTable
                ORRTimer.Interval = TimeSpan.FromSeconds(30);
                ORRTimer.Tick += ORRTimer_Tick;
                ORRTimer.Start();

            }

            if (Settings.Default.RoutingAlgorithm == "ORW")
            {
                ORWCompute(); //ORW相关参数的初始化，函数执行完之后每个节点的forward中的节点都将加入MiniFlowTable

            }

            // fill flows:

            //转发表相关的初始化
            foreach (Sensor sen in myNetWork)
            {
                if(sen.ID != 0)
                UplinkRouting.ComputeUplinkFlowEnery(sen);
            }

            

            EngageMacAndRadioProcol();//为节点增加醒睡模式和MAC层相关设置
            MyGraph = Graph.ConvertNodeToVertex(myNetWork);


        }

        private void ORWCompute()
        {
           

            
            Algorithm2_ORW();



        }

        Queue<Sensor> U_ORW = new Queue<Sensor>();
        private void Algorithm2_ORW()
        {
            foreach (Sensor sensor in myNetWork)
            {
                if (sensor.ID == PublicParamerters.SinkNode.ID)
                {
                    //sink节点EDC为0
                    myNetWork[PublicParamerters.SinkNode.ID].EDC = 0;

                }
                else if (sensor.HopsToSink == 1) 
                {
                    //sink的邻居节点的EDC
                    sensor.EDC = PublicParamerters.Ta/PublicParamerters.T;
                    //sink的邻居节点的转发表中仅包含唯一一个sink节点
                    
                    sensor.Forwarders.Add(PublicParamerters.SinkNode);
                }
                else
                {
                    sensor.EDC = PublicParamerters.EDC0;
                    sensor.Forwarders.Clear();
                    U_ORW.Enqueue(sensor);

                }
            }
            while (U_ORW.Count > 0)
            {
                Sensor sensor = U_ORW.Dequeue();
                double EDC_Old = sensor.EDC;
                Algorithm1_ORW(sensor);
                if (sensor.EDC < EDC_Old)
                {
                    foreach (NeighborsTableEntry nei in sensor.NeighborsTable)
                    {
                        if (sensor.EDC < nei.NeiNode.EDC)
                        {
                            U_ORW.Enqueue(nei.NeiNode);
                        }
                    }
                }
            }



        }

        private void Algorithm1_ORW(Sensor sensor)
        {
            List<Sensor> V = new List<Sensor>();
            sensor.Forwarders.Clear();
            sensor.EDC = PublicParamerters.EDC0;
            foreach (NeighborsTableEntry nei in sensor.NeighborsTable)
            {
                if (nei.NeiNode.EDC < sensor.EDC)
                {
                    V.Add(nei.NeiNode);
                }

            }

            while (V.Count > 0 )
            {

                Sensor Node_MinEDC = FindNode_MinEDC(V);
                V.Remove(Node_MinEDC);
                if (Node_MinEDC.EDC < sensor.EDC)
                {
                    sensor.Forwarders.Add(Node_MinEDC);

                }
                else
                {
                    break;
                }

                //recalculate EDCi using Formula_1
                sensor.EDC = Formula_1(sensor);
                

            }
        }

        private double Formula_1(Sensor sensor)
        {
            //假设Pij均为1，w=0
            double number = sensor.Forwarders.Count;
            double sum = 0;
            foreach (Sensor i in sensor.Forwarders)
            {
                sum += i.EDC;
            }
            double firstterm = 1.0 / number;
            double secondterm = sum / number;
            double thirdterm = 0;
            return firstterm + secondterm + thirdterm;
        }

        private Sensor FindNode_MinEDC(List<Sensor> v)
        {
            Sensor res = null;
            double min = Double.MaxValue;
            foreach (Sensor i in v)
            {
                if (min > i.EDC)
                {
                    res = i;
                    min = i.EDC;
                }
            }
            return res;
        }

        private void ORRTimer_Tick(object sender, EventArgs e)
        {
            //重新计算nmax和生成节点的转发集
            ORRCompute();
        }

        Queue<Sensor> U = new Queue<Sensor>();
        private void ORRCompute()
        {
            int nmax = 3;

            double EstimatedForwardingCost = 0;
            double EstimatedForwardingCost_Min = Double.MaxValue;
            //循环尝试可能的n值，取最小代价的n为nmax；
            for (int n = 3; n < 20; n++) {
                //Algorithm2,假设算法中nmax值为n
                Algorithm2_ORR(n);
                //在当前nmax=n情况下计算平均消耗
                EstimatedForwardingCost = CalculateEstimatedForwardingCost();
               // Console.WriteLine("n: "+ n +"   EstimatedForwardingCost: " + EstimatedForwardingCost);

                if (EstimatedForwardingCost < EstimatedForwardingCost_Min)
                {
                    EstimatedForwardingCost_Min = EstimatedForwardingCost;
                    nmax = n;

                }


            }
           // Console.WriteLine("nmax: " + nmax );

            Algorithm2_ORR(nmax);




        }

        private double CalculateEstimatedForwardingCost()
        {
            //数组下标表示对应ID的节点，初始值为0，最终值表示对应节点包含在多少节点的Forwarders内,
            int[] NonLeafNodes = new int[myNetWork.Count];
            //需要计算的节点的队列
            Queue<Sensor> queue = new Queue<Sensor>();

            double cost = 0;
            foreach (Sensor sensor in myNetWork)
            {
                //每个节点的初始Expected_number_of_transmisstion为1，表示自身产生一个数据包
                if (sensor.ID != 0)
                    sensor.Expected_number_of_transmisstion = 1;

                //非叶子节点在数组对应位置的值表示有多少个节点的forward中包含该节点
                foreach (Sensor node in sensor.Forwarders)
                {
                    NonLeafNodes[node.ID] += 1;
                }



            }
            //构建初始队列，该队列中的节点均为叶子节点
            foreach (Sensor sensor in myNetWork)
            {
                if (NonLeafNodes[sensor.ID] == 0)
                {                   
                    queue.Enqueue(sensor);
                }

            }

            int Enqueuecount = 0;
            while (queue.Count > 0)
            {
                 Sensor topsensor = queue.Dequeue();
                //此函数中NonLeafNodes数组值为零表示可以将该节点的ENT加入总Cost中
                if (NonLeafNodes[topsensor.ID] == 0)
                {
                    foreach (Sensor sensor in topsensor.Forwarders)
                    {
                        //来自上一层节点的ENT增值
                        sensor.Expected_number_of_transmisstion += Formula_26(topsensor);

                        NonLeafNodes[sensor.ID]--;

                        if (queue.Contains(sensor) == false)
                            queue.Enqueue(sensor);
                    }
                    cost += topsensor.Expected_number_of_transmisstion;
                }
                //否则表明该节点还有上一层节点的ENT未计算完成
                else
                {
                   
                    queue.Enqueue(topsensor);

                    Enqueuecount++;
                    //出现死循环，此次nmax计算无效
                    if (Enqueuecount > 1000)
                    {
                        return Double.MaxValue;
                    }

                }
                    

            }
            

            return cost;
        }

        private double Formula_26(Sensor topsensor)
        {
            double Uplink_ENT = 0;
            double number = topsensor.Forwarders.Count;
            double R = Formula_25(topsensor);

            Uplink_ENT = R * topsensor.Expected_number_of_transmisstion / number;
           // Uplink_ENT = topsensor.Expected_number_of_transmisstion / number;
            return Uplink_ENT;
        }

        private double Formula_25(Sensor topsensor)
        {
            double value = 0;
            int n = topsensor.Forwarders.Count;
           
            
            double S = PublicParamerters.T / PublicParamerters.Ta;

            
            for (int i = 2; i <= S; i++)
            {
                for (int j = 1; j <= n - 1; j++)
                {
                    value += (j + 1) * Formula_23(n, S, j, i) * Formula_24(j, i - 1);
                }
            }
            
            

            value += n * Formula_24(n, S); 
            return value;


        }
        
        private double Formula_24(int l, double m)
        {
            double sum = 0;
            for (int k = 1; k <= m; k++)
            {
                sum += Formula_21(l, m, k);
            }
            return 1 - sum;
        }
        
        
        
        private double Formula_21(int n, double S, int m)
        {
            double res = 0;
            int t = n;
            if (m < n)
            {
                t = m;
            }
            for (int i = 1; i <= t; i++)
            {
                //Console.WriteLine("Formula_21");
                res += Math.Pow(-1, i + 1) * CombineNumber(m - 1, i - 1) * Formula_15(n, S, i);
            }
            return res;
        }

        private double Formula_15(int n, double S, int k)
        {
            //Console.WriteLine("Formula_15");
            double term_1 = CombineNumber(n, k);
            double term_2 = factorial(k);
            double term_3 = Math.Pow(1.0 / S, k);
            double term_4 = Math.Pow(1 - k / S, n - k);
            double res = term_1 * term_2 * term_3 * term_4;
            return res;
        }

        private double Formula_23(int n, double S, int l, double m)
        {
            //Console.WriteLine("Formula_23  term_1");
            double term_1 = CombineNumber(n, l);
            //Console.WriteLine("Formula_23  term_2");
            double term_2 = CombineNumber(n - l, 1);
            double term_3 = Math.Pow((double)(m - 1) / S, l);
            double term_4 = 1.0 / S;
            double term_5 = Math.Pow((double)(S - m) / S, n - l - 1);
            double res = term_1 * term_2 * term_3 * term_4 * term_5;
            return res;
        }
        
        
        
        public int CombineNumber(int n, int r)
        {
            int res = 0;
            //Console.WriteLine("n:"+ n);
            //res = (double)factorial(n) / (double)(factorial(r) * factorial(n - r));
            res = factorial(n) / (factorial(r) * factorial(n - r));
            return res;
        }
        
        
       
       
        
        public int factorial(int n)
        {
            int res = 1;
            for (int i = n; i > 1; i--)
            {
                res *= i;
            }
            return res;
        }
        
        
        private void Algorithm2_ORR(int n)
        {
            foreach (Sensor sensor in myNetWork)
            {
                if (sensor.ID == PublicParamerters.SinkNode.ID)
                {
                    //sink节点FS为0
                    myNetWork[PublicParamerters.SinkNode.ID].FS = 0;

                }
                else if (sensor.HopsToSink == 1)
                {
                    //sink的邻居节点的FS由公式9计算得到,转发表仅包括sink节点
                    sensor.FS = Formula_9(sensor);
                    //sink的邻居节点的转发表中仅包含唯一一个sink节点
                    if(sensor.Forwarders.Contains(PublicParamerters.SinkNode) == false)
                        sensor.Forwarders.Add(PublicParamerters.SinkNode);
                }
                else
                {
                    sensor.FS = PublicParamerters.FS0;
                    sensor.Forwarders.Clear();
                    U.Enqueue(sensor);

                }
            }
            while (U.Count > 0)
            {
                Sensor sensor = U.Dequeue();
                double FS_Old = sensor.FS;
                Algorithm1_ORR(sensor, n);
                if (sensor.FS < FS_Old)
                {
                    foreach (NeighborsTableEntry nei in sensor.NeighborsTable)
                    {
                        if (sensor.FS < nei.NeiNode.FS)
                        {
                            U.Enqueue(nei.NeiNode);
                        }
                    }
                }
            }

        }

        //ORR Algorithm1
        private void Algorithm1_ORR(Sensor sensor, int nmax)
        {
            List<Sensor> V = new List<Sensor>();          
            sensor.Forwarders.Clear();
            sensor.FS = PublicParamerters.FS0;
            foreach ( NeighborsTableEntry nei in sensor.NeighborsTable)
            {
                if (nei.NeiNode.FS < sensor.FS)
                {
                    V.Add(nei.NeiNode);
                }

            }

            while (V.Count > 0 && sensor.Forwarders.Count < nmax)
            {
                
                Sensor Node_MinFS = FindNode_MinFS(V);
                V.Remove(Node_MinFS);
                if (Node_MinFS.FS < sensor.FS)
                {
                    sensor.Forwarders.Add(Node_MinFS);

                }
                else {
                    break;
                }

                //recalculate FSi using Formula_7
                sensor.FS = Formula_7(sensor);

            }


        }

        private double Formula_7(Sensor sensor)
        {
            double number = sensor.Forwarders.Count;
            double sum = 0;
            foreach (Sensor i in sensor.Forwarders)
            {
                sum += i.FS;
            }
            double firstterm = 1.0 / ((number + 1) * Math.Pow(Formula_8(sensor), 1));
            double secondterm = sum / number;
            return firstterm + secondterm;
        }

        private Sensor FindNode_MinFS(List<Sensor> v)
        {
            Sensor res = null;
            double min = Double.MaxValue;
            foreach (Sensor i in v)
            {
                if (min > i.FS)
                {
                    res = i;
                    min = i.FS;
                }
            }
            return res;
        }

        private double Formula_9(Sensor sensor)
        {
            double Residual_Energy_Percentage = Formula_8(sensor);
            double Ta = PublicParamerters.Ta;
            double T = PublicParamerters.T;
            //假设公式9中指数值为1
            double FSi = Ta / (T * Math.Pow(Residual_Energy_Percentage, 1));
            return FSi;
        }

        private double Formula_8(Sensor sensor)
        {
            double g = 10;
            double rate = sensor.ResidualEnergy / PublicParamerters.BatteryIntialEnergy;
            return Math.Ceiling(rate * g);
        }

        private void Coverage_Click(object sender, RoutedEventArgs e)
        {
            if (!Settings.Default.IsIntialized)
            {
                if (myNetWork.Count > 0)
                {
                    MenuItem item = sender as MenuItem;
                    string Header = item.Name.ToString();
                    switch (Header)
                    {
                        case "btn_Random":
                            {
                                RandomDeplayment(0);
                            }

                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Please imort the nodes from Db.");
                }
            }
            else
            {
                MessageBox.Show("Network is deployed already. please clear first if you want to re-deploy.");
            }
        }

       
        private void Display_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string Header = item.Name.ToString();
            switch (Header)
            {
                case "_show_id":
                    foreach (Sensor sensro in myNetWork)
                    {
                        if (sensro.lbl_Sensing_ID.Visibility == Visibility.Hidden)
                        {
                            sensro.lbl_Sensing_ID.Visibility = Visibility.Visible;
                            sensro.lbl_hops_to_sink.Visibility = Visibility.Visible;

                            Settings.Default.ShowID = true;
                        }
                        else
                        {
                            sensro.lbl_Sensing_ID.Visibility = Visibility.Hidden;
                            sensro.lbl_hops_to_sink.Visibility = Visibility.Hidden;
                            Settings.Default.ShowID = false;
                        }
                    }
                    break;

                case "_show_sen_range":
                    foreach (Sensor sensro in myNetWork)
                    {
                        if (sensro.Ellipse_Sensing_range.Visibility == Visibility.Hidden)
                        {
                            sensro.Ellipse_Sensing_range.Visibility = Visibility.Visible;
                            Settings.Default.ShowSensingRange = true;
                        }
                        else
                        {
                            sensro.Ellipse_Sensing_range.Visibility = Visibility.Hidden;
                            Settings.Default.ShowSensingRange = false;
                        }
                    }
                    break;
                case "_show_com_range":
                    foreach (Sensor sensro in myNetWork)
                    {
                        if (sensro.Ellipse_Communication_range.Visibility == Visibility.Hidden)
                        {
                           // sensro.Ellipse_Communication_range.Visibility = Visibility.Visible;
                            Settings.Default.ShowComunicationRange = true;
                        }
                        else
                        {
                            sensro.Ellipse_Communication_range.Visibility = Visibility.Hidden;
                            Settings.Default.ShowComunicationRange = false;
                        }
                    }
                    break;
              
                case "_show_battrey":
                    foreach (Sensor sensro in myNetWork)
                    {
                        if (sensro.Prog_batteryCapacityNotation.Visibility == Visibility.Hidden)
                        {
                            sensro.Prog_batteryCapacityNotation.Visibility = Visibility.Visible;
                            Settings.Default.ShowBattry = true;
                        }
                        else
                        {
                            sensro.Prog_batteryCapacityNotation.Visibility = Visibility.Hidden;
                            Settings.Default.ShowBattry = false;
                        }
                    }
                    break;
                case "_Show_Routing_Paths":
                    {
                        if (Settings.Default.ShowRoutingPaths == true)
                        {
                            Settings.Default.ShowRoutingPaths = false;
                        }
                        else
                        {
                            Settings.Default.ShowRoutingPaths = true;
                        }
                    }
                    break;

                case "_Show_Packets_animations":
                    {
                        if (Settings.Default.ShowAnimation == true)
                        {
                            Settings.Default.ShowAnimation = false;
                        }
                        else
                        {
                            Settings.Default.ShowAnimation = true;
                        }
                    }
                    break;
            }
        }

        private void btn_other_Menu(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string Header = item.Header.ToString();
            switch (Header)
            {

                //
                case "_Show Dead Node":
                    {
                        if (myNetWork.Count > 0)
                        {
                            if (PublicParamerters.DeadNodeList.Count > 0)
                            {
                                UiNetworkLifetimeReport xx = new UiNetworkLifetimeReport();
                                xx.Title = "LORA Lifetime report";
                                xx.dg_grid.ItemsSource = PublicParamerters.DeadNodeList;
                                xx.Show();
                            }
                            else
                                MessageBox.Show("No Dead node.");
                        }
                        else
                        {
                            MessageBox.Show("No Network is selected.");
                        }
                    }
                    break;

                case "_Show Resultes":
                    {
                        if (myNetWork.Count > 0)
                        {
                            ExpReport xx = new ExpReport(this);
                            xx.Show();
                        }
                    }
                    break;
                case "_Draw Tree":

                    break;
                case "_Print Info":
                    UIshowSensorsLocations uIlocations = new UIshowSensorsLocations(myNetWork);
                    uIlocations.Show();
                    break;
                case "_Entir Network Routing Log":
                    UiRoutingDetailsLong routingLogs = new ui.UiRoutingDetailsLong(myNetWork);
                    routingLogs.Show();
                    break;
                case "_Log For Each Sensor":

                    break;
                //_Relatives:
                case "_Node Forwarding Probability Distributions":
                    {
                        UiShowLists windsow = new UiShowLists();
                        windsow.Title = "Forwarding Probability Distributions For Each Node";
                        foreach (Sensor source in myNetWork)
                        {
                            
                        }
                        windsow.Show();
                        break;
                    }
                //
                case "_Expermental Results":
                    UIExpermentResults xxxiu = new UIExpermentResults();
                    xxxiu.Show();
                    break;
                case "_Probability Matrix":
                    {
                       
                    }
                    break;
                //
                case "_Packets Paths":
                    UiRecievedPackertsBySink packsInsinkList = new UiRecievedPackertsBySink();
                    packsInsinkList.Show();

                    break;
                //
                case "_Random Numbers":

                    List<KeyValuePair<int, double>> rands = new List<KeyValuePair<int, double>>();
                    int index = 0;
                    foreach (Sensor sen in myNetWork)
                    {
                        foreach (RoutingLog log in sen.Logs)
                        {
                            if (log.IsSend)
                            {
                                index++;
                                rands.Add(new KeyValuePair<int, double>(index, log.ForwardingRandomNumber));
                            }
                        }
                    }

                    UiRandomNumberGeneration wndsow = new ui.UiRandomNumberGeneration();
                    wndsow.chart_x.DataContext = rands;
                    wndsow.Show();

                    break;
                case "_Nodes Load":
                    {
                        /*
                        SegmaManager sgManager = new SegmaManager();
                        Sensor sink = PublicParamerters.SinkNode;
                        List<string> Paths = new List<string>();
                        if (sink != null)
                        {
                            foreach (Packet pck in sink.PacketsList)
                            {
                                Paths.Add(pck.Path);
                            }

                        }*/
                        /*
                        sgManager.Filter(Paths);
                        UiShowLists windsow = new UiShowLists();
                        windsow.Title = "Nodes Load";
                        SegmaCollection collectionx = sgManager.GetCollection;
                        foreach (SegmaSource source in collectionx.GetSourcesList)
                        {
                            source.NumberofPacketsGeneratedByMe = myNetWork[source.SourceID].NumberofPacketsGeneratedByMe;
                            ListControl List = new conts.ListControl();
                            List.lbl_title.Content = "Source:" + source.SourceID + " Pks:" + source.NumberofPacketsGeneratedByMe + " Relays:" + source.RelaysCount + " Hops:" + source.HopsSum + " Mean:" + source.Mean + " Variance:" + source.Veriance + " E:" + source.PathsSpread;
                            List.dg_date.ItemsSource = source.GetRelayNodes;
                            windsow.stack_items.Children.Add(List);
                        }
                        windsow.Show();
                      */
                    }
                    break;
                //_Distintc Paths
                case "_Distintc Paths":
                    {
                        
                        UiShowLists windsow = new UiShowLists();
                        windsow.Title = "Distinct Paths for each Source";
                        DisPathConter dip = new DisPathConter();
                        List<ClassfyPathsPerSource> classfy = dip.ClassyfyDistinctPathsPerSources();
                        foreach (ClassfyPathsPerSource source in classfy)
                        {
                            ListControl List = new conts.ListControl();
                            List.lbl_title.Content = "Source:" + source.SourceID;
                            List.dg_date.ItemsSource = source.DistinctPathsForThisSource;
                            windsow.stack_items.Children.Add(List);
                        }
                        windsow.Show();
                       
                    }
                    break;
            }
        }

        int rounds = 0;
        int alreadPassedRound = 0;

        private void Btn_rounds_uplinks_mousedown(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.IsIntialized)
            {
                MenuItem slected = sender as MenuItem;
                int rnd = Convert.ToInt16(slected.Header.ToString().Split('_')[1]);
              

                 rounds = rnd;
                 alreadPassedRound = 0;
              
                RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(5);
                RandomSelectSourceNodesTimer.Start();
                RandomSelectSourceNodesTimer.Tick += RoundsPacketsGeneator; 
                

            }
            else
            {
                MessageBox.Show("Please selete the coverage.Coverage->Random");
            }
        }

        private void RoundsPacketsGeneator(object sender, EventArgs e)
        {
            alreadPassedRound++;
            if (alreadPassedRound <= rounds)
            {
                lbl_rounds.Content = alreadPassedRound;
                foreach (Sensor sen in myNetWork)
                {
                    if (sen.ID != PublicParamerters.SinkNode.ID)
                    {
                        sen.GenerateDataPacket();
                    }
                }
            }
            else
            {
                RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(0);
                RandomSelectSourceNodesTimer.Stop();
            }
        }



        private void Btn_rounds_downlinks_mousedown(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.IsIntialized)
            {
                // not random:
                MenuItem slected = sender as MenuItem;
                int pktsNumber = Convert.ToInt16(slected.Header.ToString().Split('_')[1]);
                rounds += pktsNumber;
                lbl_rounds.Content = rounds;

                for (int i = 1; i <= pktsNumber; i++)
                {
                    foreach (Sensor sen in myNetWork)
                    {
                        PublicParamerters.SinkNode.GenerateControlPacket(sen);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please selete the coverage.Coverage->Random");
            }
        }

        private void BuildTheTree(object sender, RoutedEventArgs e)
        {

        }

        private void tconrol_charts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        public void ClearExperment()
        {
            try
            {
                PublicParamerters.NumberofDropedPacket = 0;
                PublicParamerters.DropedbecauseofCannotSend = 0;
                PublicParamerters.DropedbecauseofNoEnergy = 0;
                PublicParamerters.DropedbecauseofTTL = 0;
                PublicParamerters.NumberofDeliveredPacket = 0;
                PublicParamerters.Rounds = 0;
                PublicParamerters.DeadNodeList.Clear();
                PublicParamerters.NumberofGeneratedPackets = 0;
                PublicParamerters.TotalWaitingTime = 0;
                PublicParamerters.TotalReduntantTransmission = 0;
                PublicParamerters.IsNetworkDied = false;
                PublicParamerters.Density = 0;
                PublicParamerters.TotalDelayMs =0;
                PublicParamerters.TotalEnergyConsumptionJoule = 0;
                PublicParamerters.TotalWastedEnergyJoule = 0;

                PublicParamerters.TotalWastedEnergyJoule = 0;
                PublicParamerters.TotalDelayMs = 0;
                PublicParamerters.NetworkName = "";
                PublicParamerters.FinishedRoutedPackets.Clear();
                PublicParamerters.NumberofNodes = 0;
                PublicParamerters.NOS = 0;
                PublicParamerters.Rounds = 0;
                PublicParamerters.SinkNode = null;

                PublicParamerters.IsNetworkDied = false;
                PublicParamerters.Density = 0;
                PublicParamerters.NetworkName = "";
                PublicParamerters.DeadNodeList.Clear();
                PublicParamerters.NOP = 0;
                PublicParamerters.NOS = 0;
                PublicParamerters.Rounds = 0;
                PublicParamerters.SinkNode = null;

                top_menu.IsEnabled = true;
                
                Canvas_SensingFeild.Children.Clear();
                if (myNetWork != null)
                    myNetWork.Clear();

                isCoverageSelected = false;


                HideSimulationParameters();
                col_Path_Efficiency.DataContext = null;
                col_Delay.DataContext = null;
                col_EnergyConsumptionForEachNode.DataContext = null;

              

                cols_hops_ditrubtions.DataContext = null;
                lbl_PowersString.Content = "";
                cols_hops_ditrubtions.DataContext = null;
                cols_energy_distribution.DataContext = null;
                cols_delay_distribution.DataContext = null;

               

                

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }


        private void ben_clear_click(object sender, RoutedEventArgs e)
        {
            TimerCounter.Stop();
            RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(0);
            RandomSelectSourceNodesTimer.Stop();

            Settings.Default.IsIntialized = false;

            ClearExperment();

        }



        public object NetworkLifeTime { get; private set; }

        private void tab_network_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           

        }

        private void lbl_show_grid_line_x_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (col_network_X_Gird.ShowGridLines == false) col_network_X_Gird.ShowGridLines = true;
            else col_network_X_Gird.ShowGridLines = false;
        }

        private void lbl_show_grid_line_y_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (col_network_Y_Gird.ShowGridLines == false) col_network_Y_Gird.ShowGridLines = true;
            else col_network_Y_Gird.ShowGridLines = false;
        }



        private void setDisributaions_Click(object sender, RoutedEventArgs e)
        {
            if (myNetWork.Count == 0)
            {
                UIPowers cc = new ui.UIPowers(this);
                cc.Show();
            }
            else
            {
                MessageBox.Show("These Parameters can not be set after deploying the nodes. please clear the feild and re-set.");
            }
        }


        private void _set_paramertes_Click(object sender, RoutedEventArgs e)
        {
            /*
            ben_clear_click(sender, e);

            UiMultipleExperments setpa = new UiMultipleExperments(this);
            this.WindowState = WindowState.Minimized;
            setpa.Show();*/

        }



        private void btn_chek_lifetime_Click(object sender, RoutedEventArgs e)
        {
            if (isCoverageSelected)
            {
                this.WindowState = WindowState.Minimized;
                for (int i = 0; ; i++)
                {
                    rounds++;
                    lbl_rounds.Content = rounds;
                    if (!PublicParamerters.IsNetworkDied)
                    {
                        foreach (Sensor sen in myNetWork)
                        {
                            if (sen.ID != PublicParamerters.SinkNode.ID)
                            {
                                sen.GenerateDataPacket();
                              //  sen.GeneratePacketAndSent(false, Settings.Default.EnergyDistCnt,
                              //  Settings.Default.TransDistanceDistCnt, Settings.Default.DirectionDistCnt, Settings.Default.PrepDistanceDistCnt);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please selete the coverage. Coverage->Random");
            }
        }

        private void btn_lifetime_s1_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.IsIntialized == false)
            {
                RandomDeplayment(0);
                UiComputeLifeTime lifewin = new UiComputeLifeTime(this);
                lifewin.Show();
                lifewin.Owner = this;
                top_menu.IsEnabled = false;
                Settings.Default.IsIntialized = true;
            }
            else
            {
                MessageBox.Show("File->clear and try agian.");
            }

           
        }


        

        /// <summary>
        /// _Randomly Select Nodes With Distance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnCon_RandomlySelectNodesWithDistance_Click(object sender, RoutedEventArgs e)
        {
            if (isCoverageSelected)
            {
                if (PublicParamerters.FinishedRoutedPackets.Count == 0)
                {
                    ui.UiSelectNodesWidthDistance win = new UiSelectNodesWidthDistance(this);
                    win.Show();
                }
                else
                {
                    MessageBox.Show("Please clear first: File->Clear!");
                }
            }
            else
            {
                MessageBox.Show("Please selected the Coverage.Coverage->Random");
            }

        }

        public void SendPackectPerSecond(double s)
        {
            if (s == 0)
            {
                Settings.Default.AnimationSpeed = s;
                RandomSelectSourceNodesTimer.Stop();
                PacketRate = "1 packet per " + s + " s";
            }
            else
            {
                if (s > 1) Settings.Default.AnimationSpeed = 0.5; else Settings.Default.AnimationSpeed = s;
                RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(s);//s为定时器周期
                RandomSelectSourceNodesTimer.Start();
                RandomSelectSourceNodesTimer.Tick += RandomSelectNodes_Tick;//RandomSelectNodes_Tick为定时器执行的具体内容
                PacketRate = "1 packet per " + s + " s";
            }
        }

        private void btn_select_sources_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string Header = item.Header.ToString();
            if (Settings.Default.IsIntialized)
            {
                switch (Header)
                {
                    case "1pck/1s":
                        SendPackectPerSecond(1);
                        break;
                    case "1pck/2s":
                        SendPackectPerSecond(2);
                        break;
                    case "1pck/4s":
                        SendPackectPerSecond(4);
                        break;
                    case "1pck/6s":
                        SendPackectPerSecond(6);
                        break;
                    case "1pck/8s":
                        SendPackectPerSecond(8);
                        break;
                    case "1pck/10s":
                        SendPackectPerSecond(10);
                        break;
                    case "1pck/0s(Stop)":
                        SendPackectPerSecond(0);
                        break;
                    case "1pck/0.1s":
                        SendPackectPerSecond(0.1);
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please select Coverage->Random. then continue.");
            }
        }


        #region Upink Generator //////////////////////////////////////////////////////////////////////
         int UplinkTobeGeneratedPackets = 0;
         int UplinkalreadyGeneratedPackets = 0;

        public void GenerateUplinkPacketsRandomly(int numofPackets)
        {
            UplinkTobeGeneratedPackets = 0;
            UplinkalreadyGeneratedPackets = 0;

            UplinkTobeGeneratedPackets = numofPackets;
            RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(0.01);
            RandomSelectSourceNodesTimer.Start();
            RandomSelectSourceNodesTimer.Tick += UplinkPacketsGenerate_Tirk;
        }
         
        private void UplinkPacketsGenerate_Tirk(object sender, EventArgs e)
        {
            if (PublicParamerters.SimulationTime > PublicParamerters.MacStartUp)
            {
                UplinkalreadyGeneratedPackets++;
                if (UplinkalreadyGeneratedPackets <= UplinkTobeGeneratedPackets)
                {
                    int index = 1 + Convert.ToInt16(UnformRandomNumberGenerator.GetUniform(PublicParamerters.NumberofNodes - 2));
                    myNetWork[index].GenerateDataPacket();
                }
                else
                {
                    RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(0);
                    RandomSelectSourceNodesTimer.Stop();
                    UplinkalreadyGeneratedPackets = 0;
                    UplinkTobeGeneratedPackets = 0;
                }
            }
                
        }

        private void btn_uplLINK_send_numbr_of_packets(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string Header = item.Header.ToString();
            if (Settings.Default.IsIntialized)
            {
                int Header_int = Convert.ToInt16(Header);
                GenerateUplinkPacketsRandomly(Header_int);
            }
            else
            {
                MessageBox.Show("Please select Coverage->Random. then continue.");
            }
        }

        #endregion ///////////////////////////////////////////////////////////////


      


        int DownlinkTobeGenerated = 0;
        int DownlinkAlreadyGenerated = 0;

        public void GenerateDownLinkPacketRandomly(int numofpackets)
        {
            DownlinkTobeGenerated = 0;
            DownlinkAlreadyGenerated = 0;

            DownlinkTobeGenerated = numofpackets;
            RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(0.01);
            RandomSelectSourceNodesTimer.Start();
            RandomSelectSourceNodesTimer.Tick += DownLINKRandomSentAnumberofPackets;
        }

        private void btn_DOWNN_send_numbr_of_packets(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string Header = item.Header.ToString();
            if (Settings.Default.IsIntialized)
            {
                int Header_int = Convert.ToInt16(Header);
                GenerateDownLinkPacketRandomly(Header_int);

            }
            else
            {
                MessageBox.Show("Please select Coverage->Random. then continue.");
            }
        }

        private void DownLINKRandomSentAnumberofPackets(object sender, EventArgs e)
        {
            DownlinkAlreadyGenerated++;
            if (DownlinkAlreadyGenerated <= DownlinkTobeGenerated)
            {
                int index = Convert.ToInt16(UnformRandomNumberGenerator.GetUniform(PublicParamerters.NumberofNodes - 2));
                Sensor EndNode = myNetWork[index];
                PublicParamerters.SinkNode.GenerateControlPacket(EndNode);
            }
            else
            {
                RandomSelectSourceNodesTimer.Interval = TimeSpan.FromSeconds(0);
                RandomSelectSourceNodesTimer.Stop();
                DownlinkAlreadyGenerated = 0;
                DownlinkTobeGenerated = 0;
            }
        }


        private void btn_simTime_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string Header = item.Header.ToString();
            if (Settings.Default.IsIntialized)
            {
                stopSimlationWhen = Convert.ToInt32(Header.ToString());
                menSimuTim.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Please select Coverage->Random. then continue.");
            }
        }

        private void Btn_comuputeEnergyCon_withinTime_Click(object sender, RoutedEventArgs e)
        {

            if (Settings.Default.IsIntialized)
            {
                MessageBox.Show("File->clear and try agian.");
            }
            else
            {
                PacketRate = "";
                stopSimlationWhen = 0;
                //UISetParEnerConsum con 是窗口，其内容由构造函数确定，即赋值号右侧函数，con中的start按钮将进行网络初始化以及实验的运行
                UISetParEnerConsum con = new UISetParEnerConsum(this);
                con.Owner = this;
                con.Show();
                top_menu.IsEnabled = false;
            }
        }

        public double StreenTimes = 1;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sctim = StreenTimes / 10;
            double x = _slider.Value;
            if (x <= sctim)
            {
                x = sctim;
                Settings.Default.SliderValue = x;
                Settings.Default.Save();
            }
            var scaler = Canvas_SensingFeild.LayoutTransform as ScaleTransform;
            Canvas_SensingFeild.LayoutTransform = new ScaleTransform(x, x, SystemParameters.FullPrimaryScreenWidth / 2, SystemParameters.FullPrimaryScreenHeight / 2);
            lbl_zome_percentage.Text = (x * 100).ToString() + "%";


            Settings.Default.SliderValue = x;
            Settings.Default.Save();


        }

        private void Canvas_SensingFeild_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;
            if (e.Delta > 0)
            {
                _slider.Value += 0.1;
            }
            else if (e.Delta < 0)
            {
                _slider.Value -= 0.1;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _slider.Value = Settings.Default.SliderValue;
            Settings.Default.IsIntialized = false;
        }
    }
}



           
