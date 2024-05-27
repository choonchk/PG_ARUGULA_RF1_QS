using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NationalInstruments.DAQmx;

namespace ClothoLibStandard
{
    public class SwitchMatrix
    {
        private ArrayList TaskList;

        private Task digitalWriteTaskP00;
        private Task digitalWriteTaskP01;
        private Task digitalWriteTaskP02;
        private Task digitalWriteTaskP03;
        private Task digitalWriteTaskP04;
        private Task digitalWriteTaskP05;
        
        private Task digitalWriteTaskP09;
        
        // private DigitalSingleChannelWriter[] writerPort;

        private DigitalSingleChannelWriter writerP00;
        private DigitalSingleChannelWriter writerP01;
        private DigitalSingleChannelWriter writerP02;
        private DigitalSingleChannelWriter writerP03;
        private DigitalSingleChannelWriter writerP04;
        private DigitalSingleChannelWriter writerP05;
        
        private DigitalSingleChannelWriter writerP09;

        
        private int[] ChannelValue = new int[12];
        
        
        public int Initiazile()
        {
            try
            {
                //digitalWriteTaskPort = new Task();
                //TaskList = new ArrayList();

                

                //for (int i = 0; i < 12; i++)
                //{
                //    TaskList.Add(digitalWriteTaskPort);

                //    //digitalWriteTaskP10.DOChannels.CreateChannel("DIO/port10", "port10",
                //    //                ChannelLineGrouping.OneChannelForAllLines);
                //    string tempString01 = "DIO/port" + i.ToString();
                //    string tempString02 = "port" + i.ToString();
                //    digitalWriteTaskPort.DOChannels.CreateChannel(tempString01, tempString02, ChannelLineGrouping.OneChannelForAllLines);

                //    //writerP04 = new DigitalSingleChannelWriter(digitalWriteTaskP04.Stream);                    
                //    writerPort[i] = new DigitalSingleChannelWriter(digitalWriteTaskPort.Stream);
                //}               

                digitalWriteTaskP00 = new Task();
                digitalWriteTaskP01 = new Task();
                digitalWriteTaskP02 = new Task();
                digitalWriteTaskP03 = new Task();
                digitalWriteTaskP04 = new Task();
                digitalWriteTaskP05 = new Task();
                
                digitalWriteTaskP09 = new Task();
                
                digitalWriteTaskP00.DOChannels.CreateChannel("DIO/port0", "port0",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP01.DOChannels.CreateChannel("DIO/port1", "port1",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP02.DOChannels.CreateChannel("DIO/port2", "port2",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP03.DOChannels.CreateChannel("DIO/port3", "port3",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP04.DOChannels.CreateChannel("DIO/port4", "port4",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP05.DOChannels.CreateChannel("DIO/port5", "port5",
                                ChannelLineGrouping.OneChannelForAllLines);
                
                digitalWriteTaskP09.DOChannels.CreateChannel("DIO/port9", "port9",
                                ChannelLineGrouping.OneChannelForAllLines);
                
                writerP00 = new DigitalSingleChannelWriter(digitalWriteTaskP00.Stream);
                writerP01 = new DigitalSingleChannelWriter(digitalWriteTaskP01.Stream);
                writerP02 = new DigitalSingleChannelWriter(digitalWriteTaskP02.Stream);
                writerP03 = new DigitalSingleChannelWriter(digitalWriteTaskP03.Stream);
                writerP04 = new DigitalSingleChannelWriter(digitalWriteTaskP04.Stream);
                writerP05 = new DigitalSingleChannelWriter(digitalWriteTaskP05.Stream);
                                
                writerP09 = new DigitalSingleChannelWriter(digitalWriteTaskP09.Stream);


                writerP00.WriteSingleSamplePort(true, 0);
                writerP01.WriteSingleSamplePort(true, 0);
                writerP02.WriteSingleSamplePort(true, 0);
                writerP03.WriteSingleSamplePort(true, 0);
                writerP04.WriteSingleSamplePort(true, 0);
                writerP05.WriteSingleSamplePort(true, 0);
                writerP09.WriteSingleSamplePort(true, 0);        


                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Initialize");
                return -1;
            }
        }

        public int SetPortPath(string PortPath)
        {
            try
            {

                // string[] SwitchNoAndStatus = PortPath.ToString().Split(' ');
                string[] SwitchNoAndStatus = PortPath.Split(' ');

                int NoOfSwitch = SwitchNoAndStatus.Length;
                int[] SwitchNo = new int[NoOfSwitch];
                int[] SwitchStatus = new int[NoOfSwitch];

                string[] tempSwitchNoAndStatus = new string[2];


                // Generate arrays for switch no and switch on/off status
                for (int i = 0; i < NoOfSwitch; i++)
                {
                    tempSwitchNoAndStatus = SwitchNoAndStatus[i].Split(':');
                    SwitchNo[i] = Convert.ToInt32(tempSwitchNoAndStatus[0]);
                    SwitchStatus[i] = Convert.ToInt32(tempSwitchNoAndStatus[1]);
                }


                // Clear the channel write need status
                bool[] ChannelWriteNeeded = new bool[12];
                int PortNo = 0;
                int ChNo = 0;

                for (int i = 0; i < NoOfSwitch; i++)
                {
                    if (SwitchNo[i] == 48)
                        PortNo = 9;
                    else
                    {
                        PortNo = Convert.ToInt32( Math.Truncate( SwitchNo[i] / 8d));
                    }

                    ChNo = SwitchNo[i] - PortNo * 8;
                    int tempChValue = Convert.ToInt32(Math.Pow(2, ChNo));
                    int tempBitAnd = (ChannelValue[PortNo] & tempChValue);

                    if ((tempBitAnd == tempChValue) && (SwitchStatus[i] == 1))
                    {
                        // ChannelValue[PortNo] += Convert.ToInt32(Math.Pow(2, ChNo) * SwitchStatus[i]);
                    }
                    else if ((tempBitAnd == tempChValue) && (SwitchStatus[i] == 0))
                    {
                        ChannelValue[PortNo] -= tempChValue;
                        ChannelWriteNeeded[PortNo] = true;
                    }
                    else if ((tempBitAnd != tempChValue) && (SwitchStatus[i] == 1))
                    {
                        ChannelValue[PortNo] += tempChValue;
                        ChannelWriteNeeded[PortNo] = true;
                    }
                    else if ((tempBitAnd != tempChValue) && (SwitchStatus[i] == 0))
                    {

                    }
                    else
                    {
                        MessageBox.Show("Error");
                    }




                    //switch (SwitchNo[i])
                    //{
                    //    case 0:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 0) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    case 1:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 1) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    case 2:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 2) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    case 3:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 3) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    case 4:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 4) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    case 5:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 5) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    case 6:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 6) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    case 7:
                    //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 7) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[0] = true;
                    //        break;

                    //    // Port 1

                    //    case 8:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 0) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;

                    //    case 9:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 1) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;

                    //    case 10:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 2) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;

                    //    case 11:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 3) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;

                    //    case 12:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 4) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;

                    //    case 13:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 5) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;

                    //    case 14:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 6) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;

                    //    case 15:
                    //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 7) * SwitchStatus[i]);
                    //        ChannelWriteNeeded[1] = true;
                    //        break;
                    //}
                }


                // Channel Write                
                if (ChannelWriteNeeded[0])
                    // writerP10.WriteSingleSamplePort(true, portValue);
                    writerP00.WriteSingleSamplePort(true, ChannelValue[0]);
                if (ChannelWriteNeeded[1])
                    // writerP10.WriteSingleSamplePort(true, portValue);
                    writerP01.WriteSingleSamplePort(true, ChannelValue[1]);
                if (ChannelWriteNeeded[2])
                    // writerP10.WriteSingleSamplePort(true, portValue);
                    writerP02.WriteSingleSamplePort(true, ChannelValue[2]);
                if (ChannelWriteNeeded[3])
                    // writerP10.WriteSingleSamplePort(true, portValue);
                    writerP03.WriteSingleSamplePort(true, ChannelValue[3]);
                if (ChannelWriteNeeded[4])
                    // writerP10.WriteSingleSamplePort(true, portValue);
                    writerP04.WriteSingleSamplePort(true, ChannelValue[4]);
                if (ChannelWriteNeeded[5])
                    // writerP10.WriteSingleSamplePort(true, portValue);
                    writerP05.WriteSingleSamplePort(true, ChannelValue[5]);
                if (ChannelWriteNeeded[9])
                    // writerP10.WriteSingleSamplePort(true, portValue);
                    writerP09.WriteSingleSamplePort(true, ChannelValue[9]);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetPortPath");
                return -1;
            }

        }
    }

    public class SwitchMatrixConstants
    {
        public const string VSGtoA1 = "00:0 01:0 03:0 04:0 05:0";
        public const string VSGtoA2 = "00:0 01:0 03:1 04:1 06:0";

        public const string A1toN1 = "04:1 06:0 07:1";
        public const string A2toN1 = "05:1 06:1 07:1";

        public const string A3toN2 = "09:1 11:0 12:1";
        public const string A4toN2 = "10:1 11:1 12:1";

        //public const string VSGtoA5 = "00:0 01:1 12:0 14:0";
        public const string VSGtoA3 = "00:0 01:1 08:0 09:0 10:0 12:1 11:1";
        //public const string VSGtoA6 = "00:0 01:1 12:1 13:0 15:0";
        public const string VSGtoA4 = "00:0 01:1 08:1 09:1 10:0 11:0 12:0";

        //public const string VSGtoA5 = "00:0 01:1 12:0 14:0";
        public const string VSAtoA3 = "08:1 09:1 11:0 12:0 25:1 27:1 28:0 30:0 31:0";
        public const string VSAtoA4 = "08:1 10:1 11:1 12:0 25:1 27:1 28:0 30:0 31:0";

        //public const string VSAtoA9 = "44:0 42:0 43:0 41:0 40:0 29:1 28:0 27:0 23:1";
        public const string VSAtoA5 = "13:1 14:1 15:1 16:0 17:0 18:0 26:0 27:0 28:0 29:0 30:0 31:0";
        //public const string VSAtoA10 = "44:0 42:0 43:0 41:0 40:0 29:1 28:0 27:1 24:1";
        public const string VSAtoA6 = "13:1 14:1 15:0 16:1 17:1 18:0 26:0 27:0 28:0 29:0 30:0 31:0";

        public const string A5toN3 = "15:1 17:0 18:1";
        public const string A6toN3 = "16:1 17:1 18:1";

        public const string A7toN4 = "20:1 22:0 23:1";
        public const string A8toN4 = "20:1 22:1 23:1";

        //public const string VSAtoA13 = "44:0 42:0 43:0 41:0 40:1 38:1 37:0 36:0 32:1";
        public const string VSAtoA7 = "19:1 20:1 21:0 22:0 23:0 24:1 25:0 26:1 27:0 28:0 29:0 30:0 31:0";
        //public const string VSAtoA14 = "44:0 42:0 43:0 41:0 40:1 38:1 37:0 36:1 33:1";
        public const string VSAtoA8 = "19:0 20:0 21:1 22:1 23:0 24:1 25:0 26:1 27:0 28:0 29:0 30:0 31:0";
        //public const string VSAtoA15 = "44:0 42:0 43:0 41:0 40:1 38:0 35:1 34:0 48:0";
        public const string VSAtoR = "20:0 21:0 22:0 23:1 24:0 25:0 26:1 27:0 28:0 29:0 30:0 31:0";
        public const string VSAtoR_3499 = "OPEN ALL; OPEN (@100)";

        //public const string VSGtoA5 = "00:0 01:1 12:0 14:0";
        public const string VSAtoA3filter = "08:1 09:1 11:0 12:0 25:1 27:1 28:1 29:1 30:0 31:1";
        public const string VSAtoA4filter = "08:1 10:1 11:1 12:0 25:1 27:1 28:1 29:1 30:0 31:1";

        public const string VSAtoA5filter = "13:1 14:1 15:1 16:0 17:0 18:0 25:1 26:0 27:0 28:1 29:1 30:0 31:1";
        public const string VSAtoA6filter = "13:1 14:1 15:0 16:1 17:1 18:0 25:1 26:0 27:0 28:1 29:1 30:0 31:1";

        public const string VSAtoA7filter = "20:1 21:0 22:0 23:0 24:1 25:0 26:1 27:0 28:1 29:1 30:0 31:1";
        public const string VSAtoA8filter = "20:0 21:1 22:1 23:0 24:1 25:0 26:1 27:0 28:1 29:1 30:0 31:1";
        //public const string VSAtoA15filter = "44:1 42:1 43:0 41:0 40:1 38:0 35:1 34:0 48:0";


        // dont' need  public const string VSAtoA5filterPS = "13:1 14:1 15:1 16:0 17:0 18:0 25:1 26:0 27:0 28:0 29:0 30:1 31:0";
        public const string VSAtoA6_6GHzHPFtoPS = "13:1 14:1 15:0 16:1 17:1 18:0 25:1 26:0 27:0 28:0 29:0 30:1 31:1 32:0 33:0";  // B34
        public const string VSAtoA7_7GHzHPFtoPS = "20:1 21:0 22:0 23:0 24:1 25:0 26:1 27:0 28:0 29:0 30:1 31:1 32:1 33:1";  // B38
        public const string VSAtoA8_7GHzHPFtoPS = "20:0 21:1 22:1 23:0 24:1 25:0 26:1 27:0 28:0 29:0 30:1 31:1 32:1 33:1";  // B40

        public const string VSGtoA1nVSAtoA5 = "00:0 01:0 03:0 04:0 05:0 13:1 14:1 15:1 16:0 17:0 18:0 25:1 26:0 27:0 28:0 29:0 30:0 31:0";
        public const string VSGtoA2nVSAtoA6 = "00:0 01:0 03:1 04:1 06:0 13:1 14:1 15:0 16:1 17:1 18:0 25:1 26:0 27:0 28:0 29:0 30:0 31:0";
        public const string VSGtoA1nVSAtoA7 = "00:0 01:0 03:0 04:0 05:0 20:1 21:0 22:0 23:0 24:1 25:0 26:1 27:0 28:0 29:0 30:0 31:0";
        public const string VSGtoA2nVSAtoA8 = "00:0 01:0 03:1 04:1 06:0 20:0 21:1 22:1 23:0 24:1 25:0 26:1 27:0 28:0 29:0 30:0 31:0";

    }
}
