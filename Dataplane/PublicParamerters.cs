﻿using MiniSDN.Dataplane;
using System;
using System.Collections.Generic;
using MiniSDN.Energy;
using MiniSDN.ExpermentsResults.Lifetime;
using MiniSDN.ui;
using System.Windows.Media;
using MiniSDN.ControlPlane.NOS;
using MiniSDN.Dataplane.NOS;
using MiniSDN.Properties;

namespace MiniSDN.Dataplane
{
    /// <summary>
    /// 
    /// </summary>
    public class PublicParamerters
    {
        //以下为ORR创建的公有参数
        public static double FS0 = Double.MaxValue;
        public static double Ta  { get { return PublicParamerters.Periods.ActivePeriod; } }
        public static double T { get { return PublicParamerters.Periods.ActivePeriod + PublicParamerters.Periods.SleepPeriod; } }
        //以上为ORR创建的公有参数

        //以下为ORW创建的公有参数
        public static double EDC0 = Double.MaxValue;
        //以上为ORW创建的公有参数

        public static long NumberofControlPackets { get; set; }
        public static double EnergyComsumedForControlPackets { get; set; }
         

        public static long NumberofDropedPacket { get; set; } 

        public static long DropedbecauseofCannotSend { get; set; }

        public static long DropedbecauseofTTL { get; set; }

        public static long DropedbecauseofNoEnergy { get; set; }


        public static long NumberofDeliveredPacket { get; set; } // the number of the pakctes recived in the sink node.
        public static long Rounds { get; set; } // how many rounds.
        public static List<DeadNodesRecord> DeadNodeList = new List<DeadNodesRecord>();
        public static long NumberofGeneratedPackets { get; set; }
        public static long TotalWaitingTime { get; set; } // how many times the node waitted for its coordinate to wake up.
        public static long TotalWaitingTimes_IN_Queue { get; set; }//由于在醒周期未发送完而进入等待队列的次数
        public static long TotalReduntantTransmission { get; set; } // how many transmission are redundant, that is to say, recived and canceled.
        public static double TotalReduntantTransmission_per_packet { get { return Math.Round((double)PublicParamerters.TotalReduntantTransmission / (double)PublicParamerters.NumberofGeneratedPackets, 2); } }


        public static bool IsNetworkDied { get; set; } // yes if the first node deide.
        public static double SensingRangeRadius { get; set; }
        public static double Density { get; set; } // average number of neighbores (stander deiviation)
        public static string NetworkName { get; set; }
        public static Sensor SinkNode { get; set; }
        public static double BatteryIntialEnergy  { get { return Settings.Default.BatteryIntialEnergy; } }//{ get { return Settings.Default.BatteryIntialEnergy; } } //J 0.5 /////////////*******////////////////////////////////////    
        public static double BatteryIntialEnergyForSink = 500; //500J.
        public static double RoutingDataLength = 1024*8; // bit
        public static double ControlDataLength = 512; // bit
        public static double PreamblePacketLength = 128; // bit 
        public static double ACKPacketLength = 128; //bit
        public static double E_elec = 50; // unit: (nJ/bit) //Energy dissipation to run the radio
        public static double Efs = 0.1;// unit( nJ/bit/m^2 ) //Free space model of transmitter amplifier
        public static double Emp = 0.0000013; // unit( nJ/bit/m^4) //Multi-path model of transmitter amplifier
        public static double CommunicationRangeRadius { get { return SensingRangeRadius * 2; } } // sensing range is R in the DB.
        public static double TransmissionRate = 250*1000;//// 250Kbit/s , 
        public static double SpeedOfLight = 299792458;//https://en.wikipedia.org/wiki/Speed_of_light // s
        public static string PowersString { get; set; }

        public static double TotalRoutingDistance { get; set; }
        public static double TotalHops { get; set; }
        public static double TotalEnergyConsumptionJoule { get; set; } // keep all energy consumption. 
        public static double TotalEnergyConsumptionJoule_per_packet { get { return TotalEnergyConsumptionJoule / (double)NumberofDeliveredPacket; } }//平均每个包的能量消耗
        public static double TotalEnergyConsumptionJoule_per_hop { get { return TotalEnergyConsumptionJoule/TotalHops; } }//平均每一跳的能量消耗
        public static double TotalEnergy { get { return (NumberofNodes-1) * BatteryIntialEnergy; } }//不包括sink节点的能量

        public static double TotalEnergyConsumptionJoule_Datapacket { get; set; } //数据包消耗的总能量
        public static double TotalEnergyConsumptionJoule_Preamblepacket { get; set; } //Preamble包消耗的总能量
        public static double TotalEnergyConsumptionJoule_ACKpacket { get; set; } //ACK包消耗的总能量

