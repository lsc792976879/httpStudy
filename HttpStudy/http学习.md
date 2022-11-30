http学习

#### 相关学习链接

链接合集：

httpwebrequest类： https://learn.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest?view=net-7.0

httpwebrequest使用：https://stackoverflow.com/questions/4699938/how-to-download-the-file-using-httpwebrequest-and-httpwebresponse-classcookies

https://blog.ndepend.com/c-async-await-explained/

异步编程： https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/

https://blog.ndepend.com/c-async-await-explained/



header（报头）：是服务器以HTTP协议传HTML资料到浏览器前所送出的字符串，是发送请求时携带的头部信息

键值对结构，每个键值对占一行，键和值用冒号空格分隔



#### 多线程

创建线程

ThreadStart childref = new ThreadStart(CallToChildThread);

Thread childThread = new Thread(childref);

childThread.Start();

利用lambda表达式简化书写

(new Thread(() => myPC.Produce(myURLs))).Start();
(new Thread(myPC.Consume)).Start();



等待1s，Thread.Sleep(1000);



**Abort()** 方法用于销毁线程。

通过抛出 **threadabortexception** 在运行时中止线程。这个异常不能被捕获，如果有 *finally* 块，控制会被送至 *finally* 块。



#### 异步编程

应用场景：I/O绑定需求、CPU占用高的运算

C#：基于任务的异步模式（TAP）

```
对于IO绑定的代码，在async方法中，await一个返回TASK或TASK<T>的方法的异步操作
对于受CPU限制的代码，使用Task.Run
```

await：非阻塞的方式来开始一个任务

async：修饰符，给编译器发出此方法包含await语句的信号，此方法返回这些操作的组合的Task

他们并不会创建一个额外的线程，

```c#
static async Task<Toast> MakeToastWithButterAndJamAsync(int number)
{
    var toast = await ToastBreadAsync(number);
    ApplyButter(toast);
    ApplyJam(toast);

    return toast;
}
```



等待所有任务完成再执行打印

```
await Task.WhenAll(eggsTask, baconTask, toastTask);
console......
```

任何一个任务执行完，返回这个任务的task对象

```
var breakfastTasks = new List<Task> { eggsTask, baconTask, toastTask };
while (breakfastTasks.Count > 0)
{
    Task finishedTask = await Task.WhenAny(breakfastTasks);
    if (finishedTask == eggsTask)
    {
        Console.WriteLine("Eggs are ready");
    }
    else if (finishedTask == baconTask)
    {
        Console.WriteLine("Bacon is ready");
    }
    else if (finishedTask == toastTask)
    {
        Console.WriteLine("Toast is ready");
    }
    breakfastTasks.Remove(finishedTask);
}
```



一些建议：

async方法需要有await关键字，没有会有警告

Async作为异步方法名称的后缀

async void只用于事件处理程序，没有返回类型

尽量这样使用，task.是阻塞当前线程，来等待task完成，容易出现思索或阻塞

![1669549309168](C:\Users\lsc\AppData\Roaming\Typora\typora-user-images\1669549309168.png)



异步方法示例：

```c#
public async Task<int> GetUrlContentLengthAsync()
{
    var client = new HttpClient();
    Task<string> getStringTask =
        client.GetStringAsync("https://docs.microsoft.com/dotnet");
    DoIndependentWork();
    string contents = await getStringTask;
    return contents.Length;
}

//GetStringAsync返回一个Task<string>，当getStringTask完成后，会返回一个string(contents)

//直到getStringTask执行完，GetUrlContentLengthAsync才可以继续，此时控制暂时回到GetUrlContentLengthAsync的调用者那

void DoIndependentWork()
{
    Console.WriteLine("Working...");
}
```



返回值：

Task，执行操作，但不返回任何值

Task<TResult>，返回值

void 事件处理，无返回值

await右边的表达式是Task<TResult>，则这个表达式返回T，如果是个Task，则是个语句

result是个阻塞属性，在任务完成前访问，则会阻塞在那儿

