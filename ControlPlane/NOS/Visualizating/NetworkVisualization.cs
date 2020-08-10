using MiniSDN.Intilization;
using MiniSDN.Dataplane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MiniSDN.Dataplane.PacketRouter;

namespace MiniSDN.ControlPlane.NOS
{
    

    public class NetworkVisualization
    {
        public static void UpLinksDrawPaths( Sensor startFrom)
        {
            // select nodes which are smaller than mine.
            /*
            foreach (Arrow arr in startFrom.MyArrows)
            {
                if (arr.To.HopsToSink < startFrom.HopsToSink)
                {
                    arr.Stroke = Brushes.Black;
                    arr.StrokeThickness = 1;
                    arr.HeadHeight = 5;
                    arr.HeadWidth = 5;
                    UpLinksDrawPaths(arr.To);
                }
        
            }
            */

            foreach (MiniFlowTableEntry Neighbor_Sensor_TableEntry in startFrom.MiniFlowTable)
            {
                foreach (Arrow arr in startFrom.MyArrows)
                {
                    if (Neighbor_Sensor_TableEntry.UpLinkAction == FlowAction.Forward && arr.To.ID == Neighbor_Sensor_TableEntry.ID)
                    {
                        arr.Stroke = Brushes.Black;
                        arr.StrokeThickness = 2;
                        arr.HeadHeight = 8;
                        arr.HeadWidth = 8;
                        
                    }

                }

            }
        }

       


    }
}
