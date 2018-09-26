using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chandao
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.RunWorkerAsync();
        }

        /// <summary>
        /// 处理bug内容
        /// </summary>
        /// <param name="index"></param>
        /// <param name="content"></param>
        private void HandleContent(int index, string content)
        {
            //var dir = @"C:\Megi\bug抓取工具\html\\" + (index / 1000).ToString();
            var dir = @"C:\Megi\bug抓取工具\html\";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            //WriteToHtml(index, content);


            //处理里面的图片
            var imgReg = new Regex("(?<=\\<img.*?setImageSize.*src=\")(?<path>.*?)(?=\")");

            if (imgReg.IsMatch(content))
            {

                var baseUrl = "http://115.28.0.167:81/zentao/";

                var matches = imgReg.Matches(content);

                foreach (Match nextMatch in matches)
                {
                    var path = nextMatch.Groups["path"].ToString();

                    if (path.Contains("data:image"))
                    {
                        this.textBox1.Text += index + "存在data类型的图片" + Environment.NewLine;
                        continue;
                    }

                    var filePath = getFodler(path);

                    var fileName = path.Split('/').Last();

                    var uploadFolder = dir + "\\" + filePath;

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var url = baseUrl + path;

                    WriteToImage(uploadFolder + "\\" + fileName, url);
                }
            }

            //var fileReg = new Regex("(?<=\\<a href=')(?<path>/zentao/file-download.*)' target.*?\\>(?<name>.*)\\</a\\>");

            //if (fileReg.IsMatch(content))
            //{

            //    var baseUrl = "http://115.28.0.167:81/";

            //    var fileFolder = dir + "\\" + index;

            //    if (!Directory.Exists(fileFolder))
            //    {
            //        Directory.CreateDirectory(fileFolder);
            //    }

            //    var matches = fileReg.Matches(content);

            //    foreach (Match nextMatch in matches)
            //    {
            //        var path = nextMatch.Groups["path"].ToString();
            //        var name = nextMatch.Groups["name"].ToString();

            //        WriteToFile(baseUrl + path, fileFolder + "\\" + name);
            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string getFodler(string path)
        {
            var str = string.Empty;

            var a = path.Split('/');

            for (var i = 0; i < a.Length - 1; i++)
            {
                str += a[i] + "\\";
            }

            return str;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        private void WriteToHtml(int index, string content)
        {
            var dir = @"C:\Megi\bug抓取工具\html\" + (index / 1000) + "\\";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var path = dir + "\\" + string.Format("bug-view-{0}.html", index);

            if (File.Exists(path))
                File.Delete(path);

            FileStream fs = new FileStream(path, FileMode.Create);

            byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
            //开始写入
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }



        public void WriteToImage(string fileName, string url)
        {

            if (fileName.EndsWith(".gif"))
            {
                WriteToFile(url, fileName);
            }
            return;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.ServicePoint.Expect100Continue = false;
            req.Method = "GET";
            req.KeepAlive = true;

            req.ContentType = "image/png";

            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

            System.IO.Stream stream = null;

            try
            {

                //以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                System.Drawing.Image.FromStream(stream).Save(fileName);
            }
            finally
            {
                // 释放资源
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }



        /// <summary>
        /// Http方式下载文件
        /// </summary>
        /// <param name="url">http地址</param>
        /// <param name="localfile">本地文件</param>
        /// <returns></returns>
        public bool WriteToFile(string url, string localfile)
        {
            bool flag = false;
            long startPosition = 0; // 上次下载的文件起始位置
            FileStream writeStream; // 写入本地文件流对象

            long remoteFileLength = GetHttpLength(url);// 取得远程文件长度
            System.Console.WriteLine("remoteFileLength=" + remoteFileLength);
            if (remoteFileLength == 745)
            {
                System.Console.WriteLine("远程文件不存在.");
                return false;
            }

            //// 判断要下载的文件夹是否存在
            //if (File.Exists(localfile))
            //{

            //    writeStream = File.OpenWrite(localfile);             // 存在则打开要下载的文件
            //    startPosition = writeStream.Length;                  // 获取已经下载的长度

            //    if (startPosition >= remoteFileLength)
            //    {
            //        System.Console.WriteLine("本地文件长度" + startPosition + "已经大于等于远程文件长度" + remoteFileLength);
            //        writeStream.Close();

            //        return false;
            //    }
            //    else
            //    {
            //        writeStream.Seek(startPosition, SeekOrigin.Current); // 本地文件写入位置定位
            //    }
            //}
            //else
            //{
            //    writeStream = new FileStream(localfile, FileMode.Create);// 文件不保存创建一个文件
            //    startPosition = 0;
            //}

            writeStream = new FileStream(localfile, FileMode.Create);// 文件不保存创建一个文件
            startPosition = 0;

            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接

                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);// 设置Range值,与上面的writeStream.Seek用意相同,是为了定义远程文件读取位置
                }


                Stream readStream = myRequest.GetResponse().GetResponseStream();// 向服务器请求,获得服务器的回应数据流


                byte[] btArray = new byte[512];// 定义一个字节数据,用来向readStream读取内容和向writeStream写入内容
                int contentSize = readStream.Read(btArray, 0, btArray.Length);// 向远程文件读第一次

                long currPostion = startPosition;

                while (contentSize > 0)// 如果读取长度大于零则继续读
                {
                    currPostion += contentSize;
                    int percent = (int)(currPostion * 100 / remoteFileLength);
                    System.Console.WriteLine("percent=" + percent + "%");

                    writeStream.Write(btArray, 0, contentSize);// 写入本地文件
                    contentSize = readStream.Read(btArray, 0, btArray.Length);// 继续向远程文件读取
                }

                //关闭流
                writeStream.Close();
                readStream.Close();

                flag = true;        //返回true下载成功
            }
            catch (Exception)
            {
                writeStream.Close();
                flag = false;       //返回false下载失败
            }

            return flag;
        }

        // 从文件头得到远程文件的长度
        private static long GetHttpLength(string url)
        {
            long length = 0;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                if (rsp.StatusCode == HttpStatusCode.OK)
                {
                    length = rsp.ContentLength;// 从文件头得到远程文件的长度
                }

                rsp.Close();
                return length;
            }
            catch (Exception e)
            {
                return length;
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var url = @"http://115.28.0.167:81/zentao/bug-view-{0}.html";

            //美记小企业管理平台
            for (var i = 25604; i >= 326; i--)
            {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(string.Format(url, i));
                    req.Method = "GET";
                    req.Headers.Add("Cookie", "lang=zh-cn; theme=default; qaBugOrder=id_desc; lastProduct=3; windowHeight=630; windowWidth=1349; sid=ft15vg4aggra4o0sdgq3lefpv5");
                    using (HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse())
                    {

                        //在这里对接收到的页面内容进行处理 

                        var reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                        string content = reader.ReadToEnd();

                        if (string.IsNullOrWhiteSpace(content)) continue;

                        if (!content.Contains("美记小企业管理平台")) continue;

                        this.textBox1.Text += " 正在处理bug" + i + Environment.NewLine;


                        HandleContent(i, content);
                    }
                }
                catch (Exception)
                {
                    this.textBox1.Text += " 处理bug" + i + "失败" + Environment.NewLine;
                }

            }
        }
    }
}
