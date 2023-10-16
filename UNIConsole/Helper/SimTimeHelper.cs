using System;
using FSUIPC;

namespace UNIConsole.Helper
{
    internal class SimTimeHelper
    {
        public static void SetToCurrent()
        {
            if (!FSUIPCConnection.IsOpen)
            {
                try
                {
                    FSUIPCConnection.Open();
                }
                catch
                {
                    return;
                }
            }
            FSUIPCConnection.UTCDateTime = DateTime.UtcNow;
        }
    }
}
