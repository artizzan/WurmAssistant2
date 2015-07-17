using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FtpHelpers
{
    public class FtpClient
    {
        private readonly string _host = null;
        private readonly string _user = null;
        private readonly string _pass = null;
        //private FtpWebRequest ftpRequest = null;
        //private FtpWebResponse ftpResponse = null;
        //private Stream ftpStream = null;
        private int bufferSize = 2048;

        /// <summary>
        /// Construct Object
        /// </summary>
        /// <param name="host">name or IP</param>
        /// <param name="userName">ftp user name</param>
        /// <param name="password">ftp user password</param>
        public FtpClient(string host, string userName, string password)
        {
            _host = host;
            _user = userName;
            _pass = password;
        }

        /// <summary>
        /// Download File
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="localFile"></param>
        public void Download(string remoteFile, string localFile)
        {
            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(_host + "/" + remoteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                /* Get the FTP Server's Response Stream */
                using (var ftpStream = ftpResponse.GetResponseStream())
                {
                    /* Open a File Stream to Write the Downloaded File */
                    using (var localFileStream = new FileStream(localFile, FileMode.Create))
                    {
                        /* Buffer for the Downloaded Data */
                        byte[] byteBuffer = new byte[bufferSize];
                        int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                        /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                        while (bytesRead > 0)
                        {
                            localFileStream.Write(byteBuffer, 0, bytesRead);
                            bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                        }
                        /* Resource Cleanup */
                    }
                }
            }
        }

        /// <summary>
        /// Upload File
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="localFile"></param>
        public void Upload(string remoteFile, string localFile)
        {
            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(_host + "/" + remoteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            /* Establish Return Communication with the FTP Server */
            using (var ftpStream = ftpRequest.GetRequestStream())
            {

                /* Open a File Stream to Read the File for Upload */
                using (var localFileStream = new FileStream(localFile, FileMode.Open))
                {
                    /* Buffer for the Downloaded Data */
                    byte[] byteBuffer = new byte[bufferSize];
                    int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                    /* Resource Cleanup */
                }
            }
        }

        public void uploadAlternate(string remoteFile, string localFile)
        {
            try
            {
                /* Create an FTP Request */
                var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(_host + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(_user, _pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                /* Establish Return Communication with the FTP Server */
                var ftpStream = ftpRequest.GetRequestStream();
                /* Open a File Stream to Read the File for Upload */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /// <summary>
        /// Delete File
        /// </summary>
        /// <param name="deleteFile"></param>
        public void Delete(string deleteFile)
        {

            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)WebRequest.Create(_host + "/" + deleteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                Console.WriteLine("Upload File Complete, status {0}", ftpResponse.StatusDescription);
            }
            /* Resource Cleanup */
        }

        /// <summary>
        /// Rename File
        /// </summary>
        /// <param name="currentFileNameAndPath"></param>
        /// <param name="newFileName"></param>
        public void Rename(string currentFileNameAndPath, string newFileName)
        {
            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)WebRequest.Create(_host + "/" + currentFileNameAndPath);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.Rename;
            /* Rename the File */
            ftpRequest.RenameTo = newFileName;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                Console.WriteLine("Upload File Complete, status {0}", ftpResponse.StatusDescription);
            }
        }

        /// <summary>
        /// Create a New Directory on the FTP Server
        /// </summary>
        /// <param name="newDirectory"></param>
        public void CreateDirectory(string newDirectory)
        {
            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)WebRequest.Create(_host + "/" + newDirectory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                Console.WriteLine("Upload File Complete, status {0}", ftpResponse.StatusDescription);
            }
        }

        /// <summary>
        /// Get the Date/Time a File was Created
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFileCreatedDateTime(string fileName)
        {
            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(_host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                /* Establish Return Communication with the FTP Server */
                using (var ftpStream = ftpResponse.GetResponseStream())
                {
                    /* Get the FTP Server's Response Stream */
                    using (var ftpReader = new StreamReader(ftpStream))
                    {
                        /* Store the Raw Response */
                        string fileInfo = null;
                        /* Read the Full Response Stream */
                        fileInfo = ftpReader.ReadToEnd();
                        /* Return File Created Date Time */
                        return fileInfo;
                    }
                }
            }
        }

        /// <summary>
        /// Get the Size of a File
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFileSize(string fileName)
        {

            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(_host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                /* Establish Return Communication with the FTP Server */
                using (var ftpStream = ftpResponse.GetResponseStream())
                {
                    /* Get the FTP Server's Response Stream */
                    using (var ftpReader = new StreamReader(ftpStream))
                    {
                        /* Store the Raw Response */
                        string fileInfo = null;
                        /* Read the Full Response Stream */

                        while (ftpReader.Peek() != -1)
                        {
                            fileInfo = ftpReader.ReadToEnd();
                        }
                        /* Return File Size */
                        return fileInfo;
                    }
                }
            }
        }

        public string[] DirectoryListSimple()
        {
            return DirectoryListSimple(string.Empty);
        }
        /// <summary>
        /// List Directory Contents File/Folder Name Only
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public string[] DirectoryListSimple(string directory)
        {
            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(_host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                /* Establish Return Communication with the FTP Server */
                using (var ftpStream = ftpResponse.GetResponseStream())
                {
                    /* Get the FTP Server's Response Stream */
                    using (var ftpReader = new StreamReader(ftpStream))
                    {
                        /* Store the Raw Response */
                        /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */

                        var list = new List<string>();

                        while (ftpReader.Peek() != -1)
                        {
                            list.Add(ftpReader.ReadLine());
                        }
                        return list.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// List Directory Contents in Detail (Name, Size, Created, etc.)
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public string[] DirectoryListDetailed(string directory)
        {
            /* Create an FTP Request */
            var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(_host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            /* Establish Return Communication with the FTP Server */
            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                /* Establish Return Communication with the FTP Server */
                using (var ftpStream = ftpResponse.GetResponseStream())
                {
                    /* Get the FTP Server's Response Stream */
                    using (var ftpReader = new StreamReader(ftpStream))
                    {
                        /* Store the Raw Response */
                        string directoryRaw = null;
                        /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                        while (ftpReader.Peek() != -1)
                        {
                            directoryRaw += ftpReader.ReadLine() + "|";
                        }
                        /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                        string[] directoryList = directoryRaw.Split("|".ToCharArray());
                        return directoryList;
                        /* Resource Cleanup */
                    }
                }
            }
        }
    }
}
