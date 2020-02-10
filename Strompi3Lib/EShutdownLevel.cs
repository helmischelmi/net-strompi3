using System.ComponentModel;

namespace Strompi3Lib
{
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
}