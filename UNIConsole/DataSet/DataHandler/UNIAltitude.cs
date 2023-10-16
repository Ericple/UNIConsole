using FSUIPC;

namespace UNIConsole.DataSet.DataHandler
{
    public class UNIAltitude : HandlerBase<double>
    {
        public UNIAltitude(long Altitude)
        {
            var alt = new FsAltitude(Altitude);
            Imperial = alt.Feet;
            Metric = alt.Metres;
        }
    }
}
