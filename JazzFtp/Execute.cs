using System;
using System.Collections;
using System.IO;
using System.Net;

namespace JazzFtp
{
    /// <summary>FTP functions for upload, download, create, list and delete of files and directories on a server  
    /// <para>Internet connection can also be checked</para>
    /// </summary>
    public static class Execute
    {
        /// <summary>Runs an execution function defined by the execution case
        /// <para></para>
        /// </summary>
        /// <param name="i_input">Input data object for the execution</param>
        /// <returns>Returns a Result object</returns>
        public static Result Run(Input i_input)
        {
            Result ret_result = null;

            if (i_input.ExecCase == Input.Case.DirExists)
            {
                ret_result = DoesDirectoryExist(i_input);
            }
            else if (i_input.ExecCase == Input.Case.DirCreate)
            {
                ret_result = CreateDirectory(i_input);
            }
            else if (i_input.ExecCase ==  Input.Case.GetDirNames || i_input.ExecCase == Input.Case.GetFileNames || i_input.ExecCase == Input.Case.GetDirFileNames)
            {
                ret_result = GetServerDirectoryDirFileNames(i_input);
            }
            else if (i_input.ExecCase == Input.Case.FileExists)
            {
                ret_result = FileExists(i_input);
            }
            else if (i_input.ExecCase == Input.Case.UpLoadFile)
            {
                ret_result = UploadFile(i_input);
            }
            else if (i_input.ExecCase == Input.Case.CheckInternetConnection)
            {
                ret_result = CheckInternetConnection(i_input);
            }
            else if (i_input.ExecCase == Input.Case.RenameFile)
            {
                ret_result = RenameFile(i_input);
            }
            else if (i_input.ExecCase == Input.Case.RenameDir)
            {
                ret_result = RenameDirectory(i_input);
            }
            else if (i_input.ExecCase == Input.Case.DownloadFile)
            {
                ret_result = DownloadFile(i_input);
            }
            else if (i_input.ExecCase == Input.Case.DeleteFile)
            {
                ret_result = DeleteFile(i_input);
            }
            else if (i_input.ExecCase == Input.Case.DeleteFiles)
            {
                ret_result = DeleteFiles(i_input);
            }
            else if (i_input.ExecCase == Input.Case.DeleteDir)
            {
                ret_result = DeleteDirectory(i_input);
            }
            else if (i_input.ExecCase == Input.Case.DirEmpty)
            {
                ret_result = IsDirectoryEmpty(i_input);
            }
            else if (i_input.ExecCase == Input.Case.CreateCheckInternetConnection)
            {
                ret_result = CreateCheckInternetConnectionDirFile(i_input);
            }
            else
            {
                ret_result.Status = false;
                ret_result.ErrorMsg = @"Execute.Run Execution case not yet implemented";
            }
            
            return ret_result;

        } // Run 

        #region Download

        /// <summary>Download of one file
        /// <para></para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the download was succesful</returns>
        public static JazzFtp.Result DownloadFile(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            string i_file_server = copy_input.GetFileUri();

            string i_local_filename = copy_input.GetLocalFileName();

            copy_input.ExecCase = Input.Case.FileExists;

            Result ret_result_file_exists = Run(copy_input);
            if (!ret_result_file_exists.Status)
            {
                return ret_result_file_exists;
            }

            if (!ret_result_file_exists.BoolResult)
            {
                ret_result.Msg = copy_input.ErrMsgDownloadFileExistsNot;
                ret_result.Status = false;
                return ret_result;
            }

            try
            {
                if (File.Exists(i_local_filename))
                {
                    File.Delete(i_local_filename);
                }
            }
            catch (Exception e_e)
            {
                ret_result.ErrorMsg = @"Execute.DownloadFile File delete failed " + e_e.Message;

                ret_result.ErrorMsg = "\n\n\n" + @"Fenster bitte zumachen und noch einmal versuchen. Die temporäre Datei " + Path.GetFileName(i_local_filename) + @" kann nicht überschrieben werden";

                ret_result.Status = false;

                return ret_result;
            }
 

            int bytes_processed = 0;

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream remote_stream = null;
            Stream local_stream = null;
            WebResponse web_response = null;

            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            try
            {
                // string file_server = ReplaceSpaces(i_file_server);

                // Create a ftp_web_request for the specified remote file name              
                string str_uri = copy_input.GetFileUri();
                // Not necessary to create an Uri. Was tested once if useful to find uri address problem
                Uri uri_file_server = new Uri(str_uri);

                WebRequest web_request = WebRequest.Create(uri_file_server);

                if (web_request != null)
                {
                    web_request.Credentials = new NetworkCredential(copy_input.FtpUser, copy_input.FtpPassword);

                    // Send the ftp_web_request to the server and retrieve the
                    // WebResponse object 
                    web_response = web_request.GetResponse();
                    if (web_response != null)
                    {
                        // Once the WebResponse object has been retrieved,
                        // get the stream object associated with the web_response's data
                        remote_stream = web_response.GetResponseStream();

                        // Create the local file
                        local_stream = File.Create(i_local_filename);

                        // Allocate a 1k buffer
                        byte[] buffer = new byte[1024];
                        int bytes_read;

                        // Simple do/while loop to read from stream until
                        // no bytes are returned
                        do
                        {
                            // Read data (up to 1k) from the stream
                            bytes_read = remote_stream.Read(buffer, 0, buffer.Length);

                            // Write the data to the local file
                            local_stream.Write(buffer, 0, bytes_read);

                            // Increment total bytes processed
                            bytes_processed += bytes_read;
                        } while (bytes_read > 0);
                    }
                }
            }
            catch (Exception e)
            {
                ret_result.ErrorMsg = @"Execute.DownloadFile failed " + e.Message;
                ret_result.Status = false;
            }
            finally
            {
                // Close the web_response and streams objects here 
                // to make sure they're closed even if an exception
                // is thrown at some point
                if (web_response != null) web_response.Close();
                if (remote_stream != null) remote_stream.Close();
                if (local_stream != null) local_stream.Close();
            }

            ret_result.Msg = copy_input.ServerFileName + copy_input.MsgFileDownloaded;
            ret_result.Status = true;

            return ret_result;

        } // DownloadFile