void返回类型，调用者无法捕获该方法抛出的异常，异常保存在task.Exception中，



#### Lock关键字

确保代码块完成执行，不会被其他线程中断，在代码块运行期间为给定对象获取互斥锁来实现的。



#### [ManualResetEvent](https://learn.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent?view=net-6.0)类

手动reset线程

private static ManualResetEvent mre = new ManualResetEvent(false);

当受到set的信号后，必须手动reset恢复，否则waitone无法阻塞，所有依赖于这个event的线程都会依次执行

示例程序：

```
using System;
using System.Threading;

public class Example
{
    // mre is used to block and release threads manually. It is
    // created in the unsignaled state.

    private static ManualResetEvent mre = new ManualResetEvent(false);

    static void Main()
    {
        Console.WriteLine("\nStart 3 named threads that block on a ManualResetEvent:\n");

        for(int i = 0; i <= 2; i++)
        {
            Thread t = new Thread(ThreadProc);
            t.Name = "Thread_" + i;
            t.Start();
        }

        Thread.Sleep(500);
        Console.WriteLine("\nWhen all three threads have started, press Enter to call Set()" +
                          "\nto release all the threads.\n");
        Console.ReadLine();

        mre.Set();

        Thread.Sleep(500);
        Console.WriteLine("\nWhen a ManualResetEvent is signaled, threads that call WaitOne()" +
                          "\ndo not block. Press Enter to show this.\n");
        Console.ReadLine();

        for(int i = 3; i <= 4; i++)
        {
            Thread t = new Thread(ThreadProc);
            t.Name = "Thread_" + i;
            t.Start();
        }

        Thread.Sleep(500);
        Console.WriteLine("\nPress Enter to call Reset(), so that threads once again block" +
                          "\nwhen they call WaitOne().\n");
        Console.ReadLine();

        mre.Reset();

        // Start a thread that waits on the ManualResetEvent.
        Thread t5 = new Thread(ThreadProc);
        t5.Name = "Thread_5";
        t5.Start();

        Thread.Sleep(500);
        Console.WriteLine("\nPress Enter to call Set() and conclude the demo.");
        Console.ReadLine();

        mre.Set();

        // If you run this example in Visual Studio, uncomment the following line:
        //Console.ReadLine();
    }

    private static void ThreadProc()
    {
        string name = Thread.CurrentThread.Name;

        Console.WriteLine(name + " starts and calls mre.WaitOne()");

        mre.WaitOne();

        Console.WriteLine(name + " ends.");
    }
}

/* This example produces output similar to the following:

Start 3 named threads that block on a ManualResetEvent:

Thread_0 starts and calls mre.WaitOne()
Thread_1 starts and calls mre.WaitOne()
Thread_2 starts and calls mre.WaitOne()

When all three threads have started, press Enter to call Set()
to release all the threads.


Thread_2 ends.
Thread_0 ends.
Thread_1 ends.

When a ManualResetEvent is signaled, threads that call WaitOne()
do not block. Press Enter to show this.


Thread_3 starts and calls mre.WaitOne()
Thread_3 ends.
Thread_4 starts and calls mre.WaitOne()
Thread_4 ends.

Press Enter to call Reset(), so that threads once again block
when they call WaitOne().


Thread_5 starts and calls mre.WaitOne()

Press Enter to call Set() and conclude the demo.

Thread_5 ends.
 */
```



#### [AutoResetEvent](https://learn.microsoft.com/en-us/dotnet/api/system.threading.autoresetevent?view=net-6.0)类

自动reset，在得到一个set信号后，会分配给下一个等待该event发生的线程，结束后自动reset

如果没有线程在等待，则状态将无限期保持为信号状态

