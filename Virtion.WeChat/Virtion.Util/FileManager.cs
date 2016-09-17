using System;
using System.Text;
using System.IO;
using System.Windows;

namespace Virtion.Util
{
    class FileManager
    {

        public static byte[] ReadFileByte(String path)
        {
            if (File.Exists(path) == false)
            {
                MessageBox.Show("File is not found! " + path);
                return null;
            }

            FileStream fs = File.OpenRead(path); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组 
            fs.Read(image, 0, filelength); //按字节流读取 
            fs.Close();
            return image;
        }

        public static String ReadFile(String path)
        {
            if (File.Exists(path) == false)
            {
                MessageBox.Show("File is not found! " + path);
                return null;
            }
            StreamReader sr = new StreamReader(path, Encoding.Default);
            StringBuilder stringBuilder = new StringBuilder();
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                stringBuilder.Append(line);
            }
            sr.Close();
            return stringBuilder.ToString();
        }

        public static void WriteFile(String path, String data)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.Write(data);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        public static void WriteFile(String path, byte[] data)
        {
            path.Replace('*','x');
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(data);
            bw.Flush();
            bw.Close();
            fs.Close();
        }

    }

}
