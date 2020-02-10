using System.ComponentModel;

namespace Strompi3Lib
{
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
}