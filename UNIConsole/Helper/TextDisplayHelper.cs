using System.Text;
using FSUIPC;

namespace UNIConsole.DataSet
{
    public class TextDisplayHelper
    {
        public static string GroupName { get; set; } = "TextDisplay";
        public static Offset<string> Text { get; set; } = new Offset<string>(GroupName, 0x3380, 128, true);
        public static Offset<short> DisplayControl { get; set; } = new Offset<short>(GroupName, 0x32FA, true);
        public static void Display(string text, bool logToConsole = true)
        {
            Text.Value = text;
            ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes(text), logToConsole);
            DisplayControl.Value = 10;
            if (FSUIPCConnection.IsOpen)
            {
                FSUIPCConnection.Process(GroupName);
                return;
            }
            try
            {
                FSUIPCConnection.Open();
                FSUIPCConnection.Process(GroupName);
            }
            catch
            {
            }
        }
        public static void Display(string text, int duration)
        {
            Text.Value = $"[UNICARS MESSAGE] {text}";
            ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes(Text.Value));
            DisplayControl.Value = (short)duration;
            if (FSUIPCConnection.IsOpen)
            {
                FSUIPCConnection.Process(GroupName);
                return;
            }
            try
            {
                FSUIPCConnection.Open();
                FSUIPCConnection.Process(GroupName);
            }
            catch
            {
            }
        }
    }
}
