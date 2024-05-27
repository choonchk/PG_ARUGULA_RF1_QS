namespace LibFBAR_TOPAZ.ANewEqLib
{
    //TODO Redesign.
    public enum e_SParametersDef
    {
        #region disabled and just keeping
        //S11 = 0,
        //S21,
        //S22,
        //S23,
        //S24,
        //S25,
        //S26,
        //S31,
        //S32,
        //S33,
        //S41,
        //S42,
        //S44,
        //S51,
        //S52,
        //S55,
        //S61,
        //S62,
        //S66,
        //GDEL,

        //S11 = 0,
        //S21,
        //S22,
        //S23,
        //S24,
        //S31,
        //S32,
        //S33,
        //S41,
        //S42,
        //S44,
        //GDEL32,
        //GDEL42, 
        #endregion disabled and just keeping

        //@6ports
        S11 = 0, S12, S13, S14, S15, S16,
        S21, S22, S23, S24, S25, S26,
        S31, S32, S33, S34, S35, S36,
        S41, S42, S43, S44, S45, S46,
        S51, S52, S53, S54, S55, S56,
        S61, S62, S63, S64, S65, S66,
        GDEL32, GDEL42, GDEL52, GDEL62, GDEL21, GDEL31

        ////@10ports
        //S11 = 0, S12, S13, S14, S15, S16, S17, S18, S19,
        //S21, S22, S23, S24, S25, S26, S27, S28, S29,
        //S31, S32, S33, S34, S35, S36, S37, S38, S39,
        //S41, S42, S43, S44, S45, S46, S47, S48, S49,
        //S51, S52, S53, S54, S55, S56, S57, S58, S59,
        //S61, S62, S63, S64, S65, S66, S67, S68, S69,
        //S71, S72, S73, S74, S75, S76, S77, S78, S79,
        //S81, S82, S83, S84, S85, S86, S87, S88, S89,
        //S91, S92, S93, S94, S95, S96, S97, S98, S99,
        //GDEL63, GDEL73, GDEL83, GDEL93, GDEL64, GDEL74, GDEL84, GDEL94, GDEL65, GDEL75, GDEL85, GDEL95,
    }
    public enum e_SFormat
    {
        MLOG = 0,
        PHAS,
        GDEL,
        GDEL2,
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
        PPH,
        NF, //Seoul 05112018
        DUTRNPI
    }
    public enum e_Format
    {
        NORM = 0,
        SWAP,
        FAST
    }
    public enum e_FormatData
    {
        ASC = 0,
        REAL,
        REAL32

    }
    public enum e_SNPFormat
    {
        AUTO = 0,
        MA,
        DB,
        RI
    }
    public enum e_SType
    {
        STAT = 0,
        CST,
        DST,
        CDST
    }
    public enum e_OnOff
    {
        Off = 0,
        On
    }
    public enum e_ModeSetting
    {
        StartStop = 0,
        CenterSpan
    }
    public enum e_SweepMode
    {
        Stepped = 0,
        Swept,
        FastStepped,
        FastSwept
    }
    public enum e_SweepGeneration
    {
        STEP = 0,
        ANAL
    }
    public enum e_SweepType
    {
        LIN = 0,
        LOG,
        SEGM,
        POW, //Seoul 05112018
        REIM
    }
    public enum e_TriggerScope
    {
        ALL = 0,
        ACT,
        CURR
    }
    public enum e_TriggerSource
    {
        INT = 0,
        EXT,
        MAN,
        BUS,
        IMM
    }
    public enum e_TriggerMode
    {
        CONT = 0,
        GRO,
        HOLD,
        SING
    }
    public enum e_CalibrationType
    {
        OPEN = 0,
        LOAD,
        SHORT,
        THRU,
        UnknownTHRU,
        ISOLATION,
        ECAL,
        SUBCLASS,
        TRLLINE,
        TRLREFLECT,
        TRLTHRU,
        NS_POWER,
        ECAL_OPEN,
        ECAL_SHORT,
        ECAL_LOAD,
        ECAL_SAVE_A,
        ECAL_SAVE_B
    }
    public enum e_PortMatchType
    {
        NONE = 0,
        SLPC,
        PCSL,
        PLSC,
        SCPL,
        PLPC,
        USER
    }
    public enum e_EDeviceDriver //Seoul 05112018
    {
        AGPM = 0,
        AGESG,
        DCSource,
        DCMeter
    }
    public enum e_TempUnit
    {
        CELS = 0,
        FAHR
    }
    public enum e_NoiseCalMethod
    {
        Vector = 0,
        SParameter,
        Scalar,
        NoiseSource,
        PowerMeter
    }
}
