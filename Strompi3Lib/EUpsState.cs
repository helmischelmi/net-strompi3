namespace Strompi3Lib;

public enum EUpsState
{
    InvalidSettings,
    BatteryLevelBelowMinimum,
    PowerOk,
    PowerIsMissing,
    PowerBack,
    ShutdownNow
}