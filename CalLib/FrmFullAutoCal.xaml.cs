using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using GuCal;
using EqLib;
using System.ComponentModel;

namespace CalLib
{
    /// <summary>
    /// Interaction logic for FrmFullAutoCal.xaml
    /// </summary>
    public partial class FrmFullAutoCal : Window
    {
        private AutoCal myAutoCal;
        private int SiteNo;
        private int TotalSite = 1;
        public RelayCommand ToDelegateCommandThatExecuteNothing
        {
            get
            {
                return new RelayCommand(o => DoNothing());
            }
        }

        public FrmFullAutoCal(AutoCal myAutoCal, int SiteNo)
        {
            this.myAutoCal = myAutoCal;
            this.SiteNo = SiteNo;
            InitializeComponent();
            GU.runningGU.SetAll(false);
            GU.runningGUIccCal.SetAll(false);

            for (int site = 0; site < TotalSite; site++)
            {
                list_guCal.Items.Add(new GuUiSiteData(SiteNo));
            }

            foreach (var batch in GU.dutIdAllLoose.Keys)
            {
                combo_guBatch.Items.Add(batch);
            }

            combo_guBatch.SelectedIndex = 0;
        }

        private void DoNothing()
        {
        }

        private async void btn_proceed_Click(object sender, RoutedEventArgs e)
        {
            myAutoCal.CalTypes = new List<AutoCal.CalType>();

            if (ch_spar.IsChecked.Value)
                myAutoCal.CalTypes.Add(AutoCal.CalType.VNA);

            foreach (GuUiSiteData item in list_guCal.Items)
            {
                GU.runningGU[item.site0Based] = item.runVerifyChecked;
                GU.runningGUIccCal[item.site0Based] = item.runIccChecked;

                //if (item.runIccChecked) GU.GuMode[item.site0Based] = GU.GuModes.IccCorrVrfy;
                //if (item.runDCVerifyChecked) GU.GuMode[item.site0Based] = GU.GuModes.DCcheckCorrVrfy;
                //else if (item.runCorrChecked) GU.GuMode[item.site0Based] = GU.GuModes.CorrVrfy;
                //else if (item.runVerifyChecked) GU.GuMode[item.site0Based] = GU.GuModes.Vrfy;
                //else GU.GuMode[item.site0Based] = GU.GuModes.None;

                if (item.runDCVerifyChecked && item.runCorrChecked && item.runVerifyChecked)
                    GU.GuMode[item.site0Based] = GU.GuModes.DCcheckCorrVrfy;
                else if (item.runDCVerifyChecked && item.runVerifyChecked)
                    GU.GuMode[item.site0Based] = GU.GuModes.DCcheckVrfy;
                else if (item.runCorrChecked && item.runVerifyChecked)
                    GU.GuMode[item.site0Based] = GU.GuModes.CorrVrfy;
                else if (item.runVerifyChecked)
                    GU.GuMode[item.site0Based] = GU.GuModes.Vrfy;
                else
                    GU.GuMode[item.site0Based] = GU.GuModes.None;
            }


            if (GU.runningGU.Contains(true))
            {
                //GU.runningGUIccCal[site] = true;
                myAutoCal.CalTypes.Add(AutoCal.CalType.GU);

                GU.selectedBatch = Convert.ToInt32(combo_guBatch.SelectedItem);
                GU.dutIdLooseUserReducedList = GU.dutIdAllLoose[GU.selectedBatch];
            }

            GU.DoInit_afterUI();

            progressTitle.Text = "";
            progressText.Text = "";
            progressBar.Value = 0;
            //var btnSkipText = new Progress<string>(x => ReportBtnSkipTitle(x));
            var progressTitleMessage = new Progress<string>(x => ReportProgressTitle(x));
            var progressDetailMessage = new Progress<string>(x => ReportProgressDetail(x));
            var progressPercentValue = new Progress<double>(x => ReportProgressPercentage(x));
            cancelToken = new CancellationTokenSource();

            progressBox.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(() => myAutoCal.Execute(cancelToken.Token, progressTitleMessage, progressDetailMessage, progressPercentValue));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export Error: " + ex.Message);   // ct.ThrowIfCancellationRequested();
            }
            finally
            {
                foreach (GuUiSiteData g in list_guCal.Items)
                {
                    g.InitializeProperties();
                }
                progressBox.Visibility = Visibility.Collapsed;

                for (int site = 0; site < TotalSite; site++)
                {
                    if (GU.thisProductsGuStatus.VerifyIsOptional(site))
                    {
                        btn_skip.IsEnabled = true;
                        btn_skip.Content = "Continue";
                        btn_proceed.Content = "Run Again";
                    }
                }

            }
            //this.Close();
        }

        private void btn_skip_Click(object sender, RoutedEventArgs e)
        {
            myAutoCal.CalTypes = new List<AutoCal.CalType>();

            GU.runningGU.SetAll(false);
            GU.runningGUIccCal.SetAll(false);

            this.Close();
        }

        private void btnCancelCal_Click(object sender, RoutedEventArgs e)
        {
            cancelToken.Cancel();
        }

        private CancellationTokenSource cancelToken;

        void ReportProgressTitle(string msg)
        {
            if (msg != null)
                progressTitle.Text = msg;
        }

        //void ReportBtnSkipTitle(string msg)
        //{
        //    if (msg != null)
        //        btn_skip.Content = msg;
        //}

