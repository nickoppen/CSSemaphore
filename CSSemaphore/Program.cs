using System.Diagnostics;
using System.Globalization;
using static Program;

class Program
{
    /// <summary>
    /// Parameterised Threads can only take one argument so 
    /// declare a struct to contain multiple parameters
    /// </summary>
    struct threadArgs
    {
        public Semaphore sem;
        public CancellationToken cTok;
    }

    /// <summary>
    /// Delegates to assign the methods to.
    /// The delegate must have the same argument list
    /// </summary>
    /// <param name="objArgs"></param>
    public delegate void trSlow(Object objArgs);
    public delegate void trFast(Object objArgs);

    /// <summary>
    /// Methods to assign to the delgate
    /// </summary>
    /// <param name="objArgs"></param>
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

        // Assign the methods to the delgates
        var del_trSlow = new trSlow(p.slowProcess);
        var del_trFast = new trFast(p.fastProcess);

        // Declare the new threads
        var trFast = new Thread(new ParameterizedThreadStart(del_trFast));
        var trSlow = new Thread(new ParameterizedThreadStart(del_trSlow));

        // Start the threads with the bundled parameters
        trSlow.Start(trArgs);
        trFast.Start(trArgs);

        // Wait on the completion of one of the threads
        trArgs.sem.WaitOne();

        // One of the threads must be still alive so report and request that the other exits
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

        // Tidy up
        cts.Dispose();
        Environment.Exit(0);
    }

    static int j = 0;
    static int k = 0;
}
