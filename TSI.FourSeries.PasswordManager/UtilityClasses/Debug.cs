using Crestron.SimplSharp;

namespace TSI.FourSeries.Debugging
{
    public static class Debug
    {
        private static bool _debug = false;

        public static bool DebugState
        {
            get { return _debug; }
            set { _debug = value; }
        }

        public static void SetDebug(ushort debugState)
        {
            DebugState = debugState == 1 ? true : false;
        }

        public static void Trace(string message)
        {
            if (_debug)
            {
                CrestronConsole.PrintLine(message);
            }
        }

        public static void ReportDebugToLog(string message, int severity)
        {

            switch (severity)
            {
                case 1:
                    ErrorLog.Notice(message);
                    break;

                case 2:
                    ErrorLog.Warn(message);
                    break;

                case 3:
                    ErrorLog.Error(message);
                    break;
            }
        }
    }
}
