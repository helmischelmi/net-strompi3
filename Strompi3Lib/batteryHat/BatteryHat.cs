using System;

namespace Strompi3Lib.batteryHat;

/// <summary>
/// represents the LiFePO4-Battery-Hat with 3 Volt and 1 Ah
/// </summary>
public class BatteryHat
{
    /// <summary>
    /// the battery charge level
    /// </summary>
    public EBatteryLevel Level { get; private set; }

    /// <summary>
    /// indicates that the battery is currently charging 
    /// </summary>
    public bool IsCharging { get; private set; }

    /// <summary>
    /// a given battery charge level: when reached, a signal is fired to start the shutdown event 
    /// </summary>
    public EBatteryShutdownLevel BatteryShutdownLevel { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sp3BatLevelShutdown"></param>
    /// <param name="sp3BatLevel"></param>
    /// <param name="sp3Charging"></param>
    public void GetBatteryHat(int sp3BatLevelShutdown, string sp3BatLevel, string sp3Charging)
    {
        try
        {
            IsCharging = Convert.ToInt32(sp3Charging) > 0;

            Level = EBatteryLevel.nothing;
            int level = Convert.ToInt32(sp3BatLevel);
            if (level >= 0 && level <= 4) Level = (EBatteryLevel)level;

            SetShutdownLevel(sp3BatLevelShutdown);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// sets the given level, if input ist valid, else EShutdownLevel.nothing 
    /// </summary>
    /// <param name="levelShutdown"></param>
    public void SetShutdownLevel(int levelShutdown)
    {
        try
        {
            BatteryShutdownLevel = EBatteryShutdownLevel.nothing;

            if (levelShutdown >= 0 && levelShutdown <= 4)
                BatteryShutdownLevel = (EBatteryShutdownLevel)levelShutdown;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}