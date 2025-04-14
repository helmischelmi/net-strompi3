using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Strompi3Lib.serialPort;


/// <summary>
/// Manages the serial port communication with customizable setting.
/// Implementing IDisposable for resource management.
/// </summary>
public class SerialPortManager : IDisposable
{
    private readonly SerialPort _serialPort;

    // Puffer für Befehlsantworten
    private readonly BlockingCollection<string> _commandResponses = new BlockingCollection<string>();


    /// <summary>
    ///  Event für asynchrone Ereignisse (z. B. Power-Fail)
    /// event is triggered when a power failure signal is received
    /// </summary>
    public event EventHandler<SerialPortEventArgs> PowerChangeDetected;

    public string PortName { get; }

    public int BaudRate { get; }

    public Parity Parity { get; }

    public int DataBits { get; }

    public StopBits StopBits { get; }

    /// <summary>
    /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
    /// </summary>
    public int ReadTimeout { get; }

    public int WriteTimeout { get; }


    /// <summary>
    /// Parameterized constructor allows for custom serial port configuration, default is used for strompi3
    /// </summary>
    /// <param name="portName">Name of the serial port,e.g. /dev/serial0 or .</param>
    /// <param name="baudRate">Baud rate for communication.</param>
    /// <param name="parity">Parity setting.</param>
    /// <param name="dataBits">Number of data bits.</param>
    /// <param name="stopBits">Stop bits setting.</param>
    /// <param name="readTimeout">Read timeout in milliseconds.</param>
    /// <param name="writeTimeout">Write timeout in milliseconds.</param>
    public SerialPortManager(string portName = @"/dev/ttyAMA0", int baudRate = 38400, int dataBits = 8,
        Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readTimeout = 1000, int writeTimeout = 1000)
    {
        PortName = portName;
        BaudRate = baudRate;
        Parity = parity;
        DataBits = dataBits;
        StopBits = stopBits;

        ReadTimeout = readTimeout;
        WriteTimeout = writeTimeout;

        _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits)
        {
            ReadTimeout = ReadTimeout,
            WriteTimeout = WriteTimeout
        };

        _serialPort.DataReceived += DataReceivedEventHandler;
    }


    /// <summary>
    /// event handler for the DataReceived event of the serial port.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            // Solange noch Bytes vorhanden sind, werden alle Zeilen abgearbeitet.
            while (_serialPort.BytesToRead > 0)
            {
                string message = _serialPort.ReadLine();
                Console.WriteLine("Received: " + message);

                if (IsAsynchronousEvent(message))
                {
                    // Verarbeitung der eingehenden Daten asynchron auslagern, damit der Event-Handler nicht blockiert
                    Task.Run(async () => await OnPowerChangedAsync(new SerialPortEventArgs(message)));
                }
                else
                {
                    // Add message as part of an answer to a command in the response buffer
                    _commandResponses.Add(message);
                }
            }
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Timeout errors while reading from serial port: {ex.Message}");
        }
    }


    /// <summary>
    /// Checks if the message is an asynchronous event, determined via the message prefix "xxx--".
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private bool IsAsynchronousEvent(string message)
    {
        return message.Trim().StartsWith("xxx--");
    }


    /// <summary>
    /// invokes the PowerChangeDetected event
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnPowerChangedAsync(SerialPortEventArgs e)
    {
        PowerChangeDetected?.Invoke(this, e);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Opens the serial port connection.
    /// </summary>
    /// <param name="bSilent"></param>
    public void Open(bool bSilent = false)
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
            if (!bSilent) Console.WriteLine("Serial port was open. Is closed");
        }
        _serialPort.Open();
        if (!bSilent) Console.WriteLine("Serial port opened.");
    }

    /// <summary>
    /// Opens the serial port connection.
    /// </summary>
    /// <param name="bSilent"></param>
    public void Close(bool bSilent = false)
    {
        if (_serialPort.IsOpen)
            _serialPort.Close();
        if (!bSilent) Console.WriteLine("Serial port closed.");
    }


    public string SendCommand(string command, bool expectResponse, int expectedResponseLines)
    {
        return SendCommand(new string[] { command }, null, expectResponse, expectedResponseLines);
    }


    /// <summary>
    /// Sends an array of command string, array of delays to the Strompi3 device.
    /// Add a bool to check whether a response is expected
    /// Using Write()-method enables control for sending CR/LF in time and characters
    /// (e.g. "\x0D"). 
    /// </summary>
    /// <param name="commands">one or more commands to send</param>
    /// <param name="delays">array of delays that will follow after each command</param>
    /// <param name="expectResponse">defines, if a response is expected</param>
    /// <param name="expectedResponseLines">expected amount of Readlines from the serailPort</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string SendCommand(string[] commands, int[]? delays, bool expectResponse, int expectedResponseLines)
    {
        if (commands == null || commands.Length == 0)
            throw new ArgumentException("commands darf nicht null oder leer sein.");

        // Leere den Puffer, um Altlasten zu entfernen.
        while (_commandResponses.TryTake(out string discard)) { }


        // Sende alle Befehle der Reihe nach
        for (int i = 0; i < commands.Length; i++)
        {
            _serialPort.Write(commands[i]);
            Console.WriteLine($"Command sent: {commands[i]}");

            // Falls für diesen Schritt eine Wartezeit definiert ist, dann warten
            if (delays != null && i < delays.Length && delays[i] > 0)
            {
                Thread.Sleep(delays[i]);
            }
        }

        if (expectResponse)
        {
            // Warte darauf, dass die erwartete Anzahl an Zeilen empfangen wird.
            List<string> lines = new List<string>();
            for (int i = 0; i < expectedResponseLines; i++)
            {
                try
                {
                    // Blockiert, bis eine Zeile verfügbar ist.
                    string line = _commandResponses.Take();
                    lines.Add(line);
                }
                catch (TimeoutException ex)
                {
                    Console.WriteLine($"Timeout beim Warten auf Zeile {i}: " + ex.Message);
                    break;
                }
            }
            return string.Join(Environment.NewLine, lines);
        }
        else
        {
            return string.Empty;
        }
    }


    /// <summary>
    /// Reads a response string from the Strompi3 device.
    /// </summary>
    /// <returns>A string representing the response from the device.</returns>
    public string ReadLine()
    {
        try
        {
            string response = _serialPort.ReadLine();
            Console.WriteLine("Response received: " + response);
            return response;
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Timeout occurred while reading from the serial port: {ex.Message}");
            return "Timeout";
        }
    }



    public void Dispose()
    {
        Close();
        _serialPort.Dispose();

        // Deregistrieren aller Eventhandler vom PowerChangeDetected-Event
        PowerChangeDetected = null;

        _commandResponses.Dispose();
        GC.SuppressFinalize(this);

        // Optional: Finalizer unterdrücken
        //  GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
        return $"Serial Port: {Environment.NewLine}" +
               $"- Portname: {PortName}{Environment.NewLine}" +
               $"- Baudrate: {BaudRate}{Environment.NewLine}" +
               $"- Parity: {Parity}{Environment.NewLine}" +
               $"- Databits: {DataBits}{Environment.NewLine}" +
               $"- Stopbits: {StopBits}{Environment.NewLine}" +
               $"- Read timeout:{ReadTimeout} msec{Environment.NewLine}" +
               $"- Write timeout: {WriteTimeout} msec{Environment.NewLine}";
    }
}