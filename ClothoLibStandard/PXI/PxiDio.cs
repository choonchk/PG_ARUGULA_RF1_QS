using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.DAQmx;

namespace ClothoLibStandard
{
    public class PxiDio
    {
        private Task digitalWriteTaskP03;
        private Task digitalWriteTaskP04;
        private Task digitalWriteTaskP05;
        private Task digitalWriteTaskP09;
        private Task digitalWriteTaskP10;
        private DigitalSingleChannelWriter writerP03;
        private DigitalSingleChannelWriter writerP04;
        private DigitalSingleChannelWriter writerP05;
        private DigitalSingleChannelWriter writerP09;
        private DigitalSingleChannelWriter writerP10;

        public int Initiazile()
        {
            try
            {
                digitalWriteTaskP03 = new Task();
                digitalWriteTaskP04 = new Task();
                digitalWriteTaskP05 = new Task();
                digitalWriteTaskP09 = new Task();
                digitalWriteTaskP10 = new Task();

                digitalWriteTaskP03.DOChannels.CreateChannel("DIO/port3", "port3",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP04.DOChannels.CreateChannel("DIO/port4", "port4",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP05.DOChannels.CreateChannel("DIO/port5", "port5",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP09.DOChannels.CreateChannel("DIO/port9", "port9",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP10.DOChannels.CreateChannel("DIO/port10", "port10",
                                ChannelLineGrouping.OneChannelForAllLines);

                writerP03 = new DigitalSingleChannelWriter(digitalWriteTaskP03.Stream);
                writerP04 = new DigitalSingleChannelWriter(digitalWriteTaskP04.Stream);
                writerP05 = new DigitalSingleChannelWriter(digitalWriteTaskP05.Stream);
                writerP09 = new DigitalSingleChannelWriter(digitalWriteTaskP09.Stream);
                writerP10 = new DigitalSingleChannelWriter(digitalWriteTaskP10.Stream);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Initialize");
                return -1;
            }
        }

        public int SetPortValue(int portNumber, int portValue)
        {
            try
            {
                if (portNumber == 3)
                    writerP03.WriteSingleSamplePort(true, portValue);
                else if (portNumber == 4)
                    writerP04.WriteSingleSamplePort(true, portValue);
                else if (portNumber == 5)
                    writerP05.WriteSingleSamplePort(true, portValue);
                else if (portNumber == 9)
                    writerP09.WriteSingleSamplePort(true, portValue);
                else if (portNumber == 10)
                    writerP10.WriteSingleSamplePort(true, portValue);
                else
                {
                    MessageBox.Show("The port is not supported", "SetDioPortValue");
                    return -1;
                }
                
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetDioPortValue");
                return -1;
            }
        }
    }
}
