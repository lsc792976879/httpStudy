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