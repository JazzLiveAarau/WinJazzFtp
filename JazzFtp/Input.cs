using System;
using System.Collections.Generic;
using System.Text;

namespace JazzFtp
{
    /// <summary>Input data for FTP execute functions  
    /// <para>Default connection member parameter values are set for the JAZZ live AARAU server</para>
    /// <para>File and folder data must be set depending on the execution case</para>
    /// <para></para>
    /// <para></para>
    /// </summary>
    public class Input
    {
        #region Execution case

        /// <summary>Defines the execution cases</summary>
        public enum Case
        {
            DownloadFile,
            DownloadFiles,
            UpLoadFile,
            UpLoadFiles,
            DeleteFile,
            DeleteFiles,
            DeleteDir,
            RenameFile,
            RenameDir,
            GetFileNames,
            GetDirNames,
            GetDirFileNames,
            DirCreate,
            DirExists,
            DirEmpty,
            FileExists,
            CheckInternetConnection,
            CreateCheckInternetConnection,
            Undefined
        };

        /// <summary>Execution case</summary>
        private Case m_ftp_exec_case = Case.Undefined;

        /// <summary>Get or set execution case</summary>
        public Case ExecCase
        { get { return m_ftp_exec_case; } set { m_ftp_exec_case = value; } }

        #endregion // Execution case

        #region Connection data

        /// <summary>FTP host</summary>
        private string m_ftp_host = "www.jazzliveaarau.ch";

        /// <summary>Get or set the FTP host</summary>
        public string FtpHost
        { get { return m_ftp_host; } set { m_ftp_host = value; } }

        /// <summary>FTP user</summary>
        private string m_ftp_user = "jazzliv1";

        /// <summary>Get or set the FTP user</summary>
        public string FtpUser
        { get { return m_ftp_user; } set { m_ftp_user = value; } }

        /// <summary>FTP password for the download and upload</summary>
        private string m_ftp_password = "42SN4bX9";

        /// <summary>Get or set the FTP password</summary>
        public string FtpPassword
        { get { return m_ftp_password; } set { m_ftp_password = value; } }

        #endregion // Connection data

        #region File and folder data

        /// <summary>The server url start part</summary>
        private string m_server_url_start_path = "ftp://";

        /// <summary>Get the server url start part</summary>
        private string ServerFileUrlStartPath
        { get { return m_server_url_start_path; } }

        /// <summary>Name of the server file (without path)</summary>
        private string m_server_file_name = "";

        /// <summary>Get or set the name of the server file (without path)</summary>
        public string ServerFileName
        { get { return m_server_file_name; } set { m_server_file_name = value; } }

        /// <summary>Name of the server directory</summary>
        private string m_server_directory = @"";

        /// <summary>Get or set the name of the server directory</summary>
        public string ServerDirectory
        { get { return m_server_directory; } set { m_server_directory = value; } }

        /// <summary>Name of the local file (without path)</summary>
        private string m_local_file_name = "";

        /// <summary>Get or set the name of the local file (without path)</summary>
        public string LocalFileName
        { get { return m_local_file_name; } set { m_local_file_name = value; } }

        /// <summary>Name of the local (exe) subdirectory. Value must not be set.</summary>
        private string m_local_directory = @"";

        /// <summary>Get or set the name of the local (exe) subdirectory. Value must not be set.</summary>
        public string LocalDirectory
        { get { return m_local_directory; } set { m_local_directory = value; } }

        /// <summary>Path to the exe directory.</summary>
        private string m_exe_directory = @"";

        /// <summary>Get or set the name of the exe directory</summary>
        public string ExeDirectory
        { get { return m_exe_directory; } set { m_exe_directory = value; } }

        /// <summary>Name of the rename server file (without path)</summary>
        private string m_server_rename_file_name = "";

        /// <summary>Get or set the name of the rename server file (without path)</summary>
        public string ServerRenameFileName
        { get { return m_server_rename_file_name; } set { m_server_rename_file_name = value; } }

        /// <summary>Name of the server directory</summary>
        private string m_server_rename_directory = @"";

