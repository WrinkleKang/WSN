using MiniSDN.Dataplane;
using System;
using System.Windows;

namespace MiniSDN.Intilization
{
    public class Operations
    {
        public static double Angle_SenderReceive_TO_SenderSink(Sensor Sender, Sensor Receive, Sensor Sink)
        {
            double Angle = 0;
            //sink节点不考虑角度值，非sink节点邻居表中加入角度值
            if(Sender.ID != 0)
            {
                //获取三点坐标
                double sender_x = Sender.CenterLocation.X;
                double sender_y = Sender.CenterLocation.Y;
                double receive_x = Receive.CenterLocation.X;
                double receive_y = Receive.CenterLocation.Y;
                double sink_x = Sink.CenterLocation.X;
                double sink_y = Sink.CenterLocation.Y;

                //获取A向量坐标 A=(sender_receive_x，sender_receive_y)，起点为sender节点，终点为receive节点
                double sender_receive_x = receive_x - sender_x;
                double sender_receive_y = receive_y - sender_y;

                //获取B向量坐标 B=(sender_sink_x，sender_sink_y)，起点为sender节点，终点为sink节点
                double sender_sink_x = sink_x - sender_x;
                double sender_sink_y = sink_y - sender_y;

                //先求AB的向量积
                double AB = sender_receive_x * sender_sink_x + sender_receive_y * sender_sink_y;

                //再求A,B的模
                double A_length = Math.Sqrt(sender_receive_x * sender_receive_x + sender_receive_y * sender_receive_y);
                double B_length = Math.Sqrt(sender_sink_x * sender_sink_x + sender_sink_y * sender_sink_y);

                //cosM = A.B/|AB|  cosM = 向量积/模的乘积
                double cosM = AB / (A_length * B_length);

                

                //反三角函数取夹角,角度与弧度需要转换
                 Angle = Math.Acos(cosM) * 180 / Math.PI;
            }
            


            return Angle;
        }

        public static double DistanceBetweenTwoSensors(Sensor sensor1, Sensor sensor2)
        {
            double dx = (sensor1.CenterLocation.X - sensor2.CenterLocation.X);
            dx *= dx;
            double dy = (sensor1.CenterLocation.Y - sensor2.CenterLocation.Y);
            dy *= dy;
            return Math.Sqrt(dx + dy);
        }

        public static double DistanceBetweenTwoPoints(Point p1, Point p2)
        {
            double dx = (p1.X - p2.X);
            dx *= dx;
            double dy = (p1.Y - p2.Y);
            dy *= dy;
            return Math.Sqrt(dx + dy);
        }

        /// <summary>
        /// the communication range is overlapped.
        /// 
        /// </summary>
        /// <param name="sensor1"></param>
        /// <param name="sensor2"></param>
        /// <returns></returns>
        public static bool isOverlapped(Sensor sensor1, Sensor sensor2)
        {
            bool re = false;
            double disttance = DistanceBetweenTwoSensors(sensor1, sensor2);
            if (disttance < (sensor1.ComunicationRangeRadius + sensor2.ComunicationRangeRadius))
            {
                re = true;
            }
            return re;
        }

        /// <summary>
        /// check if j is within the range of i.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static bool isInMySensingRange(Sensor i, Sensor j)
        {
            bool re = false;
            double disttance = DistanceBetweenTwoSensors(i, j);
            if (disttance <= (i.VisualizedRadius))
            {
                re = true;
            }
            return re;
        }

        /// <summary>
        /// commnication=sensing rang*2
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static bool isInMyComunicationRange(Sensor i, Sensor j)
        {
            bool re = false;
            double disttance = DistanceBetweenTwoSensors(i, j);
            if (disttance <= (i.ComunicationRangeRadius))
            {
                re = true;
            }
            return re;
        }

        public static double FindNodeArea(double com_raduos)
        {
            return Math.PI * Math.Pow(com_raduos, 2);
        }

        /// <summary>
        /// n!
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double Factorial(int n)
        {
            long i, fact;
            fact = n;
            for (i = n - 1; i >= 1; i--)
            {
                fact = fact * i;
            }
            return fact;
        }

        /// <summary>
        /// combination 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double Combination(int n, int k)
        {
            if (k == 0 || n == k) return 1;
            if (k == 1) return n;
            int dif = n - k;
            int max = Max(dif, k);
            int min = Min(dif, k);

            long i, bast;
            bast = n;
            for (i = n - 1; i > max; i--)
            {
                bast = bast * i;
            }
            double mack = Factorial(min);
            double x = bast / mack;
            return x;
        }


        private static int Max(int n1,int n2) { if (n1 > n2) return n1; else return n2; }
        private static int Min(int n1, int n2) { if (n1 < n2) return n1; else return n2; } 
    }
}
