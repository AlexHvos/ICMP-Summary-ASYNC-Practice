using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

class Program
{
    #region Fields & Properties

    static int _PingCount = 2;
    static int _PingInterval = 500;
    static Stopwatch _StopWatch;
    static List<string> _HostsNames = new List<string>()
        {
            "cnn.com",
            "sbs.com.au",
            "bbc.co.uk",
            "maariv.co.il",
            "brazilian.report"
        };

    #endregion
    public static void Main()
    {
        PrintStars();
        PrintReport(GetHostsReplies);
        //
        //PrintStars();
        //PrintReport(GetHostsRepliesWithThreads);
        //
        //PrintStars();
        //PrintReport(GetHostsRepliesWithThreadPool);
        //
        PrintStars();
        PrintReport(GetHostsRepliesWithTasks);
        //
        //PrintStars();
        //PrintReport(GetHostsRepliesWithParallelFor);
        //
        //PrintStars();
        //PrintReport(GetHostsRepliesWithParallelForEach);
        //
        //PrintStars();
        //PrintReport(GetHostsRepliesWithParallelInvoke);
    }

    static Dictionary<string, List<PingReply>> GetHostsReplies()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        foreach (var hostName in _HostsNames)
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(hostName));
                Thread.Sleep(_PingInterval);
            }
            hostsReplies.Add(hostName, pingReplies);
        }
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreads()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Thread> threads = new List<Thread>();
        foreach (var hostName in _HostsNames)
        {
            threads.Add(new Thread(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();
                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }
                hostsReplies.Add(hostName, pingReplies);
            }));
        }
        foreach (Thread t in threads)
        {
            t.Start();
        }
        foreach (Thread t in threads)
        {
            t.Join();
        }
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreadPool()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<EventWaitHandle> ewhs = new List<EventWaitHandle>();
        foreach (var hostName in _HostsNames)
        {
            EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
            ThreadPool.QueueUserWorkItem((data) => {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();
                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }
                hostsReplies.Add(hostName, pingReplies);
                ewh.Set();
            });
            ewhs.Add(ewh);
        }  
        foreach (EventWaitHandle t in ewhs)
        {
            t.WaitOne();
        }
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTasks()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Task> tasks = new List<Task>();
        foreach (var hostName in _HostsNames)
        {
            tasks.Add(new Task(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();
                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }
                hostsReplies.Add(hostName, pingReplies);
            }));
        }
        foreach (Task t in tasks)
        {
            t.Start();
        }
        foreach (Task t in tasks)
        {
            t.Wait();
        }
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelForEach()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        Parallel.ForEach(_HostsNames, (hostName) =>
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(hostName));
                Thread.Sleep(_PingInterval);
            }
            hostsReplies.Add(hostName, pingReplies);
        });
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelFor()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        Parallel.For(0,_HostsNames.Count, (index) =>
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(_HostsNames[index]));
                Thread.Sleep(_PingInterval);
            }
            hostsReplies.Add(_HostsNames[index], pingReplies);
        });
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelInvoke()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        Parallel.Invoke(() => {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            pingReplies.Add(ping.Send(_HostsNames[0]));
            Thread.Sleep(_PingInterval);
            hostsReplies.Add(_HostsNames[0], pingReplies);
        },
        () => {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            pingReplies.Add(ping.Send(_HostsNames[1]));
            Thread.Sleep(_PingInterval);
            hostsReplies.Add(_HostsNames[1], pingReplies);
        },
        () => {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            pingReplies.Add(ping.Send(_HostsNames[2]));
            Thread.Sleep(_PingInterval);
            hostsReplies.Add(_HostsNames[2], pingReplies);
        },
        () => {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            pingReplies.Add(ping.Send(_HostsNames[3]));
            Thread.Sleep(_PingInterval);
            hostsReplies.Add(_HostsNames[3], pingReplies);
        },
        () => {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            pingReplies.Add(ping.Send(_HostsNames[4]));
            Thread.Sleep(_PingInterval);
            hostsReplies.Add(_HostsNames[4], pingReplies);
        });
        return hostsReplies;
    }
    static Dictionary<string, PingReplyStatistics> GetHostsRepliesStatistics(Dictionary<string, List<PingReply>> hostsReplies)
    {
        Dictionary<string, PingReplyStatistics> hrs = new Dictionary<string, PingReplyStatistics>();
        foreach (var hr in hostsReplies)
            hrs.Add(hr.Key, new PingReplyStatistics(hr.Value));
        return hrs;
    }
    static void PrintLine() => Console.WriteLine("---------------------------");
    static void PrintStars() => Console.WriteLine("***************************");
    static void PrintReport(Func<Dictionary<string, List<PingReply>>> getHostsReplies)
    {
        Console.WriteLine($"Started {getHostsReplies.Method.Name}");
        _StopWatch = Stopwatch.StartNew();
        Dictionary<string, List<PingReply>> hostsReplies = getHostsReplies();
        _StopWatch.Stop();
        Console.WriteLine($"Finished {getHostsReplies.Method.Name}");
        PrintLine();
        Console.WriteLine($"Printing {getHostsReplies.Method.Name} report:");
        if (hostsReplies != null)
            PrintHostsRepliesReports(hostsReplies);
        PrintLine();
    }
    static void PrintHostsRepliesReports(Dictionary<string, List<PingReply>> hostsReplies)
    {
        long hostsTotalRoundtripTime = 0;
        Dictionary<string, PingReplyStatistics> hrs = GetHostsRepliesStatistics(hostsReplies);
        PrintTotalRoundtripTime(hrs);
        PrintLine();
        hostsTotalRoundtripTime = hrs.Sum(hr => hr.Value.TotalRoundtripTime);
        Console.WriteLine($"Report took {_StopWatch.ElapsedMilliseconds} ms to generate,{_PingCount * _HostsNames.Count} total pings took total {hostsTotalRoundtripTime} ms hosts roundtrip time");
    }
    static void PrintTotalRoundtripTime(Dictionary<string, PingReplyStatistics> hrs, bool ascendingOrder = true)
    {
        string orderDescription = ascendingOrder ? "ascending" : "descending";
        Console.WriteLine($"Hosts total roundtrip time in {orderDescription} order: (HostName:X,Replies statistics:Y)");
        var orderedHrs = ascendingOrder ? hrs.OrderBy(hr => hr.Value.TotalRoundtripTime) : hrs.OrderByDescending(hr => hr.Value.TotalRoundtripTime);
        foreach (var hr in orderedHrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }
    static void PrintHostsRepliesStatistics(Dictionary<string, PingReplyStatistics> hrs)
    {
        Console.WriteLine("Hosts replies statistics: (HostName:X,Replies statistics:Y)");
        foreach (var hr in hrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }

}