        /// <summary>Get or set the name of the server directory</summary>
        public string ServerRenameDirectory
        { get { return m_server_rename_directory; } set { m_server_rename_directory = value; } }

        #endregion // File and folder data

        #region Check Internet connection

        /// <summary>Name of the internet connection check file</summary>
        private string m_server_internet_connection_file_name = "InternetConnectionCheckFile.txt";

        /// <summary>Get or set the name of the internet connection check file</summary>
        public string CheckInternetFileName
        { get { return m_server_internet_connection_file_name; } set { m_server_internet_connection_file_name = value; } }

        /// <summary>Name of the server internet check connection directory</summary>
        private string m_server_internet_connection_directory = @"www/InternetConnectionCheckDirectory";

        /// <summary>Get or set the server internet check connection directory</summary>
        public string CheckInternetServerDirectory
        { get { return m_server_internet_connection_directory; } set { m_server_internet_connection_directory = value; } }

        /// <summary>Text for the file m_server_internet_connection_file_name</summary>
        private string m_content_internet_connection_file = @"Please do not delete this file nor the directory. " + "\r\n" +
                                                            @"Please also don't put any other files in this directory. " + "\r\n" +
                                                            @"The file and the directory are used to check the Internet connection";

        /// <summary>Get the text for the file with the name CheckInternetFileName</summary>
        public string ContentCheckInternetFile
        { get { return m_content_internet_connection_file; }  }

        #endregion // Check Internet connection

        #region Utility functions

        /// <summary>Returns the full URI as string for the server directory</summary>
        public string GetDirectoryUri()
        {
            string ret_uri = ServerFileUrlStartPath + FtpHost;
            if (ServerDirectory.Length > 0)
            {
                ret_uri = ret_uri + "/" + ServerDirectory + "/";
            }

            return ret_uri;

        } // GetDirectoryUri

        /// <summary>Returns the full URI as string for the server file</summary>
        public string GetFileUri()
        {
            string ret_uri = GetDirectoryUri() + ServerFileName;

            return ret_uri;

        } // GetFileUri

        /// <summary>Returns the full name for the local directory
        /// <para>ExeDirectory may be empty (or an URL containing :\\ for instance C:\\)</para>
        /// <para>LocalDirectory may be empty if ExeDirectory is defined. Case 1</para>
        /// <para>LocalDirectory may be a subdirectory if ExeDirectory is defined. Case 2</para>
        /// <para>LocalDirectory may be a full URL (containing :\\) if ExeDirectory not is defined. Case 3</para>
        /// <para>Please note that for the case that a local subdirectory not exists, the file will not be downloaded and there is no execution error</para>
        /// </summary>
        public string GetLocalDirectoryName()
        {
            string ret_name = "";

            bool b_exec_dir_full_address = false;

            bool b_exec_dir_empty = true;

            if (ExeDirectory.Trim().Length > 0)
            {
                b_exec_dir_empty = false;

                b_exec_dir_full_address = ExeDirectory.Contains(":\\");
            }           

            bool b_local_dir_full_address = false;

            bool b_local_dir_empty = true;

            if (LocalDirectory.Trim().Length > 0)
            {
                b_local_dir_empty = false;

                b_local_dir_full_address = LocalDirectory.Contains(":\\");
            }

            if (b_exec_dir_empty && b_local_dir_empty)
            {
                ret_name = "Input.GetLocalDirectoryName Error: ExeDirectory and LocalDirectory are empty";

                return ret_name;
            }

            if (!b_exec_dir_empty && b_local_dir_empty && !b_exec_dir_full_address)
            {
                ret_name = "Input.GetLocalDirectoryName Error: LocalDirectory is empty and ExeDirectory is not a full address";

                return ret_name;
            }

            if (b_local_dir_empty && !b_exec_dir_empty && b_exec_dir_full_address) // Case 1
            {
                ret_name = ret_name + ExeDirectory.Trim() + @"\";
            }
            else if (!b_local_dir_empty && !b_local_dir_full_address && !b_exec_dir_empty && b_exec_dir_full_address) // Case 2
            {
                ret_name = ret_name + ExeDirectory.Trim() + @"\" + LocalDirectory.Trim() + @"\";
            }
            else if (!b_local_dir_empty && b_local_dir_full_address && b_exec_dir_empty) // Case 3
            {
                ret_name = ret_name + LocalDirectory.Trim() + @"\";
            }
            else
            {
                ret_name = "Input.GetLocalDirectoryName Programming error";

                return ret_name;
            }

            return ret_name;

        } // GetLocalDirectoryName

