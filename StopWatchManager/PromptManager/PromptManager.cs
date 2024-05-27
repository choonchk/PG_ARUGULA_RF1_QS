using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MPAD_TestTimer
{
    /// <summary>
    /// Centralized dialog prompting.
    /// </summary>
    public class PromptManager
    {
        private static PromptManager instance;

        public static PromptManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PromptManager();
                }

                return instance;
            }
        }

        public bool IsAutoAnswer { get; set; }

        public void ShowError(string message)
        {
            ErrorDisplayDialog errDialog = new ErrorDisplayDialog();
            errDialog.Initialize(message);
            errDialog.ShowDialog();

        }
        public void ShowError(string message, string title)
        {
            ErrorDisplayDialog errDialog = new ErrorDisplayDialog();
            errDialog.Initialize(message, title);
            errDialog.ShowDialog();
        }

        public void ShowError(string message1, string message2, string title)
        {
            ErrorDisplayDialog errDialog = new ErrorDisplayDialog();
            errDialog.Initialize(message1, message2, title);
            errDialog.ShowDialog();
        }

        public void ShowError(Exception ex)
        {
            if (ex == null) return;
            ErrorDisplayDialog errDialog = new ErrorDisplayDialog();
            errDialog.Initialize(ex);
            errDialog.ShowDialog();
        }

        public void ShowError(string message, Exception ex)
        {
            ErrorDisplayDialog errDialog = new ErrorDisplayDialog();
            errDialog.Initialize(message, ex);
            errDialog.ShowDialog();
        }

        public void ShowError(ValidationDataObject vdo)
        {
            if (vdo.IsValidated) return;

            ErrorDisplayDialog errDialog = new ErrorDisplayDialog();
            errDialog.Initialize(vdo.ErrorMessage, vdo.ErrorTitle, vdo.Exception);
            errDialog.ShowDialog();
        }

        public void ShowError(ValidationDataCollection vdc)
        {
            if (vdc.IsValidated) return;

            foreach (ValidationDataObject vdo in vdc.ValidationData)
            {
                ShowError(vdo);
            }
        }

        public DialogResult ShowDialogYesNoCancel(string message, string title)
        {
            DialogResult result = DialogResult.Cancel;
            if (IsAutoAnswer)
            {
                return result;
            }

            Dictionary<string, string> selectionList = new Dictionary<string, string>();
            selectionList.Add("Yes", "Yes");
            selectionList.Add("No", "No");
            selectionList.Add("Cancel", "Cancel");

            string response = PromptManager.Instance.ShowMultiSelectionDialog(message,
                title, selectionList, "Cancel");

            switch (response)
            {
                case "Yes":
                    result = DialogResult.Yes;
                    break;
                case "No":
                    result = DialogResult.No;
                    break;
                case "Cancel":
                    result = DialogResult.Cancel;
                    break;
            }

            return result;
        }

        public DialogResult ShowDialogRetryCancel(string message, string title)
        {
            DialogResult dr = MessageBox.Show(message, title, MessageBoxButtons.RetryCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            return dr;
        }

        public DialogResult ShowDialogOKCancel(string message, string title)
        {
            DialogResult dr = MessageBox.Show(message, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            return dr;
        }

        public string ShowMultiSelectionDialog(string textBox1text, string dialogTitle,
            Dictionary<string, string> selectionList, string defaultSelectedKeyName)
        {
            if (IsAutoAnswer)
            {
                return defaultSelectedKeyName;
            }

            MultipleSelectionDialog dialog = new MultipleSelectionDialog();
            dialog.SetMessage(textBox1text, dialogTitle, selectionList, defaultSelectedKeyName);
            dialog.ShowDialog();
            return dialog.SelectedButton;
        }

        public string ShowMultiSelectionDialog(string textBox1text, string dialogTitle,
            string mess1, string mess2, string mess3, string mess4, string defaultSelectedKeyName)
        {
            MultipleSelectionDialog dialog = new MultipleSelectionDialog();
            dialog.SetMessage(textBox1text, dialogTitle, mess1, mess2, mess3, mess4, defaultSelectedKeyName);
            dialog.ShowDialog();
            return dialog.SelectedButton;
        }

        public string ShowTextInputDialog(string messageLine1, string messageLine2,
            string dialogTitle, string defaultInput)
        {
            TextInputDialog dialog = new TextInputDialog();
            dialog.SetMessage(messageLine1, messageLine2, dialogTitle, defaultInput);
            dialog.ShowDialog();
            return dialog.InputText;
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message);
        }

        public void ShowInfo(string message, string title)
        {
            MessageBox.Show(message, title);
        }
    }
}
