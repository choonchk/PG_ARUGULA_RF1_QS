using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agilent.AgM9300.Interop;

namespace EqLib
{
        public class KeysightReferenceClock 
        {
            public static iRFCLK Get(byte site)
            {
                iRFCLK EqRfCLK = new KeysightReferenceClock.ActiveClk();
                return EqRfCLK;
            }
            public abstract class iRFCLK
            {
                public abstract void Initialize(object InstrAddress);
            }
            public class ActiveClk : iRFCLK
            {
                public override void Initialize(object InstrAddress)
                {
                    AgM9300 reference = new AgM9300();
                    reference.Initialize(ResourceName: InstrAddress.ToString(), IdQuery: true, Reset: true);

                    reference.ReferenceBase.Out1Enabled = true;
                    reference.ReferenceBase.Out2Enabled = true;
                    reference.ReferenceBase.Out3Enabled = true;
                    reference.ReferenceBase.Out4Enabled = true;
                    reference.ReferenceBase.Out5Enabled = true;
                    reference.ReferenceBase.Ref10MHzOutEnabled = true;
                    reference.Apply();
                }
            }
            

        }  
    
}