        /// <summary>Returns the full URI as string for the server rename directory</summary>
        public string GetServerRenameDirectoryUri()
        {
            string ret_uri = ServerFileUrlStartPath + FtpHost;
            if (ServerRenameDirectory.Length > 0)
            {
                ret_uri = ret_uri + "/" + ServerRenameDirectory + "/";
            }

            return ret_uri;

        } // GetDirectoryUri

        // 

        /// <summary>Returns the full  name for the local file</summary>
        public string GetLocalFileName()
        {
            string ret_file_name = GetLocalDirectoryName() + LocalFileName;

            return ret_file_name;

        } // GetLocalFileName

        #endregion // Utility functions

        #region Constructor

        /// <summary>Constructor setting the execution directory parameter value
        /// <para></para>
        /// </summary>
        /// <param name="i_exe_directory">Execution directory</param>
        public Input(string i_exe_directory)
        {
            m_exe_directory = i_exe_directory;

            m_ftp_exec_case = Case.Undefined;

        } // Constructor

        /// <summary>Constructor setting the connection data, execution directory and execution case parameter values
        /// <para></para>
        /// </summary>
        /// <param name="i_ftp_host">FTP host</param>
        /// <param name="i_ftp_user">FTP user</param>
        /// <param name="i_ftp_password">FTP password</param>
        /// <param name="i_exe_directory">Execution directory</param>
        /// <param name="i_case">Execution case</param>
        public Input(string i_ftp_host, string i_ftp_user, string i_ftp_password, string i_exe_directory, Case i_case)
        {
            FtpHost = i_ftp_host;

            FtpUser = i_ftp_user;

            FtpPassword = i_ftp_password;

            m_exe_directory = i_exe_directory;

            m_ftp_exec_case = i_case;

        } // Constructor

        /// <summary>Constructor setting the connection data and execution case parameter values
        /// <para></para>
        /// </summary>
        /// <param name="i_ftp_host">FTP host</param>
        /// <param name="i_ftp_user">FTP user</param>
        /// <param name="i_ftp_password">FTP password</param>
        /// <param name="i_case">Execution case</param>
        public Input(string i_ftp_host, string i_ftp_user, string i_ftp_password, Case i_case)
        {
            FtpHost = i_ftp_host;

            FtpUser = i_ftp_user;

            FtpPassword = i_ftp_password;

            m_ftp_exec_case = i_case;

        } // Constructor

        /// <summary>Constructor setting the connection data
        /// <para></para>
        /// </summary>
        /// <param name="i_ftp_host">FTP host</param>
        /// <param name="i_ftp_user">FTP user</param>
        /// <param name="i_ftp_password">FTP password</param>
        public Input(string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            FtpHost = i_ftp_host;

            FtpUser = i_ftp_user;

            FtpPassword = i_ftp_password;

        } // Constructor

        /// <summary>Constructor setting the execution directory and execution case parameter values
        /// <para></para>
        /// </summary>
        /// <param name="i_exe_directory">Execution directory</param>
        ///  <param name="i_case">Execution case</param>
        public Input(string i_exe_directory, Case i_case)
        {
            m_exe_directory = i_exe_directory;

            m_ftp_exec_case = i_case;

        } // Constructor

        /// <summary>Constructor 
        /// <para></para>
        /// </summary>
        public Input()
        {

        } // Constructor

        #endregion // Constructor

        #region Messages

        /// <summary>Message: File is uploaded</summary>
        private string m_msg_file_uploaded = @" ist auf dem Server gespeichert";

        /// <summary>Get or set the message: File is uploaded</summary>
        public string MsgFileUploaded
        { get { return m_msg_file_uploaded; } set { m_msg_file_uploaded = value; } }

