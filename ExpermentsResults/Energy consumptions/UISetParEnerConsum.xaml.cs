using MiniSDN.Dataplane;
using MiniSDN.Properties;
using MiniSDN.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MiniSDN.ExpermentsResults.Energy_consumptions
{
    /// <summary>
    /// Interaction logic for UISetParEnerConsum.xaml
    /// </summary>
    public partial class UISetParEnerConsum : Window
    {
        MainWindow _MainWindow;
        public UISetParEnerConsum(MainWindow __MainWindow_)
        {
            InitializeComponent();
            _MainWindow = __MainWindow_;
            //设置窗口的一些参数的预选项或取值范围

            try
            {
                //醒周期时间到了仍然有数据包没有发送出去，则
                //Sleep at once ==>立马睡  
                //One more active period==>多延迟一个醒周期  
                //Waitting all the time==>一直等

                comb_active_no_receive.Items.Add("Sleep at once");
                comb_active_no_receive.Items.Add("One more active period");
                comb_active_no_receive.Items.Add("Waitting all the time");


                comb_active_no_receive.Text = Settings.Default.ActiveNoReceive;


                //选择路由协议
                comb_routing_algorithm.Items.Add("LORA");
                comb_routing_algorithm.Items.Add("AHP_Fuzzy");
                comb_routing_algorithm.Items.Add("ORR");
                comb_routing_algorithm.Items.Add("ORW");
                comb_routing_algorithm.Text = Settings.Default.RoutingAlgorithm;



                //每损失多少能量广播一次消息
                for (int i = 5; i <= 50; i++)
                {
                    com_UpdateLossPercentage.Items.Add(i);
                }
                //Min-Flow算法中参数的可选值
                for (int j = 0; j <= 9; j++)
                {
                    string str = "0." + j;
                    double dc = Convert.ToDouble(str);
                    com_D.Items.Add(dc);
                    com_H.Items.Add(dc);
                    com_L.Items.Add(dc);
                    com_R.Items.Add(dc);
                    com_Dir.Items.Add(dc);
                }


                for (int j = 1; j <= 10; j++)
                {

                    com_D.Items.Add(j);
                    com_H.Items.Add(j);
                    com_L.Items.Add(j);
                    com_R.Items.Add(j);
                    com_Dir.Items.Add(j);
                }

                

                com_H.Text = Settings.Default.ExpoHCnt.ToString();
                com_L.Text = Settings.Default.ExpoLCnt.ToString();
                com_R.Text = Settings.Default.ExpoRCnt.ToString();
                com_D.Text = Settings.Default.ExpoDCnt.ToString();
                com_Dir.Text = Settings.Default.ExpoECnt.ToString();


                
            }
            catch
            {
                MessageBox.Show("Error!!!.");
            }
            //有关实验运行过程中的一些动画或显示功能
            com_UpdateLossPercentage.Text = Settings.Default.UpdateLossPercentage.ToString();
            Settings.Default.ShowRoutingPaths = false;
            Settings.Default.SaveRoutingLog = false;
            Settings.Default.ShowAnimation = false;
            Settings.Default.ShowRadar = false;

            //仿真时间，当选择第一个节点死亡时停止模拟时，默认值是300，该Items不可选
            for (int i = 60; i <= 1000; i = i + 60)
            {
                comb_simuTime.Items.Add(i);
               
            }
            comb_simuTime.Text = "300";

            comb_packet_rate.Items.Add("0.001");
            comb_packet_rate.Items.Add("0.01");
            comb_packet_rate.Items.Add("0.1");
            comb_packet_rate.Items.Add("0.5");
            for (int i = 1; i <= 5; i++)
            {
                comb_packet_rate.Items.Add(i);
            }

            comb_packet_rate.Text = "0.1";

            for(int i=5;i<=15;i++)
            {
                comb_startup.Items.Add(i);
            }
            comb_startup.Text = "10";



            //可供选择的醒睡周期值

            for (int i = 1; i <= 9; i++)
            {
                int j = i * 100;
                comb_active.Items.Add(j);
                comb_sleep.Items.Add(j);
            }

            for (int i=1;i<=5;i++)
            {
                int j = i * 1000;
                comb_active.Items.Add(j);
                comb_sleep.Items.Add(j);
            }



            //设置醒睡周期，该值将修改默认值，即重新设定默认值
            comb_active.Text = "1000";
            comb_sleep.Text = "2000";



            //设置检测等待队列计时器周期
            comb_queueTime.Text = "50";
            for (int i = 1; i <= 10; i++)
            {

                int j = i * 10;
                comb_queueTime.Items.Add(j);

            }


            for (int i = 1; i <= 10; i++)
            {
              
                int j = i * 100;
                comb_queueTime.Items.Add(j);
            
            }

            comb_batteryIntialEnergy.Text = "0.05";
            comb_batteryIntialEnergy.Items.Add("0.005");
            comb_batteryIntialEnergy.Items.Add("0.01");
            comb_batteryIntialEnergy.Items.Add("0.025");
            comb_batteryIntialEnergy.Items.Add("0.05");
            comb_batteryIntialEnergy.Items.Add("0.1");
            comb_batteryIntialEnergy.Items.Add("0.25");
            comb_batteryIntialEnergy.Items.Add("0.5");
            comb_batteryIntialEnergy.Items.Add("1");
            comb_batteryIntialEnergy.Items.Add("5");





            int conrange = 5;
            for (int i = 0; i <= conrange; i++)
            {
                if (i == conrange)
                {
                    double dc = Convert.ToDouble(i);
                   
                }
                else
                {
                    for (int j = 0; j <= 9; j++)
                    {
                        string str = i + "." + j;
                        double dc = Convert.ToDouble(str);
                       

                    }
                }
            }

        



        }


        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            //初始化拓扑图



            //预设某些默认参数的初始值，其来源是窗口中各部件的值
            //等待队列计时器值以及醒睡周期的值
            Settings.Default.CheckQueueTime = Convert.ToInt16(comb_queueTime.Text);
            Settings.Default.BatteryIntialEnergy = Convert.ToDouble(comb_batteryIntialEnergy.Text);
            Settings.Default.ActivePeriod = Convert.ToInt16(comb_active.Text);
            Settings.Default.SleepPeriod = Convert.ToInt16(comb_sleep.Text);
            Settings.Default.MacStartUp = (Settings.Default.ActivePeriod + Settings.Default.SleepPeriod)/1000;//节点启动醒睡模式将在一个醒睡周期内完成



            Settings.Default.UpdateLossPercentage = Convert.ToInt16(com_UpdateLossPercentage.Text);
            Settings.Default.DrawPacketsLines = Convert.ToBoolean(chk_drawrouts.IsChecked);
            Settings.Default.KeepLogs= Convert.ToBoolean(chk_save_logs.IsChecked);
            Settings.Default.StopeWhenFirstNodeDeid = Convert.ToBoolean(chk_stope_when_first_node_deis.IsChecked);
           

            Settings.Default.ExpoRCnt = Convert.ToDouble(com_R.Text);
            Settings.Default.ExpoLCnt = Convert.ToDouble(com_L.Text);
            Settings.Default.ExpoHCnt = Convert.ToDouble(com_H.Text);
            Settings.Default.ExpoDCnt = Convert.ToDouble(com_D.Text);
            Settings.Default.ExpoECnt = Convert.ToDouble(com_Dir.Text);


            if (Settings.Default.StopeWhenFirstNodeDeid == false)
            {
                int stime = Convert.ToInt16(comb_simuTime.Text);

                double packetRate = Convert.ToDouble(comb_packet_rate.Text);
                _MainWindow.stopSimlationWhen = stime;
                _MainWindow.RandomDeplayment(0);
                double numpackets = Convert.ToDouble(stime) / packetRate;
                _MainWindow.GenerateUplinkPacketsRandomly(Convert.ToInt32(numpackets));
                _MainWindow.PacketRate = "1 packet per " + packetRate + " s";
            }
            else if (Settings.Default.StopeWhenFirstNodeDeid == true)//当选择第一个节点能量耗尽时停止程序
            {
                int stime = 100000000;
                double packper = Convert.ToDouble(comb_packet_rate.Text);//发包速率
                _MainWindow.stopSimlationWhen = stime;



                _MainWindow.RandomDeplayment(0);//网络初始化,0表示sink节点ID，重点理解与掌握


               

                _MainWindow.SendPackectPerSecond(packper);//根据设定的发包速率进行发包

            }

            Close();

        }

        private void comb_startup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_startup.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            Settings.Default.MacStartUp = va;
        }

        //修改默认醒周期值
        private void comb_active_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_active.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            Settings.Default.ActivePeriod = va;
        }

        //修改默认睡周期值
        private void comb_sleep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_sleep.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            Settings.Default.SleepPeriod = va;
        }
        //修改默认等待队列周期
        private void comb_queueTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_queueTime.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            Settings.Default.CheckQueueTime = va;

        }





        private void chk_stope_when_first_node_deis_Checked(object sender, RoutedEventArgs e)
        {
            comb_simuTime.IsEnabled = false;
        }

        private void chk_stope_when_first_node_deis_Unchecked(object sender, RoutedEventArgs e)
        {
            comb_simuTime.IsEnabled = true;
        }

      

        private void chk_drawrouts_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRoutingPaths = true;
        }

        private void chk_drawrouts_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRoutingPaths = false;
        }

        private void chk_save_logs_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SaveRoutingLog = true;
        }

        private void chk_save_logs_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.SaveRoutingLog = false;
        }

        private void chek_show_radar_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRadar = true;
        }

        private void chek_show_radar_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRadar = false;
        }

        private void chek_animation_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowAnimation = true;
        }

        private void chek_animation_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowAnimation = false;
        }

        private void comb_batteryIntialEnergy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_batteryIntialEnergy.SelectedItem as object;
           double va = Convert.ToDouble(objval);
            Settings.Default.BatteryIntialEnergy = va;

        }

        private void comb_active_no_Receive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            
            Settings.Default.ActiveNoReceive = comb_active_no_receive.SelectedItem.ToString();
            //  string va = objval;
            //  Settings.Default.ActiveNoReceive = va;

        }

        private void comb_routing_algorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.RoutingAlgorithm = comb_routing_algorithm.SelectedItem.ToString();
        }
    }
}
