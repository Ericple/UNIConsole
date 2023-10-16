using System.Text;
using Newtonsoft.Json;

namespace UNIConsole.DataSet
{
    public enum PacketType
    {
        Data,
        Error
    }
    public class IPCDataPacket<T>
    {
        public PacketType Type;
        public string Origin = typeof(T).Name;
        public T Data;
        public IPCDataPacket(PacketType type, T data)
        {
            Type = type;
            Data = data;
        }
        public string ToJson()
        {
            var result = JsonConvert.SerializeObject(this, Formatting.None);
            ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes(result));
            return result;
        }
        public byte[] EncodeJson()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this, Formatting.None));
        }
    }
}
