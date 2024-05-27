using System;
using System.Diagnostics;

namespace MPAD_TestTimer
{
    public class StopWatchManager2
    {
        private static StopWatchManager2 instance;
        private bool m_isActivated;
        private bool m_isOutputDebugMessage;
        private PaStopwatchCollection m_model;

        private StopWatchManager2()
        {
            m_isActivated = true;
            m_isOutputDebugMessage = true;
            m_model = new PaStopwatchCollection();
        }

        public static StopWatchManager2 Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StopWatchManager2();
                }
                return instance;
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

        public void Start(string name)
        {
            if (!m_isActivated) return;
            m_model.Start(name);
        }

        public void StartTest(string name, string testType)
        {
            if (!m_isActivated) return;
            m_model.StartTest(name, testType);
        }

        public void Start()
        {
            if (!m_isActivated) return;
            StackTrace zStackTrace = new StackTrace();
            string callingmethod = zStackTrace.GetFrame(1).GetMethod().Name;
            m_model.Start(callingmethod);
        }

        public void Start(string name, string parentName)
        {
            if (!m_isActivated) return;
            m_model.Start(name, parentName);
        }

        public void Stop(string name)
        {
            if (!m_isActivated) return;
            m_model.Stop(name);
        }

        public void Stop(string name, bool ResetTimer)
        {
            if (!m_isActivated) return;
            m_model.Stop(name);
            
        }

        public void Stop()
        {
            if (!m_isActivated) return;
            StackTrace zStackTrace = new StackTrace();
            string callingmethod = zStackTrace.GetFrame(1).GetMethod().Name;
            m_model.Stop(callingmethod);
        }


        public void Stop(string name, string parentName)
        {
            if (!m_isActivated) return;
            m_model.Stop(name, parentName);
        }

        public Stopwatch GetStopwatch(string name)
        {
            return m_model.GetStopwatch(name);
        }

        public Stopwatch GetStopwatch(string name, string parentName)
        {
            return m_model.GetStopwatch(name, parentName);
        }

        public string SaveToFile()
        {
            string reportPath = @"C:\Temp\StopWatchManagerOutputFile.txt";
            string header = "Insert your header description";
            return SaveToFile(reportPath, header);
        }

        public string SaveToFile(string fullPath, string headerDesc)
        {
            if (!m_isActivated)
            {
                WriteDebugLine("StopWatch manager is not active.");
                return String.Empty;
            }
            return m_model.SaveToFile(fullPath, headerDesc, '\0');
        }

        public string SaveToFile(string fullPath, string headerDesc, char delimiter)
        {
            if (!m_isActivated)
            {
                WriteDebugLine("StopWatch manager is not active.");
                return String.Empty;
            }
            return m_model.SaveToFile(fullPath, headerDesc, delimiter);
        }

        public void Clear()
        {
            m_model.Clear();
        }

        private void WriteDebugLine(string message)
        {
            if (!m_isOutputDebugMessage) return;
            Debug.WriteLine(message);
        }
    }
}