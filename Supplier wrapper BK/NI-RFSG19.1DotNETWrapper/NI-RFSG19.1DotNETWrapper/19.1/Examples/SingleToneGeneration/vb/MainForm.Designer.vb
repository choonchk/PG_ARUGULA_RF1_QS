Namespace NationalInstruments.Examples.SingleToneGeneration
    Partial Class MainForm
        ''' <summary>
        ''' Required designer variable.
        ''' </summary>
        Private components As System.ComponentModel.IContainer = Nothing

        ''' <summary>
        ''' Clean up any resources being used.
        ''' </summary>
        ''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso (components IsNot Nothing) Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

#Region "Windows Form Designer generated code"

        ''' <summary>
        ''' Required method for Designer support - do not modify
        ''' the contents of this method with the code editor.
        ''' </summary>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.configurationGroupBox = New System.Windows.Forms.GroupBox()
            Me.updateButton = New System.Windows.Forms.Button()
            Me.frequencyNumeric = New System.Windows.Forms.NumericUpDown()
            Me.frequencyLabel = New System.Windows.Forms.Label()
            Me.powerLevelNumeric = New System.Windows.Forms.NumericUpDown()
            Me.powerLevelLabel = New System.Windows.Forms.Label()
            Me.startButton = New System.Windows.Forms.Button()
            Me.resourceNameLabel = New System.Windows.Forms.Label()
            Me.errorLabel = New System.Windows.Forms.Label()
            Me.errorTextBox = New System.Windows.Forms.TextBox()
            Me.stopButton = New System.Windows.Forms.Button()
            Me.rfsgStatusTimer = New System.Windows.Forms.Timer(Me.components)
            Me.ResourceNameTextBox = New System.Windows.Forms.TextBox()
            Me.timer1 = New System.Windows.Forms.Timer(Me.components)
            Me.timer2 = New System.Windows.Forms.Timer(Me.components)
            Me.configurationGroupBox.SuspendLayout()
            DirectCast(Me.frequencyNumeric, System.ComponentModel.ISupportInitialize).BeginInit()
            DirectCast(Me.powerLevelNumeric, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            ' 
            ' configurationGroupBox
            ' 
            Me.configurationGroupBox.Controls.Add(Me.updateButton)
            Me.configurationGroupBox.Controls.Add(Me.frequencyNumeric)
            Me.configurationGroupBox.Controls.Add(Me.frequencyLabel)
            Me.configurationGroupBox.Controls.Add(Me.powerLevelNumeric)
            Me.configurationGroupBox.Controls.Add(Me.powerLevelLabel)
            Me.configurationGroupBox.Location = New System.Drawing.Point(8, 63)
            Me.configurationGroupBox.Name = "configurationGroupBox"
            Me.configurationGroupBox.Size = New System.Drawing.Size(360, 137)
            Me.configurationGroupBox.TabIndex = 13
            Me.configurationGroupBox.TabStop = False
            Me.configurationGroupBox.Text = "Configuration"
            ' 
            ' updateButton
            ' 
            Me.updateButton.Enabled = False
            Me.updateButton.Location = New System.Drawing.Point(246, 41)
            Me.updateButton.Name = "updateButton"
            Me.updateButton.Size = New System.Drawing.Size(75, 23)
            Me.updateButton.TabIndex = 3
            Me.updateButton.Text = "&Update"
            Me.updateButton.UseVisualStyleBackColor = True
            AddHandler Me.updateButton.Click, New System.EventHandler(AddressOf Me.updateButton_Click)
            ' 
            ' frequencyNumeric
            ' 
            Me.frequencyNumeric.DecimalPlaces = 6
            Me.frequencyNumeric.Increment = New Decimal(New Integer() {1000000, 0, 0, 0})
            Me.frequencyNumeric.Location = New System.Drawing.Point(13, 44)
            Me.frequencyNumeric.Maximum = New Decimal(New Integer() {-1, -1, -1, 0})
            Me.frequencyNumeric.Minimum = New Decimal(New Integer() {-1, -1, -1, -2147483648})
            Me.frequencyNumeric.Name = "frequencyNumeric"
            Me.frequencyNumeric.Size = New System.Drawing.Size(120, 20)
            Me.frequencyNumeric.TabIndex = 1
            Me.frequencyNumeric.Value = New Decimal(New Integer() {1000000000, 0, 0, 0})
            ' 
            ' frequencyLabel
            ' 
            Me.frequencyLabel.AutoSize = True
            Me.frequencyLabel.Location = New System.Drawing.Point(13, 23)
            Me.frequencyLabel.Name = "frequencyLabel"
            Me.frequencyLabel.Size = New System.Drawing.Size(113, 13)
            Me.frequencyLabel.TabIndex = 1
            Me.frequencyLabel.Text = "Center Frequency [Hz]"
            ' 
            ' powerLevelNumeric
            ' 
            Me.powerLevelNumeric.DecimalPlaces = 2
            Me.powerLevelNumeric.Location = New System.Drawing.Point(13, 98)
            Me.powerLevelNumeric.Maximum = New Decimal(New Integer() {-1, -1, -1, 0})
            Me.powerLevelNumeric.Minimum = New Decimal(New Integer() {-1, -1, -1, -2147483648})
            Me.powerLevelNumeric.Name = "powerLevelNumeric"
            Me.powerLevelNumeric.Size = New System.Drawing.Size(120, 20)
            Me.powerLevelNumeric.TabIndex = 2
            Me.powerLevelNumeric.Value = New Decimal(New Integer() {20, 0, 0, -2147483648})
            ' 
            ' powerLevelLabel
            ' 
            Me.powerLevelLabel.AutoSize = True
            Me.powerLevelLabel.Location = New System.Drawing.Point(13, 77)
            Me.powerLevelLabel.Name = "powerLevelLabel"
            Me.powerLevelLabel.Size = New System.Drawing.Size(96, 13)
            Me.powerLevelLabel.TabIndex = 2
            Me.powerLevelLabel.Text = "Power Level [dBm]"
            ' 
            ' startButton
            ' 
            Me.startButton.Location = New System.Drawing.Point(212, 25)
            Me.startButton.Name = "startButton"
            Me.startButton.Size = New System.Drawing.Size(75, 23)
            Me.startButton.TabIndex = 14
            Me.startButton.Text = "St&art"
            Me.startButton.UseVisualStyleBackColor = True
            AddHandler Me.startButton.Click, New System.EventHandler(AddressOf Me.startButton_Click)
            ' 
            ' resourceNameLabel
            ' 
            Me.resourceNameLabel.AutoSize = True
            Me.resourceNameLabel.Location = New System.Drawing.Point(8, 6)
            Me.resourceNameLabel.Name = "resourceNameLabel"
            Me.resourceNameLabel.Size = New System.Drawing.Size(84, 13)
            Me.resourceNameLabel.TabIndex = 11
            Me.resourceNameLabel.Text = "Resource Name"
            ' 
            ' errorLabel
            ' 
            Me.errorLabel.AutoSize = True
            Me.errorLabel.Location = New System.Drawing.Point(8, 211)
            Me.errorLabel.Name = "errorLabel"
            Me.errorLabel.Size = New System.Drawing.Size(120, 13)
            Me.errorLabel.TabIndex = 15
            Me.errorLabel.Text = "Warning/Error Message"
            ' 
            ' errorTextBox
            ' 
            Me.errorTextBox.Location = New System.Drawing.Point(8, 232)
            Me.errorTextBox.Multiline = True
            Me.errorTextBox.Name = "errorTextBox"
            Me.errorTextBox.[ReadOnly] = True
            Me.errorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both
            Me.errorTextBox.Size = New System.Drawing.Size(360, 60)
            Me.errorTextBox.TabIndex = 17
            Me.errorTextBox.TabStop = False
            Me.errorTextBox.Text = "No error."
            ' 
            ' stopButton
            ' 
            Me.stopButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.stopButton.Enabled = False
            Me.stopButton.Location = New System.Drawing.Point(293, 25)
            Me.stopButton.Name = "stopButton"
            Me.stopButton.Size = New System.Drawing.Size(75, 23)
            Me.stopButton.TabIndex = 16
            Me.stopButton.Text = "St&op"
            Me.stopButton.UseVisualStyleBackColor = True
            AddHandler Me.stopButton.Click, New System.EventHandler(AddressOf Me.stopButton_Click)
            ' 
            ' rfsgStatusTimer
            ' 

            AddHandler Me.rfsgStatusTimer.Tick, New System.EventHandler(AddressOf Me.rfsgStatusTimer_Tick)
            rfsgStatusTimer.Enabled = False
            ' 
            ' ResourceNameTextBox
            ' 
            Me.ResourceNameTextBox.Location = New System.Drawing.Point(21, 25)
            Me.ResourceNameTextBox.Name = "ResourceNameTextBox"
            Me.ResourceNameTextBox.Size = New System.Drawing.Size(120, 20)
            Me.ResourceNameTextBox.TabIndex = 18
            ' 
            ' MainForm
            ' 
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0F, 13.0F)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(379, 313)
            Me.Controls.Add(Me.ResourceNameTextBox)
            Me.Controls.Add(Me.configurationGroupBox)
            Me.Controls.Add(Me.startButton)
            Me.Controls.Add(Me.resourceNameLabel)
            Me.Controls.Add(Me.errorLabel)
            Me.Controls.Add(Me.errorTextBox)
            Me.Controls.Add(Me.stopButton)
            Me.Name = "MainForm"
            Me.Text = "Single Tone Generation"
            AddHandler Me.FormClosing, New System.Windows.Forms.FormClosingEventHandler(AddressOf Me.MainForm_FormClosing)
            Me.configurationGroupBox.ResumeLayout(False)
            Me.configurationGroupBox.PerformLayout()
            DirectCast(Me.frequencyNumeric, System.ComponentModel.ISupportInitialize).EndInit()
            DirectCast(Me.powerLevelNumeric, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub


#End Region

        Private configurationGroupBox As System.Windows.Forms.GroupBox
        Private updateButton As System.Windows.Forms.Button
        Private frequencyNumeric As System.Windows.Forms.NumericUpDown
        Private frequencyLabel As System.Windows.Forms.Label
        Private powerLevelNumeric As System.Windows.Forms.NumericUpDown
        Private powerLevelLabel As System.Windows.Forms.Label
        Private startButton As System.Windows.Forms.Button
        Private resourceNameLabel As System.Windows.Forms.Label
        Private errorLabel As System.Windows.Forms.Label
        Private errorTextBox As System.Windows.Forms.TextBox
        Private stopButton As System.Windows.Forms.Button
        Private rfsgStatusTimer As System.Windows.Forms.Timer
        Private ResourceNameTextBox As System.Windows.Forms.TextBox
        Private timer1 As System.Windows.Forms.Timer
        Private timer2 As System.Windows.Forms.Timer

    End Class
End Namespace

