using System.Windows.Forms;
using System.Diagnostics;




namespace ClothoLibAlgo
{
    /// <summary>
    /// A more user friendly stopwatch
    /// </summary>
    public static class sw
    {
        private static Stopwatch s = new Stopwatch();

        public static void start()
        {
            s.Restart();
        }

        public static long elapsed()
        {
            long t = s.ElapsedMilliseconds;
            return t;

        }

        public static long printElapsed()
        {
            long t = s.ElapsedMilliseconds;
            s.Stop();
            s.Reset();
            MessageBox.Show(t.ToString() + "ms");
            return t;
        }
    }

}
