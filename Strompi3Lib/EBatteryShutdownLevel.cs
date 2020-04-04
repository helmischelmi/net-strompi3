using System.ComponentModel;

namespace Strompi3Lib
{
    public enum EBatteryShutdownLevel
    {
        [Description("[Disabled]")]
        Disabled = 0,

        [Description("[10%]")]
        TenPercent = 1,

        [Description("[25%]")]
        QuarterLeft = 2,

        [Description("[50%]")]
        HalfEmpty = 3,

        [Description("[nothing]")]
        nothing = 4
    }
}