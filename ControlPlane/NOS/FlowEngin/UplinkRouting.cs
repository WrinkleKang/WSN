using MiniSDN.Dataplane;
using MiniSDN.Dataplane.PacketRouter;
using MiniSDN.Forwarding;
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
       
        public static int number_loop_decrease = 0;

        public static void UpdateUplinkFlowEnery(Sensor sender)
        {
            
            if (Settings.Default.RoutingAlgorithm == "AHP_Fuzzy")
            {

                if (sender.HopsToSink == 1)
                {
                    Queue<Sensor> queue = new Queue<Sensor>();
                    foreach (NeighborsTableEntry neighborsTableEntry in sender.NeighborsTable)
                    {
                        foreach (MiniFlowTableEntry mini in neighborsTableEntry.NeiNode.MiniFlowTable)
                        {
                            if (mini.ID == sender.ID && mini.UpLinkAction == FlowAction.Forward)
                            {
                                queue.Enqueue(neighborsTableEntry.NeiNode);

                            }

                        }


                    }
                    while (queue.Count > 0)
                    {
                        ComputeUplinkFlowEnery_Updataenergy(queue.Dequeue());

                    }


                }


              
                ComputeUplinkFlowEnery_Updataenergy(sender);
               

            }
            else {
                sender.MiniFlowTable.Clear();
                ComputeUplinkFlowEnery(sender);

            }


        }

        private static void ComputeUplinkFlowEnery_Updataenergy(Sensor sender)
        {

            //每次更新表之前先清空原表
            sender.MiniFlowTable.Clear();

            //非初始化路由表的更新，距离和角度矩阵不发生改变，因而对应的特征向量不变，所以仅需改变能量矩阵，故可较少与自主化服务器的交互，节省时长
            //每次更新信息时，AHP第一层矩阵都会发生变化，矩阵元素是自身剩余能量的函数
            if (sender.HopsToSink > 1)
            {
                //从邻居表中筛选出部分节点加入路由表中
                foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                {
                    //筛选一：仅当夹角为锐角且距离sink跳数更近的邻居节点才加入MiniFlowTable，初始UpLinkAction = Forward 
                    if (neiEntry.angle < 90 && neiEntry.NeiNode.HopsToSink <= sender.HopsToSink )
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

                //全零矩阵，用作上传数据至自动化Matlab中时充当虚部矩阵
                double[] pr = new double[number_of_MiniFlowTable];

                //构建相关矩阵
                for (int i = 0; i < number_of_MiniFlowTable; i++)
                {
                    Energy[i] = sender.MiniFlowTable.ToArray()[i].ResidualEnergyPercentage;
                  
                }

                //原始矩阵数据归一化
                for (int i = 0; i < number_of_MiniFlowTable; i++)
                {
                    //值越大越好
                    Energy[i] = Energy[i] / Energy_max;

                }

                

                //定义最终需要上传的能量矩阵
                double[] Energy_Up_To_Matlab = new double[number_of_MiniFlowTable];

             

                //构建最终需要上传的属性矩阵
                for (int i = 0; i < number_of_MiniFlowTable; i++)
                {
                    //值越大越好,比重与数值成正比
                    Energy_Up_To_Matlab[i] = Standardization_Energy(Energy[i]);
                    

                  
                }

                //与Matlab建立连接

                MLApp.MLApp matlab = new MLApp.MLApp();
                matlab.Execute(@"cd C:\Users\Kang\Documents\MATLAB\Fuzzy");
                //上传Energy_Up_To_Matlab矩阵到自动化服务器工作区，
                matlab.PutFullMatrix("Energy_Up_To_Matlab", "base", Energy_Up_To_Matlab, pr);
                //上传Distance_Up_To_Matlab矩阵自动化服务器工作区
                

                //在自动化服务器中执行此命令
                matlab.Execute("AHP_Energy_output=AHP_Fuzzy_Energy(Energy_Up_To_Matlab)");



                //定义对应的二维数组，表示Forward Set中节点相对应各属性的AHP矩阵

                //关于能量的AHP矩阵，AHP_Energy[i][j]表示第i个节点相对与第j个节点的能量比重
                Array AHP_Energy = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

               

                //接收自动化Matlab中矩阵时充当虚部矩阵，此参数为函数所必须，不可省略
                Array AHP_out_pr = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                //接收Matlab自动化服务器中处理过的矩阵，其中的值的含义与AHP矩阵中值含义相同

                //接收Energy矩阵，实部存放在AHP_Energy中，虚部存放在ref AHP_out_pr中
                matlab.GetFullMatrix("AHP_Energy_output", "base", ref AHP_Energy, ref AHP_out_pr);

                



                //获取矩阵的最大特征值和特征向量
                //最大特征值
                double AHP_Energy_Eigenvalue;
               
                //对应的特征向量
                Array AHP_Energy_Eigenvector = new double[number_of_MiniFlowTable];
              
                //接收来自Matlab自动化服务器的特征向量的虚部,此矩阵为全零矩阵
                Array AHP_Eigenvector_pr = new double[number_of_MiniFlowTable];

                //通过Matlab求AHP_Energy矩阵的最大特征值和对应的特征向量
                matlab.PutFullMatrix("Matrix", "base", AHP_Energy, AHP_out_pr);
                matlab.Execute("[Eigenvalue,Eigenvector] = Get_Max_Eigenvalue_Eigenvector(Matrix)");
                AHP_Energy_Eigenvalue = matlab.GetVariable("Eigenvalue", "base");
                matlab.GetFullMatrix("Eigenvector", "base", ref AHP_Energy_Eigenvector, ref AHP_Eigenvector_pr);

                //归一化
                Array AHP_Energy_Eigenvector_Normalization = Normalization(AHP_Energy_Eigenvector);
               

                //构建AHP矩阵

                double[,] array =
                {
                       {1,      1,  3 },
                       {1 ,     1,  2 },
                       {1.0/3,  1.0/2,  1 }
                    };
                //第一层AHP矩阵
                Array AHP_Level1 = new double[3, 3];
                AHP_Level1 = array;

                Array AHP_Level1_pr_in = new double[3, 3];
                Array AHP_Level1_pr_out = new double[3];
                double AHP_Level1_Eigenvalue = 0;

                //第一层特征向量,依次表示能量，距离，角度的权重
                Array AHP_Level1_Eigenvector = new double[3];


                //通过Matlab求AHP_Level1矩阵的最大特征值和对应的特征向量
                matlab.PutFullMatrix("Matrix", "base", AHP_Level1, AHP_Level1_pr_in);
                matlab.Execute("[Eigenvalue,Eigenvector] = Get_Max_Eigenvalue_Eigenvector(Matrix)");
                AHP_Level1_Eigenvalue = matlab.GetVariable("Eigenvalue", "base");
                matlab.GetFullMatrix("Eigenvector", "base", ref AHP_Level1_Eigenvector, ref AHP_Level1_pr_out);

                //第一层特征向量归一化
                Array AHP_Level1_Eigenvector_Normalization = Normalization(AHP_Level1_Eigenvector);


                //求Priority == 属性在总目标上的权重 * 节点在该属性上的权重   然后求和
                for (int i = 0; i < sender.MiniFlowTable.Count; i++)
                {

                    double Priority_Energy = (double)AHP_Level1_Eigenvector_Normalization.GetValue(0) * (double)AHP_Energy_Eigenvector_Normalization.GetValue(i);
                    double Priority_Distance = (double)AHP_Level1_Eigenvector_Normalization.GetValue(1) * (double)sender.AHP_Distance_Eigenvector_Normalization.GetValue(i);
                    double Priority_Angle = (double)AHP_Level1_Eigenvector_Normalization.GetValue(2) * (double)sender.AHP_Angle_Eigenvector_Normalization.GetValue(i);
                    sender.MiniFlowTable.ToArray()[i].UpLinkPriority = Priority_Energy + Priority_Distance + Priority_Angle;
                    //具体得分比重，用于显示观察，鼠标移动到节点上可观测具体数值
                    sender.MiniFlowTable.ToArray()[i].UpLinkPriority_Energy = Priority_Energy;
                    sender.MiniFlowTable.ToArray()[i].UpLinkPriority_Distance = Priority_Distance;
                    sender.MiniFlowTable.ToArray()[i].UpLinkPriority_Angle = Priority_Angle;

                }
           
                //排序
                sender.MiniFlowTable.Sort(new MiniFlowTableSorterUpLinkPriority());


                //候选集大小阈值，邻居节点总数开平方，然后向上取整
                int Ftheashoeld = Convert.ToInt16(Math.Ceiling(Math.Sqrt(sender.NeighborsTable.Count)));
                sender.forwardnumber = 0;
                foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                {
                    if (sender.forwardnumber < Ftheashoeld)
                    {
                        MiniEntry.UpLinkAction = FlowAction.Forward;
                        sender.forwardnumber++;
                    }
                    else
                        MiniEntry.UpLinkAction = FlowAction.Drop;

                }

                //环检测
                foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                {
                    if (MiniEntry.UpLinkAction == FlowAction.Forward)
                    {
                        foreach (MiniFlowTableEntry mini in MiniEntry.NeighborEntry.NeiNode.MiniFlowTable)
                        {
                            //发现环
                            if (mini.ID == sender.ID && mini.UpLinkAction == FlowAction.Forward)
                            {
                                //破环
                                //破环原则：保留周边路由节点平均能量较低者的路由表项
                                if (sender.MiniFlowTable_Energy_average >= MiniEntry.NeighborEntry.NeiNode.MiniFlowTable_Energy_average)
                                {
                                    MiniEntry.UpLinkAction = FlowAction.Drop;
                                    sender.forwardnumber--;

                                }
                                else
                                {
                                    mini.UpLinkAction = FlowAction.Drop;
                                    MiniEntry.NeighborEntry.NeiNode.forwardnumber--;

                                }
                               
                            }

                        }
                    }
                }


            }
            else {
                //转发表中只有sink节点
                foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                {
                    if (neiEntry.ID == 0)
                    {
                        MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                        MiniEntry.NeighborEntry = neiEntry;
                        MiniEntry.UpLinkPriority = 1;
                        sender.MiniFlowTable.Add(MiniEntry);


                    }

                }

            }


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

                //与sink不相邻的节点
                if (sender.HopsToSink > 1)

                {

                    //从邻居表中筛选出部分节点加入路由表中
                    foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                    {
                        //筛选一：仅当夹角为锐角且距离sink跳数更近的邻居节点才加入MiniFlowTable，初始UpLinkAction = Forward 
                        if (neiEntry.angle < 90 && neiEntry.NeiNode.HopsToSink <= sender.HopsToSink)
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
                    for (int i = 0; i < number_of_MiniFlowTable; i++)
                    {
                        Energy[i] = sender.MiniFlowTable.ToArray()[i].ResidualEnergyPercentage;
                        Distance[i] = sender.MiniFlowTable.ToArray()[i].Distance_Sender_To_Receiver;
                        Angle[i] = sender.MiniFlowTable.ToArray()[i].Angle;
                    }

                    //原始矩阵数据归一化
                    for (int i = 0; i < number_of_MiniFlowTable; i++)
                    {
                        //值越大越好
                        Energy[i] = Energy[i] / Energy_max;

                        //值越小越好
                        Distance[i] = Distance[i] / Distance_max;

                        //值越小越好
                        Angle[i] = Angle[i] / Angle_max;

                    }

                    //定义最终需要上传的属性矩阵

                    //定义最终需要上传的能量矩阵
                    double[] Energy_Up_To_Matlab = new double[number_of_MiniFlowTable];

                    //定义最终需要上传的距离矩阵
                    double[] Distance_Up_To_Matlab = new double[number_of_MiniFlowTable];

                    //定义最终需要上传的角度矩阵
                    double[] Angle_Up_To_Matlab = new double[number_of_MiniFlowTable];

                    //构建最终需要上传的属性矩阵
                    for (int i = 0; i < number_of_MiniFlowTable; i++)
                    {
                        //值越大越好,比重与数值成正比
                        Energy_Up_To_Matlab[i] = Standardization_Energy(Energy[i]);
                       // Energy_Up_To_Matlab[i] = Energy[i];

                        //值越小越好,比重与数值成反比
                        Distance_Up_To_Matlab[i] = Standardization_Distance(Distance[i]);
                        //Distance_Up_To_Matlab[i] = 1 - Distance[i];
                        //值越小越好,比重与数值成反比
                        Angle_Up_To_Matlab[i] = Standardization_Angle(Angle[i]);
                        //Angle_Up_To_Matlab[i] = 1 - Angle[i];
                    }

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
                    //// Create the MATLAB instance 
                      MLApp.MLApp matlab = new MLApp.MLApp(); 

                    // Change to the directory where the function is located 
                     matlab.Execute(@"cd c:\temp\example"); 

                     // Define the output 
                     object result = null; 

                     // Call the MATLAB function myfunc
                     matlab.Feval("myfunc", 2, out result, 3.14, 42.0, "world"); 

                     //Display result 
                     object[] res = result as object[]; 

                     Console.WriteLine(res[0]); 
                     Console.WriteLine(res[1]); 
                     Console.ReadLine(); 
                      */


                    //与Matlab建立连接

                    MLApp.MLApp matlab = new MLApp.MLApp();
                    matlab.Execute(@"cd C:\Users\Kang\Documents\MATLAB\Fuzzy");
                    //上传Energy_Up_To_Matlab矩阵到自动化服务器工作区，
                    matlab.PutFullMatrix("Energy_Up_To_Matlab", "base", Energy_Up_To_Matlab, pr);
                    //上传Distance_Up_To_Matlab矩阵自动化服务器工作区
                    matlab.PutFullMatrix("Distance_Up_To_Matlab", "base", Distance_Up_To_Matlab, pr);
                    //上传Angle_Up_To_Matlab矩阵自动化服务器工作区
                    matlab.PutFullMatrix("Angle_Up_To_Matlab", "base", Angle_Up_To_Matlab, pr);

                    //在自动化服务器中执行此命令
                    matlab.Execute("[AHP_Energy_output,AHP_Distance_output,AHP_Angle_output]=AHP_Fuzzy(Energy_Up_To_Matlab,Distance_Up_To_Matlab,Angle_Up_To_Matlab)");



                    //定义对应的二维数组，表示Forward Set中节点相对应各属性的AHP矩阵

                    //关于能量的AHP矩阵，AHP_Energy[i][j]表示第i个节点相对与第j个节点的能量比重
                    Array AHP_Energy = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                    //关于距离的AHP矩阵，AHP_Distance[i][j]表示第i个节点相对与第j个节点的距离比重
                    Array AHP_Distance = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                    //关于角度的AHP矩阵，AHP_Angle[i][j]表示第i个节点相对与第j个节点的角度比重
                    Array AHP_Angle = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                    //接收自动化Matlab中矩阵时充当虚部矩阵，此参数为函数所必须，不可省略
                    Array AHP_out_pr = new double[number_of_MiniFlowTable, number_of_MiniFlowTable];

                    //接收Matlab自动化服务器中处理过的矩阵，其中的值的含义与AHP矩阵中值含义相同

                    //接收Energy矩阵，实部存放在AHP_Energy中，虚部存放在ref AHP_out_pr中
                    matlab.GetFullMatrix("AHP_Energy_output", "base", ref AHP_Energy, ref AHP_out_pr);

                    //接收Distance矩阵，存放在AHP_Distance中
                    matlab.GetFullMatrix("AHP_Distance_output", "base", ref AHP_Distance, ref AHP_out_pr);

                    //接收Angle矩阵，存放在AHP_Angle中
                    matlab.GetFullMatrix("AHP_Angle_output", "base", ref AHP_Angle, ref AHP_out_pr);



                    //获取矩阵的最大特征值和特征向量
                    //最大特征值
                    double AHP_Energy_Eigenvalue;
                    double AHP_Distance_Eigenvalue;
                    double AHP_Angle_Eigenvalue;
                    //对应的特征向量
                    Array AHP_Energy_Eigenvector = new double[number_of_MiniFlowTable];
                    Array AHP_Distance_Eigenvector = new double[number_of_MiniFlowTable];
                    Array AHP_Angle_Eigenvector = new double[number_of_MiniFlowTable];

                    //接收来自Matlab自动化服务器的特征向量的虚部,此矩阵为全零矩阵
                    Array AHP_Eigenvector_pr = new double[number_of_MiniFlowTable];

                    //通过Matlab求AHP_Energy矩阵的最大特征值和对应的特征向量
                    matlab.PutFullMatrix("Matrix", "base", AHP_Energy, AHP_out_pr);
                    matlab.Execute("[Eigenvalue,Eigenvector] = Get_Max_Eigenvalue_Eigenvector(Matrix)");
                    AHP_Energy_Eigenvalue = matlab.GetVariable("Eigenvalue", "base");
                    matlab.GetFullMatrix("Eigenvector", "base", ref AHP_Energy_Eigenvector, ref AHP_Eigenvector_pr);


                    //通过Matlab求AHP_Distance矩阵的最大特征值和对应的特征向量
                    matlab.PutFullMatrix("Matrix", "base", AHP_Distance, AHP_out_pr);
                    matlab.Execute("[Eigenvalue,Eigenvector] = Get_Max_Eigenvalue_Eigenvector(Matrix)");
                    AHP_Distance_Eigenvalue = matlab.GetVariable("Eigenvalue", "base");
                    matlab.GetFullMatrix("Eigenvector", "base", ref AHP_Distance_Eigenvector, ref AHP_Eigenvector_pr);


                    //通过Matlab求AHP_Angle矩阵的最大特征值和对应的特征向量
                    matlab.PutFullMatrix("Matrix", "base", AHP_Angle, AHP_out_pr);
                    matlab.Execute("[Eigenvalue,Eigenvector] = Get_Max_Eigenvalue_Eigenvector(Matrix)");
                    AHP_Angle_Eigenvalue = matlab.GetVariable("Eigenvalue", "base");
                    matlab.GetFullMatrix("Eigenvector", "base", ref AHP_Angle_Eigenvector, ref AHP_Eigenvector_pr);

                    //归一化
                    Array AHP_Energy_Eigenvector_Normalization = Normalization(AHP_Energy_Eigenvector);
                    Array AHP_Distance_Eigenvector_Normalization = Normalization(AHP_Distance_Eigenvector);
                    Array AHP_Angle_Eigenvector_Normalization = Normalization(AHP_Angle_Eigenvector);


                    //保留距离和角度归一化向量
                    sender.AHP_Angle_Eigenvector_Normalization = AHP_Angle_Eigenvector_Normalization;
                    sender.AHP_Distance_Eigenvector_Normalization = AHP_Distance_Eigenvector_Normalization;



                    //构建AHP矩阵

                    double[,] array =
                    {
                       {1,      1,  3 },
                       {1 ,     1,  2 },
                       {1.0/3,  1.0/2,  1 }
                    };
                    //第一层AHP矩阵
                    Array AHP_Level1 = new double[3,3];
                    AHP_Level1 = array ;

                    Array AHP_Level1_pr_in = new double[3, 3];
                    Array AHP_Level1_pr_out = new double[3];
                    double AHP_Level1_Eigenvalue = 0; 

                    //第一层特征向量,依次表示能量，距离，角度的权重
                    Array AHP_Level1_Eigenvector = new double[3];
                    

                    //通过Matlab求AHP_Level1矩阵的最大特征值和对应的特征向量
                    matlab.PutFullMatrix("Matrix", "base", AHP_Level1, AHP_Level1_pr_in);
                    matlab.Execute("[Eigenvalue,Eigenvector] = Get_Max_Eigenvalue_Eigenvector(Matrix)");
                    AHP_Level1_Eigenvalue = matlab.GetVariable("Eigenvalue", "base");
                    matlab.GetFullMatrix("Eigenvector", "base", ref AHP_Level1_Eigenvector, ref AHP_Level1_pr_out);

                    //第一层特征向量归一化
                    Array AHP_Level1_Eigenvector_Normalization = Normalization(AHP_Level1_Eigenvector);






                    //求Priority == 属性在总目标上的权重 * 节点在该属性上的权重   然后求和
                    for (int i = 0; i < sender.MiniFlowTable.Count; i++)
                    {

                        double Priority_Energy = (double)AHP_Level1_Eigenvector_Normalization.GetValue(0) * (double)AHP_Energy_Eigenvector_Normalization.GetValue(i);
                        double Priority_Distance = (double)AHP_Level1_Eigenvector_Normalization.GetValue(1) * (double)AHP_Distance_Eigenvector_Normalization.GetValue(i);
                        double Priority_Angle = (double)AHP_Level1_Eigenvector_Normalization.GetValue(2) * (double)AHP_Angle_Eigenvector_Normalization.GetValue(i);
                        sender.MiniFlowTable.ToArray()[i].UpLinkPriority = Priority_Energy + Priority_Distance + Priority_Angle;
                        //具体得分比重，用于显示观察，鼠标移动到节点上可观测具体数值
                        sender.MiniFlowTable.ToArray()[i].UpLinkPriority_Energy = Priority_Energy ;
                        sender.MiniFlowTable.ToArray()[i].UpLinkPriority_Distance = Priority_Distance ;
                        sender.MiniFlowTable.ToArray()[i].UpLinkPriority_Angle = Priority_Angle ;

                    }
                    
                    //排序
                    sender.MiniFlowTable.Sort(new MiniFlowTableSorterUpLinkPriority());

                    
                    //候选集大小阈值，邻居节点总数开平方，然后向上取整
                    int Ftheashoeld = Convert.ToInt16(Math.Ceiling(Math.Sqrt(sender.NeighborsTable.Count)));
                    sender.forwardnumber = 0;
                    foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                    {
                        if (sender.forwardnumber < Ftheashoeld)
                        {
                            MiniEntry.UpLinkAction = FlowAction.Forward;
                            sender.forwardnumber++;
                        }
                        else
                            MiniEntry.UpLinkAction = FlowAction.Drop;

                    }

                    //环检测
                    foreach (MiniFlowTableEntry MiniEntry in sender.MiniFlowTable)
                    {
                        if (MiniEntry.UpLinkAction == FlowAction.Forward)
                        {
                            foreach (MiniFlowTableEntry mini in MiniEntry.NeighborEntry.NeiNode.MiniFlowTable)
                            {   
                                //发现环
                                if (mini.ID == sender.ID && mini.UpLinkAction == FlowAction.Forward)
                                {
                                    //破环
                                    if (sender.forwardnumber >= MiniEntry.NeighborEntry.NeiNode.forwardnumber)
                                    {
                                        MiniEntry.UpLinkAction = FlowAction.Drop;
                                        sender.forwardnumber--;

                                    }
                                    else
                                    {
                                        mini.UpLinkAction = FlowAction.Drop;
                                        MiniEntry.NeighborEntry.NeiNode.forwardnumber--;

                                    }
                                    number_loop_decrease++;
                                }

                            }
                        }
                    }
                }

                //与sink相邻的节点
                else
                {
                    //转发表中只有sink节点
                    foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                    {
                        if (neiEntry.ID == 0)
                        {
                            MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                            MiniEntry.NeighborEntry = neiEntry;
                            MiniEntry.UpLinkPriority = 1;
                            sender.MiniFlowTable.Add(MiniEntry);


                        }

                    }


                }





            }

            if (Settings.Default.RoutingAlgorithm == "ORR")
            {
                //sink的邻居节点
                if (sender.HopsToSink == 1)
                {
                    //转发表中仅包括sink节点
                    foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                    {
                        if (neiEntry.ID == 0)
                        {
                            MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                            MiniEntry.NeighborEntry = neiEntry;
                            MiniEntry.UpLinkPriority = 1;
                            sender.MiniFlowTable.Add(MiniEntry);
                        }
                    }
                }
                else
                {
                    foreach (NeighborsTableEntry nei in sender.NeighborsTable)
                    {
                        if (sender.Forwarders.Contains(nei.NeiNode))
                        {
                            MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                            MiniEntry.NeighborEntry = nei;
                            //FS表示优先级
                            MiniEntry.UpLinkPriority = nei.NeiNode.FS;
                            sender.MiniFlowTable.Add(MiniEntry);
                        }

                    }


                }

            }

            if (Settings.Default.RoutingAlgorithm == "ORW")
            {
                //sink的邻居节点
                if (sender.HopsToSink == 1)
                {
                    //转发表中仅包括sink节点
                    foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                    {
                        if (neiEntry.ID == 0)
                        {
                            MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                            MiniEntry.NeighborEntry = neiEntry;
                            MiniEntry.UpLinkPriority = 1;
                            sender.MiniFlowTable.Add(MiniEntry);
                        }
                    }
                }
                else
                {
                    foreach (NeighborsTableEntry nei in sender.NeighborsTable)
                    {
                        if (sender.Forwarders.Contains(nei.NeiNode))
                        {
                            MiniFlowTableEntry MiniEntry = new MiniFlowTableEntry();
                            MiniEntry.NeighborEntry = nei;
                            //EDC表示优先级
                            MiniEntry.UpLinkPriority = nei.NeiNode.EDC;
                            sender.MiniFlowTable.Add(MiniEntry);
                        }

                    }


                }

            }

        }
        //logistic函数 增函数
        private static double Standardization_Energy(double energy) {

            double y = 0;
            double A1 = 0.0065285;
            double A2 = 1.16336;
            double x0 = 0.25684;
            double p = 1.34544;
            
            y = A2 + (A1 - A2) / (1 + Math.Pow((energy / x0), p));

            //防止溢出、越界
            if (y >= 1)
                y = 1;
            if (y <= 0)
                y = 0;

            return y;
        }
        //Boltzmann函数 减函数
        private static double Standardization_Distance(double distance)
        {

            double y = 0;
            double k = 1;//函数返回值的系数[0，1]
            double A1 = 0.979;
            double A2 = -0.00809;
            double x0 = 0.55135;
            double dx = 0.09909;

            y = A2 + (A1 - A2) / (1+Math.Exp((distance-x0)/dx));

            //防止溢出、越界
            if (y >= 1)
                y = 1;
            if (y <= 0)
                y = 0;

            return k*y;
        }
        //Hill函数 减函数
        private static double Standardization_Angle(double angle)
        {

            double y = 0;
            double a = 1;//返回值系数
            double START = 0.93095;
            double END = 0.14541;
            double k = 0.5383;
            double n = 4.37126;

            y = START + (END - START) * Math.Pow(angle, n) / (Math.Pow(k, n) + Math.Pow(angle, n));


            //防止溢出、越界
            if (y >= 1)
                y = 1;
            if (y <= 0)
                y = 0;

            return a*y;
        }

        private static Array Normalization(Array Eigenvector)
        {

            Array Eigenvector_Normalization = new double[Eigenvector.Length];
            double sum = 0 ;

            for (int i = 0;i<Eigenvector.Length;i++)
            {
                sum = sum + (double)Eigenvector.GetValue(i);

            }
            for (int i = 0; i < Eigenvector.Length; i++)
            {
                Eigenvector_Normalization.SetValue((double)Eigenvector.GetValue(i)/sum, i);

            }


            return Eigenvector_Normalization;

        }
    }
}
