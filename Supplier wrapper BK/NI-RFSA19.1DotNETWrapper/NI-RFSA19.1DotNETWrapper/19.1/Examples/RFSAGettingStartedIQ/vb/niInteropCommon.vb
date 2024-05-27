' Contains types that are common among RF Toolkits and Modular Instruments wrappers.
Imports System.Runtime.InteropServices

Namespace NationalInstruments.ModularInstruments.Interop
	<StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack := 8)> _
	Public Structure niComplexNumber

		Public Real As Double

		Public Imaginary As Double

	End Structure

	<StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack := 8)> _
	Public Structure niComplexI16

		Public Real As Short

		Public Imaginary As Short

	End Structure

	<StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack := 8)> _
	Public Structure niComplexNumberF32

		Public Real As Single

		Public Imaginary As Single

	End Structure

    <StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack:=8)> _
    <Obsolete("This struct is obsolete. Use struct niComplexNumberF32.")> _
    Public Structure niComplexF32

        Public Real As Single

        Public Imaginary As Single

    End Structure
End Namespace
