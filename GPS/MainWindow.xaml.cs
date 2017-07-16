using System;
using System.Collections.Generic;
using System.Windows;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

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
        List<position> allGPSPosition = new List<position>();
        List<position> rposition = new List<position>();
        List<List<string>> N0;
        List<List<string>> O;
        string[] GPS;

        // GPS在8/10点的位置
        private void positionOfGPS(object sender, RoutedEventArgs e)
        {
            int index = 0;
            allGPSPosition = read.readfile();

            allGPSPosition.ForEach(item =>
            {
                GPSpositionBox.Text += String.Format("{5}. PRN: {0}, TIME: {1}\n X: {2}\n Y: {3}\n Z: {4} \n\n",
                    item.PRN.ToString(),
                    item.hour.ToString(),
                    item.X.ToString(),
                    item.Y.ToString(),
                    item.Z.ToString(),
                    index++);
            });
        }
        // O文件里的卫星在指定时间的位置
        private void positionOfReceiver(object sender, RoutedEventArgs e)
        {
            // GPS在9点半的位置
            Dictionary<int, double[]> position = N.GPSPositionAt(GPS, allGPSPosition, 90);
            for (int i = 0; i < O.Count; ++i)
            {
                //position p = receiverPosition(position, i);
                position p = read.getReceiver()[i];
                ReceiverPositionBox.Text += String.Format("接收机：{0}\nX: {1}\n Y: {2}\n Z: {3}\n\n", p.PRN, p.X, p.Y, p.Z);
            }

        }
        // 基线长度
        private void lengthOfBaseline(object sender, RoutedEventArgs e)
        {
            position[] rposition = read.getReceiver();
            double baseLine = Math.Sqrt(Math.Pow(rposition[1].X - rposition[0].X, 2) + Math.Pow(rposition[1].Y - rposition[0].Y, 2) + Math.Pow(rposition[1].Z - rposition[0].Z, 2));
            baseLineBox.Text = baseLine.ToString();
        }
        // 读取O文件
        private void readO(object sender, RoutedEventArgs e)
        {
            fileReader fileReader = new fileReader();
            OPath.Text = fileReader.getPath();
            O = fileReader.readFile(12, true);
            string receiver0 = O[0][0], receiver = O[1][0];
            GPS = O[0][8].Split('G');
            OPath.Text = fileReader.getPath();

            ////
            fileReader r = new fileReader();

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

        // 接收机坐标
        //position 可见卫星在某个时刻的坐标
        //index O文件索引（2）
        private position receiverPosition(Dictionary<int, double[]> position, int index)
        {
            // 初始位置和是中距离偏差
            double[,] initXYZD = new double[4, 1];
            for(int jj = 0; jj < 4; ++jj){

                // 卫星的数量
                int count = Convert.ToInt32(GPS[0]);
                int[] newGPS = Array.ConvertAll(GPS.SubArray(1, GPS.Length - 1), s => int.Parse(s));
                // 方向系数
                double[,] eji = new double[count, 3];
                // eji专用索引
                int k = 0;
                foreach (int PRN in newGPS)
                {
                    double d = Math.Sqrt(Math.Pow(position[PRN][0] - initXYZD[0, 0], 2) + Math.Pow(position[PRN][1] - initXYZD[1, 0], 2) + Math.Pow(position[PRN][2] - initXYZD[2, 0], 2));
                    for (int j = 0; j < 3; ++j)
                    {
                        eji[k, j] = position[PRN][j] - initXYZD[j, 0] / d;
                    }
                    k++;
                }
                // 伪距集合
                double[,] Pt = new double[count,1];
                for (int i = 0; i < count - 9; ++i)
                {
                    string a = O[index][9 + i];
                    Pt[i, 0] = Convert.ToDouble(O[index][9 + i]);
                }
                // 转置矩阵
                int h = eji.GetLength(0);
                int w = eji.GetLength(1);
                Matrix<double> Gu = DenseMatrix.OfArray(new double[h, w + 1]);
                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w + 1; ++j)
                    {
                        if (j < w) Gu[i, j] = eji[i, j];
                        else Gu[i, j] = 1;
                    }
                }
                Matrix<double> GuT = Gu.Transpose();

                // 求逆
                Matrix<double> GuInverse = (GuT * Gu).Inverse();
                // Au * St
                Matrix<double> AuSt = new DenseMatrix(10, 1);
                int ii = 0;
                foreach (int PRN in newGPS)
                {
                    AuSt.Add(Gu[ii, 0] * position[PRN][0] + Gu[ii, 1] * position[PRN][1] + Gu[ii, 2] * position[PRN][2] + position[PRN][3]);
                    ii++;
                }
                // Xt
                Matrix<double> Xt = GuInverse * GuT * (AuSt - DenseMatrix.OfArray(Pt));
                initXYZD = Xt.ToArray();
            }
            position receiverPosition = new position();
            receiverPosition.X = initXYZD[0, 0];
            receiverPosition.Y = initXYZD[1, 0];
            receiverPosition.Z = initXYZD[2, 0];
            return receiverPosition;
        }
    }

}