```
using System;
using System.Threading;

// Visual Studio: Replace the default class in a Console project with 
//                the following class.
class Example
{
    private static AutoResetEvent event_1 = new AutoResetEvent(true);
    private static AutoResetEvent event_2 = new AutoResetEvent(false);

    static void Main()
    {
        Console.WriteLine("Press Enter to create three threads and start them.\r\n" +
                          "The threads wait on AutoResetEvent #1, which was created\r\n" +
                          "in the signaled state, so the first thread is released.\r\n" +
                          "This puts AutoResetEvent #1 into the unsignaled state.");
        Console.ReadLine();
            
        for (int i = 1; i < 4; i++)
        {
            Thread t = new Thread(ThreadProc);
            t.Name = "Thread_" + i;
            t.Start();
        }
        Thread.Sleep(250);

        for (int i = 0; i < 2; i++)
        {
            Console.WriteLine("Press Enter to release another thread.");
            Console.ReadLine();
            event_1.Set();
            Thread.Sleep(250);
        }

        Console.WriteLine("\r\nAll threads are now waiting on AutoResetEvent #2.");
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine("Press Enter to release a thread.");
            Console.ReadLine();
            event_2.Set();
            Thread.Sleep(250);
        }

        // Visual Studio: Uncomment the following line.
        //Console.Readline();
    }

    static void ThreadProc()
    {
        string name = Thread.CurrentThread.Name;

        Console.WriteLine("{0} waits on AutoResetEvent #1.", name);
        event_1.WaitOne();
        Console.WriteLine("{0} is released from AutoResetEvent #1.", name);

        Console.WriteLine("{0} waits on AutoResetEvent #2.", name);
        event_2.WaitOne();
        Console.WriteLine("{0} is released from AutoResetEvent #2.", name);

        Console.WriteLine("{0} ends.", name);
    }
}

/* This example produces output similar to the following:

Press Enter to create three threads and start them.
The threads wait on AutoResetEvent #1, which was created
in the signaled state, so the first thread is released.
This puts AutoResetEvent #1 into the unsignaled state.

Thread_1 waits on AutoResetEvent #1.
Thread_1 is released from AutoResetEvent #1.
Thread_1 waits on AutoResetEvent #2.
Thread_3 waits on AutoResetEvent #1.
Thread_2 waits on AutoResetEvent #1.
Press Enter to release another thread.

Thread_3 is released from AutoResetEvent #1.
Thread_3 waits on AutoResetEvent #2.
Press Enter to release another thread.

Thread_2 is released from AutoResetEvent #1.
Thread_2 waits on AutoResetEvent #2.

All threads are now waiting on AutoResetEvent #2.
Press Enter to release a thread.

Thread_2 is released from AutoResetEvent #2.
Thread_2 ends.
Press Enter to release a thread.

Thread_1 is released from AutoResetEvent #2.
Thread_1 ends.
Press Enter to release a thread.

Thread_3 is released from AutoResetEvent #2.
Thread_3 ends.
 */
```



#### Producer&Consumer模型

```
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

class Program
{
    private static AutoResetEvent hasRequestEvent = new AutoResetEvent(false);
    
    public class Producer_Cunsumer
    {
        readonly object myLock = new Object();
        public int bufferLength = 4;
        public int consumerCount = 2;
        public Queue<string> marketBuffer = new Queue<string>();

        public bool isFull()
        {
            if(marketBuffer.Count == bufferLength) return true;
            return false;
        }

        public void Produce(string s)
        {
            while(isFull())
            {
                Console.WriteLine("队列已满，等待消费者");
                Thread.Sleep(5000);
            }
            //有空位了，则放入缓冲队列中
            lock (myLock)
            {
                marketBuffer.Enqueue(s);
            }
            Thread.Sleep(5000);
        }

        public void Consume()
        {
            var name = Thread.CurrentThread.Name;
            while (true)
            {
                hasRequestEvent.WaitOne();
                Console.WriteLine("消费者收到通知");
                lock (myLock)
                {
                    if (marketBuffer.Count != 0)
                    {
                        var tmp = marketBuffer.Dequeue();
                        Console.WriteLine("线程{0}拿到了{1}", name, tmp);
                    }
                }
            }
        }

        public void queueMonitor()
        {
            while (true)
            {
                if(marketBuffer.Count != 0)
                {
                    Console.WriteLine("队列不为空，通知消费者");
                    hasRequestEvent.Set();
                }
                else
                {
                    Console.WriteLine("队列目前为空，等待生产者");
                }
                Thread.Sleep(1000);
            }
        }
    }

    static Producer_Cunsumer myPC = new Producer_Cunsumer();
    static string[] myURLs =
    {
        "任务1",
        "任务2",
        "任务3",
        "任务4",
        "任务5",
        "任务6",
    };

    static void Main()
    {
        (new Thread(myPC.queueMonitor)).Start();
        (new Thread(Thread_Producer)).Start();
        for(int i = 0; i < myPC.consumerCount; i++)
        {
            Thread tmp = new Thread(myPC.Consume);
            tmp.Name = "消费者" + i;
            tmp.Start();
        }
        for(int i = 0; i < 1000; i++)
        {
            Console.WriteLine("第{0}秒，主线程未堵塞",i);
            Thread.Sleep(1000);
        }
    }
    static void Thread_Producer()
    {
        for(int i = 0; i < myURLs.Length; i++)
        {
            myPC.Produce(myURLs[i]);
        }
    }
}
```



