using System;
using FSUIPC;

namespace UNIConsole.DataSet
{
    [Serializable]
    public class AircraftFuelDataInfo
    {
        public double FuelLLM { get; set; }
        public double FuelLLA { get; set; }
        public double FuelLLT { get; set; }
        public double FuelLRM { get; set; }
        public double FuelLRA { get; set; }
        public double FuelLRT { get; set; }
        public double FuelLCT { get; set; }
        public double FuelWeightLbs = FSUIPCConnection.PayloadServices.FuelWeightLbs;
        public double FuelLevelLitres = FSUIPCConnection.PayloadServices.FuelLevelLitres;
    }
}
