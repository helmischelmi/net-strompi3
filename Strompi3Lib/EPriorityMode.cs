using System.ComponentModel;

namespace Strompi3Lib;

/// <summary>
/// 
/// </summary>
public enum EPriorityMode
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