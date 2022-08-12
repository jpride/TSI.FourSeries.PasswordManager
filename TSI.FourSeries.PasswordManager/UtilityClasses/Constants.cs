namespace TSI.FourSeries.Utility
{
    class Constants
    {
        internal static string FileReadErrorMessage = "File Read Error: {0}";
        internal static string UpdateListErrorMessage = "Update List Error @ Index: {0}:  {1}";
        internal static string DeserializeErrorMessage = "Deserialization Error: {0}";
        internal static string DeserializationEventErrorMessage = "Deserialization Event Error: {0}";
        internal static string WriteFileErrorMessage = "File Write Error: {0}";
        internal static string FileDoesNotExistMessage = "File does not exist.";
        internal static string FileContentssMessage = "File Contents: {0}";

        public const string FileNotFoundMessage = "TSI.FourSeries.PasswordManager.FileOperations.ReadFile() | Error: File Not Found!";
        public const string WriteFilePayloadReport = "TSI.FourSeries.PasswordManager.FileOperations.StringProcessor.WriteFile() | Message: payload written to file: {0}";
        public const string WriteFileExceptionStackTrace = "TSI.FourSeries.PasswordManager.FileOperations.WriteFile() | Exception: {0}";

        public const string DefaultFileContents = "{\"Passwords\": [\"password\"]}";
        public const string InitializeExceptionMessage = "TSI.FourSeries.PasswordManager.Initialize() error | {0}";

    }
}
