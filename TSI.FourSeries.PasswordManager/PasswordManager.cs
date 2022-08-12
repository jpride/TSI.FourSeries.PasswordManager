using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using System.Timers;
using Crestron.SimplSharp.CrestronIO;
using TSI.FourSeries.FileUtilities;
using TSI.FourSeries.Utility;
using TSI.FourSeries.Debugging;



namespace TSI.FourSeries.PasswordManager
{

    public class PasswordManager
    {

        //Vars
        private Timer AutoWriteTimer = new Timer(); //a global timer

        private JsonObject _jsonObject = new JsonObject(); //object to temporarily hold the info to be read / written
        private List<string> _stringList = new List<string>(); //list of strings to manipulate prior to serializing for write

        private string _filelocation;
        private ushort _autoWriteEnable;
        private bool _bAutoWrite;

        //--------------------*-*- Events -*-*---------------------//
        public event EventHandler<FileContentsReadSuccessfullyEventArgs> FileContentsReadSuccessfullyEvent;
        public event EventHandler<DeserializationSuccessEventArgs> DeserializationSuccessEvent;
        public event EventHandler<FileWriteSuccessEventArgs> FileWriteSuccessEvent;

        public event EventHandler<PasswordMatchEventArgs> BackdoorPasswordMatchEvent;
        public event EventHandler<PasswordMatchEventArgs> PasswordMatchEvent;
        public event EventHandler<PasswordNoMatchEventArgs> PasswordNoMatchEvent;

        //--------------------*-*- Properties -*-*-----------------//
        private string FileContents { get; set; }

        public string FileLocation
        {
            get { return this._filelocation; }
            set
            {   //set appropriate directory path is the platform is server, otherwise pass the filepath in unchanged
                //this is the only location in code where the filepath is altered for this purpose
                if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Server)
                {
                    this._filelocation = System.IO.Path.Combine(Directory.GetApplicationRootDirectory(), "User", value.TrimStart('/'));
                }
                else
                {
                    this._filelocation = value;
                }
            }
        }

        //called from Simpl+
        public ushort AutoWriteEnable
        {
            get { return this._autoWriteEnable; }
            set
            {
                this._bAutoWrite = value == 1;
                this._autoWriteEnable = value;
            }
        }

        public ushort AutoWriteTimeout { get; set; }

        private string _backDoor;
        public string BackDoor
        {
            get { return String.IsNullOrEmpty(_backDoor) ? "3108" : _backDoor; }
            set { _backDoor = value; }
        }


        //Initializer
        public PasswordManager()
        {
            //emtpy initializer
        }

        //--------------------*-*- Methods -*-*--------------------//

        //called from Simpl+
        public void SetDebugState(ushort state)
        {
            Debug.SetDebug(state);
        }

        //called from Simpl+
        public void Initialize(string filePath)
        {
            FileLocation = filePath;
            Debug.Trace($"***StringProcessor - Initialize. FileLocation: {FileLocation}");

            AutoWriteTimer.Interval = AutoWriteTimeout;
            AutoWriteTimer.Elapsed += OnTimerElapsed;
            AutoWriteTimer.AutoReset = false;
            AutoWriteTimer.Enabled = false;

            if (FileOperations.CheckFileExists(FileLocation))
            {
                FileContents = FileOperations.ReadFile(FileLocation);
            }
            else
            {
                //create a default json string
                try
                {
                    CreateDefaultFile();
                }
                catch (Exception ex)
                {
                    CrestronConsole.PrintLine(Constants.InitializeExceptionMessage, ex.Message);
                }
            }
        }

        private void CreateDefaultFile()
        {
            string jsonTemplate = Constants.DefaultFileContents;
            Debug.Trace($"Default File Contents Created: {jsonTemplate}");

            FileOperations.WriteFile(FileLocation, jsonTemplate);

            //Bypass having to read the file and just set _filecontents to what we just wrote to file
            FileContents = jsonTemplate;
            DeserializeContents(); //new
        }

        //called from Simpl+
        public void ReadFile()
        {
            if (AutoWriteTimer.Enabled)
            {
                AutoWriteTimer.Stop();
            }

            try
            {
                if (File.Exists(FileLocation))
                {
                    FileContents = FileOperations.ReadFile(FileLocation);
                }
                else
                {
                    CreateDefaultFile();
                }

                if (!FileContents.Equals(null) || !FileContents.Equals(String.Empty))
                {
                    DeserializeContents();
                }

                FileContentsReadSuccessfullyEventArgs args = new FileContentsReadSuccessfullyEventArgs
                {
                    FileContentsArg = !FileContents.Equals(null) ? FileContents : String.Empty
                };

                FileContentsReadSuccessfullyEvent?.Invoke(this, args);

            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine(Constants.FileReadErrorMessage, ex.Message);
            }
        }

