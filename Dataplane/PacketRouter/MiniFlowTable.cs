using MiniSDN.Dataplane.NOS;

namespace MiniSDN.Dataplane.PacketRouter
{
    public enum FlowAction { Forward, Drop }

    public class MiniFlowTableEntry
    {
        //获取节点ID
        public int ID { get { return NeighborEntry.NeiNode.ID; } }
        //获取sender节点与邻居节点的距离
        public double Distance_Sender_To_Receiver { get { return NeighborEntry.R; } }
        //获取邻居节点的剩余能量
        public double ResidualEnergyPercentage { get { return NeighborEntry.NeiNode.ResidualEnergyPercentage; } }
        //获取角度
        public double Angle { get { return NeighborEntry.angle; } }
        //获取邻居节点的跳数
        public double Hops_To_Sink { get { return NeighborEntry.NeiNode.HopsToSink; } }

        public double UpLinkPriority { get; set; }
        public FlowAction UpLinkAction { get; set; }
        public double UpLinkStatistics { get; set; }  

        public double DownLinkPriority { get; set; }
        public FlowAction DownLinkAction { get; set; }
        public double DownLinkStatistics { get; set; }

        public SensorState SensorState { get { return NeighborEntry.NeiNode.CurrentSensorState; } }
        public double Statistics { get { return UpLinkStatistics + DownLinkStatistics; } }
        public  NeighborsTableEntry NeighborEntry { get; set; } 

    }
}