        #endregion // Download

        #region Upload

        /// <summary>
        /// Upload file binary mode
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the upload was succesful</returns>
        private static JazzFtp.Result UploadFile(JazzFtp.Input i_input)
        {
            // Code is exctracted from: 
            // http://www.codeproject.com/Articles/17202/Simple-FTP-demo-application-using-C-Net-2-0

            JazzFtp.Result ret_result = new JazzFtp.Result();

            string local_filename_with_path = i_input.GetLocalFileName();

            if (!File.Exists(local_filename_with_path))
            {
                ret_result.ErrorMsg = i_input.ErrMsgUploadFileExistsNot + i_input.LocalFileName;
                ret_result.Status = false;
                return ret_result;
            }

            FtpWebRequest ftp_web_request = null;

            FileInfo file_info = new FileInfo(local_filename_with_path);
            string str_uri = i_input.GetFileUri();

            Uri object_uri = new Uri(str_uri);

            try
            {
                ftp_web_request = (FtpWebRequest)FtpWebRequest.Create(object_uri);
            }
            catch (Exception e)
            {
                ret_result.ErrorMsg = "Execute.UploadFile FtpWebRequest failed: " + e.Message;
                ret_result.Status = false;
                return ret_result;
            }
            if (null == ftp_web_request)
            {
                ret_result.ErrorMsg = "Execute.UploadFile Web ftp_web_request failed. Pointer is null.";
                ret_result.Status = false;
                return ret_result;
            }

            // Provide the WebPermission Credentials
            ftp_web_request.Credentials = new NetworkCredential(i_input.FtpUser, i_input.FtpPassword);

            // By default KeepAlive is true, where the control connection is not closed
            // after a command is executed.
            ftp_web_request.KeepAlive = false;

            // Specify the command to be executed.
            ftp_web_request.Method = WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            ftp_web_request.UseBinary = true;

            // Notify the server about the size of the uploaded file
            ftp_web_request.ContentLength = file_info.Length;

            // The buffer size is set to 2kb
            int buffer_size = 2048;
            byte[] read_buffer = new byte[buffer_size];
            int content_length;

            // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
            FileStream file_stream = null;

            // Stream to which the file to be upload is written
            Stream ftp_stream = null;

            try
            {
                file_stream = file_info.OpenRead();

                ftp_stream = ftp_web_request.GetRequestStream();

                // Read from the file stream 2kb at a time
                content_length = file_stream.Read(read_buffer, 0, buffer_size);

                // Till Stream content ends
                while (content_length != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    ftp_stream.Write(read_buffer, 0, content_length);
                    content_length = file_stream.Read(read_buffer, 0, buffer_size);
                }

                // Close the file stream and the Request Stream
                ftp_stream.Close();
                file_stream.Close();
            }
            catch (Exception ex)
            {
                ret_result.ErrorMsg = "Execute.UploadFile Upload failed. " + ex.Message;
                ret_result.Status = false;
                return ret_result;

            }

            finally
            {
                if (null != ftp_stream) ftp_stream.Close();
                if (null != file_stream) file_stream.Close();
            }

            ret_result.Msg = i_input.LocalFileName + i_input.MsgFileUploaded;
            return ret_result;

        } // UploadFile

