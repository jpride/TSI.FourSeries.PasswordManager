using System;

namespace TSI.FourSeries.PasswordManager
{
    public class FileContentsReadSuccessfullyEventArgs : EventArgs
    {
        public string FileContentsArg { get; set; }
    }

    public class DeserializationSuccessEventArgs : EventArgs
    {
        public string[] StringArray { get; set; }
        public ushort ListCount { get; set; }
    }

    public class FileWriteSuccessEventArgs : EventArgs
    {
        public string LastUpdate { get; set; }
    }

    public class PasswordMatchEventArgs : EventArgs
    {
        public ushort index { get; set; }
    }

    public class PasswordNoMatchEventArgs : EventArgs
    {
        public string failMsg { get; set; }
    }
}