        public static double TotalEnergyConsumptionJoule_Datapacket_by_Send { get; set; }//发送数据包消耗的总能量
        public static double TotalEnergyConsumptionJoule_Datapacket_by_Rcceive { get; set; }//接收数据包消耗的总能量

        public static double TotalEnergyConsumptionJoule_Preamblepacket_by_Send { get; set; }//发送preamble消耗的总能量
        public static double TotalEnergyConsumptionJoule_Preamblepacket_by_Rcceive { get; set; }//接收preamble消耗的总能量

        public static double TotalEnergyConsumptionJoule_ACKpacket_by_Send { get; set; }//发送ACK包消耗的总能量
        public static double TotalEnergyConsumptionJoule_ACKpacket_by_Rcceive { get; set; }//接收ACK包消耗的总能量


        //公有变量 delay相关     
        public static double TotalDelayMs { get; set; } // in ms 总时延
        public static double TotalDelay_PreamblePackets { get; set; }//发送preamble包产生的总时延
        public static double TotalDelay_DataPackets { get; set; }//发送data包产生的总时延
        public static double TotalDelay_NO_ACK { get; set; }//因为收不到ACK而等待下一次检查等待队列产生的总时延
        public static double TotalDelay_IN_Queue { get; set; }//因为收不到ACK而进入睡眠模式产生的总时延



        public static double TotalWastedEnergyJoule { get; set; } // idel listening energy
        
        public static List<Packet> FinishedRoutedPackets = new List<Packet>(); // all the packets whatever dliverd or not.
        public static double ThresholdDistance  //Distance threshold ( unit m) 
        {
            get { return Math.Sqrt(Efs / Emp); }
        }


        public static double ControlPacketsPercentage { get { return 100 * (NumberofControlPackets / NumberofGeneratedPackets); } }
        public static double ControlPacketsEnergyConsmPercentage { get { return 100 * (EnergyComsumedForControlPackets / TotalEnergyConsumptionJoule); } } 



        //冗余传输消耗的能量占消耗能量的占比
        public static double WastedEnergyPercentage { get { return Math.Round(100 * (TotalWastedEnergyJoule / TotalEnergyConsumptionJoule),4); } }  

        public static List<Color> RandomColors { get; set; }

        public static double SensingFeildArea
        {
            get; set;
        }
        public static int NumberofNodes
        {
            get; set;
        }

        public static long NumberofInAllQueuePackets//所有等待队列中的等待发送的数据包总和
        {
            /* 
            get
            {
                return NumberofGeneratedPackets - NumberofDeliveredPacket - NumberofDropedPacket;
            }
            */
            get; set;

        }

        public static double DeliveredRatio
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(NumberofDeliveredPacket) / Convert.ToDouble(NumberofGeneratedPackets)), 2);

                /*
                double ratio = 100 * (Convert.ToDouble(NumberofDeliveredPacket) / Convert.ToDouble(NumberofGeneratedPackets));
                int i = (int)(ratio * 100);
                ratio = (double) i / 100;
                return ratio;
                */
            }
        }

        public static double NewDeliveredRatio
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(NumberofDeliveredPacket) / (Convert.ToDouble(NumberofDeliveredPacket)+Convert.ToDouble(NumberofDropedPacket))), 2);


            }


        }

        public static double NewDropedRatio
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(NumberofDropedPacket) / (Convert.ToDouble(NumberofDeliveredPacket) + Convert.ToDouble(NumberofDropedPacket))), 2);


            }

        }

        public static double Total_Energy_Consumption_Percentage
        {
            get
            {
                //Math.Round(x,2)将X保留两位小数
                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule) / Convert.ToDouble(TotalEnergy)), 4);

            }
        }

        public static double Total_Data_Packet_Consumption_Percentage
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_Datapacket) / Convert.ToDouble(TotalEnergyConsumptionJoule)), 3);
            }


        }

        public static double Data_Packet_Consumption_Send_Percentage
        {
            get {

                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_Datapacket_by_Send) / Convert.ToDouble(TotalEnergyConsumptionJoule_Datapacket)), 2);
            }

        }

        public static double Data_Packet_Consumption_Receive_Percentage
        {
            get
            {

                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_Datapacket_by_Rcceive) / Convert.ToDouble(TotalEnergyConsumptionJoule_Datapacket)), 2);
            }

        }

        public static double Total_Preamble_Packet_Consumption_Percentage
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_Preamblepacket) / Convert.ToDouble(TotalEnergyConsumptionJoule)), 3);
            }

        }

        public static double Preamble_Packet_Consumption_Send_Percentage
        {
            get {

                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_Preamblepacket_by_Send) / Convert.ToDouble(TotalEnergyConsumptionJoule_Preamblepacket)), 2);
            }

        }

        public static double Preamble_Packet_Consumption_Receive_Percentage
        {
            get
            {

                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_Preamblepacket_by_Rcceive) / Convert.ToDouble(TotalEnergyConsumptionJoule_Preamblepacket)), 2);
            }

        }

        public static double Total_ACK_Packet_Consumption_Percentage
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_ACKpacket) / Convert.ToDouble(TotalEnergyConsumptionJoule)), 3);
            }

        }

        public static double ACK_Packet_Consumption_Send_Percentage
        {
            get
            {

                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_ACKpacket_by_Send) / Convert.ToDouble(TotalEnergyConsumptionJoule_ACKpacket)), 2);
            }

        }

        public static double ACK_Packet_Consumption_Receive_Percentage
        {
            get
            {

                return Math.Round(100 * (Convert.ToDouble(TotalEnergyConsumptionJoule_ACKpacket_by_Rcceive) / Convert.ToDouble(TotalEnergyConsumptionJoule_ACKpacket)), 2);
            }

        }

        public static double Total_Delay_by_Waiting_In_Queue_Percentage
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(TotalDelay_IN_Queue) / Convert.ToDouble(TotalDelayMs)), 2);

            }

        }

        public static double Total_Delay_by_No_ACK_Percentage
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(TotalDelay_NO_ACK) / Convert.ToDouble(TotalDelayMs)), 2);

            }

        }

        public static double Total_Delay_by_Data_Packet_Percentage
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(TotalDelay_DataPackets) / Convert.ToDouble(TotalDelayMs)), 2);

            }

        }


        public static double Total_Delay_by_Preamble_Packet_Percentage
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(TotalDelay_PreamblePackets) / Convert.ToDouble(TotalDelayMs)), 2);

            }

        }

        public static double Total_Average_Delay_One_Hop
        {
            get {
                return Math.Round((Convert.ToDouble(TotalDelayMs/1000) / Convert.ToDouble(TotalHops)), 3);

            }

        }
        public static double Total_Average_Delay_End_TO_End
        {
            get
            {
                return Math.Round((Convert.ToDouble(TotalDelayMs / 1000) / Convert.ToDouble(NumberofDeliveredPacket)), 3);

            }

        }


        public static double DropedRatio
        {
            get
            {
                return Math.Round(100 * (Convert.ToDouble(NumberofDropedPacket) / Convert.ToDouble(NumberofGeneratedPackets)), 2);
            }
        }

        public static MainWindow MainWindow { get; set; } 

        /// <summary>
        /// Each time when the node loses 5% of its energy, it shares new energy percentage with its neighbors. The neighbor nodes update their energy distributions according to the new percentage immediately as explained by Algorithm 2. 
        /// </summary>
        public static int UpdateLossPercentage
        {
            get
            {
                return Settings.Default.UpdateLossPercentage;
            }
            set
            {
                Settings.Default.UpdateLossPercentage = value;
            }
        }

        // lifetime paramerts:
        public static int NOS { get; set; } // NUMBER OF RANDOM SELECTED SOURCES
        public static int NOP { get; set; } // NUMBER OF PACKETS TO BE SEND.



        /// <summary>
        /// in sec.
        /// </summary>
        public static class Periods
        {
            public static double ActivePeriod { get { return Settings.Default.ActivePeriod; } } //  the node trun on and check for CheckPeriod mseconds.// +1
            public static double SleepPeriod { get { return Settings.Default.SleepPeriod; } }  // the node trun off and sleep for SleepPeriod mseconds.
        }



        /// <summary>
        /// When all forwarders are sleep, 
        /// the sender try agian until its formwarder is wake up. the sender try agian each 500 ms.
        /// when the sensor retry to send the back is it's forwarders are in sleep mode.
        /// </summary>
        public static TimeSpan QueueTime
        {
            get
            {
                return TimeSpan.FromMilliseconds(Settings.Default.QueueTime);
            }
        }

        /// <summary>
        /// the timer interval between 1 and 5 sec.
        /// </summary>
        public static double MacStartUp
        {
            get
            {
                return Settings.Default.MacStartUp;
            }
        }

        /// <summary>
        /// the runnunin time of simulator. in SEC
        /// </summary>
        public static int SimulationTime
        {
            get;set;
        }


        public static List<BatRange> getRanges()
        {
            List<BatRange> re = new List<BatRange>();

            int x = 100 / UpdateLossPercentage;
            for (int i = 1; i <= x; i++)
            {
                BatRange r = new Energy.BatRange();
                r.isUpdated = false;
                r.Rang[0] = (i - 1) * UpdateLossPercentage;
                r.Rang[1] = i * UpdateLossPercentage;
                r.ID = i;
                re.Add(r);
            }

            re[re.Count - 1].isUpdated = true;

            return re;
        }
    }
}
