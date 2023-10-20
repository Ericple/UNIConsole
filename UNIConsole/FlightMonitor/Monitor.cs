using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSUIPC;
using Newtonsoft.Json;
using UNIConsole.DataSet;
using UNIConsole.Helper;

namespace UNIConsole.FlightMonitor
{
    internal enum FlightPhase
    {
        PreFlight, PushBack, TaxiToRwy, Rolling, AbortTakeOff, TakeOff, Climb, Cruise, Descend, Approach, FinalApproach, GoAround, TaxiToGate, Shutdown, Crash, TaxiToGateSe, TaxiToRwySe
    }
    internal class Monitor
    {
        private static FlightPhase _phase;
        private static readonly AircraftData AcData = new AircraftData();
        private static readonly InstrumentData InsData = new InstrumentData();
        private static readonly FlightControlData FdData = new FlightControlData();
        private static readonly SimulatorData SimData = new SimulatorData();
        private static readonly SimWeatherData SwData = new SimWeatherData();
        private static readonly LightsData LightsData = new LightsData();
        private static readonly AircraftFuelData AcFData = new AircraftFuelData();
        private readonly FlightLog _flightLog = new FlightLog();
        private static readonly WarningData WarningData = new WarningData();
        private static PayloadServices _payload;
        private DateTime _engine2StartTime;
        private bool _connectionChecked;
        private static bool _readyPerform;
        private static bool _readyDisArm;
        private static bool _cabinDoorClosed;
        private bool _decentPrepared;
        private bool _cabinDoorDisArmed;
        private bool _landingPrepared;
        private bool _takeoffPrepared;
        private bool _climbPa;
        private bool _engine1Start;
        private bool _engine2Start;
        private bool _engine3Start = false;
        private bool _engine4Start = false;
        private bool _engine5Start = false;
        private bool _engine6Start = false;
        private Task FlightAnly;
        private Task FlightTimer;
        private Task voiceRecog;
        private Task posReport;
        private Task fInfoSend;
        private Task logRefill;
        private int _cruiseAltitude = 10000;
        private bool _gearRetracted;
        private FlightInfo _flightInfo;
        private readonly string _reqUrl;
        private readonly int _voiceEngEnable;
        private readonly int _logRefreshEnable;
        private readonly string _bidId;
        private readonly int _forceSyncTime;
        private readonly string _activeRoute;
        private SpeechRecognitionEngine _speechRecognitionEngine;
        private Grammar _grammar;
        private readonly SoundPlayer _welcomePlayer;
        private readonly SoundPlayer _safetyPlayer;

        private static readonly string[] RecKeyWord = {
            "客舱","乘务组","乘务员", "关闭舱门", "下降准备", "起飞准备", "着陆准备", "解除", "坐腿上", "舱门预位", "预位", "舱门", "颠簸"
        };
        private readonly SpeechRecognitionEngine _engine;
        private bool _phaseAnalyseStopFlag;
        private bool _isFlapsExtendingOrRetracting = false;
        private bool _landingLightWarningSentInThisPhase = false;

        public void ShowAcData()
        {
            AcData.Refresh();
            Console.WriteLine(new IPCDataPacket<AircraftDataInfo>(PacketType.Data, (AircraftDataInfo)AcData.ToInfo()).ToJson());
        }
        public void ShowInsData()
        {
            InsData.Refresh();
            Console.WriteLine(new IPCDataPacket<InstrumentDataInfo>(PacketType.Data, (InstrumentDataInfo)InsData.ToInfo()).ToJson());
        }
        public void ShowFdData()
        {
            FdData.Refresh();
            Console.WriteLine(new IPCDataPacket<FlightControlDataInfo>(PacketType.Data, (FlightControlDataInfo)FdData.ToInfo()).ToJson());
        }
        public void ShowLtData()
        {
            LightsData.Refresh();
            Console.WriteLine(new IPCDataPacket<LightsDataInfo>(PacketType.Data, (LightsDataInfo)LightsData.ToInfo()).ToJson());
        }
        private readonly string _soundPackName;