#### **http客户端程序-第一版**

1、http实现一个客户端，打印http响应的所有header，以及string方式打印响应的body；不能卡主线程，遇到异常或超时要打印信息

2、实现对2进制文件的下载功能，显示下载进度的百分比和字节数

3、支持下载中断续传功能

```C#
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class RequestState
{
    // This class stores the State of the request.
    const int BUFFER_SIZE = 1024;
    public byte[] BufferRead;
    public StringBuilder requestData;
    public HttpWebRequest request;
    public HttpWebResponse response;
    public Stream streamResponse;
    public string tempFilePath;
    public long length;

    public FileStream fileStream;

    public RequestState()
    {
        BufferRead = new byte[BUFFER_SIZE];
        requestData = new StringBuilder("");
        request = null;
        streamResponse = null;
    }
}

class HttpWebRequest_BeginGetResponse
{
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    //为false时，所有线程被阻塞，为true时所有线程被释放，reset后为false。set后为true


    const int BUFFER_SIZE = 1024;
    const int DefaultTimeout = 2 * 60 * 1000; // 2 minutes timeout

    // Abort the request if the timer fires.
    private static void TimeoutCallback(object state, bool timedOut)
    {
        //如果超时
        if (timedOut)
        {
            //如果存在响应请求，则取消请求
            HttpWebRequest request = state as HttpWebRequest;
            if (request != null)
            {
                request.Abort();
            }
        }
    }
    
    static void Main()
    {

        try
        {
            // Create a HttpWebrequest object to the desired URL.
            // 创建请求对象
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("https://dldir1.qq.com/qqfile/qq/PCQQ9.6.1/QQ9.6.1.28732.exe");
            // https://vod-progressive.akamaized.net/exp=1669359177~acl=%2Fvimeo-prod-skyfire-std-us%2F01%2F561%2F21%2F527808435%2F2481973821.mp4~hmac=c8f006df3dda0bd184e6d1ceca2c0ac73e2ed4f818a9df41560e4fa5194da31e/vimeo-prod-skyfire-std-us/01/561/21/527808435/2481973821.mp4?download=1&filename=pexels-sunsetoned-7235781.mp4
            // https://download.unitychina.cn/download_unity/46637c7592ee/Windows64EditorInstaller/UnitySetup64.exe
            // https://dldir1.qq.com/qqfile/qq/PCQQ9.6.1/QQ9.6.1.28732.exe
            // Create an instance of the RequestState and assign the previous myHttpWebRequest
            // object to its request field.
            // 创建状态对象，并把请求添加到状态，以便来回传递
            RequestState myRequestState = new RequestState();
            myRequestState.request = myHttpWebRequest;
            myRequestState.tempFilePath = "C:\\Users\\lsc\\Desktop\\";
            myRequestState.fileStream = File.OpenWrite(myRequestState.tempFilePath + "test.temp");


            if (File.Exists(myRequestState.tempFilePath + "test.temp"))
            {
                myRequestState.length = myRequestState.fileStream.Length;
                myRequestState.fileStream.Seek(myRequestState.length,SeekOrigin.Current);
                myRequestState.request.AddRange((int)myRequestState.length);
            }

            // Start the asynchronous request.（begingetresonse开始一个异步请求,new AsyncCallback是异步回调的委托，配置完成后会自动执行）
            IAsyncResult result = (IAsyncResult)myHttpWebRequest.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);
            
            // this line implements the timeout, if there is a timeout, the callback fires and the request becomes aborted，如果超时，则触发回调，中止请求
            // waitHandle收到信号，或者没收到信号超时了，就会调用waitortimercallback方法，，超时时间是defaulttimeout 2分钟，如果设为-1，收不到信号，waitortimecallback就不会运行
            // executeOnlyOnce，执行一次，调用委托后，线程不会在waitObject参数上等待
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), myHttpWebRequest, DefaultTimeout, true);


            // 阻塞线程，不收到信号，则不会return
            allDone.WaitOne();

            File.Move(myRequestState.tempFilePath + "test.temp",myRequestState.tempFilePath + "test.exe");

            // Release the HttpWebResponse resource.
            myRequestState.response.Dispose();
            myRequestState.response.Close();
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
    private async static void RespCallback(IAsyncResult asynchronousResult)
    {
        try
        {
            // State of request is asynchronous.
            //从异步结果获取状态对象
            RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
            //获取请求对象
            HttpWebRequest myHttpWebRequest = myRequestState.request;
            //由请求对象，获取最终响应的对象
            myRequestState.response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);


            // Read the response into a Stream object.
            // 有了响应的对象，从响应流读取数据
            Stream responseStream = myRequestState.response.GetResponseStream();
            // 读取异步完成，每次读取的，调用readcallback，存入RequestState中
            myRequestState.streamResponse = responseStream;

            

            int len;
            while((len = await responseStream.ReadAsync(myRequestState.BufferRead,0,myRequestState.BufferRead.Length)) != 0)
            {
                myRequestState.fileStream.Write(myRequestState.BufferRead, 0, len);
                myRequestState.requestData.Append(Encoding.UTF8.GetString(myRequestState.BufferRead, 0, len));
                Console.Write("\r下载进度{0}%，文件大小{1}/{2}          ", (double)(myRequestState.requestData.Length + myRequestState.length) / (myRequestState.response.ContentLength + myRequestState.length) * 100, myRequestState.requestData.Length + myRequestState.length, myRequestState.response.ContentLength + myRequestState.length);
            }
            
            Console.WriteLine("\r下载进度100%，文件大小{0}B/{1}B        ", myRequestState.response.ContentLength, myRequestState.response.ContentLength);


            Console.WriteLine("\nHeaders are :");
            Console.WriteLine(myRequestState.response.Headers);

            /*Console.WriteLine("\nbodys are");
            if (myRequestState.requestData.Length > 1)
            {
                string stringContent;
                stringContent = myRequestState.requestData.ToString();
                Console.WriteLine(stringContent);
            }*/
            Console.WriteLine("Press any key to continue..........");
            Console.ReadLine();
            responseStream.Dispose();
            responseStream.Close();

            // Begin the Reading of the contents of the HTML page and print it to the console.
            // BufferRead被传入beginread中，数据读入buffer中，发出另一个异步调用，读取更多的数据，读取结束会调用readcallback
            // IAsyncResult asynchronousInputRead = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
            return;

        }

        catch (WebException e)
        {
            Console.WriteLine("\nRespCallback Exception raised!");
            Console.WriteLine("\nMessage:{0}", e.Message);
            Console.WriteLine("\nStatus:{0}", e.Status);
        }
        allDone.Set();
    }
    public static int count = 0;
    private static void ReadCallBack(IAsyncResult asyncResult)
    {
        try
        {
            RequestState myRequestState = (RequestState)asyncResult.AsyncState;
            Stream responseStream = myRequestState.streamResponse;
            int read = responseStream.EndRead(asyncResult);
            // Read the HTML page and then print it to the console.
            if (read > 0)
            {
                //把缓冲区的内容编码为UTF8，加在requestData中
                myRequestState.fileStream.Write(myRequestState.BufferRead, 0, read);
                myRequestState.requestData.Append(Encoding.UTF8.GetString(myRequestState.BufferRead, 0, read));
                //Console.Write("\r下载进度{0}%，文件大小{1}/{2}",(double)myRequestState.requestData.Length / myRequestState.response.ContentLength * 100,myRequestState.requestData.Length,myRequestState.response.ContentLength);
                count++;
                Console.WriteLine(count + " " + read);
                Console.WriteLine(Environment.CurrentManagedThreadId);
                //发出另一个异步调用，继续读取数据
                IAsyncResult asynchronousResult = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
                Console.WriteLine(count + " afterAsnyc");
                return;
            }
            else
            {
                Console.WriteLine("\r下载进度100%，文件大小{0}B/{1}B",myRequestState.response.ContentLength,myRequestState.response.ContentLength);
                Console.WriteLine("\nHeaders are :");
                Console.WriteLine(myRequestState.response.Headers);

                Console.WriteLine("\nbodys are");
                if (myRequestState.requestData.Length > 1)
                {
                    string stringContent;
                    stringContent = myRequestState.requestData.ToString();
                    //Console.WriteLine(stringContent);
                }
                Console.WriteLine("Press any key to continue..........");
                Console.ReadLine();

                responseStream.Close();
            }
        }
        catch (WebException e)
        {
            Console.WriteLine("\nReadCallBack Exception raised!");
            Console.WriteLine("\nMessage:{0}", e.Message);
            Console.WriteLine("\nStatus:{0}", e.Status);
        }
        allDone.Set();
    }
}

```

