using FSUIPC;
using UNIConsole.Helper;

namespace UNIConsole.DataSet
{
    public class AircraftFuelData : DataSetBase
    {
        public int FuelLLM { get; set; }
        public int FuelLLA { get; set; }
        public int FuelLLT { get; set; }
        public int FuelLRM { get; set; }
        public int FuelLRA { get; set; }
        public int FuelLRT { get; set; }
        public int FuelLCT { get; set; }
        public static string GroupName { get; set; } = "AircraftFuelData";
        public Offset[] Offsets { get; set; } = {
            new Offset<int>(GroupName, 0x0B7C),
            new Offset<int>(GroupName, 0x0B84),
            new Offset<int>(GroupName, 0x0B8C),
            new Offset<int>(GroupName, 0x0B94),
            new Offset<int>(GroupName, 0x0B9C),
            new Offset<int>(GroupName, 0x0BA4),
            new Offset<int>(GroupName, 0x0B74),
        };
        public AircraftFuelData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            return new AircraftFuelDataInfo
            {
                FuelLLM = ValueHelper.FuelLevel(FuelLLM),
                FuelLLA = ValueHelper.FuelLevel(FuelLLA),
                FuelLLT = ValueHelper.FuelLevel(FuelLLT),
                FuelLRM = ValueHelper.FuelLevel(FuelLRM),
                FuelLRA = ValueHelper.FuelLevel(FuelLRA),
                FuelLRT = ValueHelper.FuelLevel(FuelLRT),
                FuelLCT = ValueHelper.FuelLevel(FuelLCT),
            };
        }
    }
}
