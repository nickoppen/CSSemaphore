using System.Diagnostics;
using System.Globalization;
using static Program;

class Program
{
    struct threadArgs
    {
        public Semaphore sem;
        public CancellationToken cTok;
    }

    public delegate void trSlow(Object objArgs);
    public delegate void trFast(Object objArgs);
    public void slowProcess(Object objArgs)
    {
        var args = (threadArgs)objArgs;

        for (int i = 0; i < 100; i++)
        {
            Program.j++;
            Thread.Sleep(100);
            if (args.cTok.IsCancellationRequested)
                return;
        }
        args.sem.Release();
    }

    public void fastProcess(Object objArgs)
    {
        var args = (threadArgs)objArgs;
        for (int i = 0; i < 50; i++)
        {
            Program.k++;
            Thread.Sleep(100);
            if (args.cTok.IsCancellationRequested)
                return;
        }
        args.sem.Release();
    }

    static void Main()
    {
        var p = new Program();
        var cts = new CancellationTokenSource();

        var trArgs = new threadArgs
        {
            sem = new Semaphore(0, 2),
            cTok = cts.Token
        };

        var del_trSlow = new trSlow(p.slowProcess);
        var del_trFast = new trFast(p.fastProcess);

        var trFast = new Thread(new ParameterizedThreadStart(del_trFast));
        var trSlow = new Thread(new ParameterizedThreadStart(del_trSlow));
        trSlow.Start(trArgs);
        trFast.Start(trArgs);

        trArgs.sem.WaitOne();

        if (trFast.IsAlive)
        {
            Console.WriteLine("fast counter: " + Program.k.ToString() + " slow counter: " + Program.j.ToString());
            cts.Cancel();
        }
        else
        {
            Console.WriteLine("fast counter: " + Program.k.ToString() + " slow counter: " + Program.j.ToString());
            cts.Cancel();
        }

        cts.Dispose();
        Environment.Exit(0);
    }

    static int j = 0;
    static int k = 0;
//    static Semaphore static_sem;
}
