using System.Diagnostics;
using Avago.ATF.Logger;
using Avago.ATF.LogService;

namespace MPAD_TestTimer
{
    /// <summary>
    /// Centralized message logging.
    /// </summary>
    public class LoggingManager
    {
        private ATFLogControl m_loggingService;
        private static LoggingManager instance;

        public static LoggingManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LoggingManager();
                }
                return instance;
            }
        }
        public void SetService(ATFLogControl loggingService)
        {
            m_loggingService = loggingService;
        }

        public void LogInfo(string message)
        {
            Debug.WriteLine(message, "Info");
            if (m_loggingService == null) return;
            m_loggingService.Log(LogLevel.Info, message);

        }

        public void LogInfoTestPlan(string message)
        {
            Debug.WriteLine(message, "Info-TestPlan");
            if (m_loggingService == null) return;
            m_loggingService.Log(LogLevel.Info, LogSource.eTestPlan, message);

        }

        public void LogWarningTestPlan(string message)
        {
            Debug.WriteLine(message, "Warning");
            if (m_loggingService == null) return;
            m_loggingService.Log(LogLevel.Warn, message);

        }

        public void LogError(string message)
        {
            Debug.WriteLine(message, "Error");
            if (m_loggingService == null) return;
            m_loggingService.Log(LogLevel.Error, message);
        }

        public void LogErrorTestPlan(string message)
        {
            Debug.WriteLine(message, "Error");
            if (m_loggingService == null) return;
            m_loggingService.Log(LogLevel.Error, LogSource.eTestPlan, message);
        }

        public void LogHighlight(string message)
        {
            Debug.WriteLine(message, "Highlight");
            if (m_loggingService == null) return;
            m_loggingService.Log(LogLevel.HighLight, message);
        }
    }
}