#### http客户端程序-第二版

加了阻塞队列，修改了一些逻辑，使用了异步编程

```
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        fileStream = File.OpenWrite(tempFilePath + ".temp");
        if(File.Exists(tempFilePath + ".temp"))
        {
            currentFileLength = fileStream.Length;
            fileStream.Seek(currentFileLength,SeekOrigin.Current);
        }
    }
}

class HttpWebRequest_BeginGetResponse
{
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    const int BUFFER_SIZE = 1024;
    const int DefaultTimeout = 2 * 60 * 1000; // 2 minutes timeout
    const string FilePath = "C:\\Users\\lsc\\Desktop\\tempfile\\";

    static void Main()
    {
        string[] myURLs =
        {
            //"https://dldir1.qq.com/qqfile/qq/PCQQ9.6.1/QQ9.6.1.28732.exe",
            "https://download.unitychina.cn/download_unity/46637c7592ee/Windows64EditorInstaller/UnitySetup64.exe",
            "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
            "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
            "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
            "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
            "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload310036.exe",
            //"https://gimg2.baidu.com/image_search/src=http%3A%2F%2Flmg.jj20.com%2Fup%2Fallimg%2F1114%2F040221103339%2F210402103339-7-1200.jpg&refer=http%3A%2F%2Flmg.jj20.com&app=2002&size=f9999,10000&q=a80&n=0&g=0n&fmt=auto?sec=1672156601&t=8cd7845b458fe11b034642bbfb237d7d",
            
            
        };

        try
        {
            Queue<RequestState> requesetQueue = new Queue<RequestState>();
            for(int i = 0; i < myURLs.Length; i++)
            {
                var requestState = new RequestState(myURLs[i],FilePath + "test" + i.ToString(),i);
                requestState.request = (HttpWebRequest)WebRequest.Create(requestState.url);
                requestState.request.AddRange((int)requestState.currentFileLength);

                requesetQueue.Enqueue(requestState);
            }


            Task<string> RequestTask = RequestSolveAsync(requesetQueue,2);
            

            Console.WriteLine("主线程执行完毕");

            Console.WriteLine(RequestTask.Result);
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
    /// 处理request队列
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="maxNum"></param>
    private static async Task<string> RequestSolveAsync(Queue<RequestState> queue,int maxNum)
    {
        if(queue.Count == 0) return "http请求队列为空";

        List<Task<RequestState>> currentTasks = new List<Task<RequestState>>();

        for(int i = 0; i < MathF.Min(maxNum,queue.Count); i++)
        {
            RequestState tmp = queue.Dequeue();
            currentTasks.Add(DownloadAsync(tmp));
        }
        
        while(queue.Count > 0 || currentTasks.Count > 0)
        {
            Task<RequestState> finishedTask = await Task.WhenAny(currentTasks);
            var finishRequest = await finishedTask;
            currentTasks.Remove(finishedTask);

            if(queue.Count > 0)
            {
                currentTasks.Add(DownloadAsync(queue.Dequeue()));
            }

            Console.WriteLine("第{0}个下载请求完成",finishedTask.Result.requestNum);
        }
        return "http请求队列执行完毕";
    }

    /// <summary>
    /// 执行下载操作
    /// </summary>
    /// <param name="myRequest"></param>
    private static async Task<RequestState> DownloadAsync(RequestState myRequest)
    {
        try
        {
            var tmpRequest = (HttpWebRequest)WebRequest.Create(myRequest.url);
            var tmpResponse = tmpRequest.GetResponse();
            if(tmpResponse.ContentLength == myRequest.currentFileLength)
            {
                Console.WriteLine("文件已经下载完成，文件路径：{0}",myRequest.tempFilePath + ".temp");
                return myRequest;
            }

            myRequest.request.Method = "GET";
            myRequest.response = (HttpWebResponse)myRequest.request.GetResponse();
            myRequest.streamResponse = myRequest.response.GetResponseStream();
            myRequest.totFileLength = myRequest.currentFileLength + myRequest.response.ContentLength;

            int length;
            while ((length = await myRequest.streamResponse.ReadAsync(myRequest.BufferRead, 0, myRequest.BufferRead.Length)) != 0)
            {
                myRequest.fileStream.Write(myRequest.BufferRead, 0, length);
                myRequest.requestData.Append(Encoding.ASCII.GetString(myRequest.BufferRead, 0, length));
                Console.WriteLine
                (
                    "下载任务{0}: 下载进度{1:f2}%，文件大小{2}B /{3}B",
                    myRequest.requestNum,
                    100 * (double)(myRequest.requestData.Length + myRequest.currentFileLength) / myRequest.totFileLength,
                    myRequest.requestData.Length + myRequest.currentFileLength,
                    myRequest.totFileLength

                );
            }

            Console.WriteLine("下载完成，文件大小：{0}B", myRequest.totFileLength);
            Console.WriteLine("\nHeaders are :");
            Console.WriteLine(myRequest.response.Headers);
            //Console.WriteLine("\nbodys are");
            //Console.WriteLine(myRequest.requestData.ToString());
            //Console.WriteLine("Press any key to continue..........");
            //Console.ReadLine();

            myRequest.streamResponse.Dispose();
            myRequest.streamResponse.Close();
            myRequest.fileStream.Dispose();
            myRequest.fileStream.Close();
            return myRequest;
        }

        catch (WebException e)
        {
            Console.WriteLine("\nRespCallback Exception raised!");
            Console.WriteLine("\nMessage:{0}", e.Message);
            Console.WriteLine("\nStatus:{0}", e.Status);
            return myRequest;
        }
        
    }
}
```



#### http客户端程序-第三版

用Producer&Consumer模式写一个多线程的Http客户端调度模块，学习C#中加锁、多线程调度方式。



生产者：生产用户请求，将任务装入内存缓冲区，一个线程用来生产，当缓冲队列满了的时候，暂时挂起等待消费者。



消费者：拿走并处理任务，当生产队列为空时，暂时挂起等待生产者



通知的时候，只要buffer不为空就通知消费者取，用ManualResetEvent类通知所有消费者，消费者取任务的时候用互斥锁，防止各个消费者一起，拿到同一个任务（可能中途被抢完了，判空？）。

```C#
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
```

