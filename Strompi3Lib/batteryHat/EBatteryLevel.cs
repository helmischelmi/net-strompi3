using System.ComponentModel;

namespace Strompi3Lib.batteryHat;

public enum EBatteryLevel
{
    [Description("nothing")]
    nothing = 0,

    [Description("[10%]")]
    TenPercent = 1,

    [Description(" [25%]")]
    TwentyfivePercent = 2,

    [Description("[50%]")]
    FiftyPercent = 3,

    [Description("[100%]")]
    HundredPercent = 4
}