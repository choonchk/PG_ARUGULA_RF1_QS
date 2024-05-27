Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports NationalInstruments.ModularInstruments.Interop


Namespace NationalInstruments.Examples.SingleToneGeneration
    Partial Public Class MainForm
        Inherits Form
        Private _rfsgSession As niRFSG

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub startGeneration()
            Dim resourceName As String
            Dim frequency As Double, power As Double
            resourceName = ResourceNameTextBox.Text
            frequency = CDbl(frequencyNumeric.Value)
            power = CDbl(powerLevelNumeric.Value)

            errorTextBox.Text = "No error."
            ' Initialize the NIRfsg session
            If _rfsgSession Is Nothing Then
                _rfsgSession = New niRFSG(resourceName, True, False)
            End If
            _rfsgSession.ConfigureRF(frequency, power)
            _rfsgSession.Initiate()
            rfsgStatusTimer.Start()
        End Sub

        Private Sub closeSession()
            If _rfsgSession IsNot Nothing Then
                _rfsgSession.close()
            End If
        End Sub
        Private Sub stopGeneration()
            Try
                If _rfsgSession IsNot Nothing Then
                    ' Disable the output.  This sets the noise floor as low as possible.
                    _rfsgSession.ConfigureOutputEnabled(False)

                    ' Close the RFSG NIRfsg session
                    _rfsgSession.close()

                    startButton.Enabled = True
                    updateButton.Enabled = False
                    stopButton.Enabled = False
                    rfsgStatusTimer.Enabled = False
                End If
                _rfsgSession = Nothing
            Catch ex As Exception
                errorTextBox.Text = "Error in StopGeneration(): " & ex.Message
            End Try
        End Sub
        Private Sub CheckGeneration()
            ' Check the status of the RFSG 
            Dim isDone As Boolean = False
            _rfsgSession.CheckGenerationStatus(isDone)
        End Sub
        Private Sub rfsgStatusTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs)
            Try
                CheckGeneration()
            Catch ex As System.Exception
                If ex.Message <> "" Then
                    errorTextBox.Text = ex.Message
                Else
                    errorTextBox.Text = "Error"
                End If
            End Try
        End Sub

        Private Sub startButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                errorTextBox.Text = "No Error"
                startButton.Enabled = False
                updateButton.Enabled = True
                stopButton.Enabled = True
                rfsgStatusTimer.Enabled = True
                startGeneration()
            Catch ex As System.Exception
                If ex.Message <> "" Then
                    errorTextBox.Text = ex.Message
                Else
                    errorTextBox.Text = "Error"
                End If
            Finally
                If _rfsgSession IsNot Nothing Then
                    _rfsgSession.Abort()
                    _rfsgSession.reset()
                End If
            End Try
        End Sub

        Private Sub stopButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            stopGeneration()
        End Sub
        Private Sub MainForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs)
            stopGeneration()
        End Sub

        Private Sub updateButton_Click(ByVal sender As Object, ByVal e As EventArgs)
            UpdateGeneration()
        End Sub

        Private Sub UpdateGeneration()
            Dim frequency As Double, power As Double
            Try
                startButton.Enabled = True
                ' Read in all the control values 
                frequency = CDbl(frequencyNumeric.Value)
                power = CDbl(powerLevelNumeric.Value)

                ' Abort generation 
                _rfsgSession.Abort()

                ' Configure the instrument 
                _rfsgSession.ConfigureRF(frequency, power)

                ' Initiate Generation 
                _rfsgSession.Initiate()

                ' Start the status checking timer 

                startButton.Enabled = False
            Catch ex As Exception
                If ex.Message <> "" Then
                    errorTextBox.Text = ex.Message
                Else
                    errorTextBox.Text = "Error"
                End If
            End Try
        End Sub

    End Class
End Namespace
