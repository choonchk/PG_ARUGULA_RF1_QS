using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace EqLib
{
    public partial class EqHSDIO
    {
        public class None : EqHSDIObase
        {
            public override bool Initialize()
            {
                return true;
            }

            public override bool ReInitializeVIO(double violevel)
            {
                return true;
            }

            public override string GetInstrumentInfo()
            {
                return "";
            }

            public override void shmoo(string nameInMemory)
            {
                return;
            }

            public override bool LoadVector(List<string> fullPaths, string nameInMemory)
            {
                return true;
            }

            public override bool LoadVector_MipiHiZ()
            {
                return true;
            }

            public override bool LoadVector_MipiReset()
            {
                return true;
            }

            public override bool LoadVector_MipiVioOff()
            {
                return true;
            }


            public override bool LoadVector_MipiVioOn()
            {
                return true;
            }

            public override bool LoadVector_MipiRegIO()
            {
                return true;
            }

            public override bool LoadVector_EEPROM()
            {
                return true;
            }
            //public override bool LoadVector_RFOnOffTest(bool isNRZ = false)    OBS
            //{
            //    return true;
            //}

            public override bool LoadVector_RFOnOffTestRx(bool isNRZ = false) //Rx Trigger
            {
                return true;
            }


            public override bool LoadVector_RFOnOffSwitchTest(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest_WithPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest_With3TxPreMipi(bool isNRZ = false)
            {
                return true;
            }


            public override bool LoadVector_RFOnOffSwitchTestRx(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTestRx_WithPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTestRx_With1Tx2RxPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2_WithPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2_With1Tx2RxPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2Rx(bool isNRZ = false)
            {
                return true;
            }

            public override bool SendRFOnOffTestVector(bool flag, string[] SwTimeCustomArry)
            {
                throw new NotImplementedException();
            }

            public override bool LoadVector_RFOnOffTest(bool isNRZ = false) { return true; }

            public override bool SendVector(string nameInMemory)
            {
                return true;
            }
            public override bool Burn(string data_hex, bool invertData = false, int efuseDataByteNum = 0, string efuseCtlAddress = "C0")
            {

                return true;
            }
            public override bool SendVectorOTP(string TargetData, string CurrentData = "00", bool isEfuseBurn = false)
            {
                return true;
            }
            public override void SendNextVectors(bool firstTest, List<string> MipiWaveformNames)
            {
            }
            public override void SendMipiCommands(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands, eMipiTestType _eMipiTestType = eMipiTestType.Write)
            {
                throw new NotImplementedException();
            }

            public override void SetSourceWaveformArry(string customMIPIlist)
            {
                throw new NotImplementedException();
            }
            public override void AddVectorsToScript(List<string> namesInMemory, bool finalizeScript)
            {
            }

            public override int GetNumExecErrors(string nameInMemory)
            {
                return 0;
            }

            //public override int InterpretPID(string nameInMemory)
            //{
            //    return 0;
            //}

            //public override double InterpretTempSense(string nameInMemory)
            //{
            //    return 0;
            //}

            public override void RegWriteMultiple(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
            {

            }
            public override void RegWrite(string registerAddress_hex, string data_hex, bool sendTrigger = false)
            {
            }

            public override string RegRead(string registerAddress_hex, bool writeHalf = false)
            {
                return "";
            }

            public override void EepromWrite(string dataWrite)
            {
            }

            public override string EepromRead()
            {
                return "";
            }

            public override bool LoadVector_UNIO_EEPROM()
            {
                return false;
            }

            public override bool UNIO_EEPROMWriteID(UNIO_EEPROMType device, string dataWrite, int bus_no = 1)
            {
                return false;
            }

            public override bool UNIO_EEPROMWriteCounter(UNIO_EEPROMType device, uint count, int bus_no = 1)
            {
                return false;
            }

            public override bool UNIO_EEPROMFreeze(UNIO_EEPROMType device, int bus_no = 1)
            {
                return false;
            }

            public override string UNIO_EEPROMReadID(UNIO_EEPROMType device, int bus_no = 1)
            {
                return "";
            }

            public override uint UNIO_EEPROMReadCounter(UNIO_EEPROMType device, int bus_no = 1)
            {
                return 0;
            }

            public override string UNIO_EEPROMReadSerialNumber(UNIO_EEPROMType device, int bus_no = 1)
            {
                return "";
            }

            public override string UNIO_EEPROMReadMID(UNIO_EEPROMType device, int bus_no = 1)
            {
                return "";
            }

            public override void Close()
            {
            }

            public override bool LoadVector_TEMPSENSEI2C(decimal TempSensorAddress)
            {
                return false;
            }

            public override void SendTRIGVectors()
            {
            }

            public override double I2CTEMPSENSERead()
            {
                return 0;
            }

            //public override bool LoadVector_TEMPSENSEI2C(decimal TempSensorAddress);
            //{
            //return false;
            //}

            public override TriggerLine TriggerOut { get; set; }
        }

    }

}