        /// <summary>Message: File is downloaded</summary>
        private string m_msg_file_downloaded = @" ist heruntergeladen";

        /// <summary>Get or set the message: File is downloaded</summary>
        public string MsgFileDownloaded
        { get { return m_msg_file_downloaded; } set { m_msg_file_downloaded = value; } }

        /// <summary>Message: File is deleted</summary>
        private string m_msg_file_deleted = @" ist gelöscht";

        /// <summary>Get or set the message: File is deleted</summary>
        public string MsgFileDeleted
        { get { return m_msg_file_deleted; } set { m_msg_file_deleted = value; } }

        /// <summary>Message: Files are deleted</summary>
        private string m_msg_files_deleted = @"Dateien sind gelöscht im Ordner ";

        /// <summary>Get or set the message: Files are deleted</summary>
        public string MsgFilesDeleted
        { get { return m_msg_files_deleted; } set { m_msg_files_deleted = value; } }

        /// <summary>Message: There is connection to Internet</summary>
        private string m_msg_internet_connection = @"Verbindung zu Internet ist vorhanden";

        /// <summary>Get or set the message: There is connection to Internet</summary>
        public string MsgInternetConnection
        { get { return m_msg_internet_connection; } set { m_msg_internet_connection = value; } }

        /// <summary>Message: No connection to Internet is available</summary>
        private string m_msg_no_internet_connection = @"Keine Verbindung zu Internet ist vorhanden";

        /// <summary>Get or set the message: No connection to Internet is available </summary>
        public string MsgNoInternetConnection
        { get { return m_msg_no_internet_connection; } set { m_msg_no_internet_connection = value; } }

        /// <summary>Message: There is no server directory with the given name</summary>
        private string m_msg_folder_exists_not = @"Es gibt kein Serverordner ";

        /// <summary>Get or set the message: There is no server directory with the given name</summary>
        public string MsgNoServerOrdnerWithTheGivenName
        { get { return m_msg_folder_exists_not; } set { m_msg_folder_exists_not = value; } }

        /// <summary>Message: There is a server directory with the given name</summary>
        private string m_msg_folder_exists = @"Es gibt ein Serverordner ";

        /// <summary>Get or set the message: There is a server directory with the given name</summary>
        public string MsgServerOrdnerWithTheGivenNameExists
        { get { return m_msg_folder_exists; } set { m_msg_folder_exists = value; } }

        /// <summary>Message: The server directory is empty</summary>
        private string m_msg_folder_is_empty = @" Ordner ist leer ";

        /// <summary>Get or set the message: The server directory is empty</summary>
        public string MsgServerOrdnerIsEmpty
        { get { return m_msg_folder_is_empty; } set { m_msg_folder_is_empty = value; } }

        /// <summary>Message: File is renamed</summary>
        private string m_msg_file_renamed = @" Name geändert zu ";

        /// <summary>Get or set the message: File is renamed</summary>
        public string MsgFileRenamed
        { get { return m_msg_file_renamed; } set { m_msg_file_renamed = value; } }

        /// <summary>Message: Directory is deleted</summary>
        private string m_msg_dir_deleted = @" Ordner ist gelöscht";

        /// <summary>Get or set the message: Directory is deleted</summary>
        public string MsgDirDeleted
        { get { return m_msg_dir_deleted; } set { m_msg_dir_deleted = value; } }

        #endregion // Messages

        #region Error messages

        /// <summary>Error message: Failure downloading file </summary>
        private string m_error_msg_failure_downloading_file = @"Fehler beim Herunterladen von Datei ";

        /// <summary>Get or set the error message: Failure downloading file </summary>
        public string ErrMsgFileDownloadFailed
        { get { return m_error_msg_failure_downloading_file; } set { m_error_msg_failure_downloading_file = value; } }

        /// <summary>Error message: Download file does not exist</summary>
        private string m_error_msg_download_file_exists_not = @"Fehler beim Herunterladen. Nicht existierende Datei ";

        /// <summary>Get or set the error message: Download file does not exist</summary>
        public string ErrMsgDownloadFileExistsNot
        { get { return m_error_msg_download_file_exists_not; } set { m_error_msg_download_file_exists_not = value; } }

