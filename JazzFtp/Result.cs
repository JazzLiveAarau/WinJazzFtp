using System;
using System.Collections.Generic;
using System.Text;

namespace JazzFtp
{
    /// <summary>Holds the results from an FTP execute function  
    /// <para></para>
    /// <para></para>
    /// </summary>
    public class Result
    {
        #region Status from execution

        /// <summary>Status</summary>
        private bool m_ftp_status = true;

        /// <summary>Get or set status</summary>
        public bool Status
        { get { return m_ftp_status; } set { m_ftp_status = value; } }

        /// <summary>Error message</summary>
        private string m_ftp_error_msg = "";

        /// <summary>Get or set error message</summary>
        public string ErrorMsg
        { get { return m_ftp_error_msg; } set { m_ftp_error_msg = value; } }

        /// <summary>Message</summary>
        private string m_ftp_msg = "";

        /// <summary>Get or set message</summary>
        public string Msg
        { get { return m_ftp_msg; } set { m_ftp_msg = value; } }

        #endregion // Status from execution

        #region Result parameters

        /// <summary>String array with for instance file or directory names</summary>
        private string[] m_ftp_string_array = null;

        /// <summary>Get or set string array with for instance file or directory names</summary>
        public string[] ArrayStr
        { get { return m_ftp_string_array; } set { m_ftp_string_array = value; } }

        /// <summary>Boolean result</summary>
        private bool m_ftp_boolean_result = false;

        /// <summary>Get or set boolean result</summary>
        public bool BoolResult
        { get { return m_ftp_boolean_result; } set { m_ftp_boolean_result = value; } }

        #endregion // Result parameters

    } // Result

} // namespace
