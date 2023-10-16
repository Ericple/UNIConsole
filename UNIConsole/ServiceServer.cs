using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using UNIConsole.FlightMonitor;

namespace UNIConsole
{
    internal class ServiceServer
    {
        private readonly Monitor _monitor;
        public ServiceServer(string bidId, int enableVoiceService, string reqUrl, int logRefresh, string activeRoute, string soundpackName, int forceSyncTime, int voiceRecService)
        {
            Console.WriteLine("当前UNICARS版本: 2.4.8");
            Console.WriteLine(bidId);
            _monitor = new Monitor(FlightPhase.PreFlight, bidId, enableVoiceService, reqUrl, logRefresh, activeRoute, soundpackName, forceSyncTime,voiceRecService);
            StartIpcService();
        }

        private void StartIpcService()
        {
            var pipeServer = new NamedPipeServerStream("unipipe_input");
            pipeServer.WaitForConnection();
            StreamReader ss = new StreamReader(pipeServer);
            for (; ; )
            {
                var input = ss.ReadLine();
                switch (input)
                {
                    case "showinfo:ac":
                        _monitor.ShowAcData();
                        break;
                    case "showinfo:fd":
                        _monitor.ShowFdData();
                        break;
                    case "showinfo:is":
                        _monitor.ShowInsData();
                        break;
                    case "showinfo:lt":
                        _monitor.ShowLtData();
                        break;
                }
            }
        }

        /// <summary>
        /// 向接收管道发送数据
        /// </summary>
        /// <param name="msg">编码后的字节流</param>
        /// <param name="logToConsole"></param>
        public static void SendBytesOverIpc(byte[] msg, bool logToConsole = false)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if(logToConsole) Console.WriteLine(Encoding.UTF8.GetString(msg));
                    Console.WriteLine(Encoding.UTF8.GetString(msg));
                    var pipeClient = new NamedPipeClientStream("unipipe_get");
                    var dPipeClient = new NamedPipeClientStream("unipipe_debug");
                    pipeClient.Connect(10);
                    dPipeClient.Connect(10);
                    dPipeClient.Write(msg, 0, msg.Length);
                    pipeClient.Write(msg, 0, msg.Length);
                }
                catch
                {
                    // ignored
                }
            });
            
        }
    }
}