        /// <summary>Error message: Download file does not exist</summary>
        private string m_error_msg_delete_file_exists_not = @"Fehler beim Löschen. Nicht existierende Datei ";

        /// <summary>Get or set the error message: Download file does not exist</summary>
        public string ErrMsgDeleteFileExistsNot
        { get { return m_error_msg_delete_file_exists_not; } set { m_error_msg_delete_file_exists_not = value; } }

        /// <summary>Error message: Failure uploading  file </summary>
        private string m_error_msg_upload_file = @"Fehler beim Hochladen von Datei ";

        /// <summary>Get or set the error message: Failure uploading file </summary>
        public string ErrMsgUploadfile
        { get { return m_error_msg_upload_file; } set { m_error_msg_upload_file = value; } }

        /// <summary>Error message: Uploading file does not exist</summary>
        private string m_error_msg_upload_file_exists_not = @"Fehler beim Hochladen. Nicht existierende Datei ";

        /// <summary>Get or set the error message: Uploading file does not exist</summary>
        public string ErrMsgUploadFileExistsNot
        { get { return m_error_msg_upload_file_exists_not; } set { m_error_msg_upload_file_exists_not = value; } }

        /// <summary>Error message: Renaming file does not exist</summary>
        private string m_error_msg_rename_file_exists_not = @"Fehler beim Umbenennen. Nicht existierende Datei ";

        /// <summary>Get or set the error message: Renaming file does not exist </summary>
        public string ErrMsgRenameFileExistsNot
        { get { return m_error_msg_rename_file_exists_not; } set { m_error_msg_rename_file_exists_not = value; } }

        /// <summary>Error message: No files in the directory</summary>
        private string m_error_msg_no_files_on_directory = @"Keine Dateien im Ordner ";

        /// <summary>Get or set the error message: No files in the directory</summary>
        public string ErrMsgNoFilesOnDirectory
        { get { return m_error_msg_no_files_on_directory; } set { m_error_msg_no_files_on_directory = value; } }

        /// <summary>Error message: Deletion directory does not exist</summary>
        private string m_error_msg_delete_dir_exists_not = @"Fehler beim Löschen. Nicht existierende Ordner ";

        /// <summary>Get or set the error message: Deletion directory does not exist</summary>
        public string ErrMsgDeleteDirExistsNot
        { get { return m_error_msg_delete_dir_exists_not; } set { m_error_msg_delete_dir_exists_not = value; } }

        /// <summary>Error message: Rename directory does not exist</summary>
        private string m_error_msg_rename_dir_exists_not = @"Fehler beim Umbenennen. Nicht existierende Ordner ";

        /// <summary>Get or set the error message: Rename directory does not exist</summary>
        public string ErrMsgRenameDirExistsNot
        { get { return m_error_msg_rename_dir_exists_not; } set { m_error_msg_rename_dir_exists_not = value; } }


        /// <summary>Error message: Deletion directory does not exist</summary>
        private string m_error_msg_delete_dir_not_empty = @"Fehler beim Löschen. Es gibt Elemente im Ordner ";

        /// <summary>Get or set the error message: Deletion directory does not exist</summary>
        public string ErrMsgDeleteDirNotEmpty
        { get { return m_error_msg_delete_dir_not_empty; } set { m_error_msg_delete_dir_not_empty = value; } }


        #endregion // Error messages

        #region Check data

        /// <summary>Checks the member parameter values
        /// <para></para>
        /// </summary>
        /// <param name="o_error">Error message</param>
        /// <returns>Returns true if data is OK</returns>
        public bool Check(out string o_error)
        {
            bool ret_check = true;
            o_error = @"";

            if (m_ftp_exec_case.Equals(Case.Undefined))
            {
                o_error = @"Input.Check Execution case is not set";
                return false;
            }

            if (m_exe_directory.Trim().Equals(@""))
            {
                o_error = @"Input.Check Exe directory is not set";
                return false;
            }

            return ret_check;

        } // Check

        #endregion // Check data

    } // Input

} // namespace
