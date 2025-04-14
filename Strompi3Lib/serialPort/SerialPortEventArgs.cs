using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strompi3Lib.serialPort;

public class SerialPortEventArgs: EventArgs
{
    public string Message { get; set; }

    public SerialPortEventArgs(string message)
    {
        Message = message;
    }
}