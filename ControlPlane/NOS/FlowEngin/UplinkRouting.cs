using MiniSDN.Dataplane;
using MiniSDN.Dataplane.PacketRouter;
using MiniSDN.Intilization;
using MiniSDN.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MiniSDN.ControlPlane.NOS.FlowEngin
{

    public class MiniFlowTableSorterUpLinkPriority : IComparer<MiniFlowTableEntry>
    {

        public int Compare(MiniFlowTableEntry y, MiniFlowTableEntry x)
        {
            return x.UpLinkPriority.CompareTo(y.UpLinkPriority);
        }
    }

    public class UplinkFlowEnery
    {

        public int CurrentID { get { return Current.ID; } } // ID
        public int NextID { get { return Next.ID; } }
        //
        public double Pr
        {
            get; set;
        }

        // Elementry values:
        public double H { get; set; } // hop to sink
        public double R { get; set; } // riss
        public double L { get; set; } // remian energy
        //
        public double HN { get; set; } // H normalized
        public double RN { get; set; } // R NORMALIZEE value of To. 
        public double LN { get; set; } // L normalized
        //
        public double HP { get; set; } // R normalized
        public double RP { get; set; } // R NORMALIZEE value of To. 
        public double LP { get; set; } // L value of To.



        // return:
        public double Mul
        {
            get
            {
                return RP * LP * HP;
            }
        }

        public Sensor Current { get; set; } // ID
        public Sensor Next { get; set; }
    }

    public class UplinkRouting
    {

        public static void UpdateUplinkFlowEnery(Sensor sender)
        {
            //sender.GenerateDataPacket(); // send packet to controller.

            //PublicParamerters.SinkNode.GenerateControlPacket(sender);// response from controller.

            sender.MiniFlowTable.Clear();
            ComputeUplinkFlowEnery(sender);


        }

        public static void ComputeUplinkFlowEnery(Sensor sender)
        {

            if (Settings.Default.RoutingAlgorithm == "LORA")
            {

                double n = Convert.ToDouble(sender.NeighborsTable.Count) + 1;

                double LControl = Settings.Default.ExpoLCnt * Math.Sqrt(n);
                double HControl = Settings.Default.ExpoHCnt * Math.Sqrt(n);
                double EControl = Settings.Default.ExpoECnt * Math.Sqrt(n);
                double RControl = Settings.Default.ExpoRCnt * Math.Sqrt(n);


                double HSum = 0; // sum of h value.
                double RSum = 0;
                foreach (NeighborsTableEntry can in sender.NeighborsTable)
                {
                    HSum += can.H;
                    RSum += can.R;
                }

                // normalized values.
                foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                {
                    if (neiEntry.NeiNode.ResidualEnergyPercentage >= 0) // the node is a live.
                    {
                        MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                        MiniEntry.NeighborEntry = neiEntry;
                        MiniEntry.NeighborEntry.HN = 1.0 / (Math.Pow((Convert.ToDouble(MiniEntry.NeighborEntry.H) + 1.0), HControl));
                        MiniEntry.NeighborEntry.RN = 1 - (Math.Pow(MiniEntry.NeighborEntry.R, RControl) / RSum);
                        MiniEntry.NeighborEntry.LN = Math.Pow(MiniEntry.NeighborEntry.L / 100, LControl);

                        MiniEntry.NeighborEntry.E = Operations.DistanceBetweenTwoPoints(PublicParamerters.SinkNode.CenterLocation, MiniEntry.NeighborEntry.CenterLocation);
                        MiniEntry.NeighborEntry.EN = (MiniEntry.NeighborEntry.E / (Operations.DistanceBetweenTwoPoints(PublicParamerters.SinkNode.CenterLocation, sender.CenterLocation) + sender.ComunicationRangeRadius));

                        sender.MiniFlowTable.Add(MiniEntry);
                    }
                }

                // pro sum
                double HpSum = 0; // sum of h value.
                double LpSum = 0;
                double RpSum = 0;
                double EpSum = 0;
                foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                {
                    HpSum += (1 - Math.Exp(MiniEntry.NeighborEntry.HN));
                    RpSum += Math.Exp(MiniEntry.NeighborEntry.RN);
                    LpSum += (1 - Math.Exp(-MiniEntry.NeighborEntry.LN));
                    EpSum += (Math.Pow((1 - Math.Sqrt(MiniEntry.NeighborEntry.EN)), EControl));
                }

                double sumAll = 0;
                foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                {
                    MiniEntry.NeighborEntry.HP = (1 - Math.Exp(MiniEntry.NeighborEntry.HN)) / HpSum;
                    MiniEntry.NeighborEntry.RP = Math.Exp(MiniEntry.NeighborEntry.RN) / RpSum;
                    MiniEntry.NeighborEntry.LP = (1 - Math.Exp(-MiniEntry.NeighborEntry.LN)) / LpSum;
                    MiniEntry.NeighborEntry.EP = (Math.Pow((1 - Math.Sqrt(MiniEntry.NeighborEntry.EN)), EControl)) / EpSum;

                    MiniEntry.UpLinkPriority = (MiniEntry.NeighborEntry.EP + MiniEntry.NeighborEntry.HP + MiniEntry.NeighborEntry.RP + MiniEntry.NeighborEntry.LP) / 4;
                    sumAll += MiniEntry.UpLinkPriority;
                }

                // normalized:
                foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                {
                    MiniEntry.UpLinkPriority = (MiniEntry.UpLinkPriority / sumAll);
                }
                // sort:
                sender.MiniFlowTable.Sort(new MiniFlowTableSorterUpLinkPriority());

                // action:
                double average = 1 / Convert.ToDouble(sender.MiniFlowTable.Count);
                int Ftheashoeld = Convert.ToInt16(Math.Ceiling(Math.Sqrt(Math.Sqrt(n)))); // theshold.
                int forwardersCount = 0;
                foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                {
                    if (MiniEntry.UpLinkPriority >= average && forwardersCount <= Ftheashoeld)
                    {
                        MiniEntry.UpLinkAction = FlowAction.Forward;
                        forwardersCount++;
                    }
                    else MiniEntry.UpLinkAction = FlowAction.Drop;
                }
            }

            if (Settings.Default.RoutingAlgorithm == "AHP_Fuzzy")
            {              
                /*
                   ------------------------------------
                  |MiniFlowTable|NeighborEntry|NeiNode |
                   ------------------------------------                            
                 */


                //构建MiniFlowTable

                //从邻居表中筛选出部分节点加入路由表中
                foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                {
                    //筛选一：仅当夹角为锐角的邻居节点才加入MiniFlowTable，初始UpLinkAction = Forward 
                    if (neiEntry.angle <= 90)
                    {
                        MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                        MiniEntry.NeighborEntry = neiEntry;
                        sender.MiniFlowTable.Add(MiniEntry);
                        
                    }
                                        
                }
                //路由表个数
                int number_of_MiniFlowTable = sender.MiniFlowTable.Count;

                //能量矩阵,取值[0,100],表示剩余能量百分比，Energy_max=100,表示剩余能量取值最大值，用作Energy矩阵元素归一化分母
                double[] Energy = new double[number_of_MiniFlowTable];
                double Energy_max = 100;
               
                //sender与forwarders之间的距离矩阵，取值[0,PublicParamerters.CommunicationRangeRadius]
                //其中Distance_max=PublicParamerters.CommunicationRangeRadius，表距离取值最大值，用作Distance矩阵元素归一化分母
                double[] Distance = new double[number_of_MiniFlowTable];
                double Distance_max = PublicParamerters.CommunicationRangeRadius;
                
                //角度矩阵，取值[0,90],因为路由表经过筛选一，Angle_max=90，表示角度取值最大值，用作Angle矩阵元素归一化分母
                double[] Angle = new double[number_of_MiniFlowTable];
                double Angle_max = 90;

                //全零矩阵，用作上传数据至自动化Matlab中时充当虚部矩阵
                double[] pr = new double[number_of_MiniFlowTable];

                //构建相关矩阵
                for (int i =0;i<number_of_MiniFlowTable;i++)
                {
                    Energy[i] = sender.MiniFlowTable.ToArray()[i].ResidualEnergyPercentage;
                    Distance[i] = sender.MiniFlowTable.ToArray()[i].Distance_Sender_To_Receiver;
                    Angle[i] = sender.MiniFlowTable.ToArray()[i].Angle;                  
                }

                //归一化
                for (int i = 0; i < number_of_MiniFlowTable; i++)
                {
                    //值越大越好
                    Energy[i] = Energy[i] / Energy_max;

                    //值越小越好
                    Distance[i] = Distance[i] / Distance_max;

                    //值越小越好
                    Angle[i] = Angle[i] / Angle_max;
                    
                }

                //最终需要上传的属性矩阵

                //最终需要上传的能量矩阵
                double[] Energy_Up_To_Matlab = new double[number_of_MiniFlowTable];

                //最终需要上传的距离矩阵
                double[] Distance_Up_To_Matlab = new double[number_of_MiniFlowTable];

                //最终需要上传的角度矩阵
                double[] Angle_Up_To_Matlab = new double[number_of_MiniFlowTable];

                //构建最终需要上传的属性矩阵
                for (int i = 0; i < number_of_MiniFlowTable; i++)
                {
                    //值越大越好,比重与数值成正比
                    Energy_Up_To_Matlab[i] = Energy[i];

                    //值越小越好,比重与数值成反比
                    Distance_Up_To_Matlab[i] =1- Distance[i] ;

                    //值越小越好,比重与数值成反比
                    Angle_Up_To_Matlab[i] = 1- Angle[i];

                }


                //定义对应的二维数组，表示Forward Set中节点相对应各属性的AHP矩阵

                //关于能量的AHP矩阵，AHP_Energy[i][j]表示第i个节点相对与第j个节点的能量比重
                double[,] AHP_Energy = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                //关于距离的AHP矩阵，AHP_Distance[i][j]表示第i个节点相对与第j个节点的距离比重
                double[,] AHP_Distance = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                //关于角度的AHP矩阵，AHP_Angle[i][j]表示第i个节点相对与第j个节点的角度比重
                double[,] AHP_Angle = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                //利用Matlab构建对应矩阵


                //首先将Matlab注册为自动化服务器
                //从 C# 客户端程序中，将对您的项目的引用添加到 MATLAB COM 对象。例如，在 Microsoft® Visual Studio® 中，打开您的项目。在项目菜单中，选择添加引用。在“添加引用”对话框中，选择 COM 选项卡。选择 MATLAB 应用程序
                //此例为调用函数示例
                /*
                 * 
                 * 
                 * 
                 * 在文件夹 c:\temp\example 中创建 MATLAB 函数 myfunc。
                 * function [x,y] = myfunc(a,b,c) 
                 * x = a + b; 
                 * y = sprintf('Hello %s',c); 
                 * 
                 * 
                 * 
                 * 
                 * 
                // Create the MATLAB instance 
                MLApp.MLApp matlab = new MLApp.MLApp();

                // Change to the directory where the function is located 
                matlab.Execute(@"cd C:\Users\Kang\Documents\MATLAB");

                // Define the output
                object result = null;

                // Call the MATLAB function myfunc
                matlab.Feval("myfunc", 2, out result, 1300, 14, "kang");
                // Display result 
                object[] res = result as object[];
                Console.WriteLine(res[0]);
                Console.WriteLine(res[1]);
                Console.ReadLine();
                */


                //与Matlab建立连接

                MLApp.MLApp matlab = new MLApp.MLApp();
                matlab.Execute(@"cd C:\Users\Kang\Documents\MATLAB\Fuzzy");
                //上传Energy_Up_To_Matlab矩阵
                matlab.PutFullMatrix("Energy_Up_To_Matlab", "base", Energy, pr);
                //上传Distance_Up_To_Matlab矩阵
                matlab.PutFullMatrix("Distance_Up_To_Matlab", "base",Distance, pr);
                //上传Angle_Up_To_Matlab矩阵
                matlab.PutFullMatrix("Angle_Up_To_Matlab", "base",Angle,pr);
               

              //  Array energy = new double[number_of_MiniFlowTable];
               
               // Array energy_pr = new double[number_of_MiniFlowTable];

               // matlab.GetFullMatrix("a","base", ref energy, ref energy_pr);

                /*
                MLApp.MLApp matlab = new MLApp.MLApp();
                matlab.Execute(@"cd C:\Users\Kang\Documents\MATLAB\Fuzzy");
                object myresult ;
                object[] res ;
                double result;

                for (int i=0;i<number_of_MiniFlowTable;i++)
                {
                    for (int j = 0; j < number_of_MiniFlowTable; j++)
                    {
                        myresult = null;
                        matlab.Feval("AHP_fuzzy", 1, out myresult, Energy[i], Energy[j]);
                        res = myresult as object[];
                        result = (double)res[0];
                        AHP_Energy[i,j] = Math.Round(result,4);
                        Console.WriteLine(AHP_Energy[i,j]);

                        myresult = null;
                        matlab.Feval("AHP_fuzzy", 1, out myresult, 1 - Distance[i], 1 - Distance[j]);
                        res = myresult as object[];
                        result = (double)res[0];
                        AHP_Distance[i,j] = Math.Round(result, 4);
                        Console.WriteLine(AHP_Distance[i, j]);

                        myresult = null;
                        matlab.Feval("AHP_fuzzy", 1, out myresult, 1 - Angle[i], 1 - Angle[j]);
                        res = myresult as object[];
                        result = (double)res[0];
                        AHP_Angle[i,j] = Math.Round(result, 4);
                        Console.WriteLine(AHP_Angle[i, j]);


                    }


                }

                */
            }





        }
    }
}
