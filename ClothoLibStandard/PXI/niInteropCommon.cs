// Contains types that are common among RF Toolkits and Modular Instruments wrappers.
using System;
using System.Runtime.InteropServices;

namespace NationalInstruments.ModularInstruments.Interop
{
    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
    public struct niComplexNumber
    {

        public double Real;

        public double Imaginary;

    }
}
