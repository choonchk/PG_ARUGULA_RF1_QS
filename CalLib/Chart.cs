using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace CalLib
{
    public partial class Chart : Form
    {
        System.Windows.Forms.DataVisualization.Charting.Chart[] mYChart;
        ChartArea chartArea;
        Legend[] legend;
        Series[] series;

        public Dictionary<string, Spec> Spec_Dic;
        public Dictionary<string, double[]> Data;
        public int site;

        public Chart(Dictionary<string, double[]> Data, int site)
        {
            InitializeComponent();
            this.Data = Data;
            this.site = site;
        }
        public Dictionary<string, Spec> VerifyData()
        {
            string Path = "C:\\Avago.ATF.Common.x64\\DataLog\\DCTrace";

            DirectoryInfo Dir = new DirectoryInfo(Path);
            if (Dir.Exists == false) Dir.Create();


            StreamWriter StreamWrite = new StreamWriter(string.Format(Path + "\\TraceData_Site{0}.csv", site + 1), false, System.Text.Encoding.UTF8);

            Spec_Dic = new Dictionary<string, Spec>();

            string Stream = "";
            int i = 0;

            foreach (string key in Data.Keys)
            {
                if (i == Data.Count - 1)
                {
                    Stream += key;
                    StreamWrite.WriteLine(Stream);
                    Stream = "";

                }
                else
                {
                    Stream += key + ",";
                }

                i++;

            }

            int Raw = 0;

            while (true)
            {
                int k = 0;
                foreach (string key in Data.Keys)
                {
                    double[] Value = Data[key];

                    if (k == Data.Count - 1)
                    {
                        Stream += Value[Raw];
                        StreamWrite.WriteLine(Stream);
                        Stream = "";

                    }
                    else
                    {
                        Stream += Value[Raw] + ",";
                    }

                    k++;

                }
                Raw++;

                if (Raw == 2000) break;
            }

            StreamWrite.Close();
            int j = 0;
            foreach (string key in Data.Keys)
            {
                switch (key.ToUpper())
                {
                    case "VCC":
                        Spec_Dic.Add(key, new Spec(1e-6, 4e-7, false));
                        break;

                    default:

                        Spec_Dic.Add(key, new Spec(4e-7, 2e-7, false));
                        break;
                }

                double[] _data = Data[key];
                Spec verify_Spec = Spec_Dic[key];

                for (int k = 0; k < 2000; k++)
                {
                    if (verify_Spec.Low * 1e9 < _data[k] * 1e9 && verify_Spec.High * 1e9 > _data[k] * 1e9) { }
                    else
                    {
                        Spec_Dic[key].Flag = true;
                    }
                }
                j++;
            }
            return Spec_Dic;
        }
        public void DrawChart()
        {

            mYChart = new System.Windows.Forms.DataVisualization.Charting.Chart[Data.Count];
            legend = new System.Windows.Forms.DataVisualization.Charting.Legend[Data.Count];
            series = new System.Windows.Forms.DataVisualization.Charting.Series[3];

            tabControl1.TabPages.Clear();

            int i = 0;
            foreach (string key in Data.Keys)
            {

                TabPage myTabPage = new TabPage(key);
                tabControl1.TabPages.Add(myTabPage);

                mYChart[i] = new System.Windows.Forms.DataVisualization.Charting.Chart();
                legend[i] = new System.Windows.Forms.DataVisualization.Charting.Legend();

                for (int j = 0; j < 3; j++)
                {
                    series[j] = new System.Windows.Forms.DataVisualization.Charting.Series();
                }

                for (int unit = 0; unit < 1; unit++)
                {
                    chartArea = new ChartArea();

                    chartArea.Name = "ChartArea" + unit;
                    mYChart[i].ChartAreas.Add(chartArea);
                }


                mYChart[i].Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                              | System.Windows.Forms.AnchorStyles.Left)
                              | System.Windows.Forms.AnchorStyles.Right)));

                mYChart[i].BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;


                legend[i].Name = "Legend1";
                mYChart[i].Legends.Add(legend[i]);
                mYChart[i].Location = new System.Drawing.Point(10, 10);
                mYChart[i].Name = "chart1";

                series[0].BackHatchStyle = System.Windows.Forms.DataVisualization.Charting.ChartHatchStyle.LargeCheckerBoard;
                series[0].ChartArea = "ChartArea1";
                series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                series[0].Legend = "Legend1";
                series[0].Name = "Series1";

                series[1].BackHatchStyle = System.Windows.Forms.DataVisualization.Charting.ChartHatchStyle.LargeCheckerBoard;
                series[1].ChartArea = "Low";
                series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                series[1].Legend = "Low spec";
                series[1].Name = "Low spec";

                series[2].BackHatchStyle = System.Windows.Forms.DataVisualization.Charting.ChartHatchStyle.LargeCheckerBoard;
                series[2].ChartArea = "High";
                series[2].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                series[2].Legend = "High spec";
                series[2].Name = "High spec";

                mYChart[i].Series.Add(series[0]);
                mYChart[i].Series.Add(series[1]);
                mYChart[i].Series.Add(series[2]);

                mYChart[i].Size = new System.Drawing.Size(tabControl1.Width - 20, tabControl1.Height - 20);

                tabControl1.TabPages[i].Controls.Add(mYChart[i]);
                tabControl1.TabPages[i].TabStop = false;

                tabControl1.TabPages[i].Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                     | System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));

                mYChart[i].ChartAreas[0].AxisX.Interval = 0;

                mYChart[i].Series.Clear();

                mYChart[i].ChartAreas[0].AxisX.IsMarginVisible = false;

                mYChart[i].ChartAreas[0].Position.X = 0;
                mYChart[i].ChartAreas[0].Position.Y = 3;

                mYChart[i].ChartAreas[0].Position.Width = 90;
                mYChart[i].ChartAreas[0].Position.Height = 90;


                mYChart[i].ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                mYChart[i].ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                mYChart[i].ChartAreas[0].AxisX.ScrollBar.Enabled = false;
                mYChart[i].ChartAreas[0].AxisY.ScrollBar.Enabled = false;

                mYChart[i].ChartAreas[0].AxisX.LabelStyle.Format = "#.##";
                mYChart[i].ChartAreas[0].AxisY.LabelStyle.Format = "#.#";

                mYChart[i].ChartAreas[0].AxisY.Title = "nA";
                mYChart[i].ChartAreas[0].AxisY.TitleForeColor = Color.Red;
                mYChart[i].ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Center; // Chart X axis Text Alignment 
                mYChart[i].ChartAreas[0].AxisY.TextOrientation = TextOrientation.Rotated270; // Chart X Axis Text Orientation 
                mYChart[i].ChartAreas[0].AxisY.TitleFont = new Font("Arial", 18, FontStyle.Bold); // Chart X axis Title Font

                mYChart[i].AlignDataPointsByAxisLabel();

                mYChart[i].Legends[0].Position.X = 100;
                mYChart[i].Legends[0].Position.Y = 10;
                mYChart[i].Legends[0].Position.Width = 10;
                mYChart[i].Legends[0].Position.Height = 30;

                series[0] = mYChart[i].Series.Add(key);
                series[0].YValueMembers = key;
                series[0].ChartType = SeriesChartType.FastLine;
                series[0].IsVisibleInLegend = true;
                series[0].IsValueShownAsLabel = false;
                series[0].BorderWidth = 2;

                series[1] = mYChart[i].Series.Add("LowL");
                series[1].YValueMembers = "Low spec";
                series[1].ChartType = SeriesChartType.FastLine;
                series[1].IsVisibleInLegend = true;
                series[1].IsValueShownAsLabel = false;
                series[1].BorderWidth = 2;

                series[2] = mYChart[i].Series.Add("HighL");
                series[2].YValueMembers = "High spec";
                series[2].ChartType = SeriesChartType.FastLine;
                series[2].IsVisibleInLegend = true;
                series[2].IsValueShownAsLabel = false;
                series[2].BorderWidth = 2;

                double[] _data = Data[key];

                Spec verify_Spec = Spec_Dic[key];

                for (int k = 0; k < 2000; k++)
                {
                    series[0].Points.AddXY(k, _data[k] * 1e9);
                    series[1].Points.AddXY(k, verify_Spec.Low * 1e9);
                    series[2].Points.AddXY(k, verify_Spec.High * 1e9);

                    if (verify_Spec.Low * 1e9 < _data[k] * 1e9 && verify_Spec.High * 1e9 > _data[k] * 1e9)
                    {

                    }
                    else
                    {
                        Spec_Dic[key].Flag = true;
                    }

                }

                i++;
            }
        }
    }

    public class Spec
    {
        public double High;
        public double Low;
        public bool Flag;
        public Spec(double High, double Low, bool Flag)
        {
            this.High = High;
            this.Low = Low;
            this.Flag = Flag;
        }
    }
}
