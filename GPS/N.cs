using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS
{
    class N
    {
        public Dictionary<string, double> GPSPosition = new Dictionary<string, double>();
        public int PRN, year, month, day, hour, min;
        public double sec, clockSkew, clockDrift, clockDriftSpeed, IODE, Crs,
                      delta_n, M0, Cuc, e, Cus, sqrt_A, TOE, Cic, OMEGA0, Cis,
                      i0, Crc, omega, OMEGA, i, L2, GPSWeek, L2P, satellitePrecision,
                      satelliteHealth, TGD, IODC, sentTime, fittingRange;

        public N(List<string> data)
        {
            GPSPosition.Add("PRN", ConvertTo<int>(data[0]));
            GPSPosition.Add("hour", ConvertTo<int>(data[4]));
            hour = ConvertTo<int>(data[4]);
            Crs = ConvertTo<double>(data[11]);
            delta_n = ConvertTo<double>(data[12]);
            M0 = ConvertTo<double>(data[13]);
            Cuc = ConvertTo<double>(data[14]);
            e = ConvertTo<double>(data[15]);
            Cus = ConvertTo<double>(data[16]);
            sqrt_A = ConvertTo<double>(data[17]);
            TOE = ConvertTo<double>(data[18]);
            Cic = ConvertTo<double>(data[19]);
            OMEGA0 = ConvertTo<double>(data[20]);
            Cis = ConvertTo<double>(data[21]);
            i0 = ConvertTo<double>(data[22]);
            Crc = ConvertTo<double>(data[23]);
            omega = ConvertTo<double>(data[24]);
            OMEGA = ConvertTo<double>(data[25]);
            i = ConvertTo<double>(data[26]);
        }

        // 数据类型转换
        public T ConvertTo<T>(string val) where T : struct
        {
            return (T)Convert.ChangeType(val, Type.GetTypeCode(typeof(T)));
        }

        // GPS位置
        public Dictionary<string, double> GPSPositionCalculate()
        {
            double MU = 3.986005 * Math.Pow(10, 14);
            double n = Math.Sqrt(MU) / Math.Pow(sqrt_A, 3) + delta_n;
            int t = (24 * 6 + hour) * 3600;
            double tk = t - TOE;
            double Mk = M0 + n * tk;
            double Ek = Mk;
            for (int i = 0; i < 5; ++i)
            {
                Ek = Mk + e * Math.Sin(Ek);
            }
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
            double omegae = 7.29211567 * Math.Pow(10, -5);
            double OMEGAk = OMEGA0 + (OMEGA - omegae) * tk - omegae * TOE; ;
            GPSPosition.Add("Xk", (xk * Math.Cos(OMEGAk) - yk * Math.Cos(ik) * Math.Sin(OMEGAk)) / 1000);
            GPSPosition.Add("Yk", (xk * Math.Sin(OMEGAk) + yk * Math.Cos(ik) * Math.Cos(OMEGAk)) / 1000);
            GPSPosition.Add("Zk", (yk * Math.Sin(ik)) / 1000);

            return GPSPosition;
        }
        
        static public Dictionary<int, double[]> GPSPositionAt(string[] GPS, List<Dictionary<string, double>> allGPSPosition, double percent)
        {
            percent = percent / 120;
            Dictionary<int, double[]> receiverPosition = new Dictionary<int, double[]>();
            //
            Dictionary<int, List<double[]>> a = new Dictionary<int, List<double[]>>();
            foreach (var item in allGPSPosition)
            {
                int PRN = (int)item["PRN"];
                double[] position = { item["Xk"], item["Yk"], item["Zk"] };
                if (a.ContainsKey(PRN))
                {
                    a[PRN].Add(position);
                }
                else
                {
                    List<double[]> p = new List<double[]>();
                    p.Add(position);
                    a.Add(PRN, p);
                }
            }
            foreach (var position in a)
            {
                receiverPosition.Add(position.Key, new double[3] {
                    (position.Value[1][0] - position.Value[0][0]) * percent,
                    (position.Value[1][1] - position.Value[0][1]) * percent,
                    (position.Value[1][2] - position.Value[0][2]) * percent
                });
            }
            return receiverPosition;
        }

    }
}
