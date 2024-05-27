using System;
using System.Collections.Generic;
//using System.Linq;
using NationalInstruments.LabVIEW.Interop;
using NationalInstruments.ModularInstruments.Interop;
using System.Runtime.InteropServices;

namespace NationalInstruments.ModularInstruments
{

    public enum ReferenceTriggerOverride
    {
        Ref_Trig_Pass_Through_No_Change = 0,
        Immediate_Ref_Trig_On_Servo_Done = 1,
        Arm_Ref_Trig_On_Servo_Done = 2,
    }

    public class niPowerServo : Object, System.IDisposable
    {
        private LVReferenceNumber _handle;
        private bool _disposed = true;

        #region Constructors


        public niPowerServo(niRFSA RFSAHandle, bool reset)
        {
            string RFSAResourceName;
            RFSAHandle.GetAttributeString("", niRFSAProperties.IoResourceDescriptor, out RFSAResourceName);
            int pInvokeResult = PInvoke.Initialize(RFSAResourceName, System.Convert.ToUInt32(RFSAHandle.Handle.Handle.ToInt32()), out _handle);
            PInvoke.TestForError(pInvokeResult);
            this._disposed = false;

        }

        ~niPowerServo()
        {
            Dispose(false);
        }
        #endregion

        #region DisposeMethods
        /// <summary>
        /// Closes the rfsa session and releases resources associated with that session. 
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed == false)
            {
                //Dispose unmanaged resources
                PInvoke.Close(this._handle);
                this._handle = new LVReferenceNumber(0);// new HandleRef(null, System.IntPtr.Zero);
            }
            // Note disposing has been done.
            this._disposed = true;
        }
        #endregion DisposeMethods

        public int Debug()
        {
            return PInvoke.TestForError(PInvoke.ShowAllFPGAProperties(this._handle));

        }

        public int Enable()
        {
            return PInvoke.TestForError(PInvoke.Enable(this._handle, System.Convert.ToUInt16(true)));
        }

        public int Disable()
        {
            return PInvoke.TestForError(PInvoke.Enable(this._handle, System.Convert.ToUInt16(false)));
        }

        public int Reset()
        {
            return PInvoke.TestForError(PInvoke.Reset(this._handle));
        }

        public int CalculateServoParameters(double DesiredPAOutputPower, double EstimatedPAGaindB, double GainAccuracydB, double WaveformHeadroom, out double VSAReferenceLeveldBm, out double VSGAveragePower)
        {
         
         return PInvoke.TestForError(PInvoke.CalculateServoParameters(this._handle, DesiredPAOutputPower, EstimatedPAGaindB, GainAccuracydB, WaveformHeadroom, out VSAReferenceLeveldBm,out VSGAveragePower));
            
        }

        public int ConfigureActiveServoWindow(bool BurstedWaveform, double BurstDuration)
        {
            return PInvoke.TestForError(PInvoke.ConfigureActiveServoWindow(this._handle, System.Convert.ToUInt16(BurstedWaveform), BurstDuration));
        }

        public int DigitalGainStepLimitEnabled(bool GainStepLimitEnabled)
        {
            return PInvoke.TestForError(PInvoke.DigitalGainStepLimitEnabled(this._handle, System.Convert.ToUInt16(GainStepLimitEnabled)));
        }

        public int Setup(double DesiredAccuracydB, double InitialAveragingTimeS, double FinalAveragingTimeS, ushort MinumumNumberofAverages, ushort NumStepsUntilError, bool Continuous, double ContinuousStepSize)
        {
            return PInvoke.TestForError(PInvoke.Setup(this._handle, DesiredAccuracydB, InitialAveragingTimeS, FinalAveragingTimeS, MinumumNumberofAverages, NumStepsUntilError, System.Convert.ToUInt16(Continuous), ContinuousStepSize));
        }

        public int Start()
        {
            return PInvoke.TestForError(PInvoke.Start(this._handle));
        }

        public int ConfigureVSAReferenceTriggerOverride(ReferenceTriggerOverride RefTriggerOverride)
        {
            return PInvoke.TestForError(PInvoke.ConfigureVSAReferenceTriggerOverride(this._handle, System.Convert.ToUInt16(RefTriggerOverride)));
        }

        public int Wait(double timeout, out ushort NumberAveragesCaptured, out bool Done, out bool Failed)
        {
            UInt16 tempDone;
            UInt16 tempFailed;
            int ErrorCode;
            ErrorCode = PInvoke.TestForError(PInvoke.Wait(this._handle, timeout, out NumberAveragesCaptured, out tempDone, out tempFailed));
            Done = System.Convert.ToBoolean(tempDone);
            Failed = System.Convert.ToBoolean(tempFailed);
            return ErrorCode;
        }

        public int GetDigitalGain(out double RawDigitalGain, out double DigitalGaindB)
        {
            return PInvoke.TestForError(PInvoke.GetDigitalGain(this._handle, out RawDigitalGain, out DigitalGaindB));
        }

        public int ResetDigitalGainOnFailureEnabled(bool ResetDigitalGainOnFailure)
        {
            return PInvoke.TestForError(PInvoke.ResetDigitalGainOnFailureEnabled(this._handle, ResetDigitalGainOnFailure));

        }
        public int FailServoOnDigitalSaturationEnabled(bool FailServoOnDigitalSaturation)
        {
            return PInvoke.TestForError(PInvoke.FailServoOnDigitalSaturationEnabled(this._handle, FailServoOnDigitalSaturation));
        }

        public int GetServoSteps(ushort NumberAveragesCaptured, bool Continuous, bool CaptureAfterServoing, double ExtraTime, out double[] MeasurementTimes, out double[] MeasurementLevels)
        {
            MeasurementTimes = new double[NumberAveragesCaptured];
            MeasurementLevels = new double[NumberAveragesCaptured];

            return PInvoke.TestForError(PInvoke.GetServoSteps(this._handle, System.Convert.ToUInt16(Continuous), System.Convert.ToUInt16(CaptureAfterServoing), ExtraTime, MeasurementTimes, MeasurementLevels, NumberAveragesCaptured));
        }

        public int Close()
        {
            return PInvoke.TestForError(PInvoke.Close(this._handle));
        }

        public static int ResetFPGA(string VSTResource, string bitfilePath, bool ResetFPGA)
        {
            return PInvoke.ResetFPGA(VSTResource,bitfilePath,System.Convert.ToUInt16(ResetFPGA));

        }

       
        #region PInvoke
        private class PInvoke
        {

            private const string PowerServoModuleName64 = @"C:\Program Files\IVI Foundation\IVI\Bin\niVSTPowerServo_64.dll";
            private const string PowerServoModuleName32 = @"C:\Program Files (x86)\IVI Foundation\IVI\Bin\niVSTPowerServo_32.dll";

            // Define the readonly field to check for process' bitness.
            private static readonly bool Is64BitProcess = (UIntPtr.Size == 8);

            #region Initialize
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Initialize", CallingConvention = CallingConvention.StdCall)]
            public static extern int Initialize64(string VSTResourceName, UInt32 RFSAHandle,  out LVReferenceNumber Handle);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Initialize", CallingConvention = CallingConvention.StdCall)]
            public static extern int Initialize32(string VSTResourceName, UInt32 RFSAHandle,  out LVReferenceNumber Handle);

            public static int Initialize(string VSTResourceName, UInt32 RFSAHandle,  out LVReferenceNumber Handle)
            {

                if (Is64BitProcess)
                {
                    int toReturn = Initialize64(VSTResourceName, RFSAHandle, out Handle);
                    //Handle = new UIntPtr(tempHandle);
                    return toReturn;
                }
                else
                {
                    // UInt32 tempHandle;
                    int toReturn = Initialize32(VSTResourceName, RFSAHandle, out Handle);
                    // Handle = new UIntPtr(tempHandle);
                    return toReturn;
                }
            }
            #endregion

            #region Enable
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Enable", CallingConvention = CallingConvention.StdCall)]
            public static extern int Enable64(ref LVReferenceNumber Handle, ushort Enable);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Enable", CallingConvention = CallingConvention.StdCall)]
            public static extern int Enable32(ref LVReferenceNumber Handle, ushort Enable);

            public static int Enable(LVReferenceNumber Handle, ushort EnableBool)
            {
                if (Is64BitProcess)
                    return Enable64(ref Handle, EnableBool);
                else
                    return Enable32(ref Handle, EnableBool);
            }
            #endregion

            #region CalculateServoParameters
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_CalculateServoParameters", CallingConvention = CallingConvention.StdCall)]
            public static extern void CalculateServoParameters64(ref LVReferenceNumber Handle, double MeasurementHeadroomDB, double GainAccuracyDB, double EstimatedPAGainDB,
                                                                             double DesiredPAOutputPowerDBm, out double VSGAveragePowerDBm, out double ReferenceLevelDBm);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_CalculateServoParameters", CallingConvention = CallingConvention.StdCall)]
            public static extern void CalculateServoParameters32(ref LVReferenceNumber Handle, double MeasurementHeadroomDB, double GainAccuracyDB, double EstimatedPAGainDB,
                                                                             double DesiredPAOutputPowerDBm, out double VSGAveragePowerDBm, out double ReferenceLevelDBm);
            public static int CalculateServoParameters(LVReferenceNumber Handle, double DesiredPAOutputPower, double EstimatedPAGaindB, double GainAccuracydB, double WaveformHeadroom, out double VSAReferenceLeveldBm, out double VSGAveragePower)
            {
                if (Is64BitProcess)
                    CalculateServoParameters64(ref Handle, WaveformHeadroom, GainAccuracydB, EstimatedPAGaindB, DesiredPAOutputPower, out VSGAveragePower, out VSAReferenceLeveldBm);
                else
                    CalculateServoParameters32(ref Handle, WaveformHeadroom, GainAccuracydB, EstimatedPAGaindB, DesiredPAOutputPower, out VSGAveragePower, out VSAReferenceLeveldBm);
                return 0;
            }
            #endregion

            #region Reset
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int Reset64(ref LVReferenceNumber Handle);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int Reset32(ref LVReferenceNumber Handle);
            public static int Reset(LVReferenceNumber Handle)
            {
                if (Is64BitProcess)
                    return Reset64(ref Handle);
                else
                    return Reset32(ref Handle);
            }
            #endregion

            #region ShowAllFPGAProperties
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Debug", CallingConvention = CallingConvention.StdCall)]
            public static extern int ShowAllFPGAProperties64(ref LVReferenceNumber Handle);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Debug", CallingConvention = CallingConvention.StdCall)]
            public static extern int ShowAllFPGAProperties32(ref LVReferenceNumber Handle);
            public static int ShowAllFPGAProperties(LVReferenceNumber Handle)
            {
                if (Is64BitProcess)
                    return ShowAllFPGAProperties64(ref Handle);
                else
                    return ShowAllFPGAProperties32(ref Handle);
            }
            #endregion

            #region FailServoOnDigitalSaturationEnabled

            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_FailServoOnDigitalSaturationEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int FailServoOnDigitalSaturationEnabled64(ref LVReferenceNumber Handle, bool FailServoOnDigitalSaturation);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_FailServoOnDigitalSaturationEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int FailServoOnDigitalSaturationEnabled32(ref LVReferenceNumber Handle, bool FailServoOnDigitalSaturation);

            public static int FailServoOnDigitalSaturationEnabled(LVReferenceNumber Handle, bool FailServoOnDigitalSaturation)
            {
                if (Is64BitProcess)
                    return FailServoOnDigitalSaturationEnabled64(ref Handle, FailServoOnDigitalSaturation);
                else
                    return FailServoOnDigitalSaturationEnabled32(ref Handle, FailServoOnDigitalSaturation);
            }

            #endregion

            #region ResetDigitalGainOnFailureEnabled
            
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_ResetDigitalGainOnFailureEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetDigitalGainOnFailureEnabled64(ref LVReferenceNumber Handle, bool ResetDigitalGainOnFailure);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_ResetDigitalGainOnFailureEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetDigitalGainOnFailureEnabled32(ref LVReferenceNumber Handle, bool ResetDigitalGainOnFailure);
            public static int ResetDigitalGainOnFailureEnabled(LVReferenceNumber Handle, bool ResetDigitalGainOnFailure)
            {
                if (Is64BitProcess)
                    return ResetDigitalGainOnFailureEnabled64(ref Handle, ResetDigitalGainOnFailure);
                else
                    return ResetDigitalGainOnFailureEnabled32(ref Handle, ResetDigitalGainOnFailure);
            }
            #endregion

            #region ResetFPGA
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_ResetFPGA", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetFPGA64(string VSTResource, string bitfilePath, ushort ResetFPGA);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_ResetFPGA", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetFPGA32(string VSTResource, string bitfilePath, ushort ResetFPGA);
            public static int ResetFPGA(string VSTResource, string bitfilePath, ushort ResetFPGA)
            {
                if (Is64BitProcess)
                    return ResetFPGA64(VSTResource, bitfilePath, ResetFPGA);
                else
                    return ResetFPGA32(VSTResource,bitfilePath, ResetFPGA);
            }
            #endregion

            #region DigitalGainStepLimitEnabled
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_DigitalGainStepLimitEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int DigitalGainStepLimitEnabled64(ref LVReferenceNumber Handle, ushort GainStepLimitEnabled);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_DigitalGainStepLimitEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int DigitalGainStepLimitEnabled32(ref LVReferenceNumber Handle, ushort GainStepLimitEnabled);
            public static int DigitalGainStepLimitEnabled(LVReferenceNumber Handle, UInt16 GainStepLimitEnabled)
            {
                if (Is64BitProcess)
                    return DigitalGainStepLimitEnabled64(ref Handle, GainStepLimitEnabled);
                else
                    return DigitalGainStepLimitEnabled32(ref Handle, GainStepLimitEnabled);
            }
            #endregion

            #region Setup
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Setup", CallingConvention = CallingConvention.StdCall)]
            public static extern int Setup64(ref LVReferenceNumber Handle, double DesiredAccuracyDB, double InitialAveragingTimeS, double FinalAveragingTimeS, UInt16 MinumumNumberOfAverages, UInt16 NumStepsUntilError,
                                            UInt16 continuous, double ContinuousStepSize);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Setup", CallingConvention = CallingConvention.StdCall)]
            public static extern int Setup32(ref LVReferenceNumber Handle, double DesiredAccuracyDB, double InitialAveragingTimeS, double FinalAveragingTimeS, UInt16 MinumumNumberOfAverages, UInt16 NumStepsUntilError,
                                            UInt16 continuous, double ContinuousStepSize);

            public static int Setup(LVReferenceNumber Handle, double DesiredAccuracydB,double InitialAveragingTimeS, double FinalAveragingTimeS, ushort MinumumNumberofAverages, ushort NumStepsUntilError, UInt16 continuous, double ContinuousStepSize)
            {
                if (Is64BitProcess)
                    return Setup64(ref Handle, DesiredAccuracydB, InitialAveragingTimeS, FinalAveragingTimeS, MinumumNumberofAverages, NumStepsUntilError, continuous, ContinuousStepSize);

                else
                    return Setup32(ref Handle, DesiredAccuracydB, InitialAveragingTimeS, FinalAveragingTimeS, MinumumNumberofAverages, NumStepsUntilError, continuous, ContinuousStepSize);
            }
            #endregion

            #region Start
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Start", CallingConvention = CallingConvention.StdCall)]
            public static extern int Start64(ref LVReferenceNumber Handle);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Start", CallingConvention = CallingConvention.StdCall)]
            public static extern int Start32(ref LVReferenceNumber Handle);
            public static int Start(LVReferenceNumber Handle)
            {
                if (Is64BitProcess)
                    return Start64(ref Handle);
                else
                    return Start32(ref Handle);
            }
            #endregion

            #region Wait
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Wait", CallingConvention = CallingConvention.StdCall)]
            public static extern int Wait64(ref LVReferenceNumber Handle, double timeout, out UInt16 Done, out UInt16 Failed, out UInt16 NumberAveragesCaptured);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Wait", CallingConvention = CallingConvention.StdCall)]
            public static extern int Wait32(ref LVReferenceNumber Handle, double timeout, out UInt16 Done, out UInt16 Failed, out UInt16 NumberAveragesCaptured);
            public static int Wait(LVReferenceNumber Handle, double timeout, out ushort NumberAveragesCaptured, out UInt16 Done, out UInt16 Failed)
            {
                if (Is64BitProcess)
                    return Wait64(ref Handle, timeout, out Done, out Failed, out NumberAveragesCaptured);
                else
                    return Wait32(ref Handle, timeout, out Done, out Failed, out NumberAveragesCaptured);
            }
            #endregion

            #region ConfigureVSAReferenceTriggerOverride
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_ConfigureVSAReferenceTriggerOverride", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureVSAReferenceTriggerOverride64(ref LVReferenceNumber Handle, UInt16 RefTrigOverride);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_ConfigureVSAReferenceTriggerOverride", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureVSAReferenceTriggerOverride32(ref LVReferenceNumber Handle, UInt16 RefTrigOverride);
            public static int ConfigureVSAReferenceTriggerOverride(LVReferenceNumber Handle, UInt16 RefTrigOverride)
            {
                if (Is64BitProcess)
                    return ConfigureVSAReferenceTriggerOverride64(ref Handle, RefTrigOverride);
                else
                    return ConfigureVSAReferenceTriggerOverride32(ref Handle, RefTrigOverride);
            }
            #endregion

            #region ConfigureActiveServoWindow
            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_ConfigureActiveServoWindow", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureActiveServoWindow64(ref LVReferenceNumber Handle, UInt16 BurstedWaveform, double waveformBurstTime);

            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_ConfigureActiveServoWindow", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureActiveServoWindow32(ref LVReferenceNumber Handle, UInt16 BurstedWaveform, double waveformBurstTime);

            public static int ConfigureActiveServoWindow(LVReferenceNumber Handle, UInt16 BurstedWaveform, double BurstDuration)
            {
                if (Is64BitProcess)
                    return ConfigureActiveServoWindow64(ref Handle, BurstedWaveform, BurstDuration);
                else
                    return ConfigureActiveServoWindow32(ref Handle, BurstedWaveform, BurstDuration);
            }
            #endregion

            #region GetServoSteps


            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_GetServoSteps", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetServoSteps64(ref LVReferenceNumber Handle, ushort Continuous, double ExtraTime, ushort CaptureAfterServoing, [Out] double[] MeasurementLevels, [Out] double[] MeasurementTimes, int MeasurementTimesLength);

            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_GetServoSteps", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetServoSteps32(ref LVReferenceNumber Handle, ushort Continuous, double ExtraTime, ushort CaptureAfterServoing, [Out] double[] MeasurementLevels, [Out] double[] MeasurementTimes, int MeasurementTimesLength);

            public static int GetServoSteps(LVReferenceNumber Handle, UInt16 Continuous, UInt16 CaptureAfterServoing, double ExtraTime, [Out] double[] MeasurementTimes, [Out] double[] MeasurementLevels, ushort NumberAveragesCaptured)
            {
                if (Is64BitProcess)
                    return GetServoSteps64(ref Handle, Continuous, ExtraTime, CaptureAfterServoing, MeasurementLevels, MeasurementTimes, System.Convert.ToInt32(NumberAveragesCaptured));
                else
                    return GetServoSteps32(ref Handle, Continuous, ExtraTime, CaptureAfterServoing, MeasurementLevels, MeasurementTimes, System.Convert.ToInt32(NumberAveragesCaptured));
            }
            #endregion

            #region GetDigitalGain

            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_GetDigitalGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetDigitalGain64(ref LVReferenceNumber Handle, out double DigitalGaindB, out double RawDigitalGain);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_GetDigitalGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetDigitalGain32(ref LVReferenceNumber Handle, out double DigitalGaindB, out double RawDigitalGain);

            public static int GetDigitalGain(LVReferenceNumber Handle, out double RawDigitalGain, out double DigitalGaindB)
            {
                if (Is64BitProcess)
                    return GetDigitalGain64(ref Handle, out DigitalGaindB, out RawDigitalGain);
                else
                    return GetDigitalGain32(ref Handle, out DigitalGaindB, out RawDigitalGain);
            }
            #endregion

            #region CheckStatus

            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_CheckStatus", CallingConvention = CallingConvention.StdCall)]
            public static extern int CheckStatus64(ref LVReferenceNumber Handle, ushort FailOnPeakPower, out ushort Done, out ushort Failed, out ushort NumberAveragesCaptured);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_CheckStatus", CallingConvention = CallingConvention.StdCall)]
            public static extern int CheckStatus32(ref LVReferenceNumber Handle, ushort FailOnPeakPower, out ushort Done, out ushort Failed, out ushort NumberAveragesCaptured);

            public static int CheckStatus(LVReferenceNumber Handle, ushort FailOnPeakPower, out ushort Done, out ushort Failed, out ushort NumberAveragesCaptured)
            {
                if (Is64BitProcess)
                    return CheckStatus64(ref Handle, FailOnPeakPower, out Done, out Failed, out NumberAveragesCaptured);
                else
                    return CheckStatus32(ref Handle, FailOnPeakPower, out Done, out Failed, out NumberAveragesCaptured);

            }

            #endregion

            #region Close

            [DllImport(PowerServoModuleName64, EntryPoint = "niVST_PwrServo_Close", CallingConvention = CallingConvention.StdCall)]
            public static extern int Close64(ref LVReferenceNumber Handle);
            [DllImport(PowerServoModuleName32, EntryPoint = "niVST_PwrServo_Close", CallingConvention = CallingConvention.StdCall)]
            public static extern int Close32(ref LVReferenceNumber Handle);

            public static int Close(LVReferenceNumber Handle)
            {
                if (Is64BitProcess)
                    return Close64(ref Handle);
                else
                    return Close32(ref Handle);

            }

            #endregion Close

            #region ErrorMethods
            public static int TestForError(int status)
            {
                if ((status < 0))
                {
                    ThrowError(status);
                }
                return status;
            }

            public static int ThrowError(int code)
            {
                /*   int size = PInvoke.GetError(Handle, out code, 0, null);
                   System.Text.StringBuilder msg = new System.Text.StringBuilder();
                   if ((size >= 0))
                   {
                       msg.Capacity = size;
                       PInvoke.GetError(Handle, out code, size, msg);
                   }
                */
                throw new System.Runtime.InteropServices.ExternalException("Error in Servo Code", code);
            }

            #endregion ErrorMethods

        }
        #endregion PInvoke
    }

}