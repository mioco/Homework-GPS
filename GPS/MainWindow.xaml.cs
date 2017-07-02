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
        List<N> nFile = new List<N>();
        fileReader fileReader = new fileReader();
        string[] GPS;

        // GPS位置
        private void positionOfGPS(object sender, RoutedEventArgs e)
        {
        }
        // 接收机坐标
        private void positionOfReceiver(object sender, RoutedEventArgs e)
        {

        }
        // 基线长度
        private void lengthOfBaseline(object sender, RoutedEventArgs e)
        {

        }
        // 读取O文件
        private void readO(object sender, RoutedEventArgs e)
        {
            List<List<string>> O = fileReader.readFile(10);
            // 要用到的卫星
            string[] GPS = O[0][7].Split('G');
        }
        // 读取N文件
        private void readN(object sender, RoutedEventArgs e)
        {
            
            List<List<string>> N0 = fileReader.readFile(8, true);
            N0.ForEach(item =>
            {

            });
        }
    }

    // 广播星历数据结构
    public class N
    {
        public static int PRN, TOC, year, month, day, hour, min;
        public static float sec, clockSkew, clockDrift, clockDriftSpeed, IODE, Crs, 
                     delta_n, M0, Cuc, e, Cus, sqrt_A, TOE, Cic, OMEGA0, Cis, 
                     i0, Crc, omega, OMEGA, i, L2, GPSWeek, L2P, satellitePrecision, 
                     satelliteHealth, TGD, IODC, sentTime, fittingRange;

        public N(string[] data)
        {
            int PRN = ConvertTo<int>(data[0]);
        }
        
        // 数据类型转换
        public T ConvertTo<T>(string val) where T : struct
        {
            return (T)Convert.ChangeType(val, Type.GetTypeCode(typeof(T)));
        }

        // GPS位置
        const double MU = 3.986005E+14;
        public double[] position()
        {
            double n = Math.Sqrt(MU) / Math.Pow(sqrt_A, 3) + delta_n;
            double tk = 0;
            double Mk = M0 + n * tk;
            double Ek = Mk;
            Ek = Mk + e * Math.Sin(Ek);
            Ek = Mk + e * Math.Sin(Ek);
            double fk = Math.Atan(Math.Sqrt(1 - e * e) * Math.Sin(Ek) / (Math.Cos(Ek) - e));
            double PHik = fk + omega;
            double deltau = delta(Cuc, Cus);
            double deltar = delta(Crc, Crs);
            double deltai = delta(Cic, Cis);
            double delta(double Cxc, double Cxs)
            {
                return Cxc * Math.Cos(2 * PHik) + Cxs * Math.Sin(2 * PHik);
            }
            double uk = PHik + deltau;
            double rk = sqrt_A * sqrt_A * (1 - e * Math.Cos(Ek)) + deltar;
            double ik = i0 + deltai + i * tk;
            double xk = rk * Math.Cos(uk);
            double yk = rk * Math.Sin(uk);


            double[] position = new double[3];
            return position;
        }


    }
}