        #endregion // Upload

        #region Delete files

        /// <summary>Delete of files on a server directory
        /// <para></para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the delete was succesful</returns>
        public static JazzFtp.Result DeleteFiles(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            copy_input.ExecCase = JazzFtp.Input.Case.GetFileNames;
            JazzFtp.Result result_file_names = Run(copy_input);
            if (!result_file_names.Status)
            {
                return result_file_names;
            }

            if (result_file_names.ArrayStr.Length == 0)
            {
                ret_result.ErrorMsg = copy_input.ErrMsgNoFilesOnDirectory + copy_input.ServerDirectory;
                ret_result.Status = true;
                ret_result.BoolResult = false;
                return ret_result;
            }

            string[] server_filenames = result_file_names.ArrayStr;
            

            for (int index_name = 0; index_name < server_filenames.Length; index_name++)
            {
                string server_file_name = server_filenames[index_name];

                copy_input.ServerFileName = server_file_name;
                copy_input.ExecCase = JazzFtp.Input.Case.DeleteFile;
                JazzFtp.Result ftp_result = JazzFtp.Execute.Run(copy_input);

                if (!ftp_result.Status)
                {
                    return ftp_result;
                }

            } // index_name

            ret_result.Status = true;
            ret_result.Msg = copy_input.MsgFilesDeleted + copy_input.ServerDirectory;

            return ret_result;

        } // DeleteFiles

        /// <summary>Delete of one file
        /// <para></para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the delete was succesful</returns>
        public static JazzFtp.Result DeleteFile(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            copy_input.ExecCase = Input.Case.FileExists;

            Result ret_result_file_exists = Run(copy_input);
            if (!ret_result_file_exists.Status)
            {
                return ret_result_file_exists;
            }

            if (!ret_result_file_exists.BoolResult)
            {
                ret_result.Msg = copy_input.ErrMsgDeleteFileExistsNot + copy_input.ServerFileName;
                ret_result.Status = false;
                return ret_result;
            }

            string ftp_response_str = "";

            WebRequest ftp_web_request = null;
            try
            {
                ftp_web_request = WebRequest.Create(copy_input.GetFileUri());

                ftp_web_request.Method = WebRequestMethods.Ftp.DeleteFile;

                ftp_web_request.Credentials = new NetworkCredential(copy_input.FtpUser, copy_input.FtpPassword);

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                string debug_str = "UpLoad.DeleteFile No error ftp_response_str= " + ftp_response_str;
                ret_result.Msg = copy_input.ServerFileName + copy_input.MsgFileDeleted;
                ret_result.Status = true;
            }
            catch (Exception e)
            {
                ret_result.Msg = "Execute.DeleteFile FtpWebRequest failed: " + e.Message;
                ret_result.Status = false;
                return ret_result;
            }

            return ret_result;

        } // DeleteFile

        #endregion // Delete files

        #region Directory functions

        /// <summary>
        /// Determines if a server directory already exists 
        /// <para>The function tries to list the content of a directory.</para>
        /// <para>If the directory is missing there an exception is thrown</para>
        /// <para>Please not that this function only works when there is an en slash of the input uri</para>
        /// <para></para>
        /// <para>Code from: https://stackoverflow.com/questions/2769137/how-to-check-if-an-ftp-directory-exists</para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with BoolResult=true if the server directory already exists </returns>
        private static JazzFtp.Result DoesDirectoryExist(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            string ftp_response_str = "";

            string directory_uri = i_input.GetDirectoryUri();

            WebRequest ftp_web_request = null;
            try
            {
                ftp_web_request = WebRequest.Create(directory_uri);

                ftp_web_request.Method = WebRequestMethods.Ftp.ListDirectory;

                ftp_web_request.Credentials = new NetworkCredential(i_input.FtpUser, i_input.FtpPassword);

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                ftp_response_str = "Directory exists: Response= " + ftp_response_str;

                ret_result.Msg = ftp_response_str;
            }
            catch (Exception e)
            {
                ftp_response_str = "Directory does not exist. Response= " + e.Message;
                ret_result.Status = true;
                ret_result.BoolResult = false;
                ret_result.Msg = i_input.MsgNoServerOrdnerWithTheGivenName + i_input.ServerDirectory;

                return ret_result;
            }

            ret_result.Status = true;
            ret_result.BoolResult = true;
            ret_result.Msg = i_input.MsgServerOrdnerWithTheGivenNameExists + i_input.ServerDirectory;

            return ret_result;

        } // DoesDirectoryExist

