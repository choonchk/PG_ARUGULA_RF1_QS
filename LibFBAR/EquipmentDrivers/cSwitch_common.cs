using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;

namespace LibFBAR.Switch
{
    #region "Enumeration Declaration"
    public enum e_Switch_Equipment
    {
        S3488 = 0,
        S3499
    }
    #endregion
    public class cSwitch_common
    {
        public static string ClassName = "Switch Common Class";
        public e_Switch_Equipment Switch_Equipment; // Do not Static, if multiple of the same equipment is used
        LibFBAR.cGeneral general = new LibFBAR.cGeneral();

        public string Address;

        private static Switch.cSwitch3488 S3488;
        private static Switch.cSwitch3499 S3499;

        public cSwitchCommand SwitchCmd;
        public cCommonFunction BasicCommand;

        public void Set_Switch_Equipment(string Equipment_Str)
        {
            switch (Equipment_Str.ToUpper())
            {
                case "S3488":
                case "S3499":
                    Switch_Equipment = (e_Switch_Equipment)Enum.Parse(typeof(e_Switch_Equipment), Equipment_Str, true);
                    break;
                default:
                    general.DisplayError(ClassName, "Unable to Set Switch Equipment Correctly",
                                                    "Please Check the Switch Defination Settings \r\nDefined Setting : " + Equipment_Str);
                    Switch_Equipment = e_Switch_Equipment.S3499;
                    break;
            }
        }
        public void Init()
        {
            Ivi.Visa.Interop.FormattedIO488 IO =  new FormattedIO488();
            switch (Switch_Equipment)
            {
                case e_Switch_Equipment.S3488:
                    S3488 = new LibFBAR.Switch.cSwitch3488();
                    S3488.Address = Address;
                    S3488.OpenIO();
                    IO = S3488.parseIO;
                    break;
                case e_Switch_Equipment.S3499:
                    S3499 = new LibFBAR.Switch.cSwitch3499();
                    S3499.Address = Address;
                    S3499.OpenIO();
                    IO = S3499.parseIO;
                    break;
            }
            SwitchCmd = new cSwitchCommand(Switch_Equipment);
            BasicCommand = new cCommonFunction(IO);
        }

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.0.01";        //  15/1/2011       KKL             Driver for Switch Common Class

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        public class cSwitchCommand
        {
            public e_Switch_Equipment Switch_Equipment;
            public cSwitchCommand(e_Switch_Equipment Sw_Equipment)
            {
                Switch_Equipment = Sw_Equipment;
            }
            public void Close(int ChannelNumber)
            {
                if (Switch_Equipment == e_Switch_Equipment.S3488)
                {
                    S3488.Standard.Close(ChannelNumber);
                }
                else if (Switch_Equipment == e_Switch_Equipment.S3499)
                {
                    S3499.Switch.Close(ChannelNumber);
                }
            }
            public void Close(string ChannelNumber)
            {
                if (Switch_Equipment == e_Switch_Equipment.S3488)
                {
                    S3488.Standard.Close(ChannelNumber);
                }
                else if (Switch_Equipment == e_Switch_Equipment.S3499)
                {
                    S3499.Switch.Close(ChannelNumber);
                }
            }
            public void Open(int ChannelNumber)
            {
                if (Switch_Equipment == e_Switch_Equipment.S3488)
                {
                    S3488.Standard.Open(ChannelNumber);
                }
                else if (Switch_Equipment == e_Switch_Equipment.S3499)
                {
                    S3499.Switch.Open(ChannelNumber);
                }
            }
            public void Open(string ChannelNumber)
            {
                if (Switch_Equipment == e_Switch_Equipment.S3488)
                {
                    S3488.Standard.Open(ChannelNumber);
                }
                else if (Switch_Equipment == e_Switch_Equipment.S3499)
                {
                    S3499.Switch.Open(ChannelNumber);
                }
            }
        }

        public class cDigital : cCommonFunction
        {
            public cInput Input;
            public cOutput Output;
            public cConfiguration Configuration;
            public cIO_Memory IO_Memory;

            private e_Switch_Equipment Switch_Equipment;

            public cDigital(e_Switch_Equipment Sw_Equipment, FormattedIO488 parse)
                : base(parse)
            {
                Switch_Equipment = Sw_Equipment;
                Input = new cInput(Sw_Equipment, parse);
                Output = new cOutput(Sw_Equipment, parse);
                Configuration = new cConfiguration(Sw_Equipment, parse);
                IO_Memory = new cIO_Memory(Sw_Equipment, parse);
            }

            public class cInput : cCommonFunction
            {
                private e_Switch_Equipment Switch_Equipment;
                public cInput(e_Switch_Equipment Sw_Equipment, FormattedIO488 parse) : base(parse) 
                {
                    Switch_Equipment = Sw_Equipment;
                }

                public int Bit(int Bit_Port)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Input.Bit(Bit_Port));
                    }
                    else
                    {
                        return (0);
                    }
                }
                public int Byte(int Port)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Input.Bit(Port));
                    }
                    else
                    {
                        return (0);
                    }
                }
                public int Word(int Port)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Input.Word(Port));
                    }
                    else
                    {
                        return (0);
                    }
                }
                public int LWord(int Port)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Input.LWord(Port));
                    }
                    else
                    {
                        return (0);
                    }
                }
                public Digital_Data Block_Byte(int Port, int Size)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Input.Block_Byte(Port, Size));
                    }
                    else
                    {
                        Digital_Data rtn = new Digital_Data();
                        rtn.digits = 0;
                        rtn.length = 0;
                        return (rtn);
                    }
                }
                public Digital_Data Block_Word(int Port, int Size)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Input.Block_Word(Port, Size));
                    }
                    else
                    {
                        Digital_Data rtn = new Digital_Data();
                        rtn.digits = 0;
                        rtn.length = 0;
                        return (rtn);
                    }
                }
                public Digital_Data Block_LWORD(int Port, int Size)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Input.Block_LWORD(Port, Size));
                    }
                    else
                    {
                        Digital_Data rtn = new Digital_Data();
                        rtn.digits = 0;
                        rtn.length = 0;
                        return (rtn);
                    }
                }
            }
            public class cOutput : cCommonFunction
            {
                private e_Switch_Equipment Switch_Equipment;
                public cOutput(e_Switch_Equipment Sw_Equipment, FormattedIO488 parse)
                    : base(parse) 
                {
                    Switch_Equipment = Sw_Equipment;
                }

                public void Bit(int Bit_Port, int data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Bit(Bit_Port, data);
                    }
                }
                public void Byte(int port, int data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Byte(port, data);
                    }
                }
                public void Word(int port, int data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Word(port, data);
                    }
                }
                public void LWord(int port, int data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.LWord(port, data);
                    }
                }
                public void Block_Byte(int port, Digital_Data block_data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Block_Byte(port, block_data);
                    }
                }
                public void Block_Byte(int port, string block_data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Block_Byte(port, block_data);
                    }
                }
                public void Block_Word(int port, Digital_Data block_data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Block_Word(port, block_data);
                    }
                }
                public void Block_Word(int port, string block_data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Block_Word(port, block_data);
                    }
                }
                public void Block_LWord(int port, Digital_Data block_data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Block_LWord(port, block_data);
                    }
                }
                public void Block_LWord(int port, string block_data)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Output.Block_LWord(port, block_data);
                    }
                }
            }
            public class cConfiguration : cCommonFunction
            {
                public cDigitialData Data;
                private e_Switch_Equipment Switch_Equipment;
                public cConfiguration(e_Switch_Equipment Sw_Equipment, FormattedIO488 parse)
                        : base(parse) 
                {
                    Switch_Equipment = Sw_Equipment;
                    Data = new cDigitialData(Sw_Equipment, parse);
                }

                public void Mode(int Port, Digital_ConfigMode dMode)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Configuration.Mode(Port, dMode);
                    }
                }
                public void Mode(int Port, int dMode)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Configuration.Mode(Port, dMode);
                    }
                }
                public Digital_ConfigMode Mode(int Port)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Configuration.Mode(Port));
                    }
                    else
                    {
                        return (Digital_ConfigMode.Full_Handshake);
                    }
                }

                public void Control_Polarity(int Slot, Polarity Polar)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Configuration.Control_Polarity(Slot, Polar);
                    }
                }
                public Polarity Control_Polarity(int Slot)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Configuration.Control_Polarity(Slot));
                    }
                    else
                    {
                        return (Polarity.Pos);
                    }
                }
                public void Flag_Polarity(int Slot, Polarity Polar)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Configuration.Flag_Polarity(Slot, Polar);
                    }
                }
                public Polarity Flag_Polarity(int Slot)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Configuration.Flag_Polarity(Slot));
                    }
                    else
                    {
                        return (Polarity.Pos);
                    }
                }
                public void IO_Polarity(int Slot, Polarity Polar)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        S3499.Digital.Configuration.IO_Polarity(Slot, Polar);
                    }
                }
                public Polarity IO_Polarity(int Slot)
                {
                    if (Switch_Equipment == e_Switch_Equipment.S3499)
                    {
                        return (S3499.Digital.Configuration.IO_Polarity(Slot));
                    }
                    else
                    {
                        return (Polarity.Pos);
                    }
                }
                public class cDigitialData : cCommonFunction
                {
                    private e_Switch_Equipment Switch_Equipment;
                    public cDigitialData(e_Switch_Equipment Sw_Equipment, FormattedIO488 parse)
                        : base(parse) 
                    {
                        Switch_Equipment = Sw_Equipment;
                    }

                    public void Byte_Polarity(int Port, Polarity Polar)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.Configuration.Data.Byte_Polarity(Port, Polar);
                        }
                    }
                    public Polarity Byte_Polarity(int Port)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            return (S3499.Digital.Configuration.Data.Byte_Polarity(Port));
                        }
                        else
                        {
                            return (Polarity.Pos);
                        }
                    }
                    public void Word_Polarity(int Port, Polarity Polar)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.Configuration.Data.Word_Polarity(Port, Polar);
                        }
                    }
                    public Polarity Word_Polarity(int Port)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            return (S3499.Digital.Configuration.Data.Word_Polarity(Port));
                        }
                        else
                        {
                            return (Polarity.Pos);
                        }
                    }
                    public void LWord_Polarity(int Port, Polarity Polar)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.Configuration.Data.LWord_Polarity(Port, Polar);
                        }
                    }
                    public Polarity LWord_Polarity(int Port)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            return (S3499.Digital.Configuration.Data.LWord_Polarity(Port));
                        }
                        else
                        {
                            return (Polarity.Pos);
                        }
                    }
                }
            }

            public class cIO_Memory : cCommonFunction
            {
                public cData Data;
                public cTrace Trace;
                private e_Switch_Equipment Switch_Equipment;
                public cIO_Memory(e_Switch_Equipment Sw_Equipment,  FormattedIO488 parse)
                    : base(parse)
                {
                    Switch_Equipment = Sw_Equipment;
                    Data = new cData(Sw_Equipment,parse);
                    Trace = new cTrace(Sw_Equipment,parse);
                }

                public class cTrace : cCommonFunction
                {
                    private e_Switch_Equipment Switch_Equipment;
                    public cTrace(e_Switch_Equipment Sw_Equipment, FormattedIO488 parse) : base(parse)
                    {
                        Switch_Equipment = Sw_Equipment;
                    }

                    public void Define(string System_Memory_Name, int size)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Trace.Define(System_Memory_Name, size);
                        }
                    }
                    public void Define(string System_Memory_Name, int size, string fill_data)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Trace.Define(System_Memory_Name, size, fill_data);
                        }
                    }
                    public int Define(string System_Memory_Name)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            return (S3499.Digital.IO_Memory.Trace.Define(System_Memory_Name));
                        }
                        else
                        {
                            return (0);
                        }
                    }
                    public string Memory_Catalog()
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            return (S3499.Digital.IO_Memory.Trace.Memory_Catalog());
                        }
                        else
                        {
                            return ("");
                        }
                    }
                    public void Delete_Memory(string System_Memory_Name)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Trace.Delete_Memory(System_Memory_Name);
                        }
                    }
                    public void Delete_ALL_Memory()
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Trace.Delete_ALL_Memory();
                        }
                    }
                }
                public class cData : cCommonFunction
                {
                    private e_Switch_Equipment Switch_Equipment;
                    public cData(e_Switch_Equipment Sw_Equipment, FormattedIO488 parse) : base(parse)
                    {
                        Switch_Equipment = Sw_Equipment;
                    }

                    public void Save_Memory(string System_Memory_Name, Digital_Data block_data)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Data.Save_Memory(System_Memory_Name, block_data);
                        }
                    }
                    public void Save_Memory(string System_Memory_Name, string block_data)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Data.Save_Memory(System_Memory_Name, block_data);
                        }
                    }
                    public void Load_Memory_Byte(int Port, string System_Memory_Name)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Data.Load_Memory_Byte(Port, System_Memory_Name);
                        }
                    }
                    public void Load_Memory_Word(int Port, string System_Memory_Name)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Data.Load_Memory_Word(Port, System_Memory_Name);
                        }
                    }
                    public void Load_Memory_LWord(int Port, string System_Memory_Name)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            S3499.Digital.IO_Memory.Data.Load_Memory_LWord(Port, System_Memory_Name);
                        }
                    }
                    public Digital_Data Save_Memory(string System_Memory_Name)
                    {
                        if (Switch_Equipment == e_Switch_Equipment.S3499)
                        {
                            return (S3499.Digital.IO_Memory.Data.Save_Memory(System_Memory_Name));
                        }
                        else
                        {
                            Digital_Data Data = new Digital_Data();
                            Data.length=0;
                            Data.digits=0;
                            return (Data);
                        }
                    }
                }
            }
        }
    }
}
