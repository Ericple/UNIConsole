using System.Collections;
using FSUIPC;

namespace UNIConsole.DataSet
{
    public enum LightType
    {
        Navigation,
        Beacon,
        Landing,
        Taxi,
        Strobes,
        Instruments,
        Recognition,
        Wing,
        Logo,
        Cabin
    }
    public class LightsData : DataSetBase
    {
        public BitArray Lights { get; set; }
        public static string GroupName { get; set; } = "LightData";
        public Offset[] Offsets { get; set; } = {
            new Offset<BitArray>(GroupName, 0x0D0C, 2)
        };
        public LightsData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            Refresh();
            return new LightsDataInfo
            {
                Navigation = Lights[(int)LightType.Navigation],
                Beacon = Lights[(int)LightType.Beacon],
                Landing = Lights[(int)LightType.Landing],
                Taxi = Lights[(int)LightType.Taxi],
                Strobes = Lights[(int)LightType.Strobes],
                Instruments = Lights[(int)LightType.Instruments],
                Recognition = Lights[(int)LightType.Recognition],
                Wing = Lights[(int)LightType.Wing],
                Logo = Lights[(int)LightType.Logo],
                Cabin = Lights[(int)LightType.Cabin]
            };
        }
    }
}
