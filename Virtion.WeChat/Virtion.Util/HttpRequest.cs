using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Virtion.Util
{
    class HttpRequest
    {
        public static Action HttpRequestErrorCallBack = null;

        public static void Post<T>(string Url, Dictionary<string, string> args, Action<T> callback)
        {
            string retString = null;
            WWWForm wWWForm = new WWWForm();
            foreach (KeyValuePair<string, string> current in args)
            {
                if (current.Value != null)
                {
                    wWWForm.AddField(current.Key, current.Value);
                }
            }

            byte[] byteArray = wWWForm.data;
            //Console.WriteLine(System.Text.Encoding.Default.GetString(byteArray));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((s, e) =>
            {
                try
                {
                    Stream myRequestStream = request.GetRequestStream();
                    myRequestStream.Write(byteArray, 0, byteArray.Length); //写入参数 
                    myRequestStream.Close();

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);

                    retString = myStreamReader.ReadToEnd();

                    myStreamReader.Close();
                    myResponseStream.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            bw.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler((s, e) =>
            {
                if (retString == null)
                {
                    MessageBox.Show("网络错误或服务器异常");
                    Console.WriteLine("网络错误或服务器异常");
                    if (HttpRequestErrorCallBack != null)
                    {
                        HttpRequestErrorCallBack();
                    }
                    return;
                }
                //Console.WriteLine(retString);
                T obj = JsonConvert.DeserializeObject<T>(retString);
                callback(obj);
            });
            bw.RunWorkerAsync("Tank");
        }

        public static void PostJson<T>(string Url, object args, Action<T> callback, string cookie = "")
        {
            string retString = null;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";
            request.Headers.Add(HttpRequestHeader.Cookie, cookie);

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((s, e) =>
            {
                try
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(args.ToString().Replace("\r\n", ""));
                    Stream myRequestStream = request.GetRequestStream();
                    myRequestStream.Write(byteArray, 0, byteArray.Length); //写入参数 
                    myRequestStream.Close();


                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);

                    retString = myStreamReader.ReadToEnd();

                    myStreamReader.Close();
                    myResponseStream.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            bw.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler((s, e) =>
            {
                if (retString == null)
                {
                    MessageBox.Show("网络错误或服务器异常");
                    Console.WriteLine("网络错误或服务器异常");
                    if (HttpRequestErrorCallBack != null)
                    {
                        HttpRequestErrorCallBack();
                    }
                    return;
                }
                //Console.WriteLine(retString);
                T obj = JsonConvert.DeserializeObject<T>(retString);
                callback(obj);
            });
            bw.RunWorkerAsync();
        }

        public static T PostJsonSync<T>(string url, object args)
        {
            T ret = default(T);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";
            try
            {
                Stream myRequestStream = request.GetRequestStream();
                byte[] byteArray = Encoding.UTF8.GetBytes(args.ToString().Replace("\r\n", ""));

                myRequestStream.Write(byteArray, 0, byteArray.Length); //写入参数 
                myRequestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);

                string s = myStreamReader.ReadToEnd();
                Console.WriteLine(s);
                ret = JsonConvert.DeserializeObject<T>(s);

                myStreamReader.Close();
                myResponseStream.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return ret;
        }

        public static void Get<T>(string Url, Dictionary<string, string> args, Action<T> callback,
            bool ShowError = true)
        {
            WWWForm wWWForm = new WWWForm();
            foreach (KeyValuePair<string, string> current in args)
            {
                if (current.Value != null)
                {
                    wWWForm.AddField(current.Key, current.Value);
                }
            }

            string s = Encoding.UTF8.GetString(wWWForm.data);
            Get<T>(Url + s, callback, ShowError);
            //Console.WriteLine(Url + s);
        }

        public static void Get<T>(string Url, Action<T> callback, bool ShowError = true)
        {
            //Console.WriteLine(Url);
            string retString = null;
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((s, e) =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                    request.Method = "GET";
                    request.ContentType = "text/html;charset=UTF-8";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                    retString = myStreamReader.ReadToEnd();

                    myStreamReader.Close();
                    myResponseStream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            bw.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler((s, e) =>
            {
                if (retString == null)
                {
                    if (ShowError == true)
                    {
                        MessageBox.Show("网络错误或服务器异常");
                    }
                    Console.WriteLine("网络错误或服务器异常");

                    if (HttpRequestErrorCallBack != null)
                    {
                        HttpRequestErrorCallBack();
                    }

                    return;
                }
                //Console.WriteLine(retString);
                T obj = JsonConvert.DeserializeObject<T>(retString);
                callback(obj);
            });
            bw.RunWorkerAsync("Tank");

        }

        public static string GetSync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string ret = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return ret;
        }

        public static void GetImage(string Url, Action<BitmapImage> callBack)
        {
            BackgroundWorker bw = new BackgroundWorker();
            byte[] buffer = null;
            bw.DoWork += new DoWorkEventHandler((s, e) =>
            {
                try
                {
                    //Console.WriteLine(Url.Trim());
                    WebClient client = new WebClient();
                    buffer = client.DownloadData(Url.Trim());

                    if (buffer.Length == 0)
                    {
                        throw new Exception("DownloadData Error " + Url);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            bw.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler((s, e) =>
            {
                callBack(ByteArrayToBitmapImage(buffer));
            });
            bw.RunWorkerAsync("Tank");

        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch (Exception ex)
            {
                bmp = null;
                //FileManager.WriteFile(@"d:/a",byteArray);
                Console.WriteLine(ex.ToString());
            }
            return bmp;
        }

    }
}
