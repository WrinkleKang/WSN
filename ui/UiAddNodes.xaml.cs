﻿using MiniSDN.Dataplane;
using MiniSDN.Forwarding;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MiniSDN.Intilization;

namespace MiniSDN.ui
{
    /// <summary>
    /// Interaction logic for UiAddNodes.xaml
    /// </summary>
    public partial class UiAddNodes : Window
    {
       
        public MainWindow MainWindow { get; set; }
        public UiAddNodes()
        {
            InitializeComponent();
        }


        private static double RdmGenerator(double max)
        {
            return max* RandomeNumberGenerator.GetUniform();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int nodeCount = MainWindow.Canvas_SensingFeild.Children.Count;
                double r = Convert.ToDouble(txt_range.Text);
                double d = 25;
                PublicParamerters.SensingRangeRadius = r;
                if (txt_nodes_number.Text.Trim().Length > 0)
                {
                    int NodesNumber = Convert.ToInt16(txt_nodes_number.Text);
                    int density = 20;
                    double width = MainWindow.Canvas_SensingFeild.Width;
                    double height = MainWindow.Canvas_SensingFeild.Height;
                    Point center = new Point(width / 2, height / 2);

                    Sensor node = new Sensor(0);
                    node.MainWindow = MainWindow;
                    Point resPos = new Point(center.X - 2 * d, center.Y - 2 * d);
                    node.Position = resPos;
                    node.VisualizedRadius = r;
                    MainWindow.Canvas_SensingFeild.Children.Add(node);
                    MainWindow.myNetWork.Add(node);

                    Queue<Point> Q = new Queue<Point>();
                    List<Point> L = new List<Point>();
                    Q.Enqueue(center);
                    while (Q.Count > 0)
                    {
                        if (L.Count > NodesNumber)
                            break;
                        Point pos = Q.Dequeue();
                        double dis = Math.Sqrt(Math.Pow(pos.X - center.X, 2) + Math.Pow(pos.Y - center.Y, 2));
                        for (int i = 0; i < density; i++)
                        {
                            double nr = RdmGenerator(d) + d;
                            double na = RdmGenerator(Math.PI * 2);
                            double x = pos.X + nr * Math.Cos(na);
                            double y = pos.Y + nr * Math.Sin(na);
                            if (x > 5 && x < width - 5 && y > 5 && y < height - 5)
                            {
                                double tdis = Math.Sqrt(Math.Pow(x - center.X, 2) + Math.Pow(y - center.Y, 2));
                                if (tdis > dis)
                                {
                                    bool tag = false;
                                    foreach (Point p in L)
                                    {
                                        if (Math.Sqrt(Math.Pow(x - p.X, 2) + Math.Pow(y - p.Y, 2)) < d)
                                        {
                                            tag = true;
                                            break;
                                        }
                                    }
                                    if (tag == false)
                                    {
                                        Point newPos = new Point(x, y);
                                        Q.Enqueue(newPos);
                                        L.Add(newPos);
                                    }
                                }
                            }
                        }
                    }

                    int id = 1;
                    while (L.Count > 0)
                    {
                        int pos = (int)RdmGenerator(L.Count);
                        node = new Sensor(id++);
                        node.MainWindow = MainWindow;
                        resPos = new Point(L[pos].X - 2 * d, L[pos].Y - 2 * d);
                        node.Position = resPos;
                        node.VisualizedRadius = r;
                        MainWindow.Canvas_SensingFeild.Children.Add(node);
                        MainWindow.myNetWork.Add(node);
                        L.RemoveAt(pos);
                    }

                    Console.WriteLine("Node number: " + id);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            finally
            {
                this.Close();
            }


        }

        private void new_node_click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Convert.ToInt16(add_more_node.Text); i++)
            {
                Sensor node = new Sensor(MainWindow.myNetWork.Count);
                node.MainWindow = MainWindow;
                Point resPos = new Point(0, 0 + 20 * i);
                node.Position = resPos;
                node.VisualizedRadius = 40;
                MainWindow.Canvas_SensingFeild.Children.Add(node);
                MainWindow.myNetWork.Add(node);
            }
        }
    }
}
