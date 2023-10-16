using System;
using FSUIPC;

namespace UNIConsole.DataSet
{
    public abstract class DataSetBase
    {
        /// <summary>
        /// 向接收端发送处理错误详情，数据中包含错误源和错误信息
        /// </summary>
        /// <param name="exception"></param>
        public void CatchError(Exception exception)
        {
            ServiceServer.SendBytesOverIpc(new IPCDataPacket<ErrorData>(PacketType.Error, new ErrorData(GetType().FullName, exception)).EncodeJson());
        }
        /// <summary>
        /// 初始化数据组，在构造器中调用
        /// </summary>
        public void Initialize()
        {
            try
            {
                if(!FSUIPCConnection.IsOpen) FSUIPCConnection.Open();
                FSUIPCConnection.Process(GetType().GetProperty("GroupName").GetValue(this).ToString());
                var properties = GetType().GetProperties();
                var dataIndex = 0;
                var OffsetsProperty = GetType().GetProperty("Offsets").GetValue(this);
                var Offsets = OffsetsProperty as Offset[];
                foreach (var property in properties)
                {
                    if (property.Name == "GroupName") continue;
                    if (dataIndex >= Offsets.Length) continue;
                    property.SetValue(this, Offsets[dataIndex].GetValue(property.PropertyType));
                    dataIndex++;
                }
            }
            catch(Exception e)
            {
                //CatchError(e);
            }
        }
        public void Refresh()
        {
            try
            {
                if (!FSUIPCConnection.IsOpen) FSUIPCConnection.Open();
                FSUIPCConnection.Process(GetType().GetProperty("GroupName").GetValue(this).ToString());
                var properties = GetType().GetProperties();
                var dataIndex = 0;
                var OffsetsProperty = GetType().GetProperty("Offsets").GetValue(this);
                var Offsets = OffsetsProperty as Offset[];
                foreach (var property in properties)
                {
                    if (property.Name == "GroupName") continue;
                    if (dataIndex >= Offsets.Length) continue;
                    property.SetValue(this, Offsets[dataIndex].GetValue(property.PropertyType));
                    dataIndex++;
                }
            }
            catch (Exception e)
            {
                //CatchError(e);
            }
        }
        public Offset<T> Build<T>(int Address)
        {
            return new Offset<T>(GetType().GetProperty("GroupName").GetValue(this).ToString(), Address);
        }
        public abstract object ToInfo();
    }
}
