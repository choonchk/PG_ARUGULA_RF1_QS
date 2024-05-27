using MPAD_TestTimer;

namespace TestLib
{
    public class TimingBase
    {
        private string m_testParaName;
        private string m_typeName;
        private bool m_isDetectRepeatedCall;
        private int m_callIteration;
        private string m_testParaName2;

        public void InitializeTiming(string testParaName)
        {
            m_testParaName = testParaName;
            m_typeName = this.GetType().Name;
            m_callIteration = 1;
        }

        /// <summary>
        /// use this if the test is ran more than once. detect repeated call.
        /// </summary>
        /// <param name="testParaName"></param>
        public void InitializeTiming2(string testParaName)
        {
            m_testParaName = testParaName;
            m_typeName = this.GetType().Name;
            m_callIteration = 1;
            m_isDetectRepeatedCall = true;
        }

        protected void SwBeginRun(byte site)
        {
            if (!m_isDetectRepeatedCall)
            {
                StopWatchManager.Instance.StartTest(m_testParaName, m_typeName, site);
            }
            else
            {
                m_testParaName2 = string.Format("{0}_Run{1}", m_testParaName, m_callIteration);
                StopWatchManager.Instance.StartTest(m_testParaName2, m_typeName, site);
            }
        }

        protected void SwEndRun(byte site)
        {
            if (!m_isDetectRepeatedCall)
            {
                StopWatchManager.Instance.Stop(m_testParaName, site);
            }
            else
            {
                StopWatchManager.Instance.Stop(m_testParaName2, site);
                m_callIteration++;
            }
        }

        protected void SwStartRun(string stepDescription, byte site)
        {
            if (!m_isDetectRepeatedCall)
            {
                StopWatchManager.Instance.Start(stepDescription, m_testParaName, site);
            }
            else
            {
                StopWatchManager.Instance.Start(stepDescription, m_testParaName2, site);
            }
        }

        protected void SwStopRun(string stepDescription, byte site)
        {
            if (!m_isDetectRepeatedCall)
            {
                StopWatchManager.Instance.Stop(stepDescription, m_testParaName, site);
            }
            else
            {
                StopWatchManager.Instance.Stop(stepDescription, m_testParaName2, site);
            }
        }

        // Not supported.
        protected void SwStartRunThread(string stepDescription)
        {
            //StopWatchManager.Instance.Start(stepDescription, m_testParaName);
        }

        protected void SwStopRunThread(string stepDescription)
        {
            //StopWatchManager.Instance.Stop(stepDescription, m_testParaName);
        }
    }
}