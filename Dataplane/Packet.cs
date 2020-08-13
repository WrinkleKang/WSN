using MiniSDN.Intilization;

namespace MiniSDN.Dataplane.NOS
{
    public enum PacketType { Beacon, Preamble, ACK, Data, Control }
    public class Packet
    {
        //: Packet section:
        public long PID { get; set; } // SEQ ID OF PACKET.
        public PacketType PacketType { get; set; }
        public bool isDelivered { get; set; }
        public double PacketLength { get; set; }
        public int H2S { get { if (PacketType == PacketType.Data) return Source.HopsToSink; else return Destination.HopsToSink; } }
        public int TimeToLive { get; set; }
        public int Hops { get; set; }
        public string Path { get; set; }
        public double RoutingDistance { get; set; }
        public double Delay { get; set; }
        public double UsedEnergy_Joule { get; set; }
        public int WaitingTimes { get; set; }

        public int ActiveCount = 0;  //活跃周期数，当数据包在节点活跃周期中未发送成功时，该值+1，当值达到某阈值时，丢弃
        public long TotalWaitingTimes_IN_Queue = 0; //该值与ActiveCount相等，表示在等待队列中等待节点醒来的次数

        //数据包 delay相关
        public  double TotalDelay { get; set; }//总时延
        public  double TotalDelay_PreamblePackets { get; set; }//发送preamble包产生的总时延
        public  double TotalDelay_DataPackets { get; set; }//发送data包产生的总时延
        public  double TotalDelay_NO_ACK { get; set; }//因为收不到ACK而等待下一次检查等待队列产生的总时延
        public  double TotalDelay_IN_Sleep { get; set; }//因为收不到ACK而进入睡眠模式产生的总时延


        public double EuclideanDistance
        {
            get { return Operations.DistanceBetweenTwoSensors(Source, Destination); }
        }

        /// <summary>
        /// eff 100%
        /// </summary>
        public double RoutingDistanceEfficiency
        {
            get
            {
                return 100 * (EuclideanDistance / RoutingDistance);
            }
        }

        /// <summary>
        /// Average Transmission Distance (ATD): for〖 P〗_b^s (g_k ), we define average transmission distance per hop as shown in (28).
        /// </summary>
        public double AverageTransDistrancePerHop
        {
            get
            {
                return (RoutingDistance / Hops);
            }
        }


        public double TransDistanceEfficiency
        {
            get
            {
                return 100 * (1 - (RoutingDistance / (PublicParamerters.CommunicationRangeRadius * Hops * (Hops + 1))));
            }
        }


        /// <summary>
        /// RoutingEfficiency
        /// </summary>
        public double RoutingEfficiency
        {
            get
            {
                return (RoutingDistanceEfficiency + TransDistanceEfficiency) / 2;
            }
        }

        public Sensor Source { get; set; }
        public Sensor Destination { get; set; }
    }
}
