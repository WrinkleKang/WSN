using MiniSDN.Intilization;
using MiniSDN.Energy;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MiniSDN.ui;
using MiniSDN.Properties;
using System.Windows.Threading;
using System.Threading;
using MiniSDN.ControlPlane.NOS;
using MiniSDN.ui.conts;
using MiniSDN.ControlPlane.NOS.FlowEngin;
using MiniSDN.Forwarding;
using MiniSDN.Dataplane.PacketRouter;
using MiniSDN.Dataplane.NOS;


namespace MiniSDN.Dataplane
{
    public enum SensorState { intalized, Active, Sleep } // defualt is not used. i 
    public enum EnergyConsumption { Transmit, Recive } // defualt is not used. i 


    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Sensor : UserControl
    {
        #region Commone parameters.

        public Radar Myradar;
        public List<Arrow> MyArrows = new List<Arrow>();
        public MainWindow MainWindow { get; set; } // the mian window where the sensor deployed.
        public static double SR { get; set; } // the radios of SENSING range.
        public double SensingRangeRadius { get { return SR; } }
        public static double CR { get; set; }  // the radios of COMUNICATION range. double OF SENSING RANGE
        public double ComunicationRangeRadius { get { return CR; } }
        public double BatteryIntialEnergy; // jouls // value will not be changed
        private double _ResidualEnergy; //// jouls this value will be changed according to useage of battery
        public List<int> DutyCycleString = new List<int>(); // return the first letter of each state.
        public BoXMAC Mac { get; set; } // the mac protocol for the node.
        public SensorState CurrentSensorState { get; set; } // state of node.
        public List<RoutingLog> Logs = new List<RoutingLog>();
        public List<NeighborsTableEntry> NeighborsTable = null; // neighboring table.
        public List<MiniFlowTableEntry> MiniFlowTable = new List<MiniFlowTableEntry>(); //flow table.
        public int NumberofPacketsGeneratedByMe = 0; // the number of packets sent by this packet.
        public FirstOrderRadioModel EnergyModel = new FirstOrderRadioModel(); // energy model.
        public int ID { get; set; } // the ID of sensor.
        public int HopsToSink = int.MaxValue; // number of hops from the node to the sink.
        public bool trun { get; set; }// this will be true if the node is already sent the beacon packet for discovering the number of hops to the sink.
        private DispatcherTimer SendPacketTimer = new DispatcherTimer();// 
        private DispatcherTimer QueuTimer = new DispatcherTimer();// to check the packets in the queue right now.
        public Queue<Packet> WaitingPacketsQueue = new Queue<Packet>(); // packets queue.

        public Queue<Packet> NewWaitingPacketsQueue = new Queue<Packet>(); // 接收的数据包都会在等待队列中


        public List<BatRange> BatRangesList = new List<Energy.BatRange>();


        public bool TransmitState = false; //标记位，表示是否处于传输模式，1表示正在传输数据包 0表示不在传输数据包


        public double UsedEnergy = 0;//节点使用的能量


        public double Energy_Used_IN_Data_Packet { get; set; } //节点数据包耗能
        //节点数据包能耗占比 = 节点数据包耗能/节点使用的能量
        public double Energy_Used_IN_Data_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Data_Packet / UsedEnergy), 2); } }
        public double Energy_Used_IN_Send_Data_Packet { get; set; }//节点发送数据包耗能
        //节点发送数据包能耗占比 = 节点发送数据包耗能/节点数据包耗能
        public double Energy_Used_IN_Send_Data_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Send_Data_Packet / Energy_Used_IN_Data_Packet), 2); } }

        public double Energy_Used_IN_Receive_Data_Packet { get; set; }//节点接收数据包耗能
        //节点接收数据包能耗占比 = 节点接收数据包耗能/节点数据包耗能
        public double Energy_Used_IN_Receive_Data_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Receive_Data_Packet / Energy_Used_IN_Data_Packet), 2); } }


        public double Energy_Used_IN_Preamble_Packet { get; set; }//节点preamble包耗能
        //节点preamble能耗占比 = 节点preamble包耗能/节点使用的能耗
        public double Energy_Used_IN_Preamble_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Preamble_Packet / UsedEnergy), 2); } }

        public double Energy_Used_IN_Send_Preamble_Packet { get; set; }//节点发送preamble包耗能
        //节点发送preamble能耗占比 = 节点发送preamble包耗能/节点preamble包耗能
        public double Energy_Used_IN_Send_Preamble_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Send_Preamble_Packet / Energy_Used_IN_Preamble_Packet), 2); } }

        public double Energy_Used_IN_Receive_Preamble_Packet { get; set; }//节点接收preamble包耗能
        //节点接收preamble能耗占比 = 节点接收preamble包能耗/节点preamble包耗能
        public double Energy_Used_IN_Receive_Preamble_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Receive_Preamble_Packet / Energy_Used_IN_Preamble_Packet), 2); } }


        public double Energy_Used_IN_ACK_Packet { get; set; }//节点ACK包耗能
        //节点ACK包能耗占比 = 节点ACK包耗能/节点使用的能耗
        public double Energy_Used_IN_ACK_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_ACK_Packet / UsedEnergy), 2); } }

        public double Energy_Used_IN_Send_ACK_Packet { get; set; }//节点发送ACK包耗能
        //节点发送ACK包能耗占比 = 节点发送ACK包耗能/节点ACK包耗能
        public double Energy_Used_IN_Send_ACK_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Send_ACK_Packet / Energy_Used_IN_ACK_Packet), 2); } }

        public double Energy_Used_IN_Receive_ACK_Packet { get; set; }//节点接收ACK包耗能
        //节点接收ACK包能耗占比 = 节点接收ACK包能耗/节点ACK包能耗
        public double Energy_Used_IN_Receive_ACK_Packet_Percentage { get { return Math.Round(100 * (Energy_Used_IN_Receive_ACK_Packet / Energy_Used_IN_ACK_Packet), 2); } }






        /// <summary>
        /// CONFROM FROM NANO NO JOUL
        /// </summary>
        /// <param name="UsedEnergy_Nanojoule"></param>
        /// <returns></returns>
        public double ConvertToJoule(double UsedEnergy_Nanojoule) //the energy used for current operation
        {
            double _e9 = 1000000000; // 1*e^-9
            double _ONE = 1;
            double oNE_DIVIDE_e9 = _ONE / _e9;
            double re = UsedEnergy_Nanojoule * oNE_DIVIDE_e9;
            return re;
        }




        /*  public void MainWindowUpdataMessage()
          {
              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_total_consumed_energy.Content = PublicParamerters.TotalEnergyConsumptionJoule + " (JOULS)", DispatcherPriority.Send);

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_num_of_gen_packets.Content = PublicParamerters.NumberofGeneratedPackets, DispatcherPriority.Normal);

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_nymber_inQueu.Content = PublicParamerters.InQueuePackets.ToString());

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Delivered_Packet.Content = PublicParamerters.NumberofDeliveredPacket, DispatcherPriority.Send);

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_number_of_control_packets.Content = PublicParamerters.NumberofControlPackets, DispatcherPriority.Normal);

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Droped_Packet.Content = PublicParamerters.NumberofDropedPacket, DispatcherPriority.Send);

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_sucess_ratio.Content = PublicParamerters.DeliveredRatio, DispatcherPriority.Send);

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Redundant_packets.Content = PublicParamerters.TotalReduntantTransmission);

              MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Wasted_Energy_percentage.Content = PublicParamerters.WastedEnergyPercentage);




          }
          */

        /// <summary>
        /// in JOULE
        /// </summary>
        public double ResidualEnergy // jouls this value will be changed according to useage of battery
        {
            get { return _ResidualEnergy; }
            set
            {
                _ResidualEnergy = value;
                Prog_batteryCapacityNotation.Value = _ResidualEnergy;
            }
        } //@unit(JOULS);


        /// <summary>
        /// 0%-100%
        /// </summary>
        public double ResidualEnergyPercentage
        {
            get { return Math.Round((ResidualEnergy / BatteryIntialEnergy) * 100, 2); }
        }

        public double UsedEnergyPercentage
        {
            get
            {


                return Math.Round((UsedEnergy / BatteryIntialEnergy) * 100, 2);
            }

        }
        /// <summary>
        /// visualized sensing range and comuinication range
        /// </summary>
        public double VisualizedRadius
        {
            get { return Ellipse_Sensing_range.Width / 2; }
            set
            {
                // sensing range:
                Ellipse_Sensing_range.Height = value * 2; // heigh= sen rad*2;
                Ellipse_Sensing_range.Width = value * 2; // Width= sen rad*2;
                SR = VisualizedRadius;
                CR = SR * 2; // comunication rad= sensing rad *2;

                // device:
                Device_Sensor.Width = value * 4; // device = sen rad*4;
                Device_Sensor.Height = value * 4;
                // communication range
                Ellipse_Communication_range.Height = value * 4; // com rang= sen rad *4;
                Ellipse_Communication_range.Width = value * 4;

                // battery:
                Prog_batteryCapacityNotation.Width = 8;
                Prog_batteryCapacityNotation.Height = 2;
            }
        }

        /// <summary>
        /// Real postion of object.
        /// </summary>
        public Point Position
        {
            get
            {
                double x = Device_Sensor.Margin.Left;
                double y = Device_Sensor.Margin.Top;
                Point p = new Point(x, y);
                return p;
            }
            set
            {
                Point p = value;
                Device_Sensor.Margin = new Thickness(p.X, p.Y, 0, 0);
            }
        }

        /// <summary>
        /// center location of node.
        /// </summary>
        public Point CenterLocation
        {
            get
            {
                double x = Device_Sensor.Margin.Left;
                double y = Device_Sensor.Margin.Top;
                Point p = new Point(x + CR, y + CR);
                return p;
            }
        }

        bool StartMove = false; // mouse start move.
        private void Device_Sensor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.Default.IsIntialized == false)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    System.Windows.Point P = e.GetPosition(MainWindow.Canvas_SensingFeild);
                    P.X = P.X - CR;
                    P.Y = P.Y - CR;
                    Position = P;
                    StartMove = true;
                }
            }
        }

        private void Device_Sensor_MouseMove(object sender, MouseEventArgs e)
        {
            if (Settings.Default.IsIntialized == false)
            {
                if (StartMove)
                {
                    System.Windows.Point P = e.GetPosition(MainWindow.Canvas_SensingFeild);
                    P.X = P.X - CR;
                    P.Y = P.Y - CR;
                    this.Position = P;
                }
            }
        }

        private void Device_Sensor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StartMove = false;
        }





        //每当节点能量发生变化时都会执行此函数
        private void Prog_batteryCapacityNotation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {



            double val = ResidualEnergyPercentage;
            if (val <= 0)
            {
                MainWindow.RandomSelectSourceNodesTimer.Stop();

                // dead certificate:
                ExpermentsResults.Lifetime.DeadNodesRecord recod = new ExpermentsResults.Lifetime.DeadNodesRecord();
                recod.DeadAfterPackets = PublicParamerters.NumberofGeneratedPackets;
                recod.DeadOrder = PublicParamerters.DeadNodeList.Count + 1;
                recod.Rounds = PublicParamerters.Rounds + 1;
                recod.DeadNodeID = ID;
                recod.NOS = PublicParamerters.NOS;
                recod.NOP = PublicParamerters.NOP;
                PublicParamerters.DeadNodeList.Add(recod);

                Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col0));
                Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col0));


                if (Settings.Default.StopeWhenFirstNodeDeid)
                {
                    //  MainWindow.TimerCounter.Stop();
                    MainWindow.RandomSelectSourceNodesTimer.Stop();
                    MainWindow.stopSimlationWhen = PublicParamerters.SimulationTime;
                    MainWindow.top_menu.IsEnabled = true;
                }
                Mac.SwichToSleep();
                Mac.SwichOnTimer.Stop();
                Mac.ActiveSleepTimer.Stop();
                if (this.ResidualEnergy <= 0)
                {
                    while (this.NewWaitingPacketsQueue.Count > 0)
                    {
                        PublicParamerters.NumberofDropedPacket += 1;
                        PublicParamerters.DropedbecauseofNoEnergy += 1;//能量耗尽丢弃的数据包
                        Packet pack = NewWaitingPacketsQueue.Dequeue();
                        PublicParamerters.NumberofInAllQueuePackets -= 1;
                        pack.isDelivered = false;
                        PublicParamerters.FinishedRoutedPackets.Add(pack);
                        Console.WriteLine("PID:" + pack.PID + " has been droped.");
                        //MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Droped_Packet.Content = PublicParamerters.NumberofDropedPacket, DispatcherPriority.Send);
                        MainWindow.MainWindowUpdataMessage();


                    }

                    if (Settings.Default.ShowRadar) Myradar.StopRadio();

                    Console.WriteLine("NID:" + this.ID + ". Queu Timer is stoped.");
                    MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.Transparent);
                    MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Hidden);

                    return;
                }
                return;


            }
            if (val >= 1 && val <= 9)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col1_9)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col1_9)));
            }

            if (val >= 10 && val <= 19)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col10_19)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col10_19)));
            }

            if (val >= 20 && val <= 29)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col20_29)));
                Dispatcher.Invoke(() => Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col20_29))));
            }

            // full:
            if (val >= 30 && val <= 39)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col30_39)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col30_39)));
            }
            // full:
            if (val >= 40 && val <= 49)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col40_49)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col40_49)));
            }
            // full:
            if (val >= 50 && val <= 59)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col50_59)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col50_59)));
            }
            // full:
            if (val >= 60 && val <= 69)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col60_69)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col60_69)));
            }
            // full:
            if (val >= 70 && val <= 79)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col70_79)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col70_79)));
            }
            // full:
            if (val >= 80 && val <= 89)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col80_89)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col80_89)));
            }
            // full:
            if (val >= 90 && val <= 100)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col90_100)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col90_100)));
            }



            // update the battery distrubtion.
            int battper = Convert.ToInt16(val);
            if (battper > PublicParamerters.UpdateLossPercentage)
            {
                int rangeIndex = battper / PublicParamerters.UpdateLossPercentage;
                if (rangeIndex >= 1)
                {
                    if (BatRangesList.Count > 0)
                    {
                        BatRange range = BatRangesList[rangeIndex - 1];
                        if (battper >= range.Rang[0] && battper <= range.Rang[1])
                        {
                            if (range.isUpdated == false)
                            {
                                range.isUpdated = true;
                                // update the uplink.
                                UplinkRouting.UpdateUplinkFlowEnery(this);

                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// show or hide the arrow in seperated thread.
        /// </summary>
        /// <param name="id"></param>
        public void ShowOrHideArrow(int id)
        {
            Thread thread = new Thread(() =>
            {
                lock (MyArrows)
                {
                    Arrow ar = GetArrow(id);
                    if (ar != null)
                    {
                        lock (ar)
                        {
                            if (ar.Visibility == Visibility.Visible)
                            {
                                Action action = () => ar.Visibility = Visibility.Hidden;
                                Dispatcher.Invoke(action);
                            }
                            else
                            {
                                Action action = () => ar.Visibility = Visibility.Visible;
                                Dispatcher.Invoke(action);
                            }
                        }
                    }
                }
            }
            );
            thread.Start();
        }


        // get arrow by ID.
        private Arrow GetArrow(int EndPointID)
        {
            foreach (Arrow arr in MyArrows) { if (arr.To.ID == EndPointID) return arr; }
            return null;
        }





        #endregion






        /// <summary>
        /// 
        /// </summary>
        public void SwichToActive()
        {
            Mac.SwichToActive();

        }

        /// <summary>
        /// 
        /// </summary>
        private void SwichToSleep()
        {
            Mac.SwichToSleep();
        }

        public void HavePactketsToSend()
        {
          //  Packet packet = NewWaitingPacketsQueue.Peek();//复制队头数据包，等发送成功后再将其删除

            //this.SendPacekt(packet);//原始版本的发包函数，便于计算方便


            NewSendPackets();



        }



        public Sensor(int nodeID)
        {
            InitializeComponent();
            //: sink is diffrent:
            if (nodeID == 0) BatteryIntialEnergy = PublicParamerters.BatteryIntialEnergyForSink; // the value will not be change
            else
                BatteryIntialEnergy = PublicParamerters.BatteryIntialEnergy;


            ResidualEnergy = BatteryIntialEnergy;// joules. intializing.
            Prog_batteryCapacityNotation.Value = BatteryIntialEnergy;
            Prog_batteryCapacityNotation.Maximum = BatteryIntialEnergy;
            lbl_Sensing_ID.Content = nodeID;
            ID = nodeID;
            QueuTimer.Interval = PublicParamerters.QueueTime;
            QueuTimer.Tick += DeliveerPacketsInQueuTimer_Tick;
            //:

            SendPacketTimer.Interval = TimeSpan.FromSeconds(1);


        }



        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {


        }

        /// <summary>
        /// hide all arrows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            /*
            Vertex ver = MainWindow.MyGraph[ID];
            foreach(Vertex v in ver.Candidates)
            {
                MainWindow.myNetWork[v.ID].lbl_Sensing_ID.Background = Brushes.Black;
            }*/

        }



        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }



        public int ComputeMaxHopsUplink
        {
            get
            {
                double DIS = Operations.DistanceBetweenTwoSensors(PublicParamerters.SinkNode, this);
                return Convert.ToInt16(Math.Ceiling((Math.Sqrt(PublicParamerters.Density) * (DIS / ComunicationRangeRadius))));
            }
        }

        public int ComputeMaxHopsDownlink(Sensor endNode)
        {
            double DIS = Operations.DistanceBetweenTwoSensors(PublicParamerters.SinkNode, endNode);
            return Convert.ToInt16(Math.Ceiling((Math.Sqrt(PublicParamerters.Density) * (DIS / ComunicationRangeRadius))));
        }

        #region send data: /////////////////////////////////////////////////////////////////////////////


        public void IdentifySourceNode(Sensor source)
        {
            if (Settings.Default.ShowAnimation)
            {
                Action actionx = () => source.Ellipse_indicator.Visibility = Visibility.Visible;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => source.Ellipse_indicator.Fill = Brushes.Yellow;
                Dispatcher.Invoke(actionxx);
            }
        }

        public void UnIdentifySourceNode(Sensor source)
        {
            if (Settings.Default.ShowAnimation)
            {
                Action actionx = () => source.Ellipse_indicator.Visibility = Visibility.Hidden;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => source.Ellipse_indicator.Fill = Brushes.Transparent;
                Dispatcher.Invoke(actionxx);
            }
        }

        /// <summary>
        /// uplink routing packets
        /// </summary>
        public void GenerateDataPacket()
        {
            //有剩余能量时既满足生成数据包的条件，该条件是否可以修改？不修改可能会导致能量为负值的情况。
            if (Settings.Default.IsIntialized && this.ResidualEnergy > 0)
            {

                PublicParamerters.NumberofGeneratedPackets += 1;   //数据包总数+1


                //打包数据包的相关信息，包括起始ID，生存时间，源节点，数据包长度，数据包类型，数据包ID，以及目的节点等
                Packet packet = new Packet();
                packet.Path = "" + this.ID;
                packet.TimeToLive = this.ComputeMaxHopsUplink;
                packet.Source = this;
                packet.PacketLength = PublicParamerters.RoutingDataLength;
                packet.PacketType = PacketType.Data;
                packet.PID = PublicParamerters.NumberofGeneratedPackets;
                packet.Destination = PublicParamerters.SinkNode;
                IdentifySourceNode(this);
                // MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_num_of_gen_packets.Content = PublicParamerters.NumberofGeneratedPackets, DispatcherPriority.Normal);


                //更新主窗口右侧信息
                MainWindow.MainWindowUpdataMessage();

                //将数据包加入到选中节点的等待队列中然后唤醒该节点。

                NewWaitingPacketsQueue.Enqueue(packet);

                PublicParamerters.NumberofInAllQueuePackets += 1;//系统生成的数据包在节点的等待队列中

                this.SwichToActive();


                //:准备发送数据包给下一跳节点
                //this.SendPacekt(packet);



            }
        }



        public void GenerateMultipleDataPackets(int numOfPackets)
        {
            for (int i = 0; i < numOfPackets; i++)
            {
                GenerateDataPacket();
                //  Thread.Sleep(50);
            }
        }





        /// <summary>
        /// downlink
        /// </summary>
        /// <param name="Destination"></param>
        public void GenerateControlPacket(Sensor endNode)
        {
            if (Settings.Default.IsIntialized && this.ResidualEnergy > 0)
            {

                PublicParamerters.NumberofGeneratedPackets += 1; // all packets.
                PublicParamerters.NumberofControlPackets += 1; // this for control.
                Packet packet = new Packet();
                packet.Path = "" + this.ID;
                packet.TimeToLive = ComputeMaxHopsDownlink(endNode);
                packet.Source = PublicParamerters.SinkNode; // the sink.
                packet.PacketLength = PublicParamerters.ControlDataLength;
                packet.PacketType = PacketType.Control;
                packet.PID = PublicParamerters.NumberofGeneratedPackets;
                packet.Destination = endNode;
                IdentifyEndNode(endNode);

                // MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_number_of_control_packets.Content = PublicParamerters.NumberofControlPackets, DispatcherPriority.Normal);
                // MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_num_of_gen_packets.Content = PublicParamerters.NumberofGeneratedPackets, DispatcherPriority.Normal);
                MainWindow.MainWindowUpdataMessage();

                this.SendPacekt(packet);

                // Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
        /// <summary>
        /// to the same endnode.
        /// </summary>
        /// <param name="numOfPackets"></param>
        /// <param name="endone"></param>
        public void GenerateMultipleControlPackets(int numOfPackets, Sensor endone)
        {
            for (int i = 0; i < numOfPackets; i++)
            {
                GenerateControlPacket(endone);
            }
        }

        public void IdentifyEndNode(Sensor endNode)
        {
            if (Settings.Default.ShowAnimation)
            {
                Action actionx = () => endNode.Ellipse_indicator.Visibility = Visibility.Visible;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => endNode.Ellipse_indicator.Fill = Brushes.DarkOrange;
                Dispatcher.Invoke(actionxx);
            }
        }

        public void UnIdentifyEndNode(Sensor endNode)
        {
            if (Settings.Default.ShowAnimation)
            {
                Action actionx = () => endNode.Ellipse_indicator.Visibility = Visibility.Hidden;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => endNode.Ellipse_indicator.Fill = Brushes.Transparent;
                Dispatcher.Invoke(actionxx);
            }
        }


        /// <summary>
        ///  select this node as a source and let it 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btn_send_packet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lbl_title = sender as Label;
            switch (lbl_title.Name)
            {
                case "btn_send_1_packet":
                    {
                        if (this.ID != PublicParamerters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(1);
                        }
                        else
                        {
                            RandomSelectEndNodes(1);
                        }

                        break;
                    }
                case "btn_send_10_packet":
                    {
                        if (this.ID != PublicParamerters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(10);
                        }
                        else
                        {
                            RandomSelectEndNodes(10);
                        }
                        break;
                    }

                case "btn_send_100_packet":
                    {
                        if (this.ID != PublicParamerters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(100);
                        }
                        else
                        {
                            RandomSelectEndNodes(100);
                        }
                        break;
                    }

                case "btn_send_300_packet":
                    {
                        if (this.ID != PublicParamerters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(300);
                        }
                        else
                        {
                            RandomSelectEndNodes(300);
                        }
                        break;
                    }

                case "btn_send_1000_packet":
                    {
                        if (this.ID != PublicParamerters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(1000);
                        }
                        else
                        {
                            RandomSelectEndNodes(1000);
                        }
                        break;
                    }

                case "btn_send_5000_packet":
                    {
                        if (this.ID != PublicParamerters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(5000);
                        }
                        else
                        {
                            // DOWN
                            RandomSelectEndNodes(5000);
                        }
                        break;
                    }
            }
        }

        // try. 该等待队列在新的发包模式下不使用
        private void DeliveerPacketsInQueuTimer_Tick(object sender, EventArgs e)
        {


            Packet toppacket = WaitingPacketsQueue.Dequeue();
            Console.WriteLine("NID:" + this.ID + " trying(preamble packet) to sent The  PID:" + toppacket.PID);
            //尝试发送等待队列中的数据包
            toppacket.WaitingTimes += 1;
            PublicParamerters.TotalWaitingTime += 1; // total;
            //某数据包尝试7次之后若还未发送成功则将其丢弃
            if (toppacket.WaitingTimes < 7)
                SendPacekt(toppacket);
            else
            {
                PublicParamerters.NumberofDropedPacket += 1;
                toppacket.isDelivered = false;
                PublicParamerters.FinishedRoutedPackets.Add(toppacket);
                Console.WriteLine("PID:" + toppacket.PID + " has been droped.");
                //  MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Droped_Packet.Content = PublicParamerters.NumberofDropedPacket, DispatcherPriority.Send);
                MainWindow.MainWindowUpdataMessage();
            }
            //等待队列无数据包时，停止定时器
            if (WaitingPacketsQueue.Count == 0)
            {
                if (Settings.Default.ShowRadar) Myradar.StopRadio();

                QueuTimer.Stop();
                Console.WriteLine("NID:" + this.ID + ". Queu Timer is stoped.");
                MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.Transparent);
                MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Hidden);
            }
            // MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_nymber_inQueu.Content = PublicParamerters.InQueuePackets.ToString());
            MainWindow.MainWindowUpdataMessage();
        }




        public void RedundantTransmisionCost(Packet pacekt, Sensor reciverNode)
        {
            // 计算冗余preamble包的能量消耗和相关参数的统计         
            PublicParamerters.TotalReduntantTransmission += 1;
            double UsedEnergy_Nanojoule = EnergyModel.Receive(PublicParamerters.PreamblePacketLength);
            double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);
            reciverNode.ResidualEnergy = reciverNode.ResidualEnergy - UsedEnergy_joule;
            // pacekt.UsedEnergy_Joule += UsedEnergy_joule;
            PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule;
            PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule;
            // MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Redundant_packets.Content = PublicParamerters.TotalReduntantTransmission);
            // MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Wasted_Energy_percentage.Content = PublicParamerters.WastedEnergyPercentage);
            MainWindow.MainWindowUpdataMessage();
        }


        public void Sendpreamble(Sensor sender)
        {
            //preamble包的传输距离为通信半径；
            double UsedEnergy_Nanojoule = EnergyModel.Transmit(PublicParamerters.PreamblePacketLength, PublicParamerters.CommunicationRangeRadius);
            double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);

            //节点相关能耗计算
            sender.ResidualEnergy = sender.ResidualEnergy - UsedEnergy_joule;//剩余能耗
            sender.UsedEnergy += UsedEnergy_joule;//使用能耗
            sender.Energy_Used_IN_Preamble_Packet += UsedEnergy_joule;//preamble包能耗
            sender.Energy_Used_IN_Send_Preamble_Packet += UsedEnergy_joule;//发送preamble能耗

            //总能耗相关计算
            PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule;//总能耗
            PublicParamerters.TotalEnergyConsumptionJoule_Preamblepacket += UsedEnergy_joule;//总preamble能耗
            PublicParamerters.TotalEnergyConsumptionJoule_Preamblepacket_by_Send += UsedEnergy_joule;//总发送preamble能耗

            //发送preamble包的时延,不管有没有收到ACK都为两倍的时延
            //若有ACK，则ACK的传输和传播时延与preamble相同，若无ACK，也至少需要这么久的时延才能确定没有节点回复ACK
            double delay = DelayModel.DelayModel.Delay_Preamble();
            delay = delay * 2;
            foreach (Packet packet in NewWaitingPacketsQueue)
            {
                packet.TotalDelay += delay;
                packet.TotalDelay_PreamblePackets += delay;


                PublicParamerters.TotalDelayMs += delay;
                PublicParamerters.TotalDelay_PreamblePackets += delay;

            }



        }

        public void Receivepreamble()
        {
            double UsedEnergy_Nanojoule = EnergyModel.Receive(PublicParamerters.PreamblePacketLength);
            double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);


            //节点相关能耗计算
            this.ResidualEnergy = this.ResidualEnergy - UsedEnergy_joule;//节点剩余能耗
            this.UsedEnergy += UsedEnergy_joule;//节点使用能耗
            this.Energy_Used_IN_Preamble_Packet += UsedEnergy_joule;//节点preamble能耗
            this.Energy_Used_IN_Receive_Preamble_Packet += UsedEnergy_joule;//节点接收preamble能耗


            //总能耗相关计算
            PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule; //总能耗
            PublicParamerters.TotalEnergyConsumptionJoule_Preamblepacket += UsedEnergy_joule;//总preamble能耗
            PublicParamerters.TotalEnergyConsumptionJoule_Preamblepacket_by_Rcceive += UsedEnergy_joule;//总接收preamb能耗

            
        }



        public void SendACK(Sensor sender, double Distance_M)
        {
            double UsedEnergy_Nanojoule = EnergyModel.Transmit(PublicParamerters.ACKPacketLength, Distance_M);
            double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);

            //节点相关能耗计算
            sender.ResidualEnergy = sender.ResidualEnergy - UsedEnergy_joule;//计算节点剩余能量 
            sender.UsedEnergy += UsedEnergy_joule;//节点使用能耗
            sender.Energy_Used_IN_ACK_Packet += UsedEnergy_joule;//节点消耗ACK能耗
            sender.Energy_Used_IN_Send_ACK_Packet += UsedEnergy_joule;//节点消耗发送ACK能耗

            //总耗能相关计算
            PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule;//总能耗
            PublicParamerters.TotalEnergyConsumptionJoule_ACKpacket += UsedEnergy_joule; //总ACK包能耗
            PublicParamerters.TotalEnergyConsumptionJoule_ACKpacket_by_Send += UsedEnergy_joule; //总发送ACK包能耗

        }


        public void ReceiveACK()
        {
            double UsedEnergy_Nanojoule = EnergyModel.Receive(PublicParamerters.ACKPacketLength);
            double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);

            //节点相关能耗计算
            this.ResidualEnergy = this.ResidualEnergy - UsedEnergy_joule;//剩余能耗
            this.UsedEnergy += UsedEnergy_joule;//消耗能耗
            this.Energy_Used_IN_ACK_Packet += UsedEnergy_joule;//节点消耗的ACK能耗
            this.Energy_Used_IN_Receive_ACK_Packet += UsedEnergy_joule;//节点消耗的接收ACK包的能耗

            //总能耗相关计算
            PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule; //总能耗
            PublicParamerters.TotalEnergyConsumptionJoule_ACKpacket += UsedEnergy_joule;//总ACK能耗
            PublicParamerters.TotalEnergyConsumptionJoule_ACKpacket_by_Rcceive += UsedEnergy_joule;//总接收ACK包能耗


        }




        /// <summary>
        /// the node which is active will accept preample packet and will be selected.
        /// match the packet.
        /// </summary>
        public MiniFlowTableEntry MatchFlow(Packet pacekt)
        {
            MiniFlowTableEntry ret = null;
            try
            {
                if (MiniFlowTable.Count > 0)
                {
                    foreach (MiniFlowTableEntry selectedflow in MiniFlowTable)
                    {
                        //醒着的标记为Forward的且不在传输状态的节点为满足条件的节点
                        if (pacekt.PacketType == PacketType.Data && selectedflow.SensorState == SensorState.Active && selectedflow.UpLinkAction == FlowAction.Forward && selectedflow.NeighborEntry.NeiNode.NewWaitingPacketsQueue.Count == 0)
                        {

                            if (ret == null)
                            {
                                //第一个满足要求的即为转发节点
                                ret = selectedflow;

                                //ret也会消耗接收preamble包的能量，原始版本并未计算,现已实现
                                ret.NeighborEntry.NeiNode.Receivepreamble();

                                if (ret.NeighborEntry.NeiNode.ID != 0) //非sink接收节点接收preamble包后会发送一个ACK包给源节点
                                {
                                    double Distance_M = Operations.DistanceBetweenTwoSensors(this, ret.NeighborEntry.NeiNode);
                                    SendACK(ret.NeighborEntry.NeiNode, Distance_M);

                                }
                                //源节点接收ACK包
                                this.ReceiveACK();
                            }
                            else
                            {
                                //计算冗余传输的能量消耗，除了转发节点外，每一个醒着的标记为forward的节点都会接受源节点的preamble包，这些都是冗余能耗
                                //原始版本没有引入ACK，是否可以引入ACK包的冗余能耗问题
                                //已引入ACK包的能量消耗和计算


                                //接收preamble相关计算
                                selectedflow.NeighborEntry.NeiNode.Receivepreamble();

                                //冗余传输（每多接收一个冗余的preamble包）
                                PublicParamerters.TotalReduntantTransmission += 1;

                                //接收的preamble能耗属于TotalWastedEnergyJoule
                                double UsedEnergy_Nanojoule_ReceivePreamble = EnergyModel.Receive(PublicParamerters.PreamblePacketLength);
                                double UsedEnergy_joule_ReceivePreamble = ConvertToJoule(UsedEnergy_Nanojoule_ReceivePreamble);
                                PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule_ReceivePreamble;


                                //冗余节点发送ACK相关计算
                                double Distance_M = Operations.DistanceBetweenTwoSensors(this, selectedflow.NeighborEntry.NeiNode);
                                SendACK(selectedflow.NeighborEntry.NeiNode, Distance_M);
                                //冗余传输（每发送一个冗余的ACK包）
                                PublicParamerters.TotalReduntantTransmission += 1;
                                //发送preamble的能耗属于TotalWastedEnergyJoule
                                double UsedEnergy_Nanojoule_SendACK = EnergyModel.Transmit(PublicParamerters.ACKPacketLength, Distance_M);
                                double UsedEnergy_joule_SendACK = ConvertToJoule(UsedEnergy_Nanojoule_SendACK);
                                PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule_SendACK;


                                //源节点接收发来的所有ACK
                                this.ReceiveACK();

                                //冗余传输（每多接收一个冗余的ACK包） 不考虑ACK冲突
                                PublicParamerters.TotalReduntantTransmission += 1;

                                //源节点接收ACK的能耗属于TotalWastedEnergyJoule
                                double UsedEnergy_Nanojoule_ReceiveACK = EnergyModel.Receive(PublicParamerters.ACKPacketLength);
                                double UsedEnergy_joule_ReceiveACK = ConvertToJoule(UsedEnergy_Nanojoule_ReceiveACK);
                                PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule_ReceiveACK;

                            }
                        }
                        else if (pacekt.PacketType == PacketType.Control && selectedflow.SensorState == SensorState.Active && selectedflow.DownLinkAction == FlowAction.Forward)
                        {
                            if (ret == null) { ret = selectedflow; }
                            else
                            {
                                // logs.
                                RedundantTransmisionCost(pacekt, selectedflow.NeighborEntry.NeiNode);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No Flow!!!. muach flow!");
                    return null;
                }
            }
            catch
            {
                ret = null;
                //  MessageBox.Show(" Null Match.!");
            }

            return ret;
        }

        public MiniFlowTableEntry NewMatchFlow()
        {

            MiniFlowTableEntry ret = null;
            try
            {
                if (MiniFlowTable.Count > 0)
                {
                    foreach (MiniFlowTableEntry selectedflow in MiniFlowTable)
                    {
                        //醒着的标记为Forward的且不在传输状态的节点为满足条件的节点
                        if (selectedflow.SensorState == SensorState.Active && selectedflow.UpLinkAction == FlowAction.Forward && selectedflow.NeighborEntry.NeiNode.NewWaitingPacketsQueue.Count==0)
                        {

                            if (ret == null)
                            {
                                //第一个满足要求的即为转发节点
                                ret = selectedflow;

                                //ret也会消耗接收preamble包的能量，原始版本并未计算,现已实现
                                ret.NeighborEntry.NeiNode.Receivepreamble();

                                if (ret.NeighborEntry.NeiNode.ID != 0) //非sink接收节点接收preamble包后会发送一个ACK包给源节点
                                {
                                    double Distance_M = Operations.DistanceBetweenTwoSensors(this, ret.NeighborEntry.NeiNode);
                                    SendACK(ret.NeighborEntry.NeiNode, Distance_M);

                                }
                                //源节点接收ACK包
                                this.ReceiveACK();
                            }
                            else
                            {
                                //计算冗余传输的能量消耗，除了转发节点外，每一个醒着的标记为forward的节点都会接受源节点的preamble包，这些都是冗余能耗
                                //原始版本没有引入ACK，是否可以引入ACK包的冗余能耗问题
                                //已引入ACK包的能量消耗和计算


                                //接收preamble相关计算
                                selectedflow.NeighborEntry.NeiNode.Receivepreamble();

                                //冗余传输（每多接收一个冗余的preamble包）
                                PublicParamerters.TotalReduntantTransmission += 1;

                                //接收的preamble能耗属于TotalWastedEnergyJoule
                                double UsedEnergy_Nanojoule_ReceivePreamble = EnergyModel.Receive(PublicParamerters.PreamblePacketLength);
                                double UsedEnergy_joule_ReceivePreamble = ConvertToJoule(UsedEnergy_Nanojoule_ReceivePreamble);
                                PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule_ReceivePreamble;


                                //冗余节点发送ACK相关计算
                                double Distance_M = Operations.DistanceBetweenTwoSensors(this, selectedflow.NeighborEntry.NeiNode);
                                SendACK(selectedflow.NeighborEntry.NeiNode, Distance_M);
                                //冗余传输（每发送一个冗余的ACK包）
                                PublicParamerters.TotalReduntantTransmission += 1;
                                //发送preamble的能耗属于TotalWastedEnergyJoule
                                double UsedEnergy_Nanojoule_SendACK = EnergyModel.Transmit(PublicParamerters.ACKPacketLength, Distance_M);
                                double UsedEnergy_joule_SendACK = ConvertToJoule(UsedEnergy_Nanojoule_SendACK);
                                PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule_SendACK;


                                //源节点接收发来的所有ACK
                                this.ReceiveACK();

                                //冗余传输（每多接收一个冗余的ACK包） 不考虑ACK冲突
                                PublicParamerters.TotalReduntantTransmission += 1;

                                //源节点接收ACK的能耗属于TotalWastedEnergyJoule
                                double UsedEnergy_Nanojoule_ReceiveACK = EnergyModel.Receive(PublicParamerters.ACKPacketLength);
                                double UsedEnergy_joule_ReceiveACK = ConvertToJoule(UsedEnergy_Nanojoule_ReceiveACK);
                                PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule_ReceiveACK;

                            }
                        }
                       
                    }
                }
                else
                {
                    MessageBox.Show("No Flow!!!. muach flow!");
                    return null;
                }
            }
            catch
            {
                ret = null;
                //  MessageBox.Show(" Null Match.!");
            }

            return ret;



        }

        // When the sensor open the channel to transmit the data.
        private void OpenChanel(int reciverID, long PID)
        {
            Thread thread = new Thread(() =>
            {
                lock (MyArrows)
                {
                    Arrow ar = GetArrow(reciverID);
                    if (ar != null)
                    {
                        lock (ar)
                        {
                            if (ar.Visibility == Visibility.Hidden)
                            {
                                if (Settings.Default.ShowAnimation)
                                {
                                    Action actionx = () => ar.BeginAnimation(PID);
                                    Dispatcher.Invoke(actionx);
                                    Action action1 = () => ar.Visibility = Visibility.Visible;
                                    Dispatcher.Invoke(action1);
                                }
                                else
                                {
                                    Action action1 = () => ar.Visibility = Visibility.Visible;
                                    Dispatcher.Invoke(action1);
                                    Dispatcher.Invoke(() => ar.Stroke = new SolidColorBrush(Colors.Black));
                                    Dispatcher.Invoke(() => ar.StrokeThickness = 1);
                                    Dispatcher.Invoke(() => ar.HeadHeight = 1);
                                    Dispatcher.Invoke(() => ar.HeadWidth = 1);
                                }
                            }
                            else
                            {
                                if (Settings.Default.ShowAnimation)
                                {
                                    int cid = Convert.ToInt16(PID % PublicParamerters.RandomColors.Count);
                                    Action actionx = () => ar.BeginAnimation(PID);
                                    Dispatcher.Invoke(actionx);
                                    Dispatcher.Invoke(() => ar.HeadHeight = 1);
                                    Dispatcher.Invoke(() => ar.HeadWidth = 1);
                                }
                                else
                                {
                                    Dispatcher.Invoke(() => ar.Stroke = new SolidColorBrush(Colors.Black));
                                    Dispatcher.Invoke(() => ar.StrokeThickness = 1);
                                    Dispatcher.Invoke(() => ar.HeadHeight = 1);
                                    Dispatcher.Invoke(() => ar.HeadWidth = 1);
                                }
                            }
                        }
                    }
                }
            }
           );
            thread.Start();
            thread.Priority = ThreadPriority.Highest;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Reciver"></param>
        /// <param name="packt"></param>
        public void SendPacekt(Packet packt)
        {
            //发送preamble包消耗的能量，原始版本未实现
            Sendpreamble(this);

            //发送包的类型是数据包
            if (packt.PacketType == PacketType.Data)
            {
                lock (MiniFlowTable)
                {
                    //按照Priority值大小排序邻居节点，前n个节点标记为Forward，剩余m-n个节点标记为Drop，
                    //n是转发节点集大小，m是邻居节点个数
                    MiniFlowTable.Sort(new MiniFlowTableSorterUpLinkPriority());

                    //按照优先级顺序第一个醒着的标记是forward节点就是转发节点，
                    //此函数包含计算接收节点接收preamble包消耗的能量和冗余preamble包的能量消耗，即非转发节点的接收冗余preamble包所消耗的能量
                    //若引入ACK机制，则此函数还应包括转发集中醒着的节点接受preamble包之后发送冗余ACK包的能量消耗。
                    MiniFlowTableEntry flowEntry = MatchFlow(packt);


                    if (flowEntry != null)//有合适的转发节点
                    {
                        //只有候选节点集合中的节点才有可能成为转发节点
                        //当候选节点醒着且不在传输数据包时才能接收preamble然后回复ACK
                        Sensor Reciver = flowEntry.NeighborEntry.NeiNode;

                        //先将源节点队头数据包删除，然后将数据包加入接收节点的等待队列中，接收节点必然处于醒状态
                        //该if语句为防止出现空队列异常
                        if (NewWaitingPacketsQueue.Count >= 1) this.NewWaitingPacketsQueue.Dequeue();

                        //源节点发送data包相关的计算，包括能量消耗以及延时
                        ComputeOverhead(packt, EnergyConsumption.Transmit, Reciver);

                        //接收data包的相关计算和处理
                        Reciver.ReceivePacket(packt);




                    }
                    else  //没有合适的转发节点，所有标记forward的节点都处于睡眠状态,等待下次发送premble包
                    {
                        // no available node right now.
                        Console.WriteLine("NID:" + ID + " Faild to sent PID:" + packt.PID);
                        //  SwichToSleep();// this.




                        if (Settings.Default.ShowRadar) Myradar.StartRadio();
                        PublicParamerters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                        PublicParamerters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                    }
                }
            }



            //发送包的类型是控制包
            else if (packt.PacketType == PacketType.Control)
            {
                lock (MiniFlowTable)
                {
                    DownLinkRouting.GetD_Distribution(this, packt.Destination);
                    MiniFlowTableEntry FlowEntry = MatchFlow(packt);
                    if (FlowEntry != null)
                    {
                        Sensor Reciver = FlowEntry.NeighborEntry.NeiNode;
                        // sender swich on the redio:
                        //   SwichToActive(); // this.
                        ComputeOverhead(packt, EnergyConsumption.Transmit, Reciver);
                        FlowEntry.DownLinkStatistics += 1;
                        // Reciver.SwichToActive();
                        Reciver.ReceivePacket(packt);
                        //  SwichToSleep();// this.
                    }
                    else
                    {
                        // no available node right now.
                        // add the packt to the wait list.
                        Console.WriteLine("NID:" + this.ID + " Faild to sent PID:" + packt.PID);
                        WaitingPacketsQueue.Enqueue(packt);
                        QueuTimer.Start();
                        Console.WriteLine("NID:" + this.ID + ". Queu Timer is started.");
                        //  SwichToSleep();// sleep.
                        if (Settings.Default.ShowRadar) Myradar.StartRadio();
                        PublicParamerters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                        PublicParamerters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                    }
                }
            }
        }



        public void NewSendPackets()
        {
            //标记位，表示有数据要发送，数据发送完毕时会再次改变此标记位
            TransmitState = true;
            //首先发送preamble，计算能量消耗和时延
            Sendpreamble(this);

            lock (MiniFlowTable)
            {
                //按照Priority值大小排序邻居节点，前n个节点标记为Forward，剩余m-n个节点标记为Drop，
                //n是转发节点集大小，m是邻居节点个数
                MiniFlowTable.Sort(new MiniFlowTableSorterUpLinkPriority());

                //按照优先级顺序第一个醒着的标记是forward节点就是转发节点，
                //此函数包含计算接收节点接收preamble包消耗的能量和冗余preamble包的能量消耗，即非转发节点的接收冗余preamble包所消耗的能量
                //若引入ACK机制，则此函数还应包括转发集中醒着的节点接受preamble包之后发送冗余ACK包的能量消耗。
                MiniFlowTableEntry flowEntry = NewMatchFlow();


                if (flowEntry != null)//有合适的转发节点
                {
                    //只有候选节点集合中的节点才有可能成为转发节点
                    //当候选节点醒着且不在传输数据包时才能接收preamble然后回复ACK
                    Sensor Reciver = flowEntry.NeighborEntry.NeiNode;

                    //一次性发完所有等待队列中的数据包
                    //先将源节点队头数据包删除，然后将数据包加入接收节点的等待队列中，接收节点必然处于醒状态
                    while(NewWaitingPacketsQueue.Count > 0)
                    {
                        Packet packt = NewWaitingPacketsQueue.Peek();
                        this.NewWaitingPacketsQueue.Dequeue();
                        //源节点发送data包相关的计算，包括能量消耗以及延时
                        ComputeOverhead(packt, EnergyConsumption.Transmit, Reciver);
                        //接收data包的相关计算和处理
                        Reciver.ReceivePacket(packt);
                    }
                    //标记位，此时表示不在传输数据，
                    TransmitState = false;
               
                }
                else  //没有合适的转发节点，所有标记forward的节点都处于睡眠状态,等待下次发送premble包
                {
                    //队列中所有数据包delay += CheckQueueTime;时延增加，该部分时延属于因为没有接收者而增加的时延
                    for (int i = 0; i < NewWaitingPacketsQueue.Count; i++)
                    {

                        NewWaitingPacketsQueue.ToArray()[i].TotalDelay += this.Mac.CheckQueueTime;
                        NewWaitingPacketsQueue.ToArray()[i].TotalDelay_NO_ACK += this.Mac.CheckQueueTime;
                        
                        PublicParamerters.TotalDelayMs += this.Mac.CheckQueueTime;
                        PublicParamerters.TotalDelay_NO_ACK += this.Mac.CheckQueueTime;
                        

                    }

                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParamerters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParamerters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packt"></param>
        /// <param name="enCon"></param>
        /// <param name="Reciver"></param>
        public void ComputeOverhead(Packet packt, EnergyConsumption enCon, Sensor Reciver)
        {
            if (enCon == EnergyConsumption.Transmit)
            {
                if (ID != PublicParamerters.SinkNode.ID)
                {

                    //计算源节点与接收节点之间的距离，确定转发节点后，数据包的传输距离即是源节点与接收节点间的距离
                    double Distance_M = Operations.DistanceBetweenTwoSensors(this, Reciver);

                    //能量消耗模型下的传输能量计算
                    double UsedEnergy_Nanojoule = EnergyModel.Transmit(packt.PacketLength, Distance_M);
                    double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);

                    //节点相关能耗计算
                    ResidualEnergy = this.ResidualEnergy - UsedEnergy_joule; //节点剩余能量
                    UsedEnergy += UsedEnergy_joule;//节点使用的能耗
                    Energy_Used_IN_Data_Packet += UsedEnergy_joule;//节点数据包能耗
                    Energy_Used_IN_Send_Data_Packet += UsedEnergy_joule;//节点发送数据包能耗

                    //总能耗相关计算                   
                    PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule;//网络总耗能
                    PublicParamerters.TotalEnergyConsumptionJoule_Datapacket += UsedEnergy_joule;//data包总能耗
                    PublicParamerters.TotalEnergyConsumptionJoule_Datapacket_by_Send += UsedEnergy_joule;//发送data包总能耗

                    //该数据包消耗的能量
                    packt.UsedEnergy_Joule += UsedEnergy_joule;

                    //数据包路由距离和跳数
                    packt.RoutingDistance += Distance_M;
                    packt.Hops += 1;

                    //总路由距离和跳数
                    PublicParamerters.TotalRoutingDistance += Distance_M;
                    PublicParamerters.TotalHops += 1;

                    //计算数据包延迟和总延迟
                    double delay = DelayModel.DelayModel.Delay_Data(this, Reciver);

                    packt.TotalDelay += delay;
                    packt.TotalDelay_DataPackets += delay;

                    PublicParamerters.TotalDelayMs += delay;
                    PublicParamerters.TotalDelay_DataPackets += delay;

                    if (Settings.Default.SaveRoutingLog)
                    {
                        RoutingLog log = new RoutingLog();
                        log.PacketType = PacketType.Data;
                        log.IsSend = true;
                        log.NodeID = this.ID;
                        log.Operation = "To:" + Reciver.ID;
                        log.Time = DateTime.Now;
                        log.Distance_M = Distance_M;
                        log.UsedEnergy_Nanojoule = UsedEnergy_Nanojoule;
                        log.RemaimBatteryEnergy_Joule = ResidualEnergy;
                        log.PID = packt.PID;
                        this.Logs.Add(log);
                    }

                    // for control packet.
                    if (packt.PacketType == PacketType.Control)
                    {
                        // just to remember how much energy is consumed here.
                        PublicParamerters.EnergyComsumedForControlPackets += UsedEnergy_joule;
                    }
                }

                if (Settings.Default.ShowRoutingPaths)
                {
                    OpenChanel(Reciver.ID, packt.PID);
                }

            }
            else if (enCon == EnergyConsumption.Recive)
            {


                double UsedEnergy_Nanojoule = EnergyModel.Receive(packt.PacketLength);
                double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);

                //节点能耗相关计算
                ResidualEnergy = ResidualEnergy - UsedEnergy_joule;//节点剩余能量
                UsedEnergy += UsedEnergy_joule;//节点使用能量
                Energy_Used_IN_Data_Packet += UsedEnergy_joule;//节点数据包能耗
                Energy_Used_IN_Receive_Data_Packet += UsedEnergy_joule;//节点接收数据包能耗


                //总能耗相关计算
                PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule;//总能耗
                PublicParamerters.TotalEnergyConsumptionJoule_Datapacket += UsedEnergy_joule;//总data包能耗
                PublicParamerters.TotalEnergyConsumptionJoule_Datapacket_by_Rcceive += UsedEnergy_joule;//总接收data包能耗


                packt.UsedEnergy_Joule += UsedEnergy_joule;

                if (packt.PacketType == PacketType.Control)
                {
                    // just to remember how much energy is consumed here.
                    PublicParamerters.EnergyComsumedForControlPackets += UsedEnergy_joule;
                }


            }

        }


        /// <summary>
        ///  data or control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reciver"></param>
        /// <param name="packt"></param>
        public void ReceivePacket(Packet packt)
        {
            packt.Path += ">" + ID;
            if (packt.Destination.ID == ID)//数据包抵达目的节点，即sink节点
            {
                packt.isDelivered = true;
                PublicParamerters.NumberofDeliveredPacket += 1;
                PublicParamerters.NumberofInAllQueuePackets -= 1;//所有在队列中的数据包总数减少1
                PublicParamerters.FinishedRoutedPackets.Add(packt);// should we add it to the packet which should be store in the sink?
                Console.WriteLine("PID:" + packt.PID + " has been delivered.");


                //接收数据包消耗的能量
                ComputeOverhead(packt, EnergyConsumption.Recive, null);

                /*
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_total_consumed_energy.Content = PublicParamerters.TotalEnergyConsumptionJoule + " (JOULS)", DispatcherPriority.Send);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Delivered_Packet.Content = PublicParamerters.NumberofDeliveredPacket, DispatcherPriority.Send);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_sucess_ratio.Content = PublicParamerters.DeliveredRatio, DispatcherPriority.Send);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_nymber_inQueu.Content = PublicParamerters.InQueuePackets.ToString());
                */


                //更新主窗口右侧相关信息
                MainWindow.MainWindowUpdataMessage();

                if (packt.PacketType == PacketType.Control)
                    UnIdentifyEndNode(packt.Destination);
                if (packt.PacketType == PacketType.Data)
                    UnIdentifySourceNode(packt.Source);

            }
            else
            {
                //非sink节点接收数据包，先计算接收此数据包的能量消耗，接收之后再决定是丢弃还是转发
                //原始版本中非sink节点成功接收数据包时并未计算能量消耗，即该else语句内应调用ComputeOverhead，现已实现。  

                ComputeOverhead(packt, EnergyConsumption.Recive, null);

                //生存周期已到丢弃该数据包，即该数据包不加入接收节点的等待队列中
                if (packt.Hops >= packt.TimeToLive)
                {


                    PublicParamerters.NumberofDropedPacket += 1;//丢弃包总数增加1
                    PublicParamerters.DropedbecauseofTTL += 1; //因生存周期而丢弃的数据包
                    PublicParamerters.NumberofInAllQueuePackets -= 1;   //总队列数据包总数减少1
                    packt.isDelivered = false;
                    PublicParamerters.FinishedRoutedPackets.Add(packt);
                    Console.WriteLine("PID:" + packt.PID + " has been droped.");

                    // MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Droped_Packet.Content = PublicParamerters.NumberofDropedPacket, DispatcherPriority.Send);
                    MainWindow.MainWindowUpdataMessage();
                }
                else //加入接收节点的等待队列中，等待队列计时器检测等待队列中的数据包然后转发
                {

                    this.NewWaitingPacketsQueue.Enqueue(packt);
                    // forward the packet.
                    // this.SendPacekt(packt);

                }
            }
        }


        #endregion






        //鼠标移动到节点上的显示信息
        private void lbl_MouseEnter(object sender, MouseEventArgs e)
        {
            /*
            ToolTip = new Label() { Content = 
                "("+ID + ") [ " + ResidualEnergyPercentage + "% ] [ " + ResidualEnergy + " J ]" 
                +"\n"+"[PacketInQueue="+NewWaitingPacketsQueue.Count+"]"};
            */

            //通信范围内为邻居节点
            this.Ellipse_Communication_range.Visibility = Visibility.Visible;
            //箭头连接表示候选节点集中的节点
            NetworkVisualization.UpLinksDrawPaths(this);

            Label label = new Label();
            string nodemessage = GetNodeMessage();
            label.Content = nodemessage;
            ToolTip = label;



            /*            
            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = 10000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;
            */










        }

        public string GetNodeMessage()
        {

            //节点消息汇总
            string nodemessage;

            //ID
            string message_ID = "ID:" + ID + "\n";

            //nodestate
            string message_State = "NodeState:" + CurrentSensorState + "\n";

            //初始能量
            string message_BatteryIntialEnergy = "BatteryIntialEnergy:" + BatteryIntialEnergy + "J" + "\n";

            //能量消耗及其占比
            string message_UsedEnergy = "UsedEnergy:" + UsedEnergy + "J   " + "UsedEnergyPercentage:" + UsedEnergyPercentage + "%" + "\n";

            //剩余能量及其占比
            string message_ResidualEnergy = "ResidualEnergy:" + ResidualEnergy + "J   " + "ResidualEnergyPercentage:" + ResidualEnergyPercentage + "%" + "\n";

            //消耗data包占比及其能耗分布
            string message_Energy_Used_In_Datapacket_Percentage = "EnergyUsedInDatapacketPercentage:" + Energy_Used_IN_Data_Packet_Percentage + "%   "
                + "Send:" + Energy_Used_IN_Send_Data_Packet_Percentage + "%   " + "Receive:" + Energy_Used_IN_Receive_Data_Packet_Percentage + "%" + "\n";

            //消耗preamble包占比及其能耗分布
            string message_Energy_Used_In_Preamblepacket_Percentage = "EnergyUsedInPreamblepacketPercentage:" + Energy_Used_IN_Preamble_Packet_Percentage + "%   "
                + "Send:" + Energy_Used_IN_Send_Preamble_Packet_Percentage + "%   " + "Receive:" + Energy_Used_IN_Receive_Preamble_Packet_Percentage + "%" + "\n";

            //消耗ACK包占比及其能耗分布
            string message_Energy_Used_In_ACKpacket_Percentage = "EnergyUsedInACKpacketPercentage:" + Energy_Used_IN_ACK_Packet_Percentage + "%   "
                + "Send:" + Energy_Used_IN_Send_ACK_Packet_Percentage + "%   " + "Receive:" + Energy_Used_IN_Receive_ACK_Packet_Percentage + "%" + "\n";

            nodemessage = message_ID + message_State + message_BatteryIntialEnergy + message_UsedEnergy + message_ResidualEnergy + message_Energy_Used_In_Datapacket_Percentage + message_Energy_Used_In_Preamblepacket_Percentage + message_Energy_Used_In_ACKpacket_Percentage;
            return nodemessage;

        }

        private void btn_show_routing_log_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Logs.Count > 0)
            {
                UiShowRelativityForAnode re = new ui.UiShowRelativityForAnode();
                re.dg_relative_shortlist.ItemsSource = Logs;
                re.Show();
            }
        }

        private void btn_draw_random_numbers_MouseDown(object sender, MouseButtonEventArgs e)
        {
            List<KeyValuePair<int, double>> rands = new List<KeyValuePair<int, double>>();
            int index = 0;
            foreach (RoutingLog log in Logs)
            {
                if (log.IsSend)
                {
                    index++;
                    rands.Add(new KeyValuePair<int, double>(index, log.ForwardingRandomNumber));
                }
            }
            UiRandomNumberGeneration wndsow = new ui.UiRandomNumberGeneration();
            wndsow.chart_x.DataContext = rands;
            wndsow.Show();
        }

        private void Ellipse_center_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void btn_show_my_duytcycling_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btn_draw_paths_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NetworkVisualization.UpLinksDrawPaths(this);
        }



        private void btn_show_my_flows_MouseDown(object sender, MouseButtonEventArgs e)
        {

            ListControl ConMini = new ui.conts.ListControl();
            ConMini.lbl_title.Content = "Mini-Flow-Table";
            ConMini.dg_date.ItemsSource = MiniFlowTable;


            ListControl ConNei = new ui.conts.ListControl();
            ConNei.lbl_title.Content = "Neighbors-Table";
            ConNei.dg_date.ItemsSource = NeighborsTable;

            UiShowLists win = new UiShowLists();
            win.stack_items.Children.Add(ConMini);
            win.stack_items.Children.Add(ConNei);
            win.Title = "Tables of Node " + ID;
            win.Show();
            win.WindowState = WindowState.Maximized;
        }

        private void btn_send_1_p_each1sec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendPacketTimer.Start();
            SendPacketTimer.Tick += SendPacketTimer_Random; // redfine th trigger.
        }



        public void RandomSelectEndNodes(int numOFpACKETS)
        {
            if (PublicParamerters.SimulationTime > PublicParamerters.MacStartUp)
            {
                int index = 1 + Convert.ToInt16(UnformRandomNumberGenerator.GetUniform(PublicParamerters.NumberofNodes - 2));
                if (index != PublicParamerters.SinkNode.ID)
                {
                    Sensor endNode = MainWindow.myNetWork[index];
                    GenerateMultipleControlPackets(numOFpACKETS, endNode);
                }
            }
        }

        private void SendPacketTimer_Random(object sender, EventArgs e)
        {
            if (ID != PublicParamerters.SinkNode.ID)
            {
                // uplink:
                GenerateMultipleDataPackets(1);
            }
            else
            { //
                RandomSelectEndNodes(1);
            }
        }

        /// <summary>
        /// i am slected as end node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_select_me_as_end_node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lbl_title = sender as Label;
            switch (lbl_title.Name)
            {
                case "Btn_select_me_as_end_node_1":
                    {
                        PublicParamerters.SinkNode.GenerateMultipleControlPackets(1, this);

                        break;
                    }
                case "Btn_select_me_as_end_node_10":
                    {
                        PublicParamerters.SinkNode.GenerateMultipleControlPackets(10, this);
                        break;
                    }
                //Btn_select_me_as_end_node_1_5sec

                case "Btn_select_me_as_end_node_1_5sec":
                    {
                        PublicParamerters.SinkNode.SendPacketTimer.Start();
                        PublicParamerters.SinkNode.SendPacketTimer.Tick += SelectMeAsEndNodeAndSendonepacketPer5s_Tick;
                        break;
                    }
            }
        }

        public void SelectMeAsEndNodeAndSendonepacketPer5s_Tick(object sender, EventArgs e)
        {
            PublicParamerters.SinkNode.GenerateMultipleControlPackets(1, this);
        }





        /*** Vistualize****/

        public void ShowID(bool isVis)
        {
            if (isVis) { lbl_Sensing_ID.Visibility = Visibility.Visible; lbl_hops_to_sink.Visibility = Visibility.Visible; }
            else { lbl_Sensing_ID.Visibility = Visibility.Hidden; lbl_hops_to_sink.Visibility = Visibility.Hidden; }
        }

        public void ShowSensingRange(bool isVis)
        {
            if (isVis) Ellipse_Sensing_range.Visibility = Visibility.Visible;
            else Ellipse_Sensing_range.Visibility = Visibility.Hidden;
        }

        public void ShowComunicationRange(bool isVis)
        {
            if (isVis) Ellipse_Communication_range.Visibility = Visibility.Visible;
            else Ellipse_Communication_range.Visibility = Visibility.Hidden;
        }

        public void ShowBattery(bool isVis)
        {
            if (isVis) Prog_batteryCapacityNotation.Visibility = Visibility.Visible;
            else Prog_batteryCapacityNotation.Visibility = Visibility.Hidden;
        }

        private void btn_update_mini_flow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UplinkRouting.UpdateUplinkFlowEnery(this);
        }

        private void lbl_MouseLeave(object sender, MouseEventArgs e)
        {


            this.Ellipse_Communication_range.Visibility = Visibility.Hidden;
            foreach (Arrow arr in this.MyArrows)
            {
                arr.StrokeThickness = 0.2;
                arr.HeadHeight = 0.2;
                arr.HeadWidth = 0.2;



            }




        }
    }
}
