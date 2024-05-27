using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPAD_TestTimer
{
    public class TimingReport
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

        public TimingReport()
        {
            m_separator = ',';
            m_testNoCounter = 1;
            m_deltaList = new List<double>();
        }

        public string Generate(List<PaStopwatch> swList, string headerDesc)
        {
            if (swList.Count == 0) return String.Empty;

            StringBuilder sb = new StringBuilder();

            // generate header.
            string separatorStr = new string(m_separator, 1);
            sb.AppendFormat("{0}{1}Timestamp: {2}", headerDesc, separatorStr, DateTime.Now);
            sb.AppendLine();
            sb.Append("Stopwatch Name");
            sb.Append(m_separator, 1);
            sb.AppendLine("Elapsed (ms)");
            separatorStr = new string(m_separator, 1);
            sb.AppendFormat("L1{0}L2{0}L3{1}L1{0}L2{0}L3", m_separator, separatorStr);
            sb.AppendLine();

            // generate content.
            foreach (PaStopwatch sw in swList)
            {
                AddWarning1(sb, sw);
                FormLine(sb, sw, 1);
            }

            // generate footer.
            //sb.Append("Total level 1 time (ms):");
            //sb.Append(m_separator, 5);
            //double totalL1 = SumLevel1(swList);
            //sb.AppendLine(Convert(totalL1));
            //sb.AppendLine();

            //sb.Append("Total level 2 time (ms):");
            //sb.Append(m_separator, 5);
            //double totalL2 = SumLevel2(swList);
            //sb.AppendLine(Convert(totalL2));
            //sb.AppendLine();

            string stat1 = String.Format("Delta: Sum:{0} Max:{1} Mean:{2}",
                m_deltaList.Sum(), m_deltaList.Max(), m_deltaList.Average());
            sb.Append(stat1);
            sb.AppendLine();

            return sb.ToString();
        }

        private void FormLine(StringBuilder sb, PaStopwatch swToAdd, int level)
        {
            switch (level)
            {
                case 1:
                    sb.Append(swToAdd.Name);
                    sb.Append(m_separator, 3);
                    string ms = Convert(swToAdd.ElapsedMs);
                    sb.Append(ms);
                    if (swToAdd.Children.Count > 0)
                    {
                        double sumL2 = SumLevel1(swToAdd.Children);
                        double deltaL1L2 = swToAdd.ElapsedMs - sumL2;
                        sb.Append(m_separator, 1);
                        sb.Append(deltaL1L2.ToString("F2"));
                        string swDebugMsg2 = String.Format("No: {0} Delta: {1:F2} L2Sum: {2:F2} ({3})",
                            m_testNoCounter, deltaL1L2, sumL2, swToAdd.NameType);
                        m_deltaList.Add(deltaL1L2);
                        sb.Append(m_separator, 3);
                        sb.Append(swDebugMsg2);
                        sb.AppendLine();
                        PublishLevel2(sb, swToAdd.Children);
                        m_testNoCounter++;
                    }
                    else
                    {
                        sb.AppendLine();
                    }


                    break;

                case 2:
                    sb.Append(m_separator, 1);
                    sb.Append(swToAdd.Name);
                    sb.Append(m_separator, 8);
                    ms = Convert(swToAdd.ElapsedMs);
                    sb.AppendLine(ms);
                    break;

                case 3:
                    sb.Append(m_separator, 2);
                    sb.Append(swToAdd.Name);
                    sb.Append(m_separator, 8);
                    ms = Convert(swToAdd.ElapsedMs);
                    sb.AppendLine(ms);
                    break;
            }
        }

        private double SumLevel1(List<PaStopwatch> swList)
        {
            double result = 0;
            foreach (PaStopwatch swL2 in swList)
            {
                result += swL2.ElapsedMs;
            }

            return result;
        }

        private double SumLevel2(List<PaStopwatch> swList)
        {
            double result = 0;
            foreach (PaStopwatch swL1 in swList)
            {
                foreach (PaStopwatch swL2 in swL1.Children)
                {
                    result += swL2.ElapsedMs;
                }
            }

            return result;
        }

        private void PublishLevel2(StringBuilder sb, List<PaStopwatch> swL2List)
        {
            foreach (PaStopwatch swL2 in swL2List)
            {
                sb.Append(m_separator, 1);
                sb.Append(swL2.Name);
                sb.Append(m_separator, 3);
                string ms = Convert(swL2.ElapsedMs);
                sb.AppendLine(ms);
            }
        }

        private void AddWarning1(StringBuilder sb, PaStopwatch swToAdd)
        {
            if (!swToAdd.IsRunning) return;

            if (swToAdd.Parent == null)      // Level 1
            {
                string msg = String.Format("Warning: Watch {0} is still running.",
                    swToAdd.Name);
                sb.AppendLine(msg);
                return;
            }
            if (swToAdd.Parent.Parent == null) // Level 2
            {
                string msg = String.Format("Warning: Watch {0} (parent: {1}) is still running.",
                    swToAdd.Name, swToAdd.Parent.Name);
                sb.AppendLine(msg);
                return;
            }
            if (swToAdd.Parent.Parent.Parent == null)// Level 3
            {
                string msg = String.Format("Warning: Watch {0} (parent: {1},{2}) is still running.",
                    swToAdd.Name, swToAdd.Parent.Name, swToAdd.Parent.Parent.Name);
                sb.AppendLine(msg);
            }
        }

        private string Convert(double timeMs)
        {
            return timeMs.ToString("F4");
        }
    }
}