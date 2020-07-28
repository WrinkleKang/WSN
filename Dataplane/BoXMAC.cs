using MiniSDN.Intilization;
using MiniSDN.Forwarding;
using System;
using System.Windows.Threading;
using static MiniSDN.Dataplane.PublicParamerters;
using System.Windows.Shapes;
using System.Windows.Media;

namespace MiniSDN.Dataplane
{

    /// <summary>
    /// implementation of BoxMAC.
    /// Ammar Hawbani.
    /// </summary>
    public class BoXMAC : Shape
    {
        /// <summary>
        /// in sec.
        /// </summary>


        private Sensor Node; // the node that runs the BoxMac

        // this timer to swich on the sensor, when to start. after swiching on this sensor, this timer will be stoped.
        public DispatcherTimer SwichOnTimer = new DispatcherTimer();// ashncrous swicher.

        // the timer to swich between the sleep and active states.
        public DispatcherTimer ActiveSleepTimer = new DispatcherTimer();


        //定时器用来定时检测等待队列中是否有数据
        public DispatcherTimer QueueTimer = new DispatcherTimer();


        private int CheckActiveSleepTime = 50;//检测醒睡状态的时间间隔，单位：ms
        private int CheckQueueTime = 10;      //检测等待队列的时间间隔，单位：ms


        private int ActiveCounter = 0;
        private int SleepCounter = 0;

        protected override Geometry DefiningGeometry
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// intilize the MAC
        /// </summary>
        /// <param name="_Node"></param>
        public BoXMAC(Sensor _Node)
        {
            Node = _Node;
            if (Node != null)
            {
                if (Node.ID != PublicParamerters.SinkNode.ID)//设置非sink节点的醒睡模式
                {
                    //为了实现异步通信，每个节点开启醒睡模式的时刻不同
                    double xpasn = UnformRandomNumberGenerator.GetUniformSleepSec(MacStartUp);
                    // the swich on timer.
                    SwichOnTimer.Interval = TimeSpan.FromSeconds(xpasn);
                    SwichOnTimer.Start();
                    SwichOnTimer.Tick += ASwichOnTimer_Tick;

                    SleepCounter = 0;  //睡计时器
                    ActiveCounter = 0; //醒计数器，表示节点处于当前模式的时间
                    // active/sleep timer:定时改变SensorState的值，分别用Active表示醒，Sleep表示睡
                    // ActiveSleepTimer.Interval = TimeSpan.FromSeconds(1);
                    ActiveSleepTimer.Interval = TimeSpan.FromMilliseconds(CheckActiveSleepTime);
                    ActiveSleepTimer.Tick += ActiveSleepTimer_Tick;

                    //检测节点等待队列定时器的相关设置
                    QueueTimer.Interval = TimeSpan.FromMilliseconds(CheckQueueTime);
                    QueueTimer.Tick += QueueTimer_Tick;





                    // intialized:
                    Node.CurrentSensorState = SensorState.intalized;
                    Node.Ellipse_MAC.Fill = NodeStateColoring.IntializeColor;
                }
                else
                {
                    // sink节点的状态永远是Active
                    PublicParamerters.SinkNode.CurrentSensorState = SensorState.Active;
                }
            }
        }