        /// <summary>
        /// Determines if a server directory is empty
        /// <para>The function tries to list the content of a directory.</para>
        /// <para>If the directory is missing there an exception is thrown</para>
        /// <para>Please not that this function only works when there is an en slash of the input uri</para>
        /// <para></para>
        /// <para>Code from: https://stackoverflow.com/questions/2769137/how-to-check-if-an-ftp-directory-exists</para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with BoolResult=true if the server directory already exists and is empty</returns>
        private static JazzFtp.Result IsDirectoryEmpty(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            copy_input.ExecCase = Input.Case.GetDirFileNames;
            JazzFtp.Result result_get_content = JazzFtp.Execute.Run(copy_input);
            if (!result_get_content.Status)
            {
                result_get_content.ErrorMsg = @"Execute.IsDirectoryEmpty JazzFtp.Execute.Run for Case.GetDirFileNames failed";
                result_get_content.Status = false;
                return result_get_content;
            }

            if (result_get_content.ArrayStr.Length == 0)
            {
                ret_result.BoolResult = true;
            }
            else
            {
                ret_result.BoolResult = false;
            }

            ret_result.Status = true;          
            ret_result.Msg = copy_input.ServerDirectory + copy_input.MsgServerOrdnerIsEmpty;

            return ret_result;

        } // DoesDirectoryExist

        /// <summary>
        /// Create directory on the server
        /// <para></para>
        /// <para></para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the directory creation was succesful</returns>
        private static JazzFtp.Result CreateDirectory(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            string directory_uri = i_input.GetDirectoryUri();

            string ftp_response_str = "";

            WebRequest ftp_web_request = null;
            try
            {
                ftp_web_request = WebRequest.Create(directory_uri);

                ftp_web_request.Method = WebRequestMethods.Ftp.MakeDirectory;

                ftp_web_request.Credentials = new NetworkCredential(i_input.FtpUser, i_input.FtpPassword);

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                ret_result.Msg = "UpLoad.CreateDirectory No error ftp_response_str= " + ftp_response_str;
            }
            catch (Exception e)
            {
                ret_result.ErrorMsg = "Execute.CreateDirectory FtpWebRequest failed: " + e.Message;
                ret_result.Status = false;
                return ret_result;
            }

            return ret_result;

        } // CreateDirectory

        /// <summary>
        /// Delete a directory on the server
        /// <para></para>
        /// <para></para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the directory deletion was succesful</returns>
        private static JazzFtp.Result DeleteDirectory(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            copy_input.ExecCase = Input.Case.DirExists;
            JazzFtp.Result result_exists = JazzFtp.Execute.Run(copy_input);
            if (!result_exists.BoolResult)
            {
                result_exists.ErrorMsg = copy_input.ErrMsgDeleteDirExistsNot + copy_input.ServerDirectory;
                result_exists.Status = false;
                return result_exists;
            }

            copy_input.ExecCase = Input.Case.DirEmpty;
            JazzFtp.Result result_empty = JazzFtp.Execute.Run(copy_input);
            if (!result_empty.BoolResult)
            {
                result_empty.ErrorMsg = copy_input.ErrMsgDeleteDirNotEmpty + copy_input.ServerDirectory;
                result_empty.Status = false;
                return result_empty;
            }

            string directory_uri = copy_input.GetDirectoryUri();

            string ftp_response_str = "";

            WebRequest ftp_web_request = null;
            try
            {
                ftp_web_request = WebRequest.Create(directory_uri);

                ftp_web_request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                ftp_web_request.Credentials = new NetworkCredential(copy_input.FtpUser, copy_input.FtpPassword);

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                string debug_str = "UpLoad.CreateDirectory No error ftp_response_str= " + ftp_response_str;
            }
            catch (Exception e)
            {
                ret_result.ErrorMsg = "Execute.DeleteDirectory FtpWebRequest failed: " + e.Message;
                ret_result.Status = false;
                return ret_result;
            }

            ret_result.Msg = copy_input.ServerDirectory + copy_input.MsgDirDeleted;
            ret_result.Status = true;

            return ret_result;

        } // DeleteDirectory

