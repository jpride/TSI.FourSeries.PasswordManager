namespace TSI.FourSeries.Utility
{
    class Constants
    {
        internal static string FileReadErrorMessage = ".ReadFile()";
        internal static string UpdateListErrorMessage = "Update List Error";
        internal static string DeserializeErrorMessage = ".DeserializeContents";
        internal static string DeserializationEventErrorMessage = "Deserialization Event Error";
        internal static string WriteFileErrorMessage = ".WriteFile()";
        internal static string FileContentssMessage = "-File Contents: {0}";

        public const string FileNotFoundMessage = ".ReadFile() | Error: File Not Found!";
        public const string WriteFilePayloadReport = ".WriteFile() | Message: payload written to file:";

        public const string WriteFileExceptionStackTrace = ".FileOperations.WriteFile() | Exception: {0}";

        public const string DefaultFileContents = "{\"Passwords\": [\"password\"]}";
        public const string InitializeExceptionMessage = ".Initialize()";

    }
}
