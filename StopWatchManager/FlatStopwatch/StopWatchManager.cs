using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MPAD_TestTimer
{
    /// <summary>
    /// Flat stop watch. Faster than nested.
    /// </summary>
    public class StopWatchManager
    {
        private static StopWatchManager instance;
        private bool m_isActivated;
        private bool m_isOutputDebugMessage;
        private PaStopwatchCollection2[] m_model;
        private byte m_numSites;

        private StopWatchManager()
        {
            m_isActivated = true;
            m_isOutputDebugMessage = true;
            m_numSites = 1;
            //Initialized paStopwatchCollection
            m_model = new PaStopwatchCollection2[m_numSites];
            for (byte site = 0; site < m_numSites; site++)
            {
                m_model[site] = new PaStopwatchCollection2();
            }
        }

        public static StopWatchManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StopWatchManager();
                }
                return instance;
            }
        }

        public byte NumSites
        {
            get { return m_numSites; }
            set
            {
                m_numSites = value;
                //Initialized paStopwatchCollection
                m_model = new PaStopwatchCollection2[m_numSites];
                for (byte site = 0; site < m_numSites; site++)
                {
                    m_model[site] = new PaStopwatchCollection2();
                }
            }
        }

        public bool IsActivated
        {
            get { return m_isActivated; }
            set { m_isActivated = value; }
        }

        public bool IsOutputDebugMessage
        {
            get { return m_isOutputDebugMessage; }
            set { m_isOutputDebugMessage = value; }
        }

        public void Start(string name, byte site)
        {

            if (!m_isActivated) return;
            m_model[site].Start(name);
        }

        public void StartTest(string name, string testType, byte site)
        {
            if (!m_isActivated) return;
            m_model[site].StartTest(name, testType);
        }

        public void StartTest(string name, string testType, Dictionary<string, string> testConditionList, byte site)
        {
            if (!m_isActivated) return;
            m_model[site].StartTest(name, testType, testConditionList);
        }

        public void Start(byte site)
        {
            if (!m_isActivated) return;
            StackTrace zStackTrace = new StackTrace();
            string callingmethod = zStackTrace.GetFrame(1).GetMethod().Name;
            m_model[site].Start(callingmethod);
        }

        public void Start(string name, string parentName, byte site)
        {
            if (!m_isActivated) return;
            m_model[site].Start(name, parentName);
        }

        public void Stop(string name, byte site)
        {
            if (!m_isActivated) return;
            m_model[site].Stop(name);
        }

        public void Stop(string name, bool ResetTimer, byte site)
        {
            if (!m_isActivated) return;
            m_model[site].Stop(name);
            
        }

        public void Stop(byte site)
        {
            if (!m_isActivated) return;
            StackTrace zStackTrace = new StackTrace();
            string callingmethod = zStackTrace.GetFrame(1).GetMethod().Name;
            m_model[site].Stop(callingmethod);
        }


        public void Stop(string name, string parentName, byte site)
        {
            if (!m_isActivated) return;
            m_model[site].Stop(name, parentName);
        }

        public List<PaStopwatch2> GetList(byte site)
        {
            return m_model[site].GetList();
        }

        public PaStopwatch2 GetStopwatch(string name, byte site)
        {
            return m_model[site].GetStopwatch(name);
        }

        public string SaveToFile(byte site)
        {
            string reportPath = @"C:\Temp\StopWatchManagerOutputFile.txt";
            string header = "Insert your header description";
            return SaveToFile(reportPath, header, site);
        }

        public string SaveToFile(string fullPath, string headerDesc, byte site)
        {
            if (!m_isActivated)
            {
                WriteDebugLine("StopWatch manager is not active.");
                return String.Empty;
            }
            return m_model[site].SaveToFile(fullPath, headerDesc, '\0');
        }

        public string SaveToFile(string fullPath, string headerDesc, char delimiter, byte site)
        {
            if (!m_isActivated)
            {
                WriteDebugLine("StopWatch manager is not active.");
                return String.Empty;
            }
            return m_model[site].SaveToFile(fullPath, headerDesc, delimiter);
        }

        /// <summary>
        /// Clear to prepare for next execution. Run history is not cleared.
        /// </summary>
        public void Clear(byte site)
        {
            m_model[site].Clear();
        }

        /// <summary>
        /// Reset all history, all run. Run history is cleared.
        /// </summary>
        public void Reset(byte site)
        {
            m_model[site].Reset();
        }

        private void WriteDebugLine(string message)
        {
            if (!m_isOutputDebugMessage) return;
            Debug.WriteLine(message);
        }
    }

    public class L2StopWatchHelper
    {
        private string m_swParent;
        private string m_swCurrent;

        public L2StopWatchHelper(string parentName)
        {
            m_swParent = parentName;
        }

        public void Start(string name, byte site)
        {
            StopWatchManager.Instance.Start(name, m_swParent, site);
            m_swCurrent = name;
        }

        public void Stop(byte site)
        {
            StopWatchManager.Instance.Stop(m_swCurrent, m_swParent, site);
        }
    }

}