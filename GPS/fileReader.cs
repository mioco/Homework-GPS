using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPS
{
    class fileReader
    {
        OpenFileDialog f = new OpenFileDialog();
        // 导入文件
        public List<List<string>> readFile(int count, bool start = false)
        {
            List<List<string>> dataChunk = new List<List<string>>();
            f.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            f.FilterIndex = 2;
            f.RestoreDirectory = true;
            Stream myStream = null;

            if (f.ShowDialog() == true)
            {
                try
                {
                    if ((myStream = f.OpenFile()) != null)
                    {
                        dataChunk = handleFile(count, start);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            return dataChunk;
        }
        // 处理文件
        public List<List<string>> handleFile(int count, bool start)
        {
            List<List<string>> dataChunk = new List<List<string>>();
            using (StreamReader r = new StreamReader(f.OpenFile()))
            {
                string line;
                List<string> chunk = new List<string>();
                int index = 0;
                while ((line = r.ReadLine()) != null)
                {
                    if (line.IndexOf('D') != -1) line = addSpace(line);
                    List<string> tempArr = line.Split(' ').ToList();
                    tempArr.RemoveAll(String.IsNullOrEmpty);
                    for (int i = 0; i < tempArr.Count; ++i)
                    {
                        if (tempArr[i].IndexOf('D') != -1)
                        {
                            tempArr[i] = tempArr[i].Replace('D', 'E'); ;
                        }
                        chunk.Add(tempArr[i]);
                    }
                    if (++index >= count)
                    {
                        index = 0;
                        dataChunk.Add(chunk);
                        chunk = new List<string>();
                    }
                }
            }
            return dataChunk;
        }

        // 在－前面加空格
        private string addSpace(string line)
        {
            int index = 20;
            while ((index = line.IndexOf('-', index)) != -1)
            {
                if (line[index - 1] != 'D') line = line.Insert(index, " ");
                index += 2;
            }
            return line;
        }

        // 科学计数法转换成浮点数
        // 虽然没什么用，但是不舍得删
        private string toNumber(string str)
        {
            str = str.Replace('D', 'E');
            int indexD = str.IndexOf('E');
            int exponent;
            int lastIndex = str.LastIndexOf('-');
            int dec = lastIndex == -1 || lastIndex == 0 ? str.LastIndexOf('+') : str.LastIndexOf('-');
            if ((exponent = Convert.ToInt32(str.Substring(dec + 1))) == 0) return str.Substring(0, indexD);
            int count = indexD - str.IndexOf('.') - 1 + exponent;
            char[] decimals = new char[count];
            for (int i = 0; i < count; ++i)
            {
                decimals[i] = '#';
            }
            string format = "0." + string.Join("", decimals);
            str = Double.Parse(str, System.Globalization.NumberStyles.Float).ToString(format);
            return str;
        }

        public string getPath()
        {
            return f.FileName;
        }
    }
}
