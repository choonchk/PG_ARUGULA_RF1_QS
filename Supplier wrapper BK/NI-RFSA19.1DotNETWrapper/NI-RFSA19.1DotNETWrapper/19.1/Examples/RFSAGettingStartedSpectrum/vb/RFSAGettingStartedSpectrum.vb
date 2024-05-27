Imports NationalInstruments.ModularInstruments.Interop

Public Class RFSAGettingStartedSpectrumExample
	Const resourceName As String = "RFSA"
	Private numberOfSpectralLines As Integer
	Private rfsa As niRFSA
	Private spectrumDataBlocks As Double()
	Private spectrumInfo As niRFSA_spectrumInfo
	Private refClockRate As Double
	Private greatestPeakPower As Double
	Private greatestPeakFrequency As Double
	Private startFrequency As Double
	Private stopFrequency As Double
	Private resolutionBandwidth As Double

	Friend Sub Run()
		Try
			InitializeVariables()
			InitializeRFSA()
			ConfigureRFSA()
			RetrieveResults()
		Catch ex As Exception
			DisplayError(ex)
		Finally
			CloseSession()
			Console.WriteLine("Press any key to exit.....")
			Console.ReadKey()
		End Try
	End Sub
	Private Sub InitializeVariables()
		spectrumInfo = New niRFSA_spectrumInfo()
		refClockRate = 10000000.0
		startFrequency = 990000000.0
		stopFrequency = 1010000000.0
		resolutionBandwidth = 10000.0
	End Sub
	Private Sub InitializeRFSA()
		rfsa = New niRFSA(resourceName, True, False)
	End Sub
	Private Sub ConfigureRFSA()
		rfsa.ConfigureRefClock("OnboardClock", refClockRate)
		rfsa.ConfigureReferenceLevel("", 0)
		rfsa.ConfigureAcquisitionType(niRFSAConstants.Spectrum)
		rfsa.ConfigureSpectrumFrequencyStartStop("", startFrequency, stopFrequency)
		rfsa.ConfigureResolutionBandwidth("", resolutionBandwidth)
	End Sub

	Private Sub RetrieveResults()
		' Read the power spectrum 

		' We need the number of spectral lines in order to know the size of the
'             * spectrum array. 


		rfsa.GetNumberOfSpectralLines("", numberOfSpectralLines)
		spectrumDataBlocks = New Double(numberOfSpectralLines - 1) {}
		rfsa.ReadPowerSpectrumF64("", 10.0, spectrumDataBlocks, numberOfSpectralLines, spectrumInfo)
		' Do something useful with the data 

		' We will find the highest peak in a bin, which is not the actual highest
'             * peak and frequency we could find in the acquisition.  For an accurate
'             * peak search, we can analyze the data with the Spectral Measurements
'             * Toolset. 

		For i As Integer = 0 To numberOfSpectralLines - 1
			If (i = 0) OrElse (spectrumDataBlocks(i) > greatestPeakPower) Then
				greatestPeakPower = spectrumDataBlocks(i)
				greatestPeakFrequency = spectrumInfo.initialFrequency + spectrumInfo.frequencyIncrement * i
			End If
		Next
		Console.WriteLine("The highest peak in a bin is {0} dBm at {1} f MHz." & Environment.NewLine, Math.Round(greatestPeakPower, 1), Math.Round(greatestPeakFrequency / 1000000.0, 3))


	End Sub
	Private Sub CloseSession()

		Try
			If rfsa IsNot Nothing Then
				rfsa.Close()
				rfsa = Nothing
			End If
		Catch ex As Exception
			DisplayError(ex)
			Environment.[Exit](0)
		End Try
	End Sub
	Private Shared Sub DisplayError(ex As Exception)
		Console.WriteLine("ERROR:" & Environment.NewLine & Convert.ToString(ex.[GetType]()) & ": " & ex.Message)
	End Sub
End Class
