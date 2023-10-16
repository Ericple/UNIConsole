using FSUIPC;

namespace UNIConsole.Helper
{
    internal class ValueHelper
    {
        public static double Altitude(long value)
        {
            var altObj = new FsAltitude(value);
            return altObj.Feet;
        }
        public static double MetricAlt(long value)
        {
            var altObj = new FsAltitude(value);
            return altObj.Metres;
        }
        public static double Meter2Feet(double value)
        {
            return value * 3.28084;
        }
        public static double Mps2Knots(double value)
        {
            return value * 1.944;
        }
        public static double Feet2Meter(double value)
        {
            return value / 3.28084;
        }
        public static double Knots2Mps(double value)
        {
            return value / 0.514;
        }
        public static double FlightInput(short value)
        {
            return value / 16383d;
        }
        /// <summary>
        /// 计算离地高度GroundAltitude和垂直速度VerticalSpeed
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double GAVS(int value)
        {
            return value / 256d;
        }
        public static double VSFPM(int value)
        {
            return (value / 256d) * 3.28084 * 60;
        }
        public static ushort FrameRate(ushort value)
        {
            return (ushort)(32768 / value);
        }
        public static double GroundSpeed(uint value)
        {
            return value / 65536d;
        }
        public static double RadioAltitude(int value)
        {
            return value / 65536d;
        }
        public static FsLongitude Longitude(long value)
        {
            return new FsLongitude(value);
        }
        public static FsLatitude Latitude(long value)
        {
            return new FsLatitude(value);
        }
        public static double AirSpeed(uint value)
        {
            return value / 128d;
        }
        public static FsTransponderCode Transponder(ushort value)
        {
            return new FsTransponderCode(value);
        }
        public static FsFrequencyCOM Com1Frequency(ushort value)
        {
            return new FsFrequencyCOM(value);
        }
        public static double PitchRoll(int value)
        {
            return value * 360d / (65536d * 65536);
        }
        public static double GearP(int value)
        {
            return value / 163.83d;
        }
        public static ushort Visibility(ushort value)
        {
            return (ushort)(value / 100);
        }
        public static int SimulationRate(ushort value)
        {
            return value / 256;
        }
        public static double WindSpdDir(ushort value)
        {
            return value * 360d / 65536;
        }
        public static double MachSpeed(ushort value)
        {
            return value / 20480d;
        }
        public static double FuelLevel(int value)
        {
            return value / 128d / 65536d;
        }
        /// <summary>
        /// GForce on touchdown
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double GFTD(short value)
        {
            return value / 625d;
        }
        /// <summary>
        /// 计算EngineLever/EngineN1/N2，百分比 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Engine(ushort value)
        {
            return 100 * value / 16384d;
        }
        public static double Engine(short value)
        {
            return 100 * value / 16384d;
        }
        /// <summary>
        /// 计算EGT，摄氏度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double EngineEGT(ushort value)
        {
            return 860 * value / 16384d;
        }
        public static double ParkingBrake(ushort value)
        {
            return 100 * value / 32767d;
        }
        public static double FlapP(int value)
        {
            return 100 * value / 16383d;
        }
        public static double ZFW(int value)
        {
            return value / 256d;
        }
        public static double ControlSurface(short value)
        {
            return value / 16383d;
        }
        public static double ControlSurface(double value)
        {
            return value / 16383d;
        }
        public static double Temperature(ushort value)
        {
            return value / 256d;
        }
    }
}
