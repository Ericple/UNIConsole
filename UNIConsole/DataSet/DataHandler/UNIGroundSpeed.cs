using UNIConsole.Helper;

namespace UNIConsole.DataSet.DataHandler
{
    public class UNIGroundSpeed : HandlerBase<uint>
    {
        public UNIGroundSpeed(uint value)
        {
            Metric = value / 65536d;
            Imperial = ValueHelper.Mps2Knots(Metric);
        }
    }
}
