Imports NationalInstruments.ModularInstruments.Interop
Public Class RFSAStartedIQExample
	Const numberOfSamples As Int32 = 1000
	Private rfsa As niRFSA
	Private resourceName As String = "RFSA"
	Private dataBlock As niComplexNumber()
	Private wfmInfo As niRFSA_wfmInfo
	Private magnitudeSquared As Double
	Private accumulator As Double
	Private refClockRate As Double
	Private carrierFrequency As Double
	Private iqRate As Double
	Friend Sub Run()
		Try
			InitializeVariables()
			InitializeRFSA()
			ConfigureRFSA()
			RetrieveResults()
		Catch ex As Exception
			DisplayError(ex)
		Finally
			' Close session 

			CloseSession()
			Console.WriteLine("Press any key to exit.....")
			Console.ReadKey()
		End Try
	End Sub
	Private Sub InitializeVariables()
		dataBlock = New niComplexNumber(numberOfSamples - 1) {}
		wfmInfo = New niRFSA_wfmInfo()
		accumulator = 0.0
		refClockRate = 10000000.0
		carrierFrequency = 1000000000.0
		iqRate = 1000000.0
	End Sub
	Private Sub InitializeRFSA()
		rfsa = New niRFSA(resourceName, True, False)
	End Sub
	Private Sub ConfigureRFSA()
		rfsa.ConfigureRefClock("OnboardClock", refClockRate)
		rfsa.ConfigureReferenceLevel("", 0)
		rfsa.ConfigureAcquisitionType(niRFSAConstants.Iq)
		rfsa.ConfigureIQCarrierFrequency("", carrierFrequency)
		rfsa.ConfigureNumberOfSamples("", True, numberOfSamples)
		rfsa.ConfigureIQRate("", iqRate)
	End Sub

	Private Sub RetrieveResults()
		rfsa.ReadIQSingleRecordComplexF64("", 10.0, dataBlock, numberOfSamples, wfmInfo)
		If numberOfSamples > 0 Then
			For i As Integer = 0 To numberOfSamples - 1
				magnitudeSquared = dataBlock(i).Real * dataBlock(i).Real + dataBlock(i).Imaginary * dataBlock(i).Imaginary

				' we need to handle this because log(0) return a range error. 

				If magnitudeSquared = 0.0 Then
					magnitudeSquared = 1E-08
				End If


				accumulator += 10.0 * Math.Log10((magnitudeSquared / (2.0 * 50.0)) * 1000.0)
			Next

			Console.WriteLine("Average power = {0} dBm" & Environment.NewLine, Math.Round(accumulator / numberOfSamples, 1))
		End If

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