        private void ActiveSleepTimer_Tick(object sender, EventArgs e)
        {

            /*
            if (Node.ID == 1)
            {

                Console.WriteLine("当前系统时间是：{0}", DateTime.Now);
                Console.WriteLine("NID: 1 State: " + Node.CurrentSensorState.ToString() + " ActiveCounter=" + ActiveCounter + " SleepCounter=" + SleepCounter);


            }*/
            // lock (Node)

            //初始版本显示会出错，已修改，具体修改内容查看Tags：修复MAC
            if (Node.CurrentSensorState == SensorState.Active)
            {
                ActiveCounter = ActiveCounter + CheckActiveSleepTime;

                /*
                 if (ActiveCounter == 1)
                 {

                     Action x = () => Node.Ellipse_MAC.Fill = NodeStateColoring.ActiveColor;
                     Dispatcher.Invoke(x);
                 }
                 else if (ActiveCounter > Periods.ActivePeriod)
                 {
                 */



                //Periods.ActivePeriod值是ActivePeriod默认值
                //可双击MiniSDN /Properties查看，可双击MiniSDN/App.config查找后进行修改

                //以下注释if语句为原始版本，原始版本没考虑等待队列中有数据包时，即使Active时间到也不进入睡眠模式

                //当醒周期超过预定时间且节点不处于传输数据包状态时才进入睡眠模式
                if (ActiveCounter >= Periods.ActivePeriod)
                {

                    while (!Node.TransmitState)
                    {

                        SwichToSleep();

                    }

                    /*

                        ActiveCounter = 0;
                        SleepCounter = 0;
                        Node.CurrentSensorState = SensorState.Sleep;
                        Action x = () => Node.Ellipse_MAC.Fill = NodeStateColoring.SleepColor;
                        Dispatcher.Invoke(x);

                    */
                }

            }
            else if (Node.CurrentSensorState == SensorState.Sleep)
            {
                SleepCounter = SleepCounter + CheckActiveSleepTime;
                /*
               if (SleepCounter == 1)
               {
                   Action x = () => Node.Ellipse_MAC.Fill = NodeStateColoring.SleepColor;
                   Dispatcher.Invoke(x);

                   // Node.DutyCycleString.Add(PublicParamerters.SimulationTime);
               }
               else if (SleepCounter > Periods.SleepPeriod)
               */
                //Periods.SleepPeriod值是SleepPeriod默认值
                //可双击MiniSDN /Properties查看，可双击MiniSDN/App.config查找后进行修改



                if (SleepCounter >= Periods.SleepPeriod)
                {
                    /*
                        ActiveCounter = 0;
                        SleepCounter = 0;
                        Node.CurrentSensorState = SensorState.Active;
                        Action x = () => Node.Ellipse_MAC.Fill = NodeStateColoring.ActiveColor;//改变节点颜色表示不同模式
                        Dispatcher.Invoke(x);
                        */
                    SwichToActive();
                }
            }


            /*
            //: Test. 
            if(Node.ID==1)
            {
            Console.WriteLine("当前系统时间是：{0}",DateTime.Now);   
            Console.WriteLine("NID: 61 State: " + Node.CurrentSensorState.ToString() + " ActiveCounter=" + ActiveCounter + " SleepCounter=" + SleepCounter);
            }*/

        }

        /// <summary>
        /// reset active counter.
        /// </summary>
        public void SwichToActive()
        {
            if (Node.ID != PublicParamerters.SinkNode.ID)
            {
                if (Node.CurrentSensorState == SensorState.Sleep)//sleep--active
                {
                    Dispatcher.Invoke(() => Node.CurrentSensorState = SensorState.Active, DispatcherPriority.Send);
                    Dispatcher.Invoke(() => SleepCounter = 0, DispatcherPriority.Send);
                    Dispatcher.Invoke(() => ActiveCounter = 0, DispatcherPriority.Send);
                }
                else//active--active 若原本是醒状态，则状态不变
                {

                }

            }

            //节点醒来之后会定时检测等待队列中是否有数据
            //有则准备发送数据，没有则在睡眠之前会继续检测
            QueueTimer.Start();



        }

        /// <summary>
        /// re set sleep counter.
        /// </summary>
        public void SwichToSleep()
        {
            if (Node.ID != PublicParamerters.SinkNode.ID)
            {
                //当等待队列中没有数据包要发送时才会进入睡眠模式,否则醒睡模式不变
                if (Node.WaitingPacketsQueue.Count == 0)
                {
                    if (Node.CurrentSensorState == SensorState.Active)//active--sleep
                    {
                        Dispatcher.Invoke(() => Node.CurrentSensorState = SensorState.Sleep, DispatcherPriority.Send);
                        Dispatcher.Invoke(() => SleepCounter = 0, DispatcherPriority.Send);
                        Dispatcher.Invoke(() => ActiveCounter = 0, DispatcherPriority.Send); ;
                    }
                    else//sleep--sleep 若原本是睡眠状态，则状态不变。
                    {
                        //SleepCounter = 0;
                    }

                }

            }

            //进入睡眠模式，停止检测等待队列，等待下次醒来
            QueueTimer.Stop();

        }

        /// <summary>
        /// run the timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ASwichOnTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => ActiveSleepTimer.Start(), DispatcherPriority.Send);
            Dispatcher.Invoke(() => Node.CurrentSensorState = SensorState.Active, DispatcherPriority.Send);
            Dispatcher.Invoke(() => Node.Ellipse_MAC.Fill = NodeStateColoring.ActiveColor, DispatcherPriority.Send);
            Dispatcher.Invoke(() => SwichOnTimer.Interval = TimeSpan.FromSeconds(0), DispatcherPriority.Send);
            Dispatcher.Invoke(() => SwichOnTimer.Stop(), DispatcherPriority.Send);// stop me
        }


        private void QueueTimer_Tick(object sender, EventArgs e)
        {
            //节点有数据要发送
            if (Node.WaitingPacketsQueue.Count > 0)
            {
                //停止检测，等处理完数据包再检测或睡觉
                QueueTimer.Stop();


                Node.HavePactketToSend();


                //发送完成后继续检测
                QueueTimer.Start();



            }


        }


    }
}
