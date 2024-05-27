namespace LibFBAR_TOPAZ.ANewEqLib
{
    public struct s_CalibrationProcedure
    {
        public e_CalibrationType CalType;
        public int ChannelNumber;
        public int No_Ports;
        public int Port_1;
        public int Port_2;
        public int Port_3;
        public int Port_4;
        public int Port_5;
        public int Port_6;
        public int Port_7;
        public int Port_8;
        public int Port_9;
        public int CalKit;
        public bool b_CalKit;
        public string Message;
        //KCC
        public string Switch_Input;
        public string Switch_Ant;
        public string Switch_Rx, Switch_Rx2, Switch_Rx3, Switch_Rx4;

        public string[] Switch;

        public string Type;
        public string CalKit_Name;
        public int TraceNumber;
        public string ParameterType; //Yoonchun Spara & NF
        public string DUT_NF_InPort_Conn, DUT_NF_OutPort_Conn; //MM, 07/25/2017 - Allows for different input and output combo for NF
        public int NF_SrcPortNum;

    }

    public struct s_SegmentTable
    {
        public e_ModeSetting mode;
        public e_OnOff ifbw;
        public e_OnOff pow;
        public e_OnOff del;
        public e_OnOff swp;
        public e_OnOff time;
        public int segm;
        public s_SegmentData[] SegmentData;

    }
    public struct s_SegmentData
    {
        public double Start;
        public double Stop;
        public int Points;
        public double ifbw_value;
        public double pow_n1_value;
        public double pow_n2_value;
        public double pow_n3_value;
        public double pow_n4_value;
        public double pow_n5_value;
        public double pow_n6_value;
        public double pow_n7_value;
        public double pow_n8_value;
        public double pow_n9_value;
        public double pow_n10_value;
        public double pow_value;
        public double del_value;
        public e_SweepMode swp_value;
        public double time_value;
    }
    public struct s_PortMatchDetailSetting
    {
        public e_PortMatchType MatchType;
        public double R;
        public double L;
        public double C;
        public double G;
        public string UserFile;
    }
    public struct s_PortMatchSetting
    {
        public s_PortMatchDetailSetting[] Port;
        public int ChannelNumber;
        public bool Enable;
    }
}
