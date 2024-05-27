using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLib
{
    public interface iTest
    {
        bool Initialize(bool FinalScript);
        int RunTest();
        void BuildResults(ref ATFReturnResult results);
    }
}
