using System;
using System.IO.Ports;
using System.Threading;

namespace Strompi3Lib
{
    public class StromPi3 : IDisposable
    {
        private SerialPort _serialPort;
        public StromPi3Settings Settings { get; }

        private const string PowerFailureMessage = "ShutdownRaspberryPi";
        private const string PowerBackMessage = "StromPiPowerBack";

        public StromPi3(bool bSilent = false)
        {
            Settings = new StromPi3Settings();
            Connect(bSilent);
        }

        /// <summary>
        /// Shows list of serial port names.
        /// </summary>
        /// <returns>TRUE, if serial port is available</returns>
        public static bool ShowAvailableSerialPorts(string strRequiredPorts, bool bSilent = false)
        {
            string[] ports = SerialPort.GetPortNames();
            if (!bSilent) Console.WriteLine("The following serial ports were found:");


            // Display each port name to the console.
            foreach (string port in ports)
            {
                if (!bSilent) Console.WriteLine($"Serial name: {port}");
                var isTTY = port.Contains(strRequiredPorts);
                if (isTTY) continue;

                Console.WriteLine($"No {strRequiredPorts}.. serial port!");
                return false;
            }

            if (!bSilent) Console.WriteLine("Yes, we have the embedded serial port available");
            return true;
        }

        /// <summary>
        /// Configures and connects the serial interface of StromPi3
        /// </summary>
        public void Connect(bool bSilent = false)
        {
            int baudRate = 38400;
            string portName = @"/dev/serial0";
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            Parity parity = Parity.None;

            // Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
            int readTimeout = 200;
            int writeTimeout = 500;

            var portParameter = new SerialPortParameter(portName, baudRate, parity, dataBits, stopBits, readTimeout, writeTimeout);

            if (_serialPort == null)
            {
                _serialPort = _serialPort.Create(portParameter);
            }

            if (!bSilent) Console.WriteLine($"connected to {portParameter.ShowStatus()}");

            if (_serialPort.IsOpen) _serialPort.Close();

            //_serialPort.DataReceived += ReceivedData;

            _serialPort.Open();

            if (!bSilent) Console.WriteLine($"serial port is open ");
        }

        /// <summary>
        /// Allows editing configuration-settings of StromPi3.
        /// <para>Requires serial-mode</para>
        /// <remarks>The functionality of strompi_config.py from joy-it is ported by this method.
        /// </remarks>
        /// </summary>
        public void Configure()
        {
            GetSettings();
            Thread.Sleep(500);

            Console.WriteLine("###########################################################################");
            Console.WriteLine("#               StromPi V3 Serial Configuration                           #");
            Console.WriteLine("###########################################################################");
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------------------------");
            Console.WriteLine("                       Main Configuration");
            Console.WriteLine("---------------------------------------------------------------------------");

            //StromPi3ConfigEditor.EditInputPriorityMode(this);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setConfig"></param>
        /// <param name="value"></param>
        /// <param name="reloadSettings"></param>
        public void TransferSetting(ESetConfig setConfig, int value, bool reloadSettings = true)
        {
            bool isPortOpen = _serialPort.IsOpen;
            if (!isPortOpen) _serialPort.Open();

            _serialPort.Write($"set-config {(int)setConfig} {value}");
            Thread.Sleep(100);
            _serialPort.Write("\r"); 
            Thread.Sleep(200);

            if (!isPortOpen) _serialPort.Close();
            if (reloadSettings) GetSettings(); 

            Console.WriteLine($"serial Write {(int)setConfig} {value} transfer successfull..");
        }


        /// <summary>
        /// Reads all status-related characteristics of the Strompi3.
        /// <para>
        /// <remarks>Requires serial-mode</remarks></para>
        /// </summary>
        public string GetSettings(bool bSilent = true)
        {
            if (!ShowAvailableSerialPorts("tty", true)) return String.Empty;

            bool isPortOpen = _serialPort.IsOpen;

            if (!isPortOpen) _serialPort.Open();

            _serialPort.Write("quit");
            _serialPort.Write("\r"); // \x0d = {13} Carriage Return
            _serialPort.Write("status-rpi");
            _serialPort.Write("\r"); // \x0d = {13} Carriage Return

            string sp3Time = _serialPort.ReadLine();
            string sp3Date = _serialPort.ReadLine();

            Settings.SetRTCDateTime(sp3Time, sp3Date);

            string sp3_weekday = _serialPort.ReadLine();  // not used
            string sp3_modus = _serialPort.ReadLine();
            Settings.SetInputPriorityMode(sp3_modus);

            Settings.AlarmSettings.GetAlarmEnabled(_serialPort.ReadLine());
            Settings.AlarmSettings.GetAlarmMode(_serialPort.ReadLine());

            string sp3AlarmHour = _serialPort.ReadLine();
            string sp3AlarmMin = _serialPort.ReadLine();
            string sp3AlarmDay = _serialPort.ReadLine();
            string sp3AlarmMonth = _serialPort.ReadLine();
            string sp3AlarmWeekday = _serialPort.ReadLine();
            Settings.AlarmSettings.GetAlarmDateTime(sp3AlarmHour, sp3AlarmMin, sp3AlarmDay, sp3AlarmMonth, sp3AlarmWeekday);

            Settings.AlarmSettings.GetAlarmPowerOffEnabled(_serialPort.ReadLine());

            string alarmPowerOffHours = _serialPort.ReadLine();
            string alarmPowerOffMinutes = _serialPort.ReadLine();
            Settings.AlarmSettings.GetAlarmPowerOffTimePeriod(alarmPowerOffHours, alarmPowerOffMinutes);

            string sp3ShutdownEnable = _serialPort.ReadLine();
            string sp3ShutdownSeconds = _serialPort.ReadLine();
            Settings.SetShutDown(sp3ShutdownEnable,Convert.ToInt32(sp3ShutdownSeconds), (int) EBatteryLevel.nothing);

            Settings.SetPowerFailWarningEnable(_serialPort.ReadLine());
            Settings.SetSerialLessEnable(_serialPort.ReadLine());

            string sp3IntervalAlarm = _serialPort.ReadLine();
            string sp3IntervallAlarmOnTimeMinutes = _serialPort.ReadLine();
            string sp3IntervallAlarmOffTimeMinutes = _serialPort.ReadLine();
            Settings.AlarmSettings.GetAlarmIntervall(sp3IntervalAlarm, sp3IntervallAlarmOnTimeMinutes, sp3IntervallAlarmOffTimeMinutes);

            string sp3BatLevelShutdown = _serialPort.ReadLine();
            string sp3BatLevel = _serialPort.ReadLine();
            string sp3Charging = _serialPort.ReadLine();
            Settings.BatteryHat.SetBatteryState(Convert.ToInt32(sp3BatLevelShutdown), sp3BatLevel, sp3Charging);

            string sp3PowerOnButtonEnable = _serialPort.ReadLine();
            string sp3PowerOnButtonTime = _serialPort.ReadLine();
            string sp3PowersaveEnable = _serialPort.ReadLine();
            string sp3PoweroffMode = _serialPort.ReadLine();
            string poweroffTimeEnableMode = _serialPort.ReadLine();
            string wakeupTimerMinutes = _serialPort.ReadLine();
            string sp3WakeupweekendEnable = _serialPort.ReadLine();

            Settings.StartStopSettings.GetStartStopSettings(sp3PowerOnButtonEnable, sp3PowerOnButtonTime, sp3PowersaveEnable, sp3PoweroffMode, poweroffTimeEnableMode,
                wakeupTimerMinutes, sp3WakeupweekendEnable);

            string sp3AdcWide = _serialPort.ReadLine();
            string sp3AdcBat = _serialPort.ReadLine();
            string sp3AdcUsb = _serialPort.ReadLine();
            string outputVolt = _serialPort.ReadLine();
            Settings.VoltageMeter.GetVoltage(sp3AdcWide, sp3AdcBat, sp3AdcUsb, outputVolt);

            Settings.SetOutputStatus(_serialPort.ReadLine());
            Settings.SetPowerFailureCounter(_serialPort.ReadLine());
            Settings.SetFirmwareVersion(_serialPort.ReadLine());

            if (!isPortOpen) _serialPort.Close();

            return ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="waitForShutdowntimerSeconds"></param>
        /// <returns></returns>
        private bool CheckPowerFailSettings(int waitForShutdowntimerSeconds)
        {
            Console.WriteLine($"Settings Shutdown Sec : {Settings.ShutdownSeconds}");
            if (waitForShutdowntimerSeconds >= Settings.ShutdownSeconds)  // force to be 10 secs lower 
            {
                waitForShutdowntimerSeconds = Settings.ShutdownSeconds - 10;
                Console.WriteLine($"***warning: lowered value of waitForPowerBackTimerSeconds to {waitForShutdowntimerSeconds} secs.");

                if (waitForShutdowntimerSeconds <= 0)
                {
                    Console.WriteLine($"***error: invalid waitForShutdowntimerSeconds [{waitForShutdowntimerSeconds}] secs! PollingShutDownOnPowerFailure aborted!");
                    return false;
                }
            }

            if (Settings.PowerFailWarningEnable) return true;

            Console.WriteLine("***error: PollingShutDownOnPowerFailure will fail, because PowerFailwarning of Strompi3 is NOT enabled!");
            return false;
        }


        /// <summary>
        /// Polls the powerFail-warning- (if enabled in configuration) and powerBack-signal.
        /// <para>Requires serial-mode</para>
        /// </summary>
        /// <param name="waitForPowerBackTimerSeconds">Seconds to wait for a powerBack-signal before shutting down the Raspberry pi.
        /// This must be set - or will be forced - lower than the configured shutdown-timer to make a safe shutdown.</param>
        public void PollAndWaitForPowerFailureToShutDown(int waitForPowerBackTimerSeconds = 10)
        {
            GetSettings(true);

            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();

            bool IsActiveCountdown = false;
            DateTime powerFailureStartTime = default;

            Console.WriteLine($"PollingShutDownOnPowerFailure (wait {waitForPowerBackTimerSeconds} secs)");

            if (!CheckPowerFailSettings(waitForPowerBackTimerSeconds)) return;

            while (true)
            {
                Thread.Sleep(100);
                string data = String.Empty;
                try
                {
                    data = _serialPort.ReadLine();
                    Console.WriteLine($"read data = {data}");
                }
                catch (TimeoutException)// ignore timeouts
                {}

                if (OnPowerFailureMessage(data) || IsActiveCountdown)
                {
                    if (IsActiveCountdown == false)
                    {
                        IsActiveCountdown = true;
                        powerFailureStartTime = DateTime.Now;
                    }

                    int countdownSeconds = Convert.ToInt32((DateTime.Now - powerFailureStartTime).TotalSeconds);
                    Console.WriteLine($"PowerFail - run countdown to shutdown the Pi ({waitForPowerBackTimerSeconds - countdownSeconds} secs)");
                    Thread.Sleep(100);
                    GetSettings();
                    Console.WriteLine($"Power-Source:   {Settings.OutputStatus,-27} ");

                    if (countdownSeconds >= waitForPowerBackTimerSeconds)
                    {
                        Shutdown();

                        Console.WriteLine($"Raspberry Pi: running shutdown...");
                        Thread.Sleep(2000);
                        Os.ShutDown();
                    }
                }

                if (OnPowerBackMessage(data) && IsActiveCountdown)
                {
                    Console.WriteLine("PowerBack - aborting Raspberry Pi shutdown");

                    GetSettings();
                    Console.WriteLine($"Power-Source:   {Settings.OutputStatus,-27} ");
                    IsActiveCountdown = false;
                }
            }
        }

                /// <summary>
        /// Polls the powerFail-warning- (if enabled in configuration) and powerBack-signal.
        /// <para>Requires serial-mode</para>
        /// </summary>
        /// <param name="waitForPowerBackTimerSeconds">Seconds to wait for a powerBack-signal before shutting down the Raspberry pi.
        /// This must be set - or will be forced - lower than the configured shutdown-timer to make a safe shutdown.</param>
        public void PollAndWaitForPowerFailureToShutDownNew(int waitForPowerBackTimerSeconds = 10)
        {
            GetSettings(true);

            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();

            bool IsActiveCountdown = false;
            DateTime powerFailureStartTime = default;

            Console.WriteLine($"PollingShutDownOnPowerFailure (wait {waitForPowerBackTimerSeconds} secs)");

            if (!CheckPowerFailSettings(waitForPowerBackTimerSeconds)) return;

            while (true)
            {
                Thread.Sleep(100);
                string data = String.Empty;
                try
                {
                    data = _serialPort.ReadLine();
                    Console.WriteLine($"read data = {data}");
                }
                catch (TimeoutException)// ignore timeouts
                {
                    continue;
                }

                Console.WriteLine("weiter...");

                if (OnPowerFailureMessage(data) || IsActiveCountdown)
                {
                    if (IsActiveCountdown == false)
                    {
                        IsActiveCountdown = true;
                        powerFailureStartTime = DateTime.Now;
                    }

                    int countdownSeconds = Convert.ToInt32((DateTime.Now - powerFailureStartTime).TotalSeconds);
                    Console.WriteLine($"PowerFail - run countdown to shutdown the Pi ({waitForPowerBackTimerSeconds - countdownSeconds} secs)");
                    GetSettings();
                    Console.WriteLine($"Power-Source:   {Settings.OutputStatus,-27} ");

                    if (countdownSeconds >= waitForPowerBackTimerSeconds)
                    {
                        Shutdown();

                        Console.WriteLine($"Raspberry Pi: running shutdown...");
                        Thread.Sleep(2000);
                        Os.ShutDown();
                    }
                }

                if (OnPowerBackMessage(data) && IsActiveCountdown)
                {
                    Console.WriteLine("PowerBack - aborting Raspberry Pi shutdown");
                    GetSettings();
                    Console.WriteLine($"Power-Source:   {Settings.OutputStatus,-27} ");
                    IsActiveCountdown = false;
                }
            }
        }

        /// <summary>
        /// command to shutdown the Strompi3, in case a second power-source is enabled.
        ///<para>
        /// <remarks>Requires serial-mode</remarks></para>
        /// </summary>
        public void Shutdown()
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();
            _serialPort.Write("quit");
            _serialPort.Write("\r");
            _serialPort.Write("poweroff");
            _serialPort.Write("\r");
            _serialPort.Close();
        }

        /// <summary>
        /// IRQ-based method to get the powerFail-warning- (if enabled in configuration) and powerBack-signal.
        /// <para>Requires serialless-mode</para>
        /// </summary>
        /// <param name="waitForPowerBackTimerSeconds">Seconds to wait for a powerBack-signal before shutting down the Raspberry pi.
        /// This must be set - or will be forced - lower than the configured shutdown-timer to make a safe shutdown.</param>
        public void WaitForPowerFailureIrqToShutDown(int waitForPowerBackTimerSeconds = 10)
        {
            bool runCountdown = false;
            DateTime powerFailureStartTime = default;
            string data = String.Empty;

            Console.WriteLine($"PollingShutDownOnPowerFailure (wait {waitForPowerBackTimerSeconds} secs)");

            if (!CheckPowerFailSettings(waitForPowerBackTimerSeconds)) return;

            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    // data = _serialPort.ReadLine();
                }
                catch (TimeoutException) { }  // ignore timeouts

                var powerFailureSignal = OnPowerFailureMessage(data);
                var powerBackSignal = OnPowerBackMessage(data);

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
                        Console.WriteLine($"Raspberry Pi: running shutdown...");
                        Thread.Sleep(1000);
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

        private bool OnPowerFailureMessage(string data)
        {
            return data.Contains(PowerFailureMessage);
        }


        private bool OnPowerBackMessage(string data)
        {
            return data.Contains(PowerBackMessage);
        }


        private void ReceivedData(object sender, SerialDataReceivedEventArgs e)
        {
            var s = e.EventType.ToString();

            string serialOutput = _serialPort.ReadExisting();
            string serialOutline = _serialPort.ReadLine();

            Console.WriteLine($"Received: Eventtype {s}, ReadExisting {serialOutput}, Readline {serialOutline}");
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
            GetSettings();

            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();

            Console.WriteLine("TimeSync-Process | Please Wait");
            Console.WriteLine($"StromPi3: Current dateTime {Settings.CurrentDateTime} ");
            var rpiDateTime = DateTime.Now;
            Console.WriteLine($"Raspi: Current dateTime {rpiDateTime} ");

            if (rpiDateTime > Settings.CurrentDateTime) // sync the Strompi
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

                GetSettings();  // re-read to get the updated datetime

                Console.WriteLine("-----------------------------------");
                Console.WriteLine("The date und time has been synced: Raspberry Pi -> StromPi'");
                Console.WriteLine($"Strompi3 is up-to-date:  {Settings.CurrentDateTime}");
                Console.WriteLine("-----------------------------------");
            }

            if (rpiDateTime < Settings.CurrentDateTime) // sync the Raspi 
            {
                //TODO: not tested so far..
                Console.WriteLine("The date und time will be synced: StromPi -> Raspberry Pi'");
                Os.SetDateTime(Settings.CurrentDateTime);

                Console.WriteLine("-----------------------------------");
                Console.WriteLine("The date und time has been synced: StromPi -> Raspberry Pi'");
                Console.WriteLine("-----------------------------------");
            }
        }


        public void Dispose()
        {
            _serialPort.Dispose();
        }

        public override string ToString()
        {
            string status = "--------------------------------------------------------------------" + Environment.NewLine;
            status += "StromPi-Status:" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;
            status += $"Firmware:       {Settings.FirmwareVersion,-27} DateTime: {Settings.CurrentDateTime} " + Environment.NewLine;
            status += $"Power-Priority: {ConverterHelper.GetEnumDescription(Settings.PriorityMode),-27} Serialless-Mode: {Settings.SerialLessEnable} " + Environment.NewLine;
            status += $"Power-Source:   {Settings.OutputStatus,-27} Power Save Mode: {Settings.StartStopSettings.PowersaveEnable}" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;
            status += $"Powerfail Warning: {Settings.PowerFailWarningEnable,-24} Battery-Level Shutdown: {ConverterHelper.GetEnumDescription(Settings.BatteryHat.ShutdownLevel)}" + Environment.NewLine;
            status += $"Powerfail-Counter: {Settings.PowerFailureCounter}" + Environment.NewLine;
            status += $"Pi Shutdown-Mode:  {Settings.ShutdownEnable,-24} Timer: {Settings.ShutdownSeconds} seconds" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;
            status += $"PowerOff Mode: {Settings.StartStopSettings.PoweroffMode}" + Environment.NewLine;
            status += $"PowerOn-Button: {Settings.StartStopSettings.PowerOnButtonEnable,-27} Timer: {Settings.StartStopSettings.PowerOnButtonSeconds} seconds" + Environment.NewLine;
            status += "---------------------------------------------------------------------" + Environment.NewLine;

            status += "---------------------------------" + Environment.NewLine;
            status += "Alarm-Modes" + Environment.NewLine;
            status += "---------------------------------" + Environment.NewLine;
            status += $"WakeUp-Alarm: {Settings.AlarmSettings.Enabled}" + Environment.NewLine;

            if (Settings.StartStopSettings.PoweroffTimeEnableMode)
                status += $" Alarm-Mode: Minute WakeUp-Alarm" + Environment.NewLine;
            else
            {
                status += $"Alarm-Mode: {Settings.AlarmSettings.Mode}" + Environment.NewLine;
            }

            status += $"Alarm-DateTime: {Settings.AlarmSettings.Day:00}.{Settings.AlarmSettings.Month:00} [dd:mm] at {Settings.AlarmSettings.Hour:00}:{Settings.AlarmSettings.Minute:00} [hh:mm]" + Environment.NewLine;
            status += $"WakeUp-Alarm: {Settings.AlarmSettings.Weekday}" + Environment.NewLine;
            status += $"Weekend Wakeup {Settings.StartStopSettings.WakeupWeekendEnable}" + Environment.NewLine;
            status += $" Minute Wakeup Timer: {Settings.StartStopSettings.WakeupTimerMinutes} minutes" + Environment.NewLine;
            status += $"PowerOff-Alarm: {Settings.AlarmSettings.PowerOffEnabled,-27} Time: {Settings.AlarmSettings.PowerOffHours:00}:{Settings.AlarmSettings.PowerOffMinutes:00} [hh:mm]" + Environment.NewLine;
            status += $"Interval-Alarm: {Settings.AlarmSettings.IntervalAlarmEnabled,-27} On/Off-Time: {Settings.AlarmSettings.IntervalAlarmOnMinutes}/{Settings.AlarmSettings.IntervalAlarmOffMinutes} minutes" + Environment.NewLine;

            status += "---------------------------------" + Environment.NewLine;
            status += "Voltage-Levels:" + Environment.NewLine;
            status += "---------------------------------" + Environment.NewLine;
            status += $"microUSB-Input Voltage: {Settings.VoltageMeter.mUsbVolt:F2}    Wide-Range-Input Voltage: {Settings.VoltageMeter.WideRangeVolt:F2}" + Environment.NewLine;
            status += $"LifePo4-Battery Voltage: {Settings.VoltageMeter.BatteryVolt:F2}  (Level: {ConverterHelper.GetEnumDescription(Settings.BatteryHat.Level)}, " +
                      $"Charging [{Settings.BatteryHat.IsCharging}])" + Environment.NewLine;

            status += $"Output-Voltage: {Settings.VoltageMeter.OutputVolt:F2}" + Environment.NewLine;

            return status;
        }


    }
}
