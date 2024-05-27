using System;
using System.Collections.Generic;

namespace MPAD_TestTimer
{
    /// <summary>
    /// Error handling data type, stores exception, error messages. Default IsValidated is true.
    /// </summary>
    public class ValidationDataObject
    {
        private List<string> m_errorMsgList;
        private string m_errorMsg;
        private string m_errorTitle;
        private Exception m_exception;
        private bool m_isValidated;

        /// <summary>
        /// create an instance with IsValidated true by default.
        /// </summary>
        public ValidationDataObject()
        {
            m_isValidated = true;
            m_errorMsgList = new List<string>();
        }

        /// <summary>
        /// create an error instance.
        /// </summary>
        public ValidationDataObject(string msg, string title, Exception ex)
        {
            m_errorMsgList = new List<string>();
            m_errorMsgList.Add(msg);
            m_errorTitle = title;
            m_exception = ex;
            m_isValidated = false;
        }

        public string ErrorMessage
        {
            get { return FormSummary(); }
            set
            {
                bool isError = !String.IsNullOrEmpty(value);
                m_errorMsgList.Clear();
                m_errorMsgList.Add(value);
                if (isError) m_isValidated = false;
            }
        }

        /// <summary>
        /// Title to be shown in error prompt.
        /// </summary>
        public string ErrorTitle
        {
            get { return m_errorTitle; }
            set { m_errorTitle = value; }
        }

        public Exception Exception
        {
            get { return m_exception; }
            set { m_exception = value; }
        }

        /// <summary>
        /// Default is true.
        /// </summary>
        public bool IsValidated
        {
            get { return m_isValidated; }
            set { m_isValidated = value; }
        }

        public void Validate(bool isValidated)
        {
            m_isValidated = m_isValidated && isValidated;
        }

        public void AppendError(string errorMessage)
        {
            m_errorMsgList.Add(errorMessage);
        }

        private string FormSummary()
        {
            string r = String.Join(Environment.NewLine, m_errorMsgList.ToArray());
            return r;
        }

    }

    /// <summary>
    /// Collect a list of error messages.
    /// </summary>
    public class ValidationDataCollection
    {
        public List<ValidationDataObject> ValidationData { get; private set; }

        public ValidationDataCollection()
        {
            ValidationData = new List<ValidationDataObject>();
        }

        public bool IsValidated
        {
            get
            {
                foreach (ValidationDataObject vdo in ValidationData)
                {
                    if (!vdo.IsValidated) return false;
                }

                return true;
            }
        }

        public void Add(ValidationDataObject vdo)
        {
            ValidationData.Add(vdo);
        }

        public void Clear()
        {
            ValidationData.Clear();
        }

    }
}
