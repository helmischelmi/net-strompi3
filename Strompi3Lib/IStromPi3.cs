namespace Strompi3Lib;

public interface IStromPi3
{
    UpsMonitor Monitor { get; }

    /// <summary>
    /// Sends initilazize command to Strompi3, expects no answer
    /// </summary>
    /// <para>
    /// <remarks>Requires serial-mode</remarks>
    /// </para>
    void Initialize();

    /// <summary>
    /// Reads the state and configuration of the StromPi3. 
    /// </summary>
    StromPi3Configuration ReceiveStatus(bool bSilent = true);

    /// <summary>
    /// Updates the configuration of the Strompi3.
    /// </summary>
    void UpdateCompleteConfiguration();

    /// <summary>
    /// command to shutdown the Strompi3, in case a second power-source is enabled.
    ///<para>
    /// <remarks>Requires serial-mode</remarks></para>
    /// </summary>
    void PowerOffStromPi3();

    /// <summary>
    /// The method replicates the essential process of the Python script by comparing the current Raspberry
    /// timestamp (DateTime.Now) with the most recently obtained StromPi3 timestamp via ReceiveStatus() to
    /// decide which time to adopt. Essentially, as in the Python script, it checks whether the Raspberry
    /// is "newer" (rpiDateTime > Cfg.CurrentDateTime) and, in that case, updates the StromPi3's RTC using serial commands;
    /// otherwise, the Raspberry is synchronized.
    /// <para>Requires serial-mode</para>
    /// <remarks>The functionality of RTCSerial.py from joy-it is ported by this method.The original py-script
    /// uses commands 'Q', '\r', 'date-rpi' and 'time-rpi' to read the current datetime
    /// of Strompi3. This steps could not be implemented successfully and were replaced by calling 'ReceiveStatus'.
    /// </remarks>
    /// </summary>
    void SyncRtc();
}