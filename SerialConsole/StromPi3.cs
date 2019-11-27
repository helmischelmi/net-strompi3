using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SerialConsole
{
    /// <summary>
    /// UPS: Strompi3 with Serial UART - Interface
    /// Example, reads from arduino: // https://www.hackster.io/sxwei123/serial-communication-with-net-core-3-0-on-rpi-linux-0f2ed4
    /// </summary>
    public class StromPi3 : IDisposable
    {
        //parameters for serial communication
        private SerialPortParameter _portParameter;
        private SerialPort _serialPort;
        public StromPi3State State { get; }

        private const string PowerFailureMessage = "ShutdownRaspberryPi";
        private const string PowerBackMessage = "StromPiPowerBack";


        public class StromPi3State
        {
            public DateTime CurrentDateTime { get; private set; }
            public EInputPriority PriorityMode { get; private set; }
            public StromPi3Alarm Alarm { get; }
            public StromPi3Battery Battery { get; }
            public StromPi3StartStop StartStop { get; }
            public StromPi3Voltage Voltage { get; }

            public bool ShutdownEnable { get; private set; }
            public int ShutdownSeconds { get; private set; }
            public bool PowerFailWarningEnable { get; private set; }
            public bool SerialLessEnable { get; private set; }

            public EOutputStatus OutputStatus { get; private set; }
            public int PowerFailureCounter { get; private set; }
            public string FirmwareVersion { get; private set; }

            public StromPi3State()
            {
                Alarm = new StromPi3Alarm();
                Battery = new StromPi3Battery();
                StartStop = new StromPi3StartStop();
                Voltage = new StromPi3Voltage();
            }

            public class StromPi3Voltage
            {
                private const double WideRangeVoltMin = 4.8;
                private const double BatteryVoltMin = 0.5;
                private const double MUsbVoltMin = 4.1;

                public double WideRangeVolt { get; private set; }
                public double mUsbVolt { get; private set; }
                public double BatteryVolt { get; private set; }
                public double OutputVolt { get; private set; }

                public void SetVoltage(string sp3AdcWide, string sp3AdcBat, string sp3AdcUsb, string outputVolt)
                {
                    try
                    {
                        WideRangeVolt = VoltageConverter(Convert.ToDouble(sp3AdcWide) / 1000, WideRangeVoltMin);
                        mUsbVolt = VoltageConverter(Convert.ToDouble(sp3AdcUsb) / 1000, MUsbVoltMin);
                        BatteryVolt = VoltageConverter(Convert.ToDouble(sp3AdcBat) / 1000, BatteryVoltMin);
                        OutputVolt = Convert.ToDouble(outputVolt) / 1000;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                private static double VoltageConverter(double voltage, double minimumVoltage)
                {
                    if (voltage > minimumVoltage)
                    {
                        return voltage;
                    }
                    return 0;
                }
            }

            public class StromPi3StartStop
            {
                public bool PowerOnButtonEnable { get; private set; }
                public int PowerOnButtonSeconds { get; private set; }
                public bool PowersaveEnable { get; private set; }
                public bool PoweroffMode { get; private set; }

                public bool PoweroffTimeEnableMode { get; private set; }
                public string WakeupTimerMinutes { get; private set; }
                public bool WakeupWeekendEnable { get; private set; }

                public void SetStartStop(string sp3PowerOnButtonEnable, string sp3PowerOnButtonTime, string sp3PowersaveEnable, string sp3PoweroffMode,
                    string poweroffTimeEnableMode, string wakeupTimerMinutes, string sp3WakeupweekendEnable)
                {
                    PowerOnButtonEnable = EnabledDisabledConverter(sp3PowerOnButtonEnable, "sp3PowerOnButtonEnable");
                    PowerOnButtonSeconds = Convert.ToInt32(sp3PowerOnButtonTime);
                    PowersaveEnable = EnabledDisabledConverter(sp3PowersaveEnable, "sp3PowersaveEnable");
                    PoweroffMode = EnabledDisabledConverter(sp3PoweroffMode, "sp3PoweroffMode");
                    PoweroffTimeEnableMode = EnabledDisabledConverter(poweroffTimeEnableMode, "poweroffTimeEnableMode");
                    WakeupTimerMinutes = wakeupTimerMinutes;
                    WakeupWeekendEnable = EnabledDisabledConverter(sp3WakeupweekendEnable, "sp3WakeupweekendEnable");
                }
            }

            public class StromPi3Battery
            {
                public EBatteryLevel Level { get; private set; }
                public bool IsCharging { get; private set; }
                public EShutdownLevel ShutdownLevel { get; private set; }

                public enum EShutdownLevel
                {
                    [Description("[Disabled]")]
                    Disabled = 0,

                    [Description("[10%]")]
                    tenPercent = 1,

                    [Description("[25%]")]
                    quarterLeft = 2,

                    [Description("[50%]")]
                    halfEmpty = 3,

                    [Description("[nothing]")]
                    nothing = 4
                }
                public enum EBatteryLevel
                {
                    [Description("nothing")]
                    nothing = 0,

                    [Description("[10%]")]
                    tenPercent = 1,

                    [Description(" [25%]")]
                    twentyfivePercent = 2,

                    [Description("[50%]")]
                    fiftyPercent = 3,

                    [Description("[100%]")]
                    hundredPercent = 4
                }

                public void SetBattery(string sp3BatLevelShutdown, string sp3BatLevel, string sp3Charging)
                {
                    try
                    {
                        IsCharging = Convert.ToInt32(sp3Charging) > 0;

                        Level = EBatteryLevel.nothing;
                        int level = Convert.ToInt32(sp3BatLevel);
                        if (level >= 0 && level <= 4) Level = (EBatteryLevel)level;

                        ShutdownLevel = EShutdownLevel.nothing;
                        var levelShutdown = Convert.ToInt32(sp3BatLevelShutdown);

                        if (levelShutdown >= 0 && levelShutdown <= 4) ShutdownLevel = (EShutdownLevel)levelShutdown;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            public class StromPi3Alarm
            {
                public bool Enabled { get; private set; }
                public bool PowerOffEnabled { get; private set; }
                public bool IntervalAlarmEnabled { get; private set; }
                public string IntervalAlarmOnMinutes { get; private set; }
                public string IntervalAlarmOffMinutes { get; private set; }
                public EAlarmMode Mode { get; private set; }
                public string PowerOffHours { get; private set; }
                public string PowerOffMinutes { get; private set; }
                public int Hour { get; private set; }
                public int Minute { get; private set; }
                public int Day { get; private set; }
                public int Month { get; private set; }
                public EWeekday Weekday { get; private set; }

                public enum EAlarmMode
                {
                    [Description("nothing")]
                    nothing = 0,

                    [Description("Time-Alarm")]
                    TimeAlarm = 1,

                    [Description("Date-Alarm")]
                    DateAlarm = 2,

                    [Description("Weekday-Alarm")]
                    WeekdayAlarm = 3
                }

                public enum EWeekday
                {
                    nothing = 0,
                    Monday = 1,
                    Tuesday = 2,
                    Wednesday = 3,
                    Thursday = 4,
                    Friday = 5,
                    Saturday = 6,
                    Sunday = 7,
                }

                public void SetEnabled(string sp3AlarmEnable)
                {
                    Enabled = EnabledDisabledConverter(sp3AlarmEnable, "sp3AlarmEnable");
                }
                public void SetMode(string sp3AlarmMode)
                {
                    Mode = EAlarmMode.nothing;
                    var mode = Convert.ToInt32(sp3AlarmMode);

                    if (mode >= 1 && mode <= 3)
                    {
                        Mode = (EAlarmMode)mode;
                    }
                }

                public void SetPowerOffEnabled(string sp3AlarmPoweroff)
                {
                    PowerOffEnabled = EnabledDisabledConverter(sp3AlarmPoweroff, "sp3AlarmPoweroff");
                }

                public void SetIntervallAlarm(string sp3IntervalAlarm, string intervallAlarmOnTimeMinutes, string intervallAlarmOffTimeMinutes)
                {
                    IntervalAlarmOnMinutes = intervallAlarmOnTimeMinutes;
                    IntervalAlarmOffMinutes = intervallAlarmOffTimeMinutes;
                    IntervalAlarmEnabled = EnabledDisabledConverter(sp3IntervalAlarm, "sp3IntervalAlarm");
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="sp3AlarmHour"></param>
                /// <param name="sp3AlarmMin"></param>
                /// <param name="sp3AlarmDay"></param>
                /// <param name="sp3AlarmMonth"></param>
                /// <param name="sp3AlarmWeekday"></param>
                public void SetAlarmDateTime(string sp3AlarmHour, string sp3AlarmMin, string sp3AlarmDay, string sp3AlarmMonth, string sp3AlarmWeekday)
                {
                    try
                    {
                        Hour = Convert.ToInt32(sp3AlarmHour);
                        Minute = Convert.ToInt32(sp3AlarmMin);
                        Day = Convert.ToInt32(sp3AlarmDay);
                        Month = Convert.ToInt32(sp3AlarmMonth);

                        Weekday = EWeekday.nothing;
                        var weekday = Convert.ToInt32(sp3AlarmWeekday);

                        if (weekday >= 1 && weekday <= 7)
                        {
                            Weekday = (EWeekday)weekday;
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                    }
                }

                public void SetPowerOffAlarmTime(string sp3PowerOffHours, string sp3PowerOffMinutes)
                {
                    PowerOffHours = sp3PowerOffHours;
                    PowerOffMinutes = sp3PowerOffMinutes;
                }

            }

            /// <summary>
            /// 
            /// </summary>
            public enum EInputPriority
            {
                [Description("nothing")]
                nothing = 0,

                [Description("mUSB -> Wide")]
                mUSB_Wide = 1,

                [Description("Wide -> mUSB")]
                Wide_mUSB = 2,

                [Description("mUSB -> Battery")]
                mUSB_Battery = 3,

                [Description("Wide -> Battery")]
                Wide_Battery = 4,

                [Description("mUSB -> Wide -> Battery")]
                mUSB_Wide_Battery = 5,

                [Description("Wide -> mUSB -> Battery")]
                Wide_mUSB_Battery = 6
            }

            public enum EOutputStatus
            {
                PowerOff = 0, // #only for Debugging-Purposes
                mUSB = 1,
                Wide = 2,
                Battery = 3,
                nothing = 4,
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="sp3Time">format=hhmmss</param>
            /// <param name="sp3Date">format=yymmdd</param>
            public void SetCurrentDateTime(string sp3Time, string sp3Date)
            {
                try
                {
                    int intSp3Time = Convert.ToInt32(sp3Time);
                    var ts = new TimeSpan(intSp3Time / 10000, intSp3Time % 10000 / 100, intSp3Time % 100);
                    int isp3Date = Convert.ToInt32(sp3Date);
                    int year = (isp3Date / 10000) + 2000;
                    int month = isp3Date % 10000 / 100;
                    int day = isp3Date % 100;
                    CurrentDateTime = new DateTime(year, month, day) + ts;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"***error: Conversion of current DateTime failed: time = '{sp3Time}', date= '{sp3Date}'");
                    CurrentDateTime = DateTime.MinValue;
                }
            }

            public void SetPriorityMode(string sp3Modus)
            {
                PriorityMode = EInputPriority.nothing;
                var modus = Convert.ToInt32(sp3Modus);

                if (modus >= 1 && modus <= 6)
                {
                    PriorityMode = (EInputPriority)modus;
                }
            }

            public void SetShutDown(string sp3ShutdownEnable, string sp3ShutdownSeconds)
            {
                try
                {
                    ShutdownSeconds = Convert.ToInt32(sp3ShutdownSeconds);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                ShutdownEnable = EnabledDisabledConverter(sp3ShutdownEnable, "sp3ShutdownEnable");
            }

            public void SetPowerFailWarningEnable(string sp3WarningEnable)
            {
                PowerFailWarningEnable = EnabledDisabledConverter(sp3WarningEnable, "sp3WarningEnable");
            }

            public void SetSerialLessEnable(string sp3SerialLessMode)
            {
                SerialLessEnable = EnabledDisabledConverter(sp3SerialLessMode, "sp3SerialLessMode");
            }

            public void SetOutputStatus(string sp3OutputState)
            {
                OutputStatus = EOutputStatus.nothing;
                var modus = Convert.ToInt32(sp3OutputState);

                if (modus >= 0 && modus <= 4)
                {
                    OutputStatus = (EOutputStatus)modus;
                }
            }

            public void SetPowerFailureCounter(string sp3PowerFailureCounter)
            {
                try
                {
                    PowerFailureCounter = Convert.ToInt32(sp3PowerFailureCounter);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void SetFirmwareVersion(string sp3FirmwareVersion)
            {
                FirmwareVersion = sp3FirmwareVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public StromPi3()
        {
            State = new StromPi3State();

            Connect();
        }

        /// <summary>
        /// Connect the serial interface of StromPi3
        /// </summary>
        public void Connect()
        {
            int baudRate = 38400;
            string portName = @"/dev/serial0";
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            Parity parity = Parity.None;

            // Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
            int readTimeout = 1000;
            int writeTimeout = 1000;

            _portParameter = new SerialPortParameter(portName, baudRate, parity, dataBits, stopBits, readTimeout, writeTimeout);

            if (_serialPort == null)
            {
                _serialPort = _portParameter.GetSerialPort();
            }

            Console.WriteLine($"connected to {_portParameter.GetStatus()}");

            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            //_serialPort.DataReceived += ReceivedData;
            _serialPort.Open();
            Console.WriteLine($"serial port is open ");
        }

        public void GetSerialProperties()
        {
            string strEol = String.Empty;
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(_serialPort.NewLine.ToCharArray()))
                strEol += b.ToString();
            Console.WriteLine("---------------vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv-------------");
            Console.WriteLine($"Handshake:{_serialPort.Handshake}, Connection broken: {_serialPort.BreakState}, Carrier-Detect-line: {_serialPort.CDHolding},{Environment.NewLine}" +
                   $"Clear-to-Send-Line (CTS): {_serialPort.CtsHolding}, Data Set Ready-Signal (DSR) was sent: {_serialPort.DsrHolding}, {Environment.NewLine}" +
                   $" Data Terminal Ready (DTR): {_serialPort.DtrEnable},  Request to Transmit (RTS): {_serialPort.RtsEnable}, ASCII-Wert for eol (Default \\n in C#):'{strEol}', {Environment.NewLine}" +
                   $"Bytes im Empfangspuffer: {_serialPort.BytesToRead}, im Sendepuffer: {_serialPort.BytesToWrite}");
            Console.WriteLine($"---------------^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-------------{Environment.NewLine}");
        }

        /// <summary>
        /// Allows editing the configuration-settings of StromPi3.
        /// <para>Requires serial-mode</para>
        /// <remarks>The functionality of strompi_config.py from joy-it is ported by this method.
        /// </remarks>
        /// </summary>
        public void Configure()
        {



        }


        public void StartSerialLessMode()
        {
            //import RPi.GPIO as GPIO
            //import time
            //import os
            //import serial
            //GPIO.setmode(GPIO.BCM)
            //GPIO_TPIN = 21

            //breakS = 0.1
            //breakL = 0.2
            //serial_port = serial.Serial()
            //serial_port.baudrate = 38400
            //serial_port.port = '/dev/serial0'
            //serial_port.timeout = 1
            //serial_port.bytesize = 8
            //serial_port.stopbits = 1
            //serial_port.parity = serial.PARITY_NONE

            //if serial_port.isOpen(): serial_port.close()
            //serial_port.open()

            //serial_port.write(str.encode('quit'))
            //time.sleep(breakS)
            //serial_port.write(str.encode('\x0D'))
            //time.sleep(breakL)
            //serial_port.write(str.encode('set-config 0 2'))
            //time.sleep(breakS)
            //serial_port.write(str.encode('\x0D'))
            //time.sleep(breakL)
            //print ("Enabled Serialless")

        }

        public void StopSerialLessMode()
        {
            //            import RPi.GPIO as GPIO
            //            import time
            //            import os
            //            import serial
            //            GPIO.setmode(GPIO.BCM)
            //            GPIO_TPIN = 21


            //            GPIO.setup(GPIO_TPIN,GPIO.OUT)
            //#GPIO.output(GPIO_TPIN, GPIO.HIGH)
            //#print ("HIGH")
            //#time.sleep(3)
            //            GPIO.output(GPIO_TPIN, GPIO.LOW)
            //            print ("Setting GPIO to LOW to Disable Serialless Mode.")
            //            print ("This will take approx. 10 seconds.")
            //            time.sleep(10)
            //            GPIO.cleanup()
            //            print ("Serialless Mode is Disabled!")

        }

                /// <summary>
        /// IRQ-based method to get the powerFail-warning- (if enabled in configuration) and powerBack-signal.
        /// <para>Requires serialless-mode</para>
        /// </summary>
        /// <param name="waitForPowerBackTimerSeconds">Seconds to wait for a powerBack-signal before shutting down the Raspberry pi.
        /// This must be set - or will be forced - lower than the configured shutdown-timer to make a safe shutdown.</param>
        public void ShutDownOnPowerFailure(int waitForPowerBackTimerSeconds = 10)
        {
            bool runCountdown = false;
            DateTime powerFailureStartTime = default;
            string data = String.Empty;

            Console.WriteLine($"PollingShutDownOnPowerFailure (wait {waitForPowerBackTimerSeconds} secs)");

            if (!HasValidStrompiSettings(waitForPowerBackTimerSeconds)) return;

            //start polling the serial port of strompi3 
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();

            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    data = _serialPort.ReadLine();
                }
                catch (TimeoutException) { }  // ignore timeouts

                var powerFailureSignal = SetPowerFailureSignal(data);
                var powerBackSignal = SetPowerBackSignal(data);

                if (powerFailureSignal || runCountdown)
                {
                    if (runCountdown == false)
                    {
                        runCountdown = true;
                        powerFailureStartTime = DateTime.Now;
                    }

                    int countdownSeconds = Convert.ToInt32((DateTime.Now - powerFailureStartTime).TotalSeconds);
                    Console.WriteLine($"PowerFail - run countdown to shutdown the Pi ({waitForPowerBackTimerSeconds - countdownSeconds} secs)");

                    if (countdownSeconds >= waitForPowerBackTimerSeconds)
                    {
                        _serialPort.Close();
                        Console.WriteLine($"Raspberry Pi: running shutdown...");
                        Thread.Sleep(500);
                        Os.ShutDown();
                    }
                }

                if (powerBackSignal && runCountdown)
                {
                    Console.WriteLine("PowerBack - aborting Raspberry Pi shutdown");
                    runCountdown = false;
                }
            }
        }


        /// <summary>
        /// Reads all status-related characteristics of the Strompi3.
        /// <para>
        /// <remarks>Requires serial-mode</remarks></para>
        /// </summary>
        public void ReadStatus()
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();

            // GetSerialProperties();
            Console.WriteLine("Read Strompi3-State");

            _serialPort.Write("quit");
            _serialPort.Write("\r"); // \x0d = {13} Carriage Return
            _serialPort.Write("status-rpi");
            _serialPort.Write("\r"); // \x0d = {13} Carriage Return

            string sp3Time = _serialPort.ReadLine();
            string sp3Date = _serialPort.ReadLine();

            State.SetCurrentDateTime(sp3Time, sp3Date);

            string sp3_weekday = _serialPort.ReadLine();  // not used
            string sp3_modus = _serialPort.ReadLine();
            State.SetPriorityMode(sp3_modus);

            string sp3AlarmEnable = _serialPort.ReadLine();
            State.Alarm.SetEnabled(sp3AlarmEnable);

            string sp3AlarmMode = _serialPort.ReadLine();
            State.Alarm.SetMode(sp3AlarmMode);

            string sp3AlarmHour = _serialPort.ReadLine();
            string sp3AlarmMin = _serialPort.ReadLine();
            string sp3AlarmDay = _serialPort.ReadLine();
            string sp3AlarmMonth = _serialPort.ReadLine();
            string sp3AlarmWeekday = _serialPort.ReadLine();
            State.Alarm.SetAlarmDateTime(sp3AlarmHour, sp3AlarmMin, sp3AlarmDay, sp3AlarmMonth, sp3AlarmWeekday);

            string sp3AlarmPoweroff = _serialPort.ReadLine();
            State.Alarm.SetPowerOffEnabled(sp3AlarmPoweroff);

            string alarmPowerOffHours = _serialPort.ReadLine();
            string alarmPowerOffMinutes = _serialPort.ReadLine();
            State.Alarm.SetPowerOffAlarmTime(alarmPowerOffHours, alarmPowerOffMinutes);

            string sp3ShutdownEnable = _serialPort.ReadLine();
            string sp3ShutdownSeconds = _serialPort.ReadLine();
            State.SetShutDown(sp3ShutdownEnable, sp3ShutdownSeconds);

            string sp3WarningEnable = _serialPort.ReadLine();
            State.SetPowerFailWarningEnable(sp3WarningEnable);

            string sp3SerialLessMode = _serialPort.ReadLine();
            State.SetSerialLessEnable(sp3SerialLessMode);

            string sp3IntervalAlarm = _serialPort.ReadLine();
            string sp3IntervallAlarmOnTimeMinutes = _serialPort.ReadLine();
            string sp3IntervallAlarmOffTimeMinutes = _serialPort.ReadLine();
            State.Alarm.SetIntervallAlarm(sp3IntervalAlarm, sp3IntervallAlarmOnTimeMinutes, sp3IntervallAlarmOffTimeMinutes);

            string sp3BatLevelShutdown = _serialPort.ReadLine();
            string sp3BatLevel = _serialPort.ReadLine();
            string sp3Charging = _serialPort.ReadLine();
            State.Battery.SetBattery(sp3BatLevelShutdown, sp3BatLevel, sp3Charging);

            string sp3PowerOnButtonEnable = _serialPort.ReadLine();
            string sp3PowerOnButtonTime = _serialPort.ReadLine();
            string sp3PowersaveEnable = _serialPort.ReadLine();
            string sp3PoweroffMode = _serialPort.ReadLine();
            string poweroffTimeEnableMode = _serialPort.ReadLine();
            string wakeupTimerMinutes = _serialPort.ReadLine();
            string sp3WakeupweekendEnable = _serialPort.ReadLine();

            State.StartStop.SetStartStop(sp3PowerOnButtonEnable, sp3PowerOnButtonTime, sp3PowersaveEnable, sp3PoweroffMode, poweroffTimeEnableMode,
                wakeupTimerMinutes, sp3WakeupweekendEnable);

            string sp3AdcWide = _serialPort.ReadLine();
            string sp3AdcBat = _serialPort.ReadLine();
            string sp3AdcUsb = _serialPort.ReadLine();
            string outputVolt = _serialPort.ReadLine();
            State.Voltage.SetVoltage(sp3AdcWide, sp3AdcBat, sp3AdcUsb, outputVolt);

            string sp3OutputStatus = _serialPort.ReadLine();
            State.SetOutputStatus(sp3OutputStatus);

            string sp3PowerFailureCounter = _serialPort.ReadLine();
            State.SetPowerFailureCounter(sp3PowerFailureCounter);

            string sp3FirmwareVersion = _serialPort.ReadLine();
            State.SetFirmwareVersion(sp3FirmwareVersion);

            _serialPort.Close();
        }

        /// <summary>
        /// Compares the actual SystemTime of the Raspberry Pi with the RTC of StromPi3.
        /// In the case of a deviation, the more recent time is adopted.
        /// <para>Requires serial-mode</para>
        /// <remarks>The functionality of RTCSerial.py from joy-it is ported by this method.The original py-script
        /// uses commands 'Q', '\r', 'date-rpi' and 'time-rpi' to read the current datetime
        /// of Strompi3. This steps could not be implemented successfully and were replaced by calling 'ReadStatus'.
        /// </remarks>
        /// </summary>
        public void SyncRTC()
        {
            // workaround to get the current settings,
            // because commands '"date-rpi' and '"time-rpi' don't work (produce timeout..) until now
            ReadStatus();

            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();

            Console.WriteLine("TimeSync-Process | Please Wait");
            Console.WriteLine($"StromPi3: Current dateTime {State.CurrentDateTime} ");
            var rpiDateTime = DateTime.Now;
            Console.WriteLine($"Raspi: Current dateTime {rpiDateTime} ");

            if (rpiDateTime > State.CurrentDateTime) // sync the Strompi
            {
                Console.WriteLine("The date und time will be synced: Raspberry Pi -> StromPi'");

                int dayOfWeekPython = (int)rpiDateTime.DayOfWeek;

                // map value of sunday (0 in .net to 7 on Strompi3)
                if (dayOfWeekPython == 0) dayOfWeekPython = 7;

                string argumentsDate = $"{rpiDateTime.Day:D2} {rpiDateTime.Month:D2} {rpiDateTime.Year % 100:D2} {dayOfWeekPython}";

                Console.WriteLine($"serial write 'set-date {argumentsDate}'");

                _serialPort.Write($"set-date {argumentsDate}");
                Thread.Sleep(500);
                _serialPort.Write("\r");
                Thread.Sleep(1000);

                string argumentsTime = $"{rpiDateTime.Hour:D2} {rpiDateTime.Minute:D2} {rpiDateTime.Second:D2}";

                Console.WriteLine($"serial write 'set-clock {argumentsTime}'");
                _serialPort.Write($"set-clock {argumentsTime}");

                Thread.Sleep(500);
                _serialPort.Write("\r");

                _serialPort.Close();

                ReadStatus();  // re-read to get the updated datetime

                Console.WriteLine("-----------------------------------");
                Console.WriteLine("The date und time has been synced: Raspberry Pi -> StromPi'");
                Console.WriteLine($"Strompi3 is up-to-date:  {State.CurrentDateTime}");
                Console.WriteLine("-----------------------------------");
            }

            if (rpiDateTime < State.CurrentDateTime) // sync the Raspi 
            {
                //TODO: not tested so far..
                Console.WriteLine("The date und time will be synced: StromPi -> Raspberry Pi'");
                Os.SetDateTime(State.CurrentDateTime);

                Console.WriteLine("-----------------------------------");
                Console.WriteLine("The date und time has been synced: StromPi -> Raspberry Pi'");
                Console.WriteLine("-----------------------------------");
            }
        }


        /// <summary>
        /// Polls the powerFail-warning- (if enabled in configuration) and powerBack-signal.
        /// <para>Requires serial-mode</para>
        /// </summary>
        /// <param name="waitForPowerBackTimerSeconds">Seconds to wait for a powerBack-signal before shutting down the Raspberry pi.
        /// This must be set - or will be forced - lower than the configured shutdown-timer to make a safe shutdown.</param>
        public void PollingShutDownOnPowerFailure(int waitForPowerBackTimerSeconds = 10)
        {
            bool runCountdown = false;
            DateTime powerFailureStartTime = default;
            string data = String.Empty;

            Console.WriteLine($"PollingShutDownOnPowerFailure (wait {waitForPowerBackTimerSeconds} secs)");

            if (!HasValidStrompiSettings(waitForPowerBackTimerSeconds)) return;

            //start polling the serial port of strompi3 
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();

            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    data = _serialPort.ReadLine();
                }
                catch (TimeoutException) { }  // ignore timeouts

                var powerFailureSignal = SetPowerFailureSignal(data);
                var powerBackSignal = SetPowerBackSignal(data);

                if (powerFailureSignal || runCountdown)
                {
                    if (runCountdown == false)
                    {
                        runCountdown = true;
                        powerFailureStartTime = DateTime.Now;
                    }

                    int countdownSeconds = Convert.ToInt32((DateTime.Now - powerFailureStartTime).TotalSeconds);
                    Console.WriteLine($"PowerFail - run countdown to shutdown the Pi ({waitForPowerBackTimerSeconds - countdownSeconds} secs)");

                    if (countdownSeconds >= waitForPowerBackTimerSeconds)
                    {
                        _serialPort.Close();
                        Console.WriteLine($"Raspberry Pi: running shutdown...");
                        Thread.Sleep(500);
                        Os.ShutDown();
                    }
                }

                if (powerBackSignal && runCountdown)
                {
                    Console.WriteLine("PowerBack - aborting Raspberry Pi shutdown");
                    runCountdown = false;
                }
            }
        }

        private bool SetPowerFailureSignal(string data)
        {
            return data.Contains(PowerFailureMessage);
        }

        private bool SetPowerBackSignal(string data)
        {
            return data.Contains(PowerBackMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private bool HasValidStrompiSettings(int seconds)
        {
            if (seconds >= State.ShutdownSeconds)  // force to be 10 secs lower 
            {
                seconds = State.ShutdownSeconds - 10;
                Console.WriteLine($"***warning: lowered value of waitForPowerBackTimerSeconds to {seconds} secs.");

                if (seconds <= 0)
                {
                    Console.WriteLine($"***error: invalid waitForShutdowntimerSeconds [{seconds}] secs! PollingShutDownOnPowerFailure will fail!");
                    return false;
                }
            }

            if (State.PowerFailWarningEnable) return true;

            Console.WriteLine("***error: PollingShutDownOnPowerFailure will fail, because PowerFailwarning of Strompi3 is NOT enabled!");
            return false;
        }


        private void ReceivedData(object sender, SerialDataReceivedEventArgs e)
        {
            var s = e.EventType.ToString();

            string serialOutput = _serialPort.ReadExisting();
            string serialOutline = _serialPort.ReadLine();

            Console.WriteLine($"Received: Eventtype {s}, ReadExisting {serialOutput}, Readline {serialOutline}");
        }

        public override string ToString()
        {
            string status = "---------------------------------" + Environment.NewLine;
            status += "StromPi-Status:" + Environment.NewLine;
            status += "---------------------------------" + Environment.NewLine;
            status += $"Current Date time: {State.CurrentDateTime}" + Environment.NewLine;

            status += $"StromPi-Output: {State.OutputStatus}" + Environment.NewLine;
            status += $"StromPi-Mode: {GetEnumDescription(State.PriorityMode)}" + Environment.NewLine;
            status += $"Raspberry Pi Shutdown: {State.ShutdownEnable}" + Environment.NewLine;
            status += $" Shutdown-Timer: {State.ShutdownSeconds} seconds" + Environment.NewLine;
            status += $"Powerfail Warning: {State.PowerFailWarningEnable}" + Environment.NewLine;
            status += $"Serial-Less Mode: {State.SerialLessEnable}" + Environment.NewLine;
            status += $"Power Save Mode: {State.StartStop.PowersaveEnable}" + Environment.NewLine;

            status += $"PowerOn-Button: {State.StartStop.PowerOnButtonEnable}" + Environment.NewLine;
            status += $"PowerOn-Button-Timer: {State.StartStop.PowerOnButtonSeconds} seconds" + Environment.NewLine;
            status += $"Battery-Level Shutdown: {GetEnumDescription(State.Battery.ShutdownLevel)}" + Environment.NewLine;
            status += $"Powerfail-Counter: {State.PowerFailureCounter}" + Environment.NewLine;
            status += $"PowerOff Mode: {State.StartStop.PoweroffMode}" + Environment.NewLine;

            status += "---------------------------------" + Environment.NewLine;
            status += "Alarm-Configuration:" + Environment.NewLine;
            status += "---------------------------------" + Environment.NewLine;
            status += $"WakeUp-Alarm: {State.Alarm.Enabled}" + Environment.NewLine;

            if (State.StartStop.PoweroffTimeEnableMode)
                status += $" Alarm-Mode: Minute WakeUp-Alarm" + Environment.NewLine;
            else
            {
                status += $"Alarm-Mode: {State.Alarm.Mode}" + Environment.NewLine;
            }

            status += $"Alarm-Time: {State.Alarm.Hour}:{State.Alarm.Minute}" + Environment.NewLine;
            status += $"Alarm-Date: {State.Alarm.Day}.{State.Alarm.Month}" + Environment.NewLine;
            status += $"WakeUp-Alarm: {State.Alarm.Weekday}" + Environment.NewLine;
            status += $"Weekend Wakeup {State.StartStop.WakeupWeekendEnable}" + Environment.NewLine;
            status += $" Minute Wakeup Timer: {State.StartStop.WakeupTimerMinutes} minutes" + Environment.NewLine;
            status += $"PowerOff-Alarm: {State.Alarm.PowerOffEnabled}" + Environment.NewLine;
            status += $" PowerOff-Alarm-Time: {State.Alarm.PowerOffHours}:{State.Alarm.PowerOffMinutes}" + Environment.NewLine;
            status += $"Interval-Alarm: {State.Alarm.IntervalAlarmEnabled}" + Environment.NewLine;
            status += $" Interval-On-Time: {State.Alarm.IntervalAlarmOnMinutes} minutes" + Environment.NewLine;
            status += $" Interval-Off-Time: {State.Alarm.IntervalAlarmOffMinutes} minutes" + Environment.NewLine;

            status += "---------------------------------" + Environment.NewLine;
            status += "Voltage-Levels:" + Environment.NewLine;
            status += "---------------------------------" + Environment.NewLine;
            status += $"Wide-Range-Input Voltage: {State.Voltage.WideRangeVolt:F2}" + Environment.NewLine;
            status += $"LifePo4-Battery Voltage: {State.Voltage.BatteryVolt:F2}; Level: {GetEnumDescription(State.Battery.Level)} " +
                      $"Charging [{State.Battery.IsCharging}], Shutdown-Level: {GetEnumDescription(State.Battery.ShutdownLevel)} " + Environment.NewLine;

            status += $"microUSB-Input Voltage: {State.Voltage.mUsbVolt:F2}" + Environment.NewLine;
            status += $"Output-Voltage: {State.Voltage.OutputVolt:F2}" + Environment.NewLine;

            return status;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~StromPi3()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        public static bool EnabledDisabledConverter(string argument, string argumentName)
        {
            bool result = false;

            if (argument == "1")
            {
                result = true;
            }
            else if (argument == "0")
            {
                result = false;
            }
            else
            {
                result = false;
                Console.WriteLine($"***error: {argumentName} not set = '{argument}'");
            }

            return result;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }


    }
}