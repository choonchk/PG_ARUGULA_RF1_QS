namespace SparamTestLib
{
    public struct SResult
    {
        public int TestNumber;
        public bool Enable;
        public string[] Header;
        public double[] Result;
        public int Misc;
    }

    public struct DCSetting
    {
        public double Volts;
        public double Current;
        public bool Test;
        public int Avgs;
        public string iParaName;
    }

    /// <summary>
    /// To be deleted.
    /// </summary>
    public enum ESparamTestCase
    {
        Trigger = 0,
        Freq_At,
        Mag_At,
        Real_At,
        Imag_At,
        Phase_At,
        Impedance_At,
        Mag_Between,
        CPL_Between,
        Ripple_Between,
        Ripple_Between_BW,
        Impendace_Bewteen,
        Bandwidth,
        Balance,
        Channel_Averaging,
        Delta,
        Sum,
        Divide,
        Multiply,
        Relative_Gain
    }

    public enum ESearchDirection
    {
        NONE = 0,
        FROM_LEFT,
        FROM_RIGHT,
        FROM_MAX_LEFT,
        FROM_MAX_RIGHT,
        FROM_EXTREME_LEFT,
        FROM_EXTREME_RIGHT,
    }
    public enum ESearchType
    {
        MIN = 0,
        MAX,
        USER
    }
}