        /// <summary>
        /// Rename directory Does not work TODO
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the renaming was succesful</returns>
        private static JazzFtp.Result RenameDirectory(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            copy_input.ExecCase = Input.Case.DirExists;
            JazzFtp.Result result_exists = JazzFtp.Execute.Run(copy_input);
            if (!result_exists.BoolResult)
            {
                result_exists.ErrorMsg = copy_input.ErrMsgRenameDirExistsNot + copy_input.ServerDirectory;
                result_exists.Status = false;
                return result_exists;
            }

            string debug_full_server_dir_uri = copy_input.GetDirectoryUri();

            if (copy_input.ServerRenameDirectory.Length == 0)
            {
                ret_result.Msg = @"ServerRenameDirectory is not set";
                ret_result.Status = false;
                return ret_result;
            }

            FtpWebRequest ftp_web_request = null;
            string str_uri = copy_input.GetFileUri();

            // int str_uri_length = str_uri.Length;
            // str_uri = str_uri.Substring(0, str_uri_length - 1);

            Uri object_uri = new Uri(str_uri);

            try
            {
                //ftp_web_request = (FtpWebRequest)FtpWebRequest.Create(object_uri);
                ftp_web_request = (FtpWebRequest)FtpWebRequest.Create(str_uri);
            }
            catch (Exception e)
            {
                ret_result.ErrorMsg = "Execute.RenameDirectory FtpWebRequest failed: " + e.Message;
                ret_result.Status = false;
                return ret_result;
            }
            if (null == ftp_web_request)
            {
                ret_result.ErrorMsg = "Execute.RenameDirectory Web ftp_web_request failed. Pointer is null.";
                ret_result.Status = false;
                return ret_result;
            }

            // Provide the WebPermission Credentials
            ftp_web_request.Credentials = new NetworkCredential(copy_input.FtpUser, copy_input.FtpPassword);

            // By default KeepAlive is true, where the control connection is not closed
            // after a command is executed.
            //??? ftp_web_request.KeepAlive = false;

            // Specify the command to be executed.
            ftp_web_request.Method = WebRequestMethods.Ftp.Rename;

            string str_new_uri = copy_input.GetServerRenameDirectoryUri();
            int str_new_length = str_new_uri.Length;
            str_new_uri = str_new_uri.Substring(0, str_new_length - 1);

            // ftp_web_request.RenameTo = str_new_uri;

            ftp_web_request.RenameTo =  copy_input.GetServerRenameDirectoryUri();

            // ftp_web_request.RenameTo = @"/appdata/NyttNamn/";

            // ftp_web_request.RenameTo = @"/appdata/NyttNamn";

            // ftp_web_request.RenameTo = @"appdata/NyttNamn";

            // ftp_web_request.RenameTo = @"appdata/NyttNamn/";

            // ftp_web_request.RenameTo = @"/NyttNamn/";

            // ftp_web_request.RenameTo = @"/NyttNamn";

            // ftp_web_request.RenameTo = @"NyttNamn";

            string ftp_response_str = @"";

            try
            {

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                string debug_message = "Execute.RenameDirectory No error ftp_response_str= " + ftp_response_str;


            }
            catch (Exception e)
            {
                string debug_error = e.Message;
                ret_result.Msg = @"Execute.RenameDirectory Error: " + copy_input.ServerDirectory + " was not renamed to " + copy_input.ServerRenameDirectory;
                ret_result.Status = false;
                return ret_result;
            }

            // ftp://www.jazzliveaarau.ch/appdata/XxxxDevelopTestDirectoryNameChanged/
            // ftp://www.jazzliveaarau.ch/appdata/XxxxDevelopTestDirectoryThree/

            // Der angeforderte URI ist für diesen FTP - Befehl ungültig.

            // Der Remoteserver hat einen Fehler zurückgegeben: (451) Lokaler Verarbeitungsfehler.



            ret_result.Msg = copy_input.ServerDirectory + copy_input.MsgFileRenamed + copy_input.ServerRenameDirectory; // TODO Message
            ret_result.Status = true;
            return ret_result;

        } // RenameDirectory

        #endregion // Directory functions

        #region File name functions

