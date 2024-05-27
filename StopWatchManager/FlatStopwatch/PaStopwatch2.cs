using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPAD_TestTimer
{
    /// <summary>
    /// Flat.
    /// </summary>
    public class PaStopwatch2
    {
        private string m_name;
        private string m_nameType;
        private string m_descriptor;
        private Stopwatch m_model;
        private string m_nameParent;
        private int m_iteration;
        private int m_id;

        public string Name
        {
            get { return m_name; }
        }

        public string NameType
        {
            get { return m_nameType; }
            set { m_nameType = value; }
        }

        public string Description
        {
            get { return m_descriptor; }
            set { m_descriptor = value; }
        }

        public string ParentName
        {
            get { return m_nameParent; }
            set { m_nameParent = value; }
        }

        public int Iteration
        {
            get { return m_iteration; }
            set { m_iteration = value; }
        }

        public double ElapsedMs
        {
            get
            {
                return m_model.Elapsed.TotalMilliseconds;
            }
        }

        public bool IsHasNameType
        {
            get { return !String.IsNullOrEmpty(m_nameType); }
        }

        public bool IsRunning
        {
            get { return m_model.IsRunning; }
        }

        public PaStopwatch2(string name, string nameType)
        {
            m_name = name;
            m_model = new Stopwatch();
            m_nameType = nameType;
            m_nameParent = String.Empty;

        }

        public PaStopwatch2(string name, string nameType, int id)
        {
            m_name = name;
            m_model = new Stopwatch();
            m_nameType = nameType;
            m_nameParent = String.Empty;
            m_id = id;

        }

        public PaStopwatch2(string name, string nameType, string parentName)
        {
            m_name = name;
            m_model = new Stopwatch();
            m_nameType = nameType;
            m_nameParent = parentName;
        }

        public Stopwatch GetStopwatch()
        {
            return m_model;
        }

        public void Start()
        {
            m_model.Start();
            WriteDebugLine("started.");

        }

        public void Stop()
        {
            m_model.Stop();
            WriteDebugLine("stopped.");
        }

        private void WriteDebugLine(string message)
        {
            //string msg = String.Format("Watch \t{0} \t{1}", m_name, message);
            //Debug.WriteLine(msg);
        }

    }
}