        void ReportProgressDetail(string msg)
        {
            if (msg != null)
                progressText.Text = msg;
        }

        void ReportProgressPercentage(double percent)
        {
            if (percent >= 0)
                progressBar.Value = percent;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }

    public class GuUiSiteData : INotifyPropertyChanged   // move to another file FrmFullAutoCal_dataContext. This cs file should only relate to UI, no other logic.
    {
        public readonly int site0Based;

        public int site1Based
        {
            get
            {
                return site0Based + 1;
            }
        }

        public bool iccIsOptional
        {
            get
            {
                return GU.thisProductsGuStatus.IccIsOptional(site0Based);
            }
        }

        public bool dcVerifyIsOptional
        {
            get
            {
                return GU.thisProductsGuStatus.DCVerifyIsOptional(site0Based);
            }
        }

        public bool corrIsOptional
        {
            get
            {
                return GU.thisProductsGuStatus.CorrIsOptional(site0Based);
            }
        }
        public bool verifyIsOptional
        {
            get
            {
                return GU.thisProductsGuStatus.VerifyIsOptional(site0Based);
            }
        }
        public string verifyIsCFFile
        {
            get
            {
                return GU.thisProductsGuStatus.IsCFFileVerify(site0Based);
            }
        }

        public string status
        {
            get
            {
                bool status = System.IO.Path.GetFileName(GU.correlationFilePath).ToUpper().Replace(".CSV", "") == GU.thisProductsGuStatus.IsCFFileVerify(0).ToUpper() ? true : false;
                
                if (GU.thisProductsGuStatus.VerifyIsOptional(site0Based) && status)
                {
                    return "valid";
                }
                //if (!GU.thisProductsGuStatus[site0Based].iccCalPassed &&(!GU.testertype.Contains("FBAR")|| !GU.testertype.Contains("SPARA")))
                //{
                //    return "Icc failed " + GU.thisProductsGuStatus[site0Based].iccCalFailures + "x";
                //}
                if (!GU.thisProductsGuStatus[site0Based].correlationFactorsPassed)
                {
                    return "Corr failed " + GU.thisProductsGuStatus[site0Based].correlationFailures + "x";
                }
                if (!GU.thisProductsGuStatus[site0Based].verificationPassed)
                {
                    return "Verify failed " + GU.thisProductsGuStatus[site0Based].verificationFailures + "x";
                }
                //if (GU.thisProductsGuStatus.IsIccExpired(site0Based) && (!GU.testertype.Contains("FBAR") || !GU.testertype.Contains("SPARA")))
                //{
                //    return "Icc expired";
                //}
                if (!status)
                {
                    return "CF File has changed";
                }
                if (GU.thisProductsGuStatus.IsDCVerifyExpired(site0Based))
                {
                    return "DC Verify expired";
                }
                if (GU.thisProductsGuStatus.IsCorrExpired(site0Based))
                {
                    return "Corr expired";
                }
                if (GU.thisProductsGuStatus.IsVerifyExpired(site0Based))
                {
                    return "Verify expired";
                }
                return "";
            }
        }

        private bool _runIccChecked;
        private bool _runDCVerifyChecked;
        private bool _runCorrChecked;
        private bool _runVerifyChecked;

        public bool runIccChecked
        {
            get
            {
                return _runIccChecked;
            }
            set
            {
                _runIccChecked = value;
                RaisePropertyChanged("runIccChecked");

                if (value) runCorrChecked = true;
            }
        }

        public bool runDCVerifyChecked
        {
            get
            {
                return _runDCVerifyChecked;
            }
            set
            {
                _runDCVerifyChecked = value;
                RaisePropertyChanged("runDCVerifyChecked");



            }
        }

        public bool runCorrChecked
        {
            get
            {
                return _runCorrChecked;
            }

            set
            {
                _runCorrChecked = value;
                RaisePropertyChanged("runCorrChecked");
                if (value) runVerifyChecked = true;
                else runIccChecked = false;
            }
        }
        public bool runVerifyChecked
        {
            get
            {
                return _runVerifyChecked;
            }
            set
            {
                _runVerifyChecked = value;
                RaisePropertyChanged("runVerifyChecked");
                if (!value)
                {
                    runCorrChecked = false;
                    runIccChecked = false;
                }
            }
        }

        public string lastRunDateInfo
        {
            get
            {
                TimeSpan t = DateTime.Now - GU.thisProductsGuStatus[site0Based].dateOfLastVerifyAttempt;

                return GU.thisProductsGuStatus[site0Based].dateOfLastVerifyAttempt.ToString() + " (" + Math.Floor(t.TotalHours).ToString() + " hours ago)";
            }
        }

        public GuUiSiteData(int site0Based)
        {
            this.site0Based = site0Based;
            InitializeProperties();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InitializeProperties()
        {
            runDCVerifyChecked = dcVerifyIsOptional;
            runIccChecked = false;  // not bound to GU CAL UI anymore
            runCorrChecked = !corrIsOptional;
            runVerifyChecked = !verifyIsOptional;
          //  runVerifyChecked = !(System.IO.Path.GetFileName(GU.correlationFilePath).ToUpper().Replace(".CSV", "") == verifyIsCFFile.ToUpper() ? true : false);

            RaisePropertyChanged("lastRunDateInfo", "status", "dcVerifyIsOptional", "iccIsOptional", "corrIsOptional", "verifyIsOptional");
        }

        public void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class IntPlus1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToInt32(value) + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToInt32(value) - 1;
        }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

}
