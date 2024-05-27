using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPAD_TestTimer
{
    /// <summary>
    /// Extends .Net Stopwatch.
    /// </summary>
    public class PaStopwatch
    {
        private string m_name;
        private string m_nameType;
        private string m_descriptor;
        private Stopwatch m_model;
        private List<PaStopwatch> m_swChildrenList;
        private PaStopwatch m_swParent;
        private int m_iteration;

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

        public int Iteration
        {
            get { return m_iteration; }
            set { m_iteration = value; }
        }

        public PaStopwatch Parent
        {
            get { return m_swParent; }
        }

        public List<PaStopwatch> Children
        {
            get { return m_swChildrenList; }
        }

        public bool IsRunning
        {
            get { return m_model.IsRunning; }
        }

        public double ElapsedMs
        {
            get
            {
                return m_model.Elapsed.TotalMilliseconds;
            }
        }

        public PaStopwatch(string name)
        {
            m_name = name;
            m_model = new Stopwatch();
            m_swChildrenList = new List<PaStopwatch>();
            m_swParent = null;
        }

        public PaStopwatch(string name, PaStopwatch parentModel)
        {
            m_name = name;
            m_model = new Stopwatch();
            m_swChildrenList = new List<PaStopwatch>();
            m_swParent = parentModel;
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