        //called internally and from Simpl+
        public void WriteFile()
        {
            if (AutoWriteTimer.Enabled)
            {
                AutoWriteTimer.Stop();
            }

            try
            {
                _jsonObject.Passwords = _stringList;
                var payload = JsonConvert.SerializeObject(_jsonObject);

                FileOperations.WriteFile(FileLocation, payload);

                FileWriteSuccessEventArgs args = new FileWriteSuccessEventArgs
                {
                    LastUpdate = DateTime.Now.ToString()
                };

                FileWriteSuccessEvent?.Invoke(this, args);

            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine(Constants.WriteFileErrorMessage, ex.Message);
                CrestronConsole.PrintLine($"FileOperations.WriteFile(): {ex.StackTrace}");
            }
        }

        private void DeserializeContents()
        {
            try
            {
                _jsonObject = JsonConvert.DeserializeObject<JsonObject>(FileContents);
                _stringList = _jsonObject.Passwords;
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine(Constants.DeserializeErrorMessage, ex.Message);
            }

            //EventArg creation and EventHandler Call
            try
            {
                DeserializationSuccessEventArgs args = new DeserializationSuccessEventArgs
                {
                    StringArray = _jsonObject.Passwords.ToArray(),
                    ListCount = (ushort)_jsonObject.Passwords.Count
                };

                if (!DeserializationSuccessEvent.Equals(null))
                {
                    DeserializationSuccessEvent(this, args);
                }
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine(Constants.DeserializationEventErrorMessage, ex.Message);
            }
        }

        //called from Simpl+
        public void UpdateListFromSimpl(ushort Index, string Item)
        {
            bool fileUpdateRequired;

            try
            {
                fileUpdateRequired = false;

                if (Index >= _stringList.Count) //if the index is greater than or equal to the list count, add new item
                {
                    Debug.Trace($"Index not in list. Adding to List");
                    _stringList.Add(Item);
                    fileUpdateRequired = true;
                }
                else //if the index is not greater than or equal to the list count, change content of list[index]
                {
                    if (_stringList[Index] != Item)
                    {
                        Debug.Trace($"Item - {Item} : _stringList - {_stringList[Index]} || Index within List and IS NOT equal to existing item");
                        fileUpdateRequired = true;
                    }
                    else
                    {
                        Debug.Trace($"Item - {Item} : _stringList - {_stringList[Index]} || Index within List and IS equal to existing item");
                        fileUpdateRequired = false;
                    }

                    _stringList[Index] = Item;
                }


                if (fileUpdateRequired && _bAutoWrite) //if a update is required AND autowrite is enabled
                {
                    try
                    {
                        if (AutoWriteTimer.Enabled) //stop and restart the timer
                        {
                            AutoWriteTimer.Stop();
                            AutoWriteTimer.Start();
                        }
                        else
                        {
                            AutoWriteTimer.Start();
                        }

                    }
                    catch (Exception eTimer)
                    {
                        CrestronConsole.PrintLine($"Error in Timer Set: {eTimer.Message}");
                    }
                }
                else
                {
                    try
                    {
                        if (AutoWriteTimer.Enabled)
                        {
                            AutoWriteTimer.Stop();
                        }

                        fileUpdateRequired = false;
                    }
                    catch (Exception killTimer)
                    {
                        CrestronConsole.PrintLine($"Error in Timer Stop: {killTimer.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine(Constants.UpdateListErrorMessage, Index, ex.Message);
            }


        }

        private void OnTimerElapsed(Object source, ElapsedEventArgs e)
        {
            Debug.Trace("Timer elapsed! Writing to File");
            WriteFile();
        }

        //called from Simpl+
        public void CheckPassword(string comparePassword)
        {
            if (comparePassword == BackDoor)
            {
                PasswordMatchEventArgs args = new PasswordMatchEventArgs
                {
                    index = 42 //42 for backdoor
                };

                BackdoorPasswordMatchEvent?.Invoke(this, args);
            }

            else
            {
                int matchIndex = _jsonObject.Passwords.FindIndex(p => p == comparePassword);
                Debug.Trace($"MatchIndex: {matchIndex}");

                if (matchIndex >= 0)
                {
                    PasswordMatchEventArgs args = new PasswordMatchEventArgs
                    {
                        index = (ushort)matchIndex
                    };

                    PasswordMatchEvent?.Invoke(this, args);
                }
                else
                {
                    PasswordNoMatchEventArgs args = new PasswordNoMatchEventArgs
                    {
                        failMsg = $"NOMATCH: Password \'{comparePassword}\' failed to match any stored passwords"
                    };

                    PasswordNoMatchEvent?.Invoke(this, args);
                }
            }
        } //method to check user entry against password list
    }
}
