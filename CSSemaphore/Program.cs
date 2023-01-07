using System.Diagnostics;
using System.Globalization;

class Program
{

    public delegate int tr1(Semaphore sem);
    public delegate int tr2(Semaphore sem);
    public static void slowProcess()
    {
        for (int i = 0; i < 10; i++)
        {
            Program.j++;
            Thread.Sleep(1000);
        }
        Program.static_sem.Release();
//        return j;
    }

    private static void fastProcess()
    {
        //        int k = 9;
        for (int i = 0; i < 5; i++)
        {
            Program.k++;
            Thread.Sleep(1000);
        }
        Program.static_sem.Release();
//        return k;
    }

    static void Main()
    {
        var p = new Program();
        Program.static_sem = new Semaphore(0, 2);

//        tr1 del_tr1 = new tr1(p.slowProcess);
//        tr2 del_tr2 = new tr2(p.fastProcess);

//        del_tr1(sem);
//        del_tr2(sem);

        // TimeSpan waitTime = TimeSpan.Zero;
        var trFast = new Thread(new ThreadStart(Program.fastProcess));
        var trSlow = new Thread(new ThreadStart(Program.slowProcess));
        trSlow.Start();
        trFast.Start();

        static_sem.WaitOne();

        if (trFast.IsAlive)
        {
            Console.WriteLine("fast counter: " + Program.k.ToString() + " slow counter: " + Program.j.ToString());
            trFast.Abort();
        }
        else
        {
            Console.WriteLine("fast counter: " + Program.k.ToString() + " slow counter: " + Program.j.ToString());
            trSlow.Abort();
        }

        Environment.Exit(0);
    }

    static int j = 0;
    static int k = 0;
    static Semaphore static_sem;
}
