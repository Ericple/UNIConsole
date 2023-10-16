using System;

namespace UNIConsole.DataSet
{
    internal class ErrorData
    {
        public string Origin;
        public string Message;
        public ErrorData(string orig, Exception exception)
        {
            Origin = orig;
            Message = exception.Message;
        }
    }
}
