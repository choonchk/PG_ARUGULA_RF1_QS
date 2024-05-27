using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MPAD_TestTimer
{
    public class PaStopwatchCollection2
    {
        private List<PaStopwatch2> m_list;
        private PaStopwatch2 m_currentStopWatchL2;
        private PaStopwatch2 m_currentStopWatchL1;
        private List<List<PaStopwatch2>> m_runCollection;
        private object lockObject = new object();

        public PaStopwatchCollection2()
        {
            m_list = new List<PaStopwatch2>();
            m_runCollection = new List<List<PaStopwatch2>>();
        }

        /// <summary>
        /// Start a L1 stopwatch.
        /// </summary>
        public void Start(string name)
        {
            StartTest(name, String.Empty);
        }

        /// <summary>
        /// Start a L1 stopwatch.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="testType"></param>
        public void StartTest(string name, string testType)
        {
            PaStopwatch2 r1 = new PaStopwatch2(name, testType);

            lock (lockObject)
            {
                m_list.Add(r1);
            }

            m_currentStopWatchL1 = r1;

            r1.Start();
        }

        /// <summary>
        /// Start a L1 stopwatch.
        /// </summary>
        public void StartTest(string name, string testType, Dictionary<string, string> testCondList)
        {
            //PaStopwatch2 r1 = new PaStopwatch2(name, testType, testCondList);
            //m_list.Add(r1);
            //m_currentStopWatchL1 = r1;

            //r1.Start();
        }

        /// <summary>
        /// Start a L2 stopwatch.
        /// </summary>
        public void Start(string name, string parentName)
        {
            PaStopwatch2 r1 = new PaStopwatch2(name, String.Empty, parentName);

            lock (lockObject)
            {
                m_list.Add(r1);
            }

            m_currentStopWatchL2 = r1;

            r1.Start();
        }

        /// <summary>
        /// Stop a L1 stopwatch.
        /// </summary>
        public void Stop(string name)
        {
            if (m_currentStopWatchL1 != null)
            {
                m_currentStopWatchL1.Stop();
                m_currentStopWatchL1 = null;
                return;
            }

            foreach (PaStopwatch2 sw in m_list)
            {                
                if (sw == null) { continue; }
                
                int isSwSameName = String.Compare(sw.Name, name);
                if (isSwSameName == 0)
                {
                    sw.Stop();
                    break;
                }          
            }
        }

        /// <summary>
        /// Stop a L2 stopwatch.
        /// </summary>
        public void Stop(string name, string parentName)
        {
            m_currentStopWatchL2.Stop();
        }

        public List<PaStopwatch2> GetList()
        {
            return m_list;
        }

        public PaStopwatch2 GetStopwatch(string swName)
        {
            foreach (PaStopwatch2 sw in m_list)
            {
                bool isSame = String.Compare(sw.Name, swName) == 0;
                if (isSame) return sw;
                isSame = String.Compare(sw.ParentName, swName) == 0;
                if (isSame) return sw;
            }

            return null;
        }

        public string SaveToFile(string fullPath, string headerDesc, char delimiter)
        {
            m_runCollection.Add(m_list);

            try
            {
                // Validate path.
                string pathDir = System.IO.Path.GetDirectoryName(fullPath);
                System.IO.Directory.CreateDirectory(pathDir);

                TimingReport3 rpt = new TimingReport3();
                rpt.Separator = delimiter;
                string output = rpt.Generate(m_runCollection, headerDesc);
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

        public string SaveToFile3(string fullPath, string headerDesc, char delimiter)
        {
            try
            {
                // Validate path.
                string pathDir = System.IO.Path.GetDirectoryName(fullPath);
                System.IO.Directory.CreateDirectory(pathDir);

                TimingReport2 rpt = new TimingReport2();
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
        /// Clear to prepare for next execution. Run history is not cleared.
        /// </summary>
        public void Clear()
        {
            foreach (PaStopwatch2 paSw in m_list)
            {
                if(paSw == null) { continue; }

                paSw.Stop();
            }
            m_list = new List<PaStopwatch2>();

            m_currentStopWatchL2 = null;
            m_currentStopWatchL1 = null;

        }

        /// <summary>
        /// Reset all history, all run. Run history is cleared.
        /// </summary>
        public void Reset()
        {
            Clear();
            m_runCollection = new List<List<PaStopwatch2>>();
        }

        private void WriteDebugLine(string message)
        {
            Debug.WriteLine(message);
        }

    }
}