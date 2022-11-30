using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

public class RequestState
{
    // This class stores the State of the request.
    const int BUFFER_SIZE = 1024;
    public byte[] BufferRead;
    public int requestNum;
    public StringBuilder requestData;
    public HttpWebRequest request;

    public HttpWebResponse response;
    public Stream streamResponse;

    public string url;
    public string tempFilePath;
    public long currentFileLength;
    public long totFileLength;

    public FileStream fileStream;

    public RequestState(string url,string filePath,int requestNum)
    {
        this.url = url;
        tempFilePath = filePath;
        this.requestNum = requestNum;
        BufferRead = new byte[BUFFER_SIZE];
        requestData = new StringBuilder("");
        request = null;
        streamResponse = null;
    }
}

class HttpWebRequest_BeginGetResponse
{
    const int BUFFER_SIZE = 1024;
    const string FilePath = "C:\\Users\\lsc\\Desktop\\tempfile\\";

    private static ManualResetEvent hasRequestEvent = new ManualResetEvent(false);
    public static int downloadID = 0;

    public delegate void CompeleteBlock();

    static string[] myURLs =
    {
        //"https://dldir1.qq.com/qqfile/qq/PCQQ9.6.1/QQ9.6.1.28732.exe",
        //"https://download.unitychina.cn/download_unity/46637c7592ee/Windows64EditorInstaller/UnitySetup64.exe",
        "https://download.unitychina.cn/download_unity/46637c7592ee/Windows64EditorInstaller/UnitySetup64.exe",
        "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
        "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
        "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
        "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
        "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
        //"https://gimg2.baidu.com/image_search/src=http%3A%2F%2Flmg.jj20.com%2Fup%2Fallimg%2F1114%2F040221103339%2F210402103339-7-1200.jpg&refer=http%3A%2F%2Flmg.jj20.com&app=2002&size=f9999,10000&q=a80&n=0&g=0n&fmt=auto?sec=1672156601&t=8cd7845b458fe11b034642bbfb237d7d",
    };
    static Producer_Cunsumer myPC = new Producer_Cunsumer();

    public class Producer_Cunsumer
    {
        readonly object queueLock = new Object();
        readonly object listLock = new Object();
        public int bufferLength = 5;
        public int consumerCount = 3;
        public bool produceComplete = false;
        public Queue<string> marketBuffer = new Queue<string>();

        public void Produce(string[] urls)
        {
            for(int i = 0; i < urls.Length; i++)
            {
                while (marketBuffer.Count == bufferLength)
                {
                    Console.WriteLine("producer: 队列已满，等待消费者");
                    Thread.Sleep(1000);
                }
                Console.WriteLine("按下回车添加一条请求");
                Console.ReadLine();
                //有空位了，则放入缓冲队列中
                lock (queueLock)
                {
                    Console.WriteLine("producer: 向队列添加一个http请求");
                    hasRequestEvent.Set();
                    marketBuffer.Enqueue(urls[i]);
                }
            }
            produceComplete = true;
            Console.WriteLine("生产完成，等待消费者完成后退出");
        }

        public async void Consume()
        {
            string url = null;
            List<RequestState> currentTask = new List<RequestState>();

            while (true)
            {
                hasRequestEvent.WaitOne();
                Console.WriteLine("consumer: 消费者收到通知");

                if(currentTask.Count == consumerCount) Console.WriteLine("消费者忙碌中，请耐心等待");
                while(currentTask.Count == consumerCount)
                {
                    Thread.Sleep(1000);
                }
                Console.WriteLine("有空闲的消费者");
                
                lock (queueLock)
                {
                    if (marketBuffer.Count != 0)
                    {
                        url = marketBuffer.Dequeue();
                        Console.WriteLine("消费者拿到的url为: {0}", url);
                    }
                    else
                    {
                        if(produceComplete) break;
                        Console.WriteLine("请求队列为空");
                        hasRequestEvent.Reset();
                        continue;
                    }
                }

                downloadID++;
                var requestState = new RequestState(url,FilePath + "test" + downloadID.ToString(),downloadID);
                lock (listLock)
                {
                    currentTask.Add(requestState);
                    Task newTask = DownloadAsync(requestState, () =>
                    {
                        currentTask.Remove(requestState);
                    });
                }
            }

            Console.WriteLine("请求队列处理完毕，请等待消费者下载完成");
            while(currentTask.Count != 0)
            {
                Thread.Sleep(1000);
            }
            Console.WriteLine("下载完成");
        }
    }

    static void Main()
    {
        try
        {
            (new Thread(() => myPC.Produce(myURLs))).Start();
            (new Thread(myPC.Consume)).Start();
            
            for(int i = 0; i < 1000; i++)
            {
                //Console.WriteLine("\n第{0}秒，主线程未堵塞\n",i);
                //Console.WriteLine(" ");
                Thread.Sleep(1000);
            }
        }

        catch (WebException e)
        {
            Console.WriteLine("\nMain Exception raised!");
            Console.WriteLine("\nMessage:{0}", e.Message);
            Console.WriteLine("\nStatus:{0}", e.Status);
            Console.WriteLine("Press any key to continue..........");
        }
        catch (Exception e)
        {
            Console.WriteLine("\nMain Exception raised!");
            Console.WriteLine("Source :{0} ", e.Source);
            Console.WriteLine("Message :{0} ", e.Message);
            Console.WriteLine("Press any key to continue..........");
            Console.Read();
        }

    }

    /// <summary>
    /// 执行下载操作
    /// </summary>
    /// <param name="myRequest"></param>
    private static async Task<RequestState> DownloadAsync(RequestState myRequest,CompeleteBlock compeleted)
    {
        try
        {
            //如果文件已经存在，更新一些文件信息
            myRequest.fileStream = File.OpenWrite(myRequest.tempFilePath + ".temp");
            if(File.Exists(myRequest.tempFilePath + ".temp"))
            {
                myRequest.currentFileLength = myRequest.fileStream.Length;
                myRequest.fileStream.Seek(myRequest.currentFileLength,SeekOrigin.Current);
            }

            //发送请求，得到临时响应，从而判断文件是否已经下载完成
            var tmpRequest = (HttpWebRequest)WebRequest.Create(myRequest.url);
            var tmpResponse = tmpRequest.GetResponse();
            if(tmpResponse.ContentLength == myRequest.currentFileLength)
            {
                Console.WriteLine("文件已经下载完成，文件路径：{0}",myRequest.tempFilePath + ".temp");
            }
            else
            {
                myRequest.request = (HttpWebRequest)WebRequest.Create(myRequest.url);
                myRequest.request.Method = "GET";
                myRequest.request.AddRange((int)myRequest.currentFileLength);
                myRequest.response = (HttpWebResponse)myRequest.request.GetResponse();
                myRequest.streamResponse = myRequest.response.GetResponseStream();
                myRequest.totFileLength = myRequest.currentFileLength + myRequest.response.ContentLength;

                int length;
                while ((length = await myRequest.streamResponse.ReadAsync(myRequest.BufferRead, 0, myRequest.BufferRead.Length)) != 0)
                {
                    myRequest.fileStream.Write(myRequest.BufferRead, 0, length);
                    myRequest.requestData.Append(Encoding.ASCII.GetString(myRequest.BufferRead, 0, length));
                    /*Console.WriteLine
                    (
                        "下载任务{0}: 下载进度{1:f2}%，文件大小{2}B /{3}B",
                        myRequest.requestNum,
                        100 * (double)(myRequest.requestData.Length + myRequest.currentFileLength) / myRequest.totFileLength,
                        myRequest.requestData.Length + myRequest.currentFileLength,
                        myRequest.totFileLength

                    );*/
                }
                Console.WriteLine("{0}下载完成，文件大小：{1}B",Path.GetFileName(myRequest.tempFilePath + ".temp") ,myRequest.totFileLength);
                //Console.WriteLine("\nHeaders are :");
                //Console.WriteLine(myRequest.response.Headers);
                //Console.WriteLine("\nbodys are");
                //Console.WriteLine(myRequest.requestData.ToString());
                //Console.WriteLine("Press any key to continue..........");
                //Console.ReadLine();
                myRequest.streamResponse.Close();
            }
            
            myRequest.fileStream.Close();
            compeleted?.Invoke();
            return myRequest;
        }

        catch (WebException e)
        {
            Console.WriteLine("\nWeb Exception raised!");
            Console.WriteLine("\nMessage:{0}", e.Message);
            Console.WriteLine("\nStatus:{0}", e.Status);
            return myRequest;
        }
        catch (Exception e)
        {
            Console.WriteLine("\nMain Exception raised!");
            Console.WriteLine("Source :{0} ", e.Source);
            Console.WriteLine("Message :{0} ", e.Message);
            return myRequest;
        }
    }
}