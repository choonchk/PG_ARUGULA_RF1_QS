using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.SystemConfiguration;

namespace EqLib
{
    public class Chassis
    {
        public static Chassis_base Get(byte site,string UserAlias)
        {
            Chassis_base Chassis_X;
            if (site == 0 || site == 1)
            {
                Chassis_X = new Chassis.Active_Chassis(UserAlias);
            }
            else if (site == 2 || site == 3)
            {
                Chassis_X = new Chassis.Active_Chassis(UserAlias);
            }
            else
            {
                throw new Exception("ENA site is not recognized, please check site number!");
            }
            return Chassis_X;
        }
        public abstract class Chassis_base
        {
            public string UserAlias { get; set; }
            public abstract double Read_Temp();
        }
        public class Active_Chassis : Chassis_base
        {
            private ProductResource myResource = null;

            public Active_Chassis(string UserAlias)
            {
                if (UserAlias.Contains("M90")) KeysightChassis(UserAlias);
                else
                {
                    SystemConfiguration mySysConfig = new SystemConfiguration("localhost");
                    ResourceCollection rawResources = mySysConfig.FindHardware();
                    //ProductResource myResource = null;
                    for (int i = 0; i < rawResources.Count; i++)
                    {
                        if (rawResources[i].UserAlias == UserAlias)
                        {
                            myResource = (ProductResource)rawResources[i];
                        }
                    }
                }
                             
            }
            public void KeysightChassis(string UserAlias)
            {
                ////string M9018resource = "PXI0::148-0.0::INSTR";
                //bool idquery = true;
                //bool reset = true;
                //// Setup the chassis backplane triggers if Resource is not blank

                //IAgM9018 M9018 = new AgM9018();
                ////Initialize M9018A
                //M9018.Initialize(UserAlias, idquery, reset, "");

                //// Configure the Chassis to allow Trigger Segment 1 to driver trigger segment 2 for PXI Trigger2.
                //// Use PXI Trig2 as it is connected to the M90XA as "EXT2"
                //M9018.TriggerBus.Connect(0, AgM9018TrigBusEnum.AgM9018TrigBus1To2);
                ////M9018.TriggerBus.Connect(1, AgM9018TrigBusEnum.AgM9018TrigBus1To2);
                //M9018.Close();
                //M9018 = null;
            }
            public override double Read_Temp()
            {
                double Temp;
                Temp = myResource.TemperatureSensors[0].Reading;
                return Temp;
            }
        }
        public class None : Chassis_base
        {
            public None(string UserAlias)
            {
                this.UserAlias = UserAlias;
               
            }
            public override double Read_Temp()
            {
                double Temp=0;
                return Temp;
            }
        }
       
    }
}
