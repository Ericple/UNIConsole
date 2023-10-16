using UNIConsole.Helper;

namespace UNIConsole.DataSet.DataHandler
{
    public class UNIAirSpeed : HandlerBase<uint>
    {
        public UNIAirSpeed(uint value)
        {
            Imperial = ValueHelper.AirSpeed(value);
            Metric = ValueHelper.Knots2Mps(Imperial);
        }
    }
}
