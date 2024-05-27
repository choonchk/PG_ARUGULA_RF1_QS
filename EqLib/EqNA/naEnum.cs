using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqLib.NA
{
    public class naEnum
    {
        public enum ESParametersDef
        {
            //@6ports
            S11 = 0, S12, S13, S14, S15, S16,
            S21, S22, S23, S24, S25, S26,
            S31, S32, S33, S34, S35, S36,
            S41, S42, S43, S44, S45, S46,
            S51, S52, S53, S54, S55, S56,
            S61, S62, S63, S64, S65, S66,
            GDEL32, GDEL42, GDEL52, GDEL62, GDEL21, GDEL31, NF
        }
        public enum ESFormat
        {
            MLOG = 0,
            PHAS,
            GDEL,
            SLIN,
            SLOG,
            SCOM,
            SMIT,
            SADM,
            PLIN,
            PLOG,
            POL,
            MLIN,
            SWR,
            REAL,
            IMAG,
            UPH,
            PPH
        }
        public enum EOnOff
        {
            Off = 0,
            On
        }
        public enum EBalunTopology
        {
            NONE,
            SSB
        }
        public enum EModeSetting
        {
            StartStop = 0,
            CenterSpan
        }

        public enum ESweepMode
        {
            Stepped = 0,
            Swept,
            FastStepped,
            FastSwept
        }
        public enum ESweepType
        {
            LIN = 0,
            LOG,
            SEGM,
            POW
        }
        public enum ETriggerSource
        {
            INT = 0,
            EXT,
            MAN,
            BUS
        }
        public enum ETriggerMode
        {
            Imm = 0,
            Single,
            Cont,
            Hold
        }

        public enum ECalibrationStandard
        {
            OPEN = 0,
            LOAD,
            SHORT,
            THRU,
            UTHRU,
            ISOLATION,
            ECAL,
            SUBCLASS,
            TRLLINE,
            TRLREFLECT,
            TRLTHRU,
            UnknownTHRU,
            NS_POWER,
            ECAL_OPEN,
            ECAL_SHORT,
            ECAL_LOAD,
            ECAL_SAVE_A,
            ECAL_SAVE_B
        }
        public enum ECalStdMedia
        {
            COAXIAL = 0,
            WAVEGUIDE
        }
        public enum ECalStdLengthType
        {
            FIXED = 0,
            SLIDING,
            OFFSET
        }

        public enum EFormat
        {
            Norm = 0,
            Swap
        }
        public enum EFormatData
        {
            Asc = 0,
            Real,
            Real32
        }
    }
}
