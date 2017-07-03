using System;
using System.Collections.Generic;
using System.Windows;
namespace GPS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        List<Dictionary<string, double>> allGPSPosition;
        List<List<string>> N0;
        List<List<string>> O;
        string[] GPS;

        // GPS位置
        private void positionOfGPS(object sender, RoutedEventArgs e)
        {
            int index = 0;
            N0.ForEach(item =>
            {
                N temp = new N(item);
                Dictionary<string, double> position = temp.GPSPositionCalculate();
                allGPSPosition.Add(position);
                GPSpositionBox.Text += String.Format("{5}. PRN: {0}, TIME: {1}\n X: {2}\n Y: {3}\n Z: {4} \n\n", 
                    position["PRN"].ToString(), 
                    position["hour"].ToString(), 
                    position["Xk"].ToString(), 
                    position["Yk"].ToString(), 
                    position["Zk"].ToString(),
                    index++);
            });
        }
        // O文件里的卫星在指定时间的位置
        private void positionOfReceiver(object sender, RoutedEventArgs e)
        {
            Dictionary<int, double[]> position = N.GPSPositionAt(GPS, allGPSPosition, 90);
            // double[] pseudorange = O

        }
        // 基线长度
        private void lengthOfBaseline(object sender, RoutedEventArgs e)
        {

        }
        // 读取O1文件
        private void readO(object sender, RoutedEventArgs e)
        {
            fileReader fileReader = new fileReader();
            O0Path.Text = fileReader.getPath();
            O = fileReader.readFile(11, true);
            string receiver0 = O[0][0], receiver = O[1][0];
            GPS = O[0][8].Split('G');
        }
        // 读取N文件
        private void readN(object sender, RoutedEventArgs e)
        {
            NPath.Text = "";
            GPSpositionBox.Text = "";
            fileReader fileReader = new fileReader();
            N0 = fileReader.readFile(8, true);
            NPath.Text = fileReader.getPath();
        }
    }
}
