using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System;

namespace ClothoLibStandard
{
    public class HiPerfTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;

        private long freq;

        public HiPerfTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported

                throw new Win32Exception();
            }
        }

        public void Start()
        {
            QueryPerformanceCounter(out startTime);
        }
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }

        public void wait(double sleep_ms)
        {
            long intstartTime, intstopTime;
            QueryPerformanceCounter(out intstartTime);
            float time;
            do
            {
                QueryPerformanceCounter(out intstopTime);
                time = ((float)(intstopTime - intstartTime) / (float)freq) * 1000;
            } while (time < (float)sleep_ms);
        }
    }
}
