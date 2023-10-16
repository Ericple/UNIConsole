using UNIConsole.Helper;

namespace UNIConsole.DataSet.DataHandler
{
    public class UNIGroundAltitude : HandlerBase<int>
    {
        public UNIGroundAltitude(int value)
        {
            Metric = value / 256d;
            Imperial = ValueHelper.Meter2Feet(Metric);
        }
    }
}