        /// <summary>Get file and directory names from a server directory
        /// <para></para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with the file and directory names in ArrayStr</returns>
        private static JazzFtp.Result GetServerDirectoryDirFileNames(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            bool b_file_names = false;
            bool b_dir_names = false;

            if (i_input.ExecCase == Input.Case.GetDirFileNames)
            {
                b_dir_names = true;
                b_file_names = true;
            }
            else if (i_input.ExecCase == Input.Case.GetDirNames)
            {
                b_dir_names = true;
                b_file_names = false;
            }
            else if (i_input.ExecCase == Input.Case.GetFileNames)
            {
                b_dir_names = false;
                b_file_names = true;
            }

            string[] o_file_name_array = null;

            ArrayList file_names_array = new ArrayList();
            o_file_name_array = (string[])file_names_array.ToArray(typeof(string));

            FtpWebResponse response = null;
            StreamReader reader = null;
            string directory_list = "";

            string str_uri = i_input.GetDirectoryUri();

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(str_uri);

                request.Method = WebRequestMethods.Ftp.ListDirectory;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(i_input.FtpUser, i_input.FtpPassword);

                response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                reader = new StreamReader(responseStream);
                directory_list = reader.ReadToEnd();

                reader.Close();
                response.Close();

            }

            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }

  
                ret_result.ErrorMsg = ex.Message;
                ret_result.Status = false;
                return ret_result;
            }

            directory_list = directory_list + "";

            o_file_name_array = _DirectoryContentToArray(directory_list, b_file_names, b_dir_names, i_input.ServerDirectory);

            ret_result.ArrayStr = o_file_name_array;

            return ret_result;

        } // GetServerDirectoryDirFileNames

        /// <summary>Determines if a file exists on a server directory
        /// <para></para>
        /// <para></para>
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the file exists</returns>
        private static JazzFtp.Result FileExists(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            copy_input.ExecCase = Input.Case.GetFileNames;
            JazzFtp.Result result_dir_file_names = Run(copy_input);
            if (!result_dir_file_names.Status)
            {
                return result_dir_file_names;
            }

            string[] file_names = result_dir_file_names.ArrayStr;

            bool b_exists = false;
            for (int index_name=0; index_name<file_names.Length; index_name++)
            {
                string current_name = file_names[index_name];

                if (current_name.Equals(copy_input.ServerFileName))
                {
                    b_exists = true;
                    break;
                }

            } // index_name

            ret_result.Status = true;
            ret_result.BoolResult = b_exists;

            return ret_result;

        } // FileExists

        /// <summary>
        /// Rename file 
        /// </summary>
        /// <param name="i_input">FTP input object</param>
        /// <returns>Returns a Result object with Status=true if the renaming was succesful</returns>
        private static JazzFtp.Result RenameFile(JazzFtp.Input i_input)
        {
            JazzFtp.Result ret_result = new JazzFtp.Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            string debug_full_server_file_uri = copy_input.GetFileUri();

            copy_input.ExecCase = Input.Case.FileExists;

            Result ret_result_file_exists = Run(copy_input);
            if (!ret_result_file_exists.Status)
            {
                return ret_result_file_exists;
            }

            if (!ret_result_file_exists.BoolResult)
            {
                ret_result.Msg = copy_input.ErrMsgRenameFileExistsNot;
                ret_result.Status = false;
                return ret_result;
            }

            if (copy_input.ServerRenameFileName.Length == 0)
            {
                ret_result.Msg = @"ServerRenameFileName is not set";
                ret_result.Status = false;
                return ret_result;
            }

            // TODO If rename directory exists, then it would mean that the file shall be moved
            // TODO This can only be implemented as first download and then upload to the other directory
            //

            FtpWebRequest ftp_web_request = null;
            string str_uri = copy_input.GetFileUri();

            Uri object_uri = new Uri(str_uri);

            try
            {
                ftp_web_request = (FtpWebRequest)FtpWebRequest.Create(object_uri);
            }
            catch (Exception e)
            {
                ret_result.ErrorMsg = "Execute.RenameFile FtpWebRequest failed: " + e.Message;
                ret_result.Status = false;
                return ret_result;
            }
            if (null == ftp_web_request)
            {
                ret_result.ErrorMsg = "Execute.RenameFile Web ftp_web_request failed. Pointer is null.";
                ret_result.Status = false;
                return ret_result;
            }

            // Provide the WebPermission Credentials
            ftp_web_request.Credentials = new NetworkCredential(copy_input.FtpUser, copy_input.FtpPassword);

            // By default KeepAlive is true, where the control connection is not closed
            // after a command is executed.
            //??? ftp_web_request.KeepAlive = false;

            // Specify the command to be executed.
            ftp_web_request.Method = WebRequestMethods.Ftp.Rename;

            ftp_web_request.RenameTo = copy_input.ServerRenameFileName;

            string ftp_response_str = @"";

            try
            {

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                string debug_message = "Execute.RenameFile No error ftp_response_str= " + ftp_response_str;

                
            }
            catch (Exception e)
            {
                string debug_error = e.Message;
                ret_result.Msg = @"Execute.Rename Error: " + copy_input.ServerFileName + " was not renamed to " + copy_input.ServerRenameFileName;
                ret_result.Status = false;
                return ret_result;
            }


            ret_result.Msg = copy_input.ServerFileName + copy_input.MsgFileRenamed + copy_input.ServerRenameFileName;
            ret_result.Status = true;
            return ret_result;

        } // RenameFile

        #endregion // File name functions

        #region Utility functions

        /// <summary>Returns a copy of the input Input object</summary>
        private static Input InputCopy(Input i_input)
        {
            Input ret_input = new Input();

            ret_input = i_input;

            return ret_input;
        } // InputCopy

        /// <summary>Convert directory content string to an array of file names and/or directory names.</summary>
        private static string[] _DirectoryContentToArray(string i_directory_list, bool i_b_file_names, bool i_b_dir_names, string i_server_dir)
        {
            string[] ret_array = null;

            ArrayList ret_array_list = new ArrayList();

            bool b_file_name = false;
            string file_name = "";

            string directory_list = i_directory_list.Substring(6);

            for (int i_char = 0; i_char < directory_list.Length; i_char++)
            {
                string current_char = directory_list.Substring(i_char, 1);

                if ("\n" == current_char && false == b_file_name)
                {
                    b_file_name = true;
                    file_name = "";
                }
                else if ("\r" == current_char && true == b_file_name)
                {
                    b_file_name = false;

                    /*
                    if (!IfDirectory(file_name))
                    {
                        ret_array_list.Add(file_name);
                    }
                    */
                    if (i_b_file_names && i_b_dir_names)
                    {
                        ret_array_list.Add(file_name);
                    }
                    else
                    {
                        bool b_is_dir = IsDirectory(file_name, i_server_dir);
                    
                        if (b_is_dir && i_b_dir_names)
                        {
                            ret_array_list.Add(file_name);
                        }
                        else if (!b_is_dir && i_b_file_names)
                        {
                            ret_array_list.Add(file_name);
                        }
                    }                

                    file_name = "";
                }
                else if (true == b_file_name)
                {
                    file_name = file_name + current_char;
                }

            }
            /*
            ".\r\n..\r\nPlakatNewsLetterJubilaeum_B.jpg\r\nPlakatNewsLetterJubilaeum_D.jpg\r\nPlakatNewsletter20120211.jpg\r\nPlakatNewsletter20120225.jpg
             \r\nPlakatNewsletter20120310.jpg\r\nPlakatNewsletter20120324.jpg\r\nPlakatNewsletterJubilaeum_F.jpg\r\n" 
             */

            ret_array = (string[])ret_array_list.ToArray(typeof(string));

            return ret_array;
        } // _DirectoryContentToArray

        /// <summary>Determines wether it is a directory or a file. Bad criterion is used.</summary>
        private static bool IfDirectory(string i_file_name)
        {
            if (i_file_name.Contains(@"."))
                return false;
            else
                return true;
        } // IfDirectory

        /// <summary>Determines wether it is a directory or a file.</summary>
        private static bool IsDirectory(string i_file_name, string i_server_dir)
        {
            
            if (i_file_name.Contains(@".txt"))
                return false;
            else if (i_file_name.Contains(@".doc"))
                return false;
            else if (i_file_name.Contains(@".docx"))
                return false;
            else if (i_file_name.Contains(@".pdf"))
                return false;
            else if (i_file_name.Contains(@".jpg"))
                return false;
            else if (i_file_name.Contains(@".JPG"))
                return false;
            else if (i_file_name.Contains(@".bmp"))
                return false;
            else if (i_file_name.Contains(@".png"))
                return false;
            else if (i_file_name.Contains(@".gif"))
                return false;
            else if (i_file_name.Contains(@".xls"))
                return false;
            else if (i_file_name.Contains(@".xlsx"))
                return false;
            else if (i_file_name.Contains(@".mp3"))
                return false;
            else if (i_file_name.Contains(@".mp4"))
                return false;
            else if (i_file_name.Contains(@".m4a"))
                return false;
            else if (i_file_name.Contains(@".wav"))
                return false;
            else if (i_file_name.Contains(@".htm"))
                return false;
            else if (i_file_name.Contains(@".html"))
                return false;
            else if (i_file_name.Contains(@".cs"))
                return false;
            else if (i_file_name.Contains(@".js"))
                return false;
            else if (i_file_name.Contains(@".zip"))
                return false;
            else if (i_file_name.Contains(@".xml"))
                return false;
            else if (i_file_name.Contains(@".xsd"))
                return false;
            else if (i_file_name.Contains(@".ppt"))
                return false;
            else if (i_file_name.Contains(@".pptx"))
                return false;
            else
            {
                JazzFtp.Input ftp_input = new JazzFtp.Input("Main.ExeDirectory", Input.Case.DirExists);

                ftp_input.ServerDirectory = i_server_dir + @"/" + i_file_name;

                JazzFtp.Result ftp_result = JazzFtp.Execute.Run(ftp_input);

                if (ftp_result.BoolResult)
                {                    
                    return true;
                }
                else
                {
                    return false;
                }
            }

        } // IfDirectory

        #endregion // Utility functions

        #region Internet connection

        /// <summary>Determine if the there is a connection to Internet
        /// <para>A directory CheckInternetServerDirectory and a file CheckInternetFileName are used to determine if there is an Internet connection</para>
        /// <para>(The directory and the file are created with execution case CreateCheckConnection)</para>
        /// <para>Check if name of Internet check file CheckInternetFileName can be retrieved. Call of Run, execution case FileExists</para>
        /// <para>Return true if file name could be retrieved.</para>
        /// </summary>
        /// <param name="i_input">Input data object for the execution</param>
        /// <returns>Returns a Result object with Status=true if there is Internet connection</returns>
        private static Result CheckInternetConnection(Input i_input)
        {
            Result ret_result = new Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            copy_input.ServerDirectory = copy_input.CheckInternetServerDirectory;
            copy_input.ServerFileName = copy_input.CheckInternetFileName;

            copy_input.ExecCase = Input.Case.FileExists;

            Result ret_result_file_exists = Run(copy_input);
            if (!ret_result_file_exists.Status)
            {
                return ret_result_file_exists;
            }

            if (ret_result_file_exists.BoolResult)
            {
                ret_result.Msg = copy_input.MsgInternetConnection;
                ret_result.BoolResult = true;
                ret_result.Status = true;
            }
            else
            {
                ret_result.ErrorMsg = copy_input.MsgNoInternetConnection;
                ret_result.BoolResult = false;
                ret_result.Status = true;
            }

            return ret_result;

        } // CheckInternetConnection


        #endregion // Internet connection

        #region Create Internet check directory and file

        /// <summary>Create the directory and the file for the server which are used to determine if there is an Internet connection
        /// <para>This function is normally only used once for an application like for instance Admin</para>
        /// <para>1. Create a temporary local (check) file in the application execution directory.</para>
        /// <para>2. Check if server directory already exists. Call of Run, execution case DirExists</para>
        /// <para>3. Create directory if not existing. Call of Run, execution case DirCreate</para>
        /// <para>4. Upload file if not already on the server. Calls of Run, execution cases FileExists and UpLoadFile</para>
        /// <para></para>
        /// </summary>
        /// <param name="i_input">Input data object for the execution</param>
        /// <returns>Returns a Result object with Status=true if the creation was sucessful</returns>
        private static Result CreateCheckInternetConnectionDirFile(Input i_input)
        {
            Result ret_result = new Result();

            JazzFtp.Input copy_input = InputCopy(i_input);

            string local_check_file_name = copy_input.CheckInternetFileName;
            string local_dir = copy_input.ExeDirectory;
            string file_content = copy_input.ContentCheckInternetFile;
            string full_file_name = local_dir + @"\" + local_check_file_name;
            File.WriteAllText(full_file_name, file_content);

            copy_input.LocalFileName = local_check_file_name;
            copy_input.LocalDirectory = @"";
            copy_input.ServerFileName = local_check_file_name;
            copy_input.ServerDirectory = copy_input.CheckInternetServerDirectory;
            copy_input.ExecCase = Input.Case.DirExists;

            JazzFtp.Result result_dir_exists = Run(copy_input);
            if (!result_dir_exists.Status)
            {
                File.Delete(full_file_name);

                return result_dir_exists;
            }


            if (!result_dir_exists.BoolResult)
            {
                copy_input.ExecCase = Input.Case.DirCreate;
                JazzFtp.Result result_dir_create = Run(copy_input);
                if (!result_dir_create.Status)
                {
                    File.Delete(full_file_name);

                    return result_dir_create;
                }  
            }

            copy_input.ExecCase = Input.Case.FileExists;
            JazzFtp.Result result_file_exists = Run(copy_input);
            if (!result_file_exists.Status)
            {
                File.Delete(full_file_name);

                return result_file_exists;
            }

            if (result_file_exists.BoolResult)
            {
                ret_result.Msg = @"Execute.CreateCheckInternetConnectionDirFile No file is created. It already exists";

                File.Delete(full_file_name);

                return ret_result;
            }

            copy_input.ExecCase = Input.Case.UpLoadFile;
            JazzFtp.Result result_upload_file = Run(copy_input);
            if (!result_upload_file.Status)
            {
                File.Delete(full_file_name);

                return result_upload_file;
            }

            File.Delete(full_file_name);

            ret_result.Msg = @"Execute.CreateCheckInternetConnectionDirFile The file is created";
            return ret_result;

        } // CreateCheckInternetConnectionDirFile

        #endregion // Create Internet check directory and file    


        /// <summary>Returns true if connection is available</summary>
        public static bool IsConnectionAvailable(string i_path_exe_dir, out string o_status_message)
        {
            o_status_message = @"";

            JazzFtp.Input ftp_input = new JazzFtp.Input(i_path_exe_dir, JazzFtp.Input.Case.CheckInternetConnection);

            JazzFtp.Result ftp_result = JazzFtp.Execute.Run(ftp_input);

            if (!ftp_result.Status)
            {
                // Programming error
                o_status_message = @"JazzFtp.Execute.IsConnectionAvailable JazzFtp.Execute.Run failed " + ftp_result.ErrorMsg;
                
                return false;
            }

            return ftp_result.BoolResult;

        } // IsConnectionAvailable

    } // Execute

} // namespace