        private void G_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(e.Result.Text);
            if (e.Result.Text.Contains("客舱") || e.Result.Text.Contains("乘务")) {
                PAHelper.PlayFASound("dingdong", _voiceEngEnable, _soundPackName);
                _readyPerform = true;
                return;
            }
            if (e.Result.Text.Contains("解除")) _readyDisArm = true;
            if ((e.Result.Text.Contains("关闭舱门") || e.Result.Text.Contains("关舱门")) && !_cabinDoorClosed && _readyPerform && _phase == FlightPhase.PreFlight)
            {
                _welcomePlayer?.Stop();
                PAHelper.PlayFASound("close_door", _voiceEngEnable, _soundPackName);
                _cabinDoorClosed = true;
                return;
            }
            if (e.Result.Text.Contains("下降准备") && _readyPerform && _phase == FlightPhase.Cruise && !_decentPrepared)
            {
                PAHelper.PlayFASound("decent", _voiceEngEnable, _soundPackName);
                _decentPrepared = true;
                return;
            }
            if (e.Result.Text.Contains("着陆准备") && _readyPerform && _phase == FlightPhase.Approach && !_landingPrepared)
            {
                PAHelper.PlayFASound("prepare_for_landing", _voiceEngEnable, _soundPackName);
                _landingPrepared = true;
                return;
            }
            if (e.Result.Text.Contains("起飞准备") && _readyPerform && _phase == FlightPhase.TaxiToRwy && !_takeoffPrepared)
            {
                _safetyPlayer.Stop();
                PAHelper.PlayFASound("dingdong", _voiceEngEnable, _soundPackName);
                _takeoffPrepared = true;
            }
            if ((e.Result.Text.Contains("舱门预位") || e.Result.Text.Contains("预位") || e.Result.Text.Contains("舱门")) && _readyDisArm && _readyPerform && AcData.OnGround == 1 && ! _cabinDoorDisArmed)
            {
                PAHelper.PlayFASound("dingdong", _voiceEngEnable, _soundPackName);
                PAHelper.PlayFASound("doors_opening", _voiceEngEnable, _soundPackName);
                _cabinDoorDisArmed = true;
            }
        }
        private void StartVoiceEngine(int enable)
        {
            if (enable != 1) return;
            IpcLog($"Voice Recognition started, given param: {enable}");
            var infoReq = new CultureInfo("zh-CN");
            foreach (var info in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (!info.Culture.Equals(infoReq)) continue;
                _speechRecognitionEngine = new SpeechRecognitionEngine(info);
                break;
            }

            if (_speechRecognitionEngine == null) return;
            _speechRecognitionEngine.SetInputToDefaultAudioDevice();
            var gb = new GrammarBuilder();
            gb.Append(new Choices(RecKeyWord));
            _grammar = new Grammar(gb);
            _grammar.SpeechRecognized += G_SpeechRecognized;
            _speechRecognitionEngine.LoadGrammar(_grammar);
            _speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }
        public string[] FSVerions =
        {
            "FS98","FS2000","CFS2","CFS1","Reserved","FS2002","FS2004","Flight Simulaotr X / X-Plane",
            "ESP","Prepar3D","Flight Simulator X","Prepar3D","Microsoft Flight Simulator 2020"
        };
        private async void StartTask(int voiceRecService)
        {
            var flightInfoReq = new HttpClient();
            var fInfo = await flightInfoReq.GetStringAsync($"{_reqUrl}fInfo&bidid={_bidId}");
            _flightInfo = JsonConvert.DeserializeObject<FlightInfo>(fInfo);
            _cruiseAltitude = int.Parse(_flightInfo.flightlevel);
            IpcMessage($"FLIGHT NUMBER: {_flightInfo.code}{_flightInfo.flightnum} ACARS MONITOR STARTED.");
            IpcMessage($"Reminder: Scheduled cruise altitude of this flight is: {_cruiseAltitude} ft. Activate route: {_activeRoute}. Wish you a wonderful journey!");
            FlightTimer = Task.Factory.StartNew(() =>
            {
                for (; ; )
                {
                    if ((_phase == FlightPhase.TaxiToGate || _phase == FlightPhase.TaxiToGateSe || _phase == FlightPhase.TaxiToRwy || _phase == FlightPhase.TaxiToRwySe) && SimData.PauseIndicator != 1)
                    {
                        _flightLog.TaxiSeconds++;
                    }
                    if (SimData.PauseIndicator != 1)
                    {
                        _flightLog.FlightSeconds++;
                    }
                    if (_flightLog.FlightSeconds == 60)
                    {
                        _flightLog.FlightSeconds = 0;
                        _flightLog.FlightMinutes++;
                    }
                    if (_flightLog.FlightMinutes == 60)
                    {
                        _flightLog.FlightMinutes = 0;
                        _flightLog.FlightHours++;
                    }
                    if (_flightLog.TaxiSeconds == 60)
                    {
                        _flightLog.TaxiSeconds = 0;
                        _flightLog.TaxiMinutes++;
                    }
                    if (_flightLog.TaxiMinutes == 60)
                    {
                        _flightLog.TaxiMinutes = 0;
                        _flightLog.TaxiHours++;
                    }
                    if ((_phase == FlightPhase.TaxiToGateSe || _phase == FlightPhase.TaxiToRwySe) && SimData.PauseIndicator != 1)
                    {
                        _flightLog.SETSeconds++;
                    }
                    if (_flightLog.SETSeconds == 60)
                    {
                        _flightLog.SETSeconds = 0;
                        _flightLog.SETMinutes++;
                    }
                    if (_flightLog.SETMinutes == 60)
                    {
                        _flightLog.SETMinutes = 0;
                        _flightLog.SETHours++;
                    }
                    Thread.Sleep(1000);
                    if (_phase == FlightPhase.Shutdown) break;
                }
            });
            voiceRecog = Task.Factory.StartNew(() =>
            {
                StartVoiceEngine(voiceRecService);
            });
            posReport = Task.Factory.StartNew(async () =>
            {
                for (;;)
                {
                    var body = new Dictionary<string, string>
                    {
                        {"latitude",AcData.Latitude.ToString()},
                        {"longitude",AcData.Longitude.ToString()},
                        {"heading",AcData.Heading.ToString(CultureInfo.InvariantCulture)},
                        {"altitude",AcData.Altitude.ToString()},
                        {"gs",AcData.GroundSpeed.ToString()},
                        {"phase",_phase.ToString()},
                    };
                    var submitReq = new HttpClient();
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    await submitReq.PostAsync($"{_reqUrl}acarsupdate&bidid={_bidId}", new CJsonContent(body));
                    Thread.Sleep(10000);
                }
            });
            FlightAnly=Task.Factory.StartNew(() =>
            {
                for (; ; )
                {
                    try
                    {
                        if (!FSUIPCConnection.IsOpen)
                        {
                            try
                            {
                                FSUIPCConnection.Open();
                                _payload = FSUIPCConnection.PayloadServices;
                                Log(FlightLogItemType.Normal, $"Flight is being monitored. you are at {_phase} phase.", 0);
                                IpcMessage($"Flying {AcData.AircraftType} on {FSVerions[SimData.FSVersion - 1]}");
                                if (_voiceEngEnable == 1) _welcomePlayer.Play();
                                _connectionChecked = true;
                            }
                            catch (Exception e)
                            {
                                IpcLog("Error269: "+JsonConvert.SerializeObject(e));
                                Thread.Sleep(10000);
                                continue;
                            }
                        }
                        if (!_connectionChecked && FSUIPCConnection.IsOpen)
                        {
                            _payload = FSUIPCConnection.PayloadServices;
                            Log(FlightLogItemType.Normal, $"Flight is being monitored. you are at {_phase} phase.", 0);
                            if (_voiceEngEnable == 1) _welcomePlayer.Play();
                            _connectionChecked = true;
                        }
                        AcData.Refresh();
                        InsData.Refresh();
                        FdData.Refresh();
                        SimData.Refresh();
                        SwData.Refresh();
                        AcFData.Refresh();
                        FlightPhaseAnalyse();
                        Thread.Sleep(100);
                    }
                    catch(Exception e)
                    {
                        IpcLog("Error292: "+JsonConvert.SerializeObject(e));
                    }
                }
            });
            //根据设置启动
            if(_logRefreshEnable == 1)
            {
                logRefill= Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        try
                        {
                            ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes("logstart"));
                            Thread.Sleep(100);
                            foreach (var log in _flightLog.Logs.ToArray())
                            {
                                ServiceServer.SendBytesOverIpc(
                                    Encoding.UTF8.GetBytes($"log*[{log.Time}] " + log.LogContent));
                                Thread.Sleep(100);
                            }

                            Thread.Sleep(100);
                            ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes("logend"));
                            Thread.Sleep(5000);
                        }
                        catch (Exception e)
                        {
                            IpcLog($"Error321: {JsonConvert.SerializeObject(e)}");
                        }
                    }
                });
            }
            fInfoSend=Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        IpcLog(JsonConvert.SerializeObject(AcData.ToInfo()));
                        Thread.Sleep(5000);
                    }
                    catch (Exception e)
                    {
                        IpcLog($"Error337: {JsonConvert.SerializeObject(e)}");
                    }
                }
            });
        }

        protected virtual void IpcMessage(string message)
        {
            Log(FlightLogItemType.Normal, message, 0);
        }

        private static void IpcLog(string json)
        {
            ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes(json));
        }
        public Monitor(FlightPhase initialPhase, string bidId, int enableVoiceService, string reqUrl, int logRefresh, string activeRoute, string soundPackName, int forceSyncTime, int voiceRecService)
        {
            _phase = initialPhase;
            _safetyPlayer = new SoundPlayer
            {
                SoundLocation = Environment.CurrentDirectory + @"\soundpack\" + soundPackName + @"\fa\safty_ins.wav"
            };
            _welcomePlayer = new SoundPlayer
            {
                SoundLocation = Environment.CurrentDirectory + @"\soundpack\" + soundPackName + @"\fa\welcome_music.wav"
            };
            _reqUrl = reqUrl;
            if(forceSyncTime == 1) SimTimeHelper.SetToCurrent();
            _voiceEngEnable = enableVoiceService;
            _bidId = bidId;
            _logRefreshEnable = logRefresh;
            StartTask(voiceRecService);
            _soundPackName = soundPackName;
            _activeRoute = activeRoute.Trim('"');
            _forceSyncTime = forceSyncTime;
        }

        private void Log(FlightLogItemType type,string logContent, int scoreAdjust, bool logToConsole = false)
        {
            if (type == FlightLogItemType.Bad) logContent = "Bad: " + logContent;
            _flightLog.Logs.Add(new FlightLogItem(type, logContent, scoreAdjust, AcData, InsData, logToConsole));
            _flightLog.Score += scoreAdjust;
        }

        private void TaxiRwyStatePreCheck()
        {
            if (!((LightsDataInfo)LightsData.ToInfo()).Navigation) Log(FlightLogItemType.Bad, "Navigation Light off.", -50);
            if (!((LightsDataInfo)LightsData.ToInfo()).Taxi) Log(FlightLogItemType.Bad, "Taxi Light off.", -50);
        }

        private void TakeoffPreCheck()
        {
            if (!((LightsDataInfo)LightsData.ToInfo()).Landing) Log(FlightLogItemType.Bad, "Landing light off before rolling", -100);
        }
        private bool _isUnderTurbulence;

        private void FlightPhaseAnalyse()
        {
            if (_phaseAnalyseStopFlag) return;
            if ((FdData.Offsets[5].ValueChanged || FdData.Offsets[6].ValueChanged) && !_isFlapsExtendingOrRetracting)
            {
                _isFlapsExtendingOrRetracting = true;
                Log(FlightLogItemType.Normal, "Flaps position changing.", 0);
            }
            if (!FdData.Offsets[5].ValueChanged && !FdData.Offsets[6].ValueChanged && _isFlapsExtendingOrRetracting)
            {
                _isFlapsExtendingOrRetracting = false;
                Log(FlightLogItemType.Normal, $"Flaps position changed to {ValueHelper.FlapP(FdData.FlapsPL):f2}%.", 0);
            }
            if (FdData.Offsets[8].ValueChanged)
            {
                IpcMessage($"Autobrake switched to {FdData.AutoBrakeSwitch}.");
            }
            if (FdData.Offsets[9].ValueChanged)
            {
                var status = FdData.AutoPilotMasterSwitch == 0 ? "off" : "on";
                IpcMessage($"Autopilot switched to {status}.");
            }
            if (FdData.Offsets[3].ValueChanged)
            {
                var status = FdData.SpoilersArm == 1 ? "armed" : "disarmed";
                IpcMessage($"Spoilers {status}.");
            }
            if (!_isUnderTurbulence && ((SimulatorDataInfo)SimData.ToInfo()).TurblencePrecentage > 10 && AcData.OnGround == 0)
            {
                _isUnderTurbulence = true;
                Task.Factory.StartNew(() =>
                {
                    PAHelper.PlayFASound("turbulance", _voiceEngEnable, _soundPackName);
                    _isUnderTurbulence = false;
                });
            }
            try
            {
                if ((SimData.ZuluHour != DateTime.UtcNow.Hour || SimData.ZuluMinute != DateTime.UtcNow.Minute) && _forceSyncTime == 1) SimTimeHelper.SetToCurrent();
                if (WarningData.StallWarning == 1)
                {
                }

                if (WarningData.OverSpeedWarning == 1)
                {
                }

                _payload.RefreshData();
                if (_payload.FuelWeightKgs > _flightLog.FuelWeight && _flightLog.FuelWeight != 0) Log(FlightLogItemType.Bad, $"Fuel weight changed from {_flightLog.FuelWeight} to {_payload.FuelWeightKgs}", -10);
                if (_phase == FlightPhase.PreFlight)
                {
                    _flightLog.FuelWeight = _payload.FuelWeightKgs;
                    _flightLog.PayloadWeight = _payload.GrossWeightKgs;
                    if (ValueHelper.Engine(InsData.Engine1N1) > 10 && !_engine1Start && ValueHelper.Engine(InsData.Engine1N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 1 Start", 0);
                        _engine1Start = true;
                    }
                    if (ValueHelper.Engine(InsData.Engine3N1) > 10 && !_engine1Start && ValueHelper.Engine(InsData.Engine3N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 3 Start", 0);
                        _engine1Start = true;
                    }
                    if (ValueHelper.Engine(InsData.Engine4N1) > 10 && !_engine1Start && ValueHelper.Engine(InsData.Engine4N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 4 Start", 0);
                        _engine1Start = true;
                    }
                    if (ValueHelper.Engine(InsData.Engine2N1) > 10 && !_engine2Start && ValueHelper.Engine(InsData.Engine2N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 2 Start", 0);
                        _engine2Start = true;
                        _engine2StartTime = DateTime.UtcNow;
                    }
                    if ((ValueHelper.Engine(InsData.Engine1N1) > 10 && ValueHelper.Engine(InsData.Engine1N1) < 40) || (ValueHelper.Engine(InsData.Engine2N1) > 10 && ValueHelper.Engine(InsData.Engine2N1) < 40))
                    {
                        if(AcData.EngineNumber == 2)
                        {
                            _phase = FlightPhase.TaxiToRwy;
                            TaxiRwyStatePreCheck();
                            Log(FlightLogItemType.Normal, $"Flight Phase Changed: Preflight --> Taxi To Runway with fuel: {_flightLog.FuelWeight:f2} kgs, GW: {_flightLog.PayloadWeight:f2} kgs", 0);
                        }
                        else if((ValueHelper.Engine(InsData.Engine3N1) > 10 && ValueHelper.Engine(InsData.Engine3N1) < 40) || (ValueHelper.Engine(InsData.Engine4N1) > 10 && ValueHelper.Engine(InsData.Engine4N1) < 40))
                        {
                            _phase = FlightPhase.TaxiToRwy;
                            TaxiRwyStatePreCheck();
                            Log(FlightLogItemType.Normal, $"Flight Phase Changed: Preflight --> Taxi To Runway with fuel: {_flightLog.FuelWeight:f2} kgs, GW: {_flightLog.PayloadWeight:f2} kgs", 0);
                        }
                    }
                    if (ValueHelper.GroundSpeed(AcData.GroundSpeed) > 0.5 && ValueHelper.GroundSpeed(AcData.GroundSpeed) < 1)
                    {
                        _phase = FlightPhase.PushBack;
                        _flightLog.FuelWeight = _payload.FuelWeightKgs;
                        Log(FlightLogItemType.Normal, $"Flight Phase Changed: Preflight --> Push Back with fuel: {_flightLog.FuelWeight:f2} kgs, GW: {_flightLog.PayloadWeight:f2} kgs", 0);
                        if (_voiceEngEnable == 1) _welcomePlayer.Stop();
                        if (_voiceEngEnable == 1) _safetyPlayer.Play();
                    }
                }
                if (_phase == FlightPhase.PushBack)
                {
                    if (ValueHelper.Engine(InsData.Engine1N1) > 10 && !_engine1Start && ValueHelper.Engine(InsData.Engine1N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 1 Start", 0);
                        _engine1Start = true;
                    }
                    if (ValueHelper.Engine(InsData.Engine2N1) > 10 && !_engine2Start && ValueHelper.Engine(InsData.Engine2N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 2 Start", 0);
                        _engine2Start = true;
                        _engine2StartTime = DateTime.UtcNow;
                    }
                    if (ValueHelper.Engine(InsData.Engine3N1) > 10 && !_engine1Start && ValueHelper.Engine(InsData.Engine3N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 3 Start", 0);
                        _engine1Start = true;
                    }
                    if (ValueHelper.Engine(InsData.Engine4N1) > 10 && !_engine1Start && ValueHelper.Engine(InsData.Engine4N1) < 40)
                    {
                        Log(FlightLogItemType.Normal, "Engine 4 Start", 0);
                        _engine1Start = true;
                    }
                    //松停机刹车后进入滑行且地速大于3并只有一发开启
                    if (ValueHelper.GroundSpeed(AcData.GroundSpeed) > 2 && (ValueHelper.Engine(InsData.Engine2N1) < 15 || ValueHelper.Engine(InsData.Engine2N1) > 100) && (ValueHelper.Engine(InsData.Engine1N1) > 15 || ValueHelper.Engine(InsData.Engine1N1) < 100))
                    {
                        Log(FlightLogItemType.Good, "Enter Single Engine Taxi Stage", 300);
                        _landingLightWarningSentInThisPhase = false;
                        _phase = FlightPhase.TaxiToRwySe;
                    }
                    //开启停机刹车并双发开启
                    if ((ValueHelper.Engine(InsData.Engine1N1) > 10 && ValueHelper.Engine(InsData.Engine1N1) < 100) && (ValueHelper.Engine(InsData.Engine2N1) > 10 && ValueHelper.Engine(InsData.Engine2N1) < 100) && ValueHelper.GroundSpeed(AcData.GroundSpeed) > 2 && ValueHelper.GroundSpeed(AcData.GroundSpeed) > 2 && -((AircraftDataInfo)AcData.ToInfo()).Pitch < 1)
                    {
                        TaxiRwyStatePreCheck();
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Push Back --> Taxi To Runway", 0);
                        _landingLightWarningSentInThisPhase = false;
                        _phase = FlightPhase.TaxiToRwy;
                    }
                    if (ValueHelper.GroundSpeed(AcData.GroundSpeed) > 10)
                    {
                        TaxiRwyStatePreCheck();
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Push Back --> Taxi To Runway", 0);
                        _landingLightWarningSentInThisPhase = false;
                        _phase = FlightPhase.TaxiToRwy;
                    }
                }
                if (_phase == FlightPhase.TaxiToRwySe)
                {
                    if (ValueHelper.Engine(InsData.Engine2N1) > 15 && !_engine2Start)
                    {
                        _engine2StartTime = DateTime.UtcNow;
                    }
                    if (ValueHelper.Engine(InsData.Engine1N1) > 70 && ValueHelper.Engine(InsData.Engine2N1) > 70)
                    {
                        // 二号发动机必须在起飞前3分钟开启，防止发动机热冲击
                        if (DateTime.Now - _engine2StartTime < TimeSpan.FromMinutes(3)) Log(FlightLogItemType.Bad, "Danger Operation: You should have started Engine N.2 at least 3 minutes before takeoff", -800);
                        TakeoffPreCheck();
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Taxi To Runway --> Rolling", 0);
                        _landingLightWarningSentInThisPhase = false;
                        _phase = FlightPhase.Rolling;
                    }
                }
                if (_phase == FlightPhase.TaxiToRwy)
                {
                    if (ValueHelper.Engine(InsData.Engine2N1) > 15 && ValueHelper.Engine(InsData.Engine2N1) < 15.5)
                    {
                        _engine2StartTime = DateTime.UtcNow;
                    }
                    if (ValueHelper.Engine(InsData.Engine1N1) > 70 && ValueHelper.Engine(InsData.Engine2N1) > 70 && FdData.ParkingBrake < 0.2)
                    {
                        _phase = FlightPhase.Rolling;
                        _landingLightWarningSentInThisPhase = false;
                        if (_voiceEngEnable == 1) _safetyPlayer.Stop();
                        // 二号发动机必须在起飞前3分钟开启，防止发动机热冲击
                        if (DateTime.Now - _engine2StartTime < TimeSpan.FromMinutes(3)) Log(FlightLogItemType.Bad, "Danger Operation: You should have started Engine N.2 at least 3 minutes before takeoff", -800);
                        TakeoffPreCheck();
                        Log(FlightLogItemType.Normal, $"Flight Phase Changed: Taxi To Runway --> Rolling | Flpas Position: {((FlightControlDataInfo)FdData.ToInfo()).FlapsPL:f2}%", 0);
                    }
                }
                if (_phase == FlightPhase.Rolling)
                {
                    if (ValueHelper.GroundSpeed(AcData.GroundSpeed) > 55)
                    {
                        _phase = FlightPhase.TakeOff;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, $"Flight Phase Changed: Rolling --> Take Off | Airspeed: {((AircraftDataInfo)AcData.ToInfo()).AirSpeed.Imperial} kts | Pitch: {-((AircraftDataInfo)AcData.ToInfo()).Pitch}°", 0);
                    }
                    if (FdData.ParkingBrake > 0.3 && ValueHelper.Engine(InsData.Engine1N1) < 60 && ValueHelper.Engine(InsData.Engine2N1) < 60 && ValueHelper.Engine(InsData.Engine3N1) < 60 && ValueHelper.Engine(InsData.Engine4N1) < 60)
                    {
                        Log(FlightLogItemType.Bad, "Flight Phase Changed: Rolling --> Abort Take Off", -800);
                        _landingLightWarningSentInThisPhase = false;
                        _phase = FlightPhase.AbortTakeOff;
                    }
                }
                if (_phase == FlightPhase.AbortTakeOff)
                {
                    if (ValueHelper.GroundSpeed(AcData.GroundSpeed) <= 15)
                    {
                        _phase = FlightPhase.TaxiToGate;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Abort Take Off --> Taxi To Gate", 0);
                    }
                }
                if (_phase == FlightPhase.TakeOff)
                {
                    if (AcData.OnGround == 0)
                    {
                        _phase = FlightPhase.Climb;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, $"Flight Phase Changed: Take Off --> Climb | Airspeed: {((AircraftDataInfo)AcData.ToInfo()).AirSpeed.Imperial:f2} kts | Pitch: {-((AircraftDataInfo)AcData.ToInfo()).Pitch:f2}°", 0);
                    }
                }
                if (_phase == FlightPhase.Climb)
                {
                    if (ValueHelper.Altitude(AcData.Altitude) - ValueHelper.GAVS(AcData.GroundAltitude) > 3000 && !_climbPa)
                    {
                        PAHelper.PlayFASound("climb", _voiceEngEnable, _soundPackName);
                        _climbPa = true;
                    }
                    if(FdData.GearControl == 0 && !_gearRetracted)
                    {
                        Log(FlightLogItemType.Normal, $"Gear retracting at {((AircraftDataInfo)AcData.ToInfo()).AirSpeed.Imperial} kts", 0);
                        _gearRetracted = true;
                    }
                    if (Math.Abs(ValueHelper.GAVS(InsData.VerticalSpeed)) < 0.2 && ValueHelper.Altitude(AcData.Altitude) > ValueHelper.Feet2Meter(_cruiseAltitude) - 1000)
                    {
                        _phase = FlightPhase.Cruise;
                        _landingLightWarningSentInThisPhase = false;
                        PAHelper.PlayFASound("cruise", _voiceEngEnable, _soundPackName);
                        Log(FlightLogItemType.Normal, $"Flight Phase Changed: Climb --> Cruise | Airspeed: {((AircraftDataInfo)AcData.ToInfo()).AirSpeed.Imperial:f2} kts | Elevation: {ValueHelper.Altitude(AcData.Altitude):f2}", 0);
                    }
                }
                if (_phase == FlightPhase.Cruise)
                {
                    if (ValueHelper.GAVS(InsData.VerticalSpeed) < -5)
                    {
                        _phase = FlightPhase.Descend;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Cruise --> Descend", 0);
                        PAHelper.PlayFASound("decent.wav", _voiceEngEnable, _soundPackName);
                    }
                    if (ValueHelper.GAVS(InsData.VerticalSpeed) > 5)
                    {
                        _phase = FlightPhase.Climb;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Cruise --> Climb", 0);
                    }
                }
                if (_phase == FlightPhase.Descend)
                {
                    if (ValueHelper.Feet2Meter(ValueHelper.Altitude(AcData.Altitude) - ValueHelper.GAVS(AcData.GroundAltitude)) < 3000)
                    {
                        _phase = FlightPhase.Approach;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Descend --> Approach", 0);
                        PAHelper.PlayFASound("prepare_for_landing", _voiceEngEnable, _soundPackName);
                    }
                    if (Math.Abs(ValueHelper.GAVS(InsData.VerticalSpeed)) < 1 && ValueHelper.Altitude(AcData.Altitude) > ValueHelper.Feet2Meter(_cruiseAltitude) - 1000)
                    {
                        _phase = FlightPhase.Cruise;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Descend --> Cruise", 0);
                    }
                }
                if (_phase == FlightPhase.Approach)
                {
                    if (ValueHelper.Meter2Feet(ValueHelper.RadioAltitude(InsData.RadioAltitude)) < 2500)
                    {
                        _phase = FlightPhase.FinalApproach;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Approach --> FinalApproach", 0);
                        PAHelper.PlayFASound("prepare_for_landing", _voiceEngEnable, _soundPackName);
                    }
                    if (ValueHelper.GAVS(InsData.VerticalSpeed) > 5)
                    {
                        _phase = FlightPhase.GoAround;
                        _landingLightWarningSentInThisPhase = false;
                        Log(FlightLogItemType.Normal, $"Flight Phase Changed: Approach --> Go Around RA: {ValueHelper.RadioAltitude(InsData.RadioAltitude)} m", 0);
                    }
                }
                if (_phase == FlightPhase.FinalApproach)
                {
                    if (AcData.Offsets[12].ValueChanged && AcData.OnGround == 1)
                    {
                        var tdd = new TouchDownData
                        {
                            Speed = ((AircraftDataInfo)AcData.ToInfo()).GroundSpeed.Imperial,
                            GForce = ValueHelper.GFTD(AcData.GForce),
                            AOA = ValueHelper.PitchRoll(AcData.Pitch),
                            LandingRate = ValueHelper.VSFPM(InsData.VerticalSpeed)
                        };
                        _flightLog.TouchDownData.Add(tdd);
                        Log(FlightLogItemType.Normal, $"Touch Down Detected at {tdd.LandingRate:f2}fpm, g force: {tdd.GForce:f4}, aoa: {-tdd.AOA:f2}, speed: {tdd.Speed:f2}", 0);
                    }
                    if (ValueHelper.GroundSpeed(AcData.GroundSpeed) < 20 && AcData.OnGround == 1)
                    {
                        _phase = FlightPhase.TaxiToGate;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: FinalApproach --> Taxi To Gate", 0);
                        if (_voiceEngEnable == 1) PAHelper.PlayFASound("landed", _voiceEngEnable, _soundPackName);
                    }
                    if (ValueHelper.GAVS(InsData.VerticalSpeed) > 3)
                    {
                        _phase = FlightPhase.GoAround;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: FinalApproach --> Go Around", 0);
                    }
                }
                if (_phase == FlightPhase.GoAround)
                {
                    if (Math.Abs(ValueHelper.GAVS(InsData.VerticalSpeed)) < 0.2)
                    {
                        _phase = FlightPhase.Approach;
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Go Around --> Approach", 0);
                    }
                }
                if (_phase == FlightPhase.TaxiToGate)
                {
                    // 当地速大于3m/s且一发运转二发关车
                    if (ValueHelper.GroundSpeed(AcData.GroundSpeed) > 3 && ValueHelper.Engine(InsData.Engine1N1) > 15 && ValueHelper.Engine(InsData.Engine2N1) < 15)
                    {
                        _phase = FlightPhase.TaxiToGateSe;
                        Log(FlightLogItemType.Good, "Enter Single Engine Taxi Stage", 300);
                    }
                    //一发和二发均关车
                    if ((ValueHelper.Engine(InsData.Engine1N1) < 15 || ValueHelper.Engine(InsData.Engine1N1) > 100) && (ValueHelper.Engine(InsData.Engine2N1) < 15 || ValueHelper.Engine(InsData.Engine2N1) > 100))
                    {
                        _phase = FlightPhase.Shutdown;
                        if (ValueHelper.FlapP(FdData.FlapsPL) != 0 || ValueHelper.FlapP(FdData.FlapsPR) != 0) Log(FlightLogItemType.Bad, "Flaps Not Retracted", -100);
                        Log(FlightLogItemType.Normal, "Flight Phase Changed: Taxi To Gate --> Shutdown", 0);
                        SubmitPirep();
                    }
                }
                if (_phase == FlightPhase.TaxiToGateSe)
                {
                    //一发二发均关车
                    if ((ValueHelper.Engine(InsData.Engine1N1) < 15 || ValueHelper.Engine(InsData.Engine1N1) > 100) && (ValueHelper.Engine(InsData.Engine2N1) < 15 || ValueHelper.Engine(InsData.Engine2N1) > 100))
                    {
                        _phase = FlightPhase.Shutdown;
                        if (ValueHelper.FlapP(FdData.FlapsPL) != 0 || ValueHelper.FlapP(FdData.FlapsPR) != 0) Log(FlightLogItemType.Bad, "Flaps Not Retracted", -100);
                        Log(FlightLogItemType.Good, $"Flight Phase Changed: Taxi To Gate --> Shutdown (SET Time: {_flightLog.SETHours}:{_flightLog.SETMinutes}:{_flightLog.SETSeconds})", 300);
                        SubmitPirep();
                    }
                }
                if (ValueHelper.Altitude(AcData.Altitude) - ValueHelper.GAVS(AcData.GroundAltitude) < 9800 && !((LightsDataInfo)LightsData.ToInfo()).Landing && AcData.OnGround == 0 
                    && _phase != FlightPhase.TakeOff
                    && _phase != FlightPhase.Climb)
                {
                    if (!_landingLightWarningSentInThisPhase)
                    {
                        Log(FlightLogItemType.Bad, "Landing Light Still off below 9800ft.", 0);
                        _landingLightWarningSentInThisPhase = true;
                    }
                    _flightLog.LandingLightScore -= 0.5;
                }
                if (ValueHelper.Altitude(AcData.Altitude) - ValueHelper.GAVS(AcData.GroundAltitude) > 11000 && ((LightsDataInfo)LightsData.ToInfo()).Landing)
                {
                    if (!_landingLightWarningSentInThisPhase)
                    {
                        Log(FlightLogItemType.Bad, "Landing Light Still on over 11000ft.", 0);
                        _landingLightWarningSentInThisPhase = true;
                    }
                    _flightLog.LandingLightScore -= 0.5;
                }
            }
            catch(Exception e)
            {
                IpcLog("Error739:"+JsonConvert.SerializeObject(e));
            }
            
        }
        public async void SubmitPirep()
        {
            try
            {
                _payload.RefreshData();
                _flightLog.FuelConsumption = _flightLog.FuelWeight - _payload.FuelWeightKgs;
                _flightLog.AirHours = _flightLog.FlightHours - _flightLog.TaxiHours;
                _flightLog.AirMinutes = _flightLog.FlightMinutes - _flightLog.TaxiMinutes;
                _flightLog.AirSeconds = _flightLog.FlightSeconds - _flightLog.TaxiSeconds;
                if (_flightLog.AirSeconds < 0)
                {
                    _flightLog.AirMinutes--;
                    _flightLog.AirSeconds = 60 + _flightLog.AirSeconds;
                }
                if (_flightLog.AirMinutes < 0)
                {
                    _flightLog.AirHours--;
                    _flightLog.AirMinutes = 60 + _flightLog.AirMinutes;
                }
                var fullLog = JsonConvert.SerializeObject(_flightLog);
                var path = AppDomain.CurrentDomain.BaseDirectory + "flightLog.json";
                var submitReq = new HttpClient();
                ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
                var body = new Dictionary<string, string>
                {
                    { "raw", fullLog }
                };
                var result = await submitReq.PostAsync($"{_reqUrl}submit&bidid={_bidId}&route={_activeRoute}", new CJsonContent(body));
                ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes($"log*[{DateTime.UtcNow}] Your pirep has been submited."));
                _phaseAnalyseStopFlag = true;
                ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes($"log*[{DateTime.UtcNow}] Flight phase analysis is now ended."));
                var response = await result.Content.ReadAsStringAsync();
                ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes($"pirepres*{response}.bidId: {_bidId}"));
                if (File.Exists(path)) File.Delete(path);
                var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                var sr = new StreamWriter(fs);
                sr.Write(fullLog);
                sr.Close();
                fs.Close();
                IpcMessage($"Flight log has been generated in {AppDomain.CurrentDomain.BaseDirectory}");
            }
            catch(Exception e)
            {
                IpcLog($"Error on submit: {JsonConvert.SerializeObject(e)}");
            }
        }
    }
}
