using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MPAD_TestTimer
{
    /// <summary>
    /// Flat report.
    /// </summary>
    public class TimingReport2
    {
        private char m_separator;
        private int m_testNoCounter;
        private List<double> m_deltaList;

        public char Separator
        {
            get { return m_separator; }
            set
            {
                if (value == '\0') return;        // null character.
                if (Char.IsWhiteSpace(value)) return;
                m_separator = value;
            }
        }

        public TimingReport2()
        {
            m_separator = ',';
            m_testNoCounter = 1;
            m_deltaList = new List<double>();
        }

        public string Generate(List<PaStopwatch2> swList, string headerDesc)
        {
            if (swList.Count == 0) return String.Empty;

            StringBuilder sb = new StringBuilder();

            // generate header.
            string separatorStr = new string(m_separator, 1);
            sb.AppendFormat("{0}{1}Timestamp: {2}", headerDesc, separatorStr, DateTime.Now);
            sb.AppendLine();

            // generate summary.
            sb.Append("Summary:Total level 2 time (ms):");
            sb.Append(m_separator, 3);
            double totalL2 = SumLevel2(swList);
            sb.AppendLine(Convert(totalL2));
            sb.AppendLine();

            sb.AppendFormat("Parent Name{0}Name{0}PTime(ms){0}Time(ms){0}Test Type{0}Test Number", m_separator);
            sb.AppendLine();

            // generate content.
            foreach (PaStopwatch2 sw in swList)
            {
                AddWarning1(sb, sw);
                FormLine(sb, sw);
            }


            return sb.ToString();
        }

        private void FormLine(StringBuilder sb, PaStopwatch2 swToAdd)
        {
            sb.Append(swToAdd.ParentName);
            sb.Append(m_separator, 1);
            sb.Append(swToAdd.Name);
            sb.Append(m_separator, 1);
            string ms = Convert(swToAdd.ElapsedMs);
            bool isParent = swToAdd.ParentName == String.Empty;
            if (isParent)
            {
                sb.Append(ms);
                sb.Append(m_separator, 2);
                sb.Append(swToAdd.NameType);
                sb.Append(m_separator, 1);
                sb.Append(m_testNoCounter);
                sb.Append(m_separator, 1);
                m_testNoCounter++;
            }
            else
            {
                // L2.
                sb.Append(m_separator, 1);
                sb.Append(ms);
            }

            sb.AppendLine();
        }

        private double SumLevel1(List<PaStopwatch2> swList)
        {
            double result = 0;
            foreach (PaStopwatch2 swL2 in swList)
            {
                if (swL2.ParentName == String.Empty)
                {
                    result += swL2.ElapsedMs;
                }
            }

            return result;
        }

        private double SumLevel2(List<PaStopwatch2> swList)
        {
            double result = 0;
            foreach (PaStopwatch2 swL2 in swList)
            {
                if (swL2.ParentName != String.Empty)
                {
                    result += swL2.ElapsedMs;
                }
            }

            return result;
        }

        private void AddWarning1(StringBuilder sb, PaStopwatch2 swToAdd)
        {
            if (!swToAdd.IsRunning) return;

            string msg = String.Format("Warning: Watch {0} is still running.",
                swToAdd.Name);
            sb.AppendLine(msg);
        }

        private string Convert(double timeMs)
        {
            return timeMs.ToString("F4");
        }
    }

    /// <summary>
    /// Report for multiple run.
    /// </summary>
    public class TimingReport3
    {
        private char m_separator;
        private int m_testNoCounter;
        private List<double> m_deltaList;
        private List<string> sl;

        public char Separator
        {
            get { return m_separator; }
            set
            {
                if (value == '\0') return;        // null character.
                if (Char.IsWhiteSpace(value)) return;
                m_separator = value;
            }
        }

        public TimingReport3()
        {
            m_separator = ',';
            m_testNoCounter = 1;
            m_deltaList = new List<double>();
            sl = new List<string>();

        }

        public string Generate(List<List<PaStopwatch2>> allRunList, string headerDesc)
        {
            if (allRunList.Count == 0) return String.Empty;

            StringBuilder sb = new StringBuilder();

            // generate header.
            string separatorStr = new string(m_separator, 1);
            sb.AppendFormat("{0}{1}Report Type: Report 3{1}Timestamp: {2}", headerDesc, separatorStr, DateTime.Now);
            sb.AppendLine();

            // generate summary.
            sb.Append("Summary-Total run");
            sb.Append(m_separator, 1);
            sb.AppendLine(allRunList.Count.ToString());
            sb.AppendLine();

            sl.Add("Test No");
            sl.Add("Parent Name");
            sl.Add("Test Name");
            int runCount = allRunList.Count;
            for (int i = 1; i <= runCount; i++)
            {
                sl.Add(string.Format("R{0}", i));
            }
            sl.Add("Stat-Average");
            sl.Add("Stat-Range");
            sl.Add("Stat-Range/Avg");


            sb.AppendLine(FormLine());

            // generate content.
            Report3Model tr = new Report3Model();
            List<Report3Line> rlList = tr.Form(allRunList);

            if (rlList == null)
            {
                sb.AppendLine("Warning: The number of stopwatch for the first run is different from the second run.");
                return sb.ToString();
            }

            Report3Line sumLine = tr.CalculateL2Time(allRunList);
            rlList.Add(sumLine);

            foreach (Report3Line r1 in rlList)
            {
                sl.Add(r1.NumberString);
                sl.Add(r1.ParentName);
                sl.Add(r1.Name);
                sl.Add(r1.ElapsedMsList2);
                sl.Add(r1.Average);
                sl.Add(r1.Range);
                sl.Add(r1.RangeDivAverage);

                sb.AppendLine(FormLine());
            }

            // Generate footer
            sb.AppendLine();

            return sb.ToString();
        }

        private string FormLine()
        {
            string result = String.Join(",", sl.ToArray());
            sl.Clear();
            return result;
        }

        private double SumLevel1(List<PaStopwatch2> swList)
        {
            double result = 0;
            foreach (PaStopwatch2 swL2 in swList)
            {
                if (swL2.ParentName == String.Empty)
                {
                    result += swL2.ElapsedMs;
                }
            }

            return result;
        }

        private double SumLevel2(List<PaStopwatch2> swList)
        {
            double result = 0;
            foreach (PaStopwatch2 swL2 in swList)
            {
                if (swL2.ParentName != String.Empty)
                {
                    result += swL2.ElapsedMs;
                }
            }

            return result;
        }

        private void AddWarning1(StringBuilder sb, PaStopwatch2 swToAdd)
        {
            if (!swToAdd.IsRunning) return;

            string msg = String.Format("Warning: Watch {0} is still running.",
                swToAdd.Name);
            sb.AppendLine(msg);
        }

        private string Convert(double timeMs)
        {
            return timeMs.ToString("F4");
        }
    }


    public class Report3Line
    {
        public int Number { get; set; }

        public string NumberString
        {
            get { return Number.ToString(); }
        }

        public string Name { get; set; }
        public string ParentName { get; set; }

        public List<double> ElapsedMsList { get; }

        public string ElapsedMsList2
        {
            get
            {
                string result = String.Join(",", ElapsedMsList.ToArray());
                return result;
            }
        }

        public string Range
        {
            get
            {
                double r = ElapsedMsList.Max() - ElapsedMsList.Min();
                return Convert(r);
            }
        }

        public string Average
        {
            get
            {
                double r = ElapsedMsList.Average();
                return Convert(r);
            }
        }

        public string RangeDivAverage
        {
            get
            {
                double r = (ElapsedMsList.Max() - ElapsedMsList.Min()) / ElapsedMsList.Average();
                return Convert(r);
            }
        }

        public Report3Line()
        {
            ElapsedMsList = new List<double>();
        }

        private string Convert(double timeMs)
        {
            return timeMs.ToString("F4");
        }

    }


    public class Report3Model
    {
        private List<Report3Line> m_reportCollection;

        public List<Report3Line> Form(List<List<PaStopwatch2>> runList)
        {
            bool isValid = Validate(runList);
            if (!isValid) return null;

            // Use the 1st run to form the row and column. Assuming same for the rest of run.
            m_reportCollection = new List<Report3Line>();
            List<Report3Line> run1Result = Form1Run(runList[0]);
            m_reportCollection.AddRange(run1Result);

            if (runList.Count==1) return m_reportCollection;

            // Take the rest of the run, except the 1st one.
            List<List<PaStopwatch2>> run234SwList = runList.GetRange(1, runList.Count - 1);

            foreach (List<PaStopwatch2> runResult in run234SwList)
            {
                FillNextRun(runResult);
            }
            return m_reportCollection;
        }

        public Report3Line CalculateL2Time(List<List<PaStopwatch2>> swList)
        {
            Report3Line r = new Report3Line();
            r.ParentName = "L2 Run time";
            foreach (List<PaStopwatch2> run in swList)
            {              
                double sumTimeL2 = 0;
                foreach (PaStopwatch2 swL2 in run)
                {
                    if (swL2 == null) { continue; }

                    bool isTest = swL2.ParentName == String.Empty;
                    if (!isTest)
                    {
                        sumTimeL2 += swL2.ElapsedMs;
                    }
                }

                r.ElapsedMsList.Add(sumTimeL2);
            }

            return r;
        }

        private List<Report3Line> Form1Run(List<PaStopwatch2> run1SwList)
        {
            List<Report3Line> run1 = new List<Report3Line>();
            int testCounter = 1;
            List<PaStopwatch2> run1SwList2 = GetFilteredList(run1SwList);
            foreach (PaStopwatch2 resultItem1 in run1SwList2)
            {
                Report3Line rl = new Report3Line();
                rl.ParentName = resultItem1.ParentName;
                bool isTest = resultItem1.ParentName == String.Empty;
                if (isTest)
                {
                    rl.Number = testCounter;
                    testCounter++;
                }
                rl.Name = resultItem1.Name;
                rl.ElapsedMsList.Add(resultItem1.ElapsedMs);
                run1.Add(rl);
            }

            return run1;
        }

        private void FillNextRun(List<PaStopwatch2> run234SwList)
        {
            List<PaStopwatch2> run234SwList2 = GetFilteredList(run234SwList);

            for (int i = 0; i < run234SwList2.Count; i++)
            {
                PaStopwatch2 run234Sw = run234SwList2[i];
                m_reportCollection[i].ElapsedMsList.Add(run234Sw.ElapsedMs);
            }
        }

        private List<PaStopwatch2> GetFilteredList(List<PaStopwatch2> swList)
        {
            List<PaStopwatch2> filteredList = new List<PaStopwatch2>();
            foreach (PaStopwatch2 resultItem1 in swList)
            {
                bool isExcluded = resultItem1 == null || resultItem1.NameType == "TRIGGER" || resultItem1.NameType == "TRIGGERNF";
                if (isExcluded) continue;
                filteredList.Add(resultItem1);
            }

            return filteredList;

        }

        private bool Validate(List<List<PaStopwatch2>> pList)
        {
            // Validate that all run are the same re-run.
            int firstRunCount = pList[0].Count;
            bool isValid = true;
            foreach (List<PaStopwatch2> dutRun in pList)
            {
                isValid = dutRun.Count == firstRunCount;
            }
            return isValid;
        }
    }

}