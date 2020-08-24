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

                //不可直接发给sink的节点
                if (sender.HopsToSink > 1)

                {

                    //从邻居表中筛选出部分节点加入路由表中
                    foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
                    {
                        //筛选一：仅当夹角为锐角且距离sin跳数更近的邻居节点才加入MiniFlowTable，初始UpLinkAction = Forward 
                        if (neiEntry.angle <= 90 && neiEntry.NeiNode.HopsToSink <= sender.HopsToSink)
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
                        Distance_Up_To_Matlab[i] = 1 - Distance[i];

                        //值越小越好,比重与数值成反比
                        Angle_Up_To_Matlab[i] = 1 - Angle[i];

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
                    matlab.PutFullMatrix("Energy_Up_To_Matlab", "base", Energy, pr);
                    //上传Distance_Up_To_Matlab矩阵自动化服务器工作区
                    matlab.PutFullMatrix("Distance_Up_To_Matlab", "base", Distance, pr);
                    //上传Angle_Up_To_Matlab矩阵自动化服务器工作区
                    matlab.PutFullMatrix("Angle_Up_To_Matlab", "base", Angle, pr);

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

                    //接收来自Matlab自动化服务器的特征向量的虚部,全零
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




                    //构建AHP矩阵
                    /*
                    Energy   Distance   Angle
                      1         1/5     1/2
                      5         1       3
                      2         1/3     1

                 特征向量        (0.1747 0.9281 0.3288)
                 归一化特征向量 （0.1217 0.6457 0.2286）

                    */
                    double[,] AHP_Level1 = { {1, 1.0/5,1.0/2 },
                                         {5 ,   1,    3 },
                                         {2,   1.0/3, 1 } };

                    //第一层特征向量,依次表示能量，距离，角度的权重
                    Array AHP_Level1_Eigenvector = new double[3];
                    AHP_Level1_Eigenvector.SetValue(0.1217, 0);
                    AHP_Level1_Eigenvector.SetValue(0.6457, 1);
                    AHP_Level1_Eigenvector.SetValue(0.2286, 2);


                    //求Priority == 属性在总目标上的权重 * 节点在该属性上的权重   然后求和
                    for (int i = 0; i < sender.MiniFlowTable.Count; i++)
                    {

                        double Priority_Energy = (double)AHP_Level1_Eigenvector.GetValue(0) * (double)AHP_Energy_Eigenvector_Normalization.GetValue(i);
                        double Priority_Distance = (double)AHP_Level1_Eigenvector.GetValue(1) * (double)AHP_Distance_Eigenvector_Normalization.GetValue(i);
                        double Priority_Angle = (double)AHP_Level1_Eigenvector.GetValue(2) * (double)AHP_Angle_Eigenvector_Normalization.GetValue(i);
                        sender.MiniFlowTable.ToArray()[i].UpLinkPriority = Priority_Energy + Priority_Distance + Priority_Angle;

                    }



                    //构建FAHP
                    //AHP中每个元素Aij都有三个值，以此增大，分别表示某主观判断下的最低，适中，和最高权重

                    //第一此主观下的三个矩阵：最低权重矩阵、适中权重矩阵、最高权重矩阵
                    double[,] FAHP_Min_First = new double[3, 3];
                    double[,] FAHP_Mid_First = new double[3, 3];
                    double[,] FAHP_Max_First = new double[3, 3];

                    //第二次主观下的三个矩阵
                    double[,] FAHP_Min_Second = new double[3, 3];
                    double[,] FAHP_Mid_Second = new double[3, 3];
                    double[,] FAHP_Max_Second = new double[3, 3];

                    //第三次主观性下的三个矩阵
                    double[,] FAHP_Min_Third = new double[3, 3];
                    double[,] FAHP_Mid_Third = new double[3, 3];
                    double[,] FAHP_Max_Third = new double[3, 3];



                    //构建AHP矩阵
                    /*
                    Energy   Distance   Angle
                      1         1/5     1/2
                      5         1       3
                      2         1/3     1


                 特征向量 （0.1217  0.6457   0.2286）

                    */






                    //将上述AHP矩阵中的值Fuzzy化得到矩阵如下



                    double[,,,] FAHP = {

                  { 
                     //AHP[0,0]
                    { {1,1,1 },{1,1,1 },{1,1,1 } },
                    //AHP[0,1]==1/5  其中共有三个一维矩阵，表示三次主观，每个一维矩阵有三个值，表示最低，中等，最高比值
                    { {3.0/16.0,1.0/5.0,2.0/9.0 },{5.0/22.0,1.0/4.0,3.0/11.0 },{2.0/11.0,1.0/5.0,5.0/24.0 } },
                    //AHP[0,2]==1/2
                    { {2.0/5.0,1.0/2.0,2.0/3.0 },{5.0/13.0,1.0/2.0,3.0/2.0 },{1.0/2.0,2.0/3.0,1.0 } },

                  },

                  { 
                     //AHP[1,0]
                    { {9.0/2.0,5,16.0/3.0 },{11.0/3.0,4,2.0/5.0 },{24.0/5.0,5,11.0/2.0 } },
                    //AHP[1,1]
                    { {1,1,1 },{1,1,1 },{1,1,1 } },
                    //AHP[1,2]
                    { {7.0/3.0,3,10.0/3.0 },{2,5.0/2,14.0/5 },{8.0/3,3,7.0/2 } },

                   },

                  {
                    //AHP[2,0]
                    { {3.0/2,2,5.0/2 },{2.0/3,2,13.0/5 },{1,3.0/2,2 } },
                    //AHP[2,1]
                    { {3.0/10,1.0/3,3.0/7 },{5.0/14,2.0/5,1.0/2 },{2.0/7,1/3.0,3.0/8 } },
                    //AHP[2,2]
                    { {1,1,1 },{1,1,1 },{1,1,1 } },

                  },


                };


                    //构建三次主观下的三个矩阵
                    // FAHP[i,j,k,m]
                    for (int k = 0; k < 3; k++)
                    {
                        for (int m = 0; m < 3; m++)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    if (k == 0 && m == 0)
                                    {
                                        //所有矩阵的第[0,0]元素，对应于第一次主观的最小矩阵的值
                                        FAHP_Min_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 0 && m == 1)
                                    {
                                        //所有矩阵的第[0,1]元素，对应于第一次主观的适中矩阵的值
                                        FAHP_Mid_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 0 && m == 2)
                                    {
                                        //所有矩阵的第[0,2]元素，对应于第一次主观的最大矩阵的值
                                        FAHP_Max_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 1 && m == 0)
                                    {
                                        //所有矩阵的第[1,0]元素，对应于第二次主观的最小矩阵的值
                                        FAHP_Min_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 1 && m == 1)
                                    {
                                        //所有矩阵的第[1,1]元素，对应于第二次主观的适中矩阵的值
                                        FAHP_Min_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 1 && m == 2)
                                    {
                                        //所有矩阵的第[1,2]元素，对应于第二次主观的最大矩阵的值
                                        FAHP_Min_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 2 && m == 0)
                                    {
                                        //所有矩阵的第[2,0]元素，对应于第三次主观的最小矩阵的值
                                        FAHP_Min_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 2 && m == 1)
                                    {
                                        //所有矩阵的第[2.1]元素，对应于第三次主观的适中矩阵的值
                                        FAHP_Min_First[i, j] = FAHP[i, j, k, m];

                                    }
                                    if (k == 2 && m == 2)
                                    {
                                        //所有矩阵的第[2,2]元素，对应于第三次主观的最大矩阵的值
                                        FAHP_Min_First[i, j] = FAHP[i, j, k, m];

                                    }

                                }

                            }

                        }

                    }









                }

                //可直接发给sink的节点
                else
                {

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
