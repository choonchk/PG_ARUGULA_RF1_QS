using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MPAD_TestTimer
{
    public class PaStopwatchCollection
    {
        private List<PaStopwatch> m_list;
        private PaStopwatch m_currentStopWatch;
        private PaStopwatch m_currentStopWatchParent;
        private object lockObject = new object();

        public PaStopwatchCollection()
        {
            m_list = new List<PaStopwatch>();
        }

        public void Start()
        {
            StackTrace zStackTrace = new StackTrace();
            string callingmethod = zStackTrace.GetFrame(1).GetMethod().Name;
            PaStopwatch r1 = GetInstance(callingmethod);
            if (r1 == null)
            {
                r1 = new PaStopwatch(callingmethod);
                lock (lockObject)
                {
                    m_list.Add(r1);
                }
            }
            r1.Start();
        }

        public void Start(string name)
        {
            StartTest(name, String.Empty);
        }

        public void StartTest(string name, string testType)
        {
            PaStopwatch r1 = new PaStopwatch(name);
            r1.NameType = testType;
            lock (lockObject)
            {
                m_list.Add(r1);
            }
            m_currentStopWatch = r1;

            r1.Start();
        }

        public void Start2(string name)
        {
            PaStopwatch r1 = GetInstance(name);
            if (r1 == null)
            {
                r1 = new PaStopwatch(name);
                lock (lockObject)
                {
                    m_list.Add(r1);
                }
            }
            r1.Start();
        }

        public void Start(string name, string parentName)
        {
            PaStopwatch rp1 = m_currentStopWatchParent;
            if (rp1 == null)
            {
                rp1 = GetInstance(parentName);
            }

            PaStopwatch r1 = new PaStopwatch(name, rp1);
            rp1.Children.Add(r1);
            m_currentStopWatch = r1;
            m_currentStopWatchParent = rp1;

            r1.Start();
        }

        public void Stop(string name)
        {
            Stop2(name);
        }

        public void Stop3(string name)
        {
            // L1 Implementation.
            m_currentStopWatch.Stop();
        }

        public void Stop2(string name)
        {
            // L2 Implementation.

            //PaStopwatch swL1 = m_currentStopWatchParent;
            //if (swL1 == null)
            //{
            //    swL1 = GetInstance(name);
            //}
            m_currentStopWatchParent.Stop();
            m_currentStopWatchParent = null;

            // Skip this for performance.
            //StopAllChildren(r1.Children);
        }


        public void Stop(string name, string parentName)
        {
            PaStopwatch swL2 = m_currentStopWatch;
            if (swL2 == null)
            {
                swL2 = GetInstance(parentName);
            }

            swL2.Stop();
            m_currentStopWatch = null;
            // Stop all children.
            //StopAllChildren(r1.Children);
        }

        public Stopwatch GetStopwatch(string name)
        {
            PaStopwatch r1 = GetInstance(name);
            if (r1 == null)
            {
                string msg = String.Format("Watch {0} is not found.", name);
                throw new StopWatchManagerException(msg);
            }
            return r1.GetStopwatch();
        }

        public Stopwatch GetStopwatch(string name, string parentName)
        {
            PaStopwatch r1 = GetInstance(name, parentName);
            if (r1 == null)
            {
                string msg = String.Format("Watch {0} (parent: {1}) is not found.",
                    name, parentName);
                throw new StopWatchManagerException(msg);
            }
            return r1.GetStopwatch();
        }

        public string SaveToFile(string fullPath, string headerDesc, char delimiter)
        {
            try
            {
                // Validate path.
                string pathDir = System.IO.Path.GetDirectoryName(fullPath);
                System.IO.Directory.CreateDirectory(pathDir);

                TimingReport rpt = new TimingReport();
                rpt.Separator = delimiter;
                string output = rpt.Generate(m_list, headerDesc);
                if (!String.IsNullOrEmpty(output))
                {
                    System.IO.File.WriteAllText(fullPath, output);
                }
                return output;
            }
            catch (Exception ex)
            {
                string msg = String.Format("Error saving timing file : {0}",
                    fullPath);
                throw new StopWatchManagerException(msg, ex);
            }
        }

        /// <summary>
        /// Clear to prepare for next execution.
        /// </summary>
        public void Clear()
        {
            foreach (PaStopwatch paSw in m_list)
            {
                paSw.Stop();
            }
            m_list.Clear();

            m_currentStopWatch = null;
            m_currentStopWatchParent = null;

        }

        private PaStopwatch GetInstance(string name)
        {
            return FindChildren(name, String.Empty, m_list);
        }

        private PaStopwatch GetInstance(string name, string parentName)
        {
            return FindChildren(name, parentName, m_list);
        }

        private PaStopwatch FindChildren(string name, string parentName,
            List<PaStopwatch> swList)
        {
            if (swList.Count == 0) return null;

            PaStopwatch r1 = null;
            foreach (PaStopwatch sw in swList)
            {
                string parentName2 = GetParentName(sw, parentName);
                bool isSame = String.Equals(sw.Name, name) &&
                    String.Equals(parentName2, parentName);
                if (isSame)
                {
                    return sw;
                }
                r1 = FindChildren(name, parentName, sw.Children);
            }
            return r1;
        }

        private void StopAllChildren(List<PaStopwatch> swList)
        {
            if (swList.Count == 0) return;

            foreach (PaStopwatch sw in swList)
            {
                if (sw.Children.Count == 0)
                {
                    if (sw.IsRunning)
                    {
                        sw.Stop();
                    }
                    continue;
                }
                StopAllChildren(sw.Children);
                if (sw.IsRunning)
                {
                    sw.Stop();
                }
            }
        }

        private string GetParentName(PaStopwatch sw, string parentName)
        {
            if (sw.Parent == null) return String.Empty;
            if (String.IsNullOrEmpty(parentName)) return String.Empty;
            return sw.Parent.Name;
        }

        private string GetParentName(PaStopwatch sw)
        {
            if (sw.Parent == null) return String.Empty;
            return sw.Parent.Name;
        }

        private void WriteDebugLine(string message)
        {
            Debug.WriteLine(message);
        }

    }

    public class StopWatchManagerException : Exception
    {
        public StopWatchManagerException()
        { }

        public StopWatchManagerException(string message)
            : base(message)
        { }

        public StopWatchManagerException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}