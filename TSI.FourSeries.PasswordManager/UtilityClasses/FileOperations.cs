using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using TSI.FourSeries.Utility;
using TSI.FourSeries.Debugging;


namespace TSI.FourSeries.FileUtilities
{
    public class FileOperations
    {
        private static CCriticalSection _fileLock = new CCriticalSection();
        public static Boolean fileExists = false;

        public static bool CheckFileExists(string filePath)
        {
            try
            {
                _fileLock.Enter();

                if (File.Exists(filePath))
                {
                    fileExists = true;
                }

                return fileExists;

            }
            finally
            {
                _fileLock.Leave();
            }
        }

        public static string ReadFile(string filePath)
        {
            string fileContents;

            try
            {
                _fileLock.Enter();


                if (CheckFileExists(filePath))
                {
                    fileContents = File.ReadToEnd(filePath, Encoding.UTF8);
                    return fileContents;
                }
                else
                {
                    ErrorLog.Error(Constants.FileNotFoundMessage);
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                Debug.Trace($"FileOperations.ReadFile() Error: {e.Message}");
                return string.Empty;
            }
            finally
            {
                _fileLock.Leave();
            }
        }

        public static void WriteFile(string filePath, string payload)
        {
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);

            try
            {
                _fileLock.Enter();
                fs.Write(payload, Encoding.UTF8);
                Debug.Trace($"{Constants.WriteFilePayloadReport} {payload}");
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine(Constants.WriteFileExceptionStackTrace, e.StackTrace);
                Debug.ReportDebugToLog($"{Constants.WriteFileExceptionStackTrace} {e.Message}", 3);
            }
            finally
            {
                fs.Close();
                _fileLock.Leave();
            }
        }

        public static void ListDirectories()
        {
            var appDir = Directory.GetApplicationDirectory();
            CrestronConsole.PrintLine($"App Directory: {appDir}");
        }

        public static eDevicePlatform GetPlatform()
        {
            return CrestronEnvironment.DevicePlatform;
        }

        public static void CreateDirectoryOnServer(string filePath)
        {
            try
            {
                string root = Directory.GetApplicationRootDirectory(); //get Application root dir

                //if directoryName starts with '/', trim it from the string
                if (filePath.StartsWith("/"))
                {
                    filePath = filePath.TrimStart('/');
                }

                //get path from file path, leaves the filename off
                string path = Path.GetDirectoryName(filePath);

                //create a path within the '%application_root_directory%/User' directory
                string dirPath = System.IO.Path.Combine(root, "User", path);
                Directory.CreateDirectory(dirPath);
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine($"Exception in CreateDirectory: {ex.Message}");
            }
        }

        //this method has been deprecated by using the FileLocation property to properly define the file path according to the platform
        public static void CreateFileOnServer(string filePath, string payload)
        {
            try
            {
                _fileLock.Enter();
                CreateDirectoryOnServer(filePath);

                if (filePath.StartsWith("/"))
                {
                    filePath = filePath.TrimStart('/');
                }

                string serverPath = System.IO.Path.Combine(Directory.GetApplicationRootDirectory(), "User", filePath);
                FileStream fs = new FileStream(serverPath, FileMode.Create);
                fs.Write(payload, Encoding.UTF8);
                fs.Close();
                _fileLock.Leave();
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine($"Error Creating file: {ex.Message}");
            }
        }

    }
}