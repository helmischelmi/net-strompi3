using System;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace Strompi3Lib
{
    public class ConsoleDigitRetriever
    {
        public virtual ConsoleKeyInfo GetNextKey()
        {
            return Console.ReadKey();// Get user input
        }
    }


    public class TestDigitRetriever : ConsoleDigitRetriever
    {

        public override ConsoleKeyInfo GetNextKey()
        {
            var simulator = new InputSimulator();

            //simulator.Keyboard.KeyPress(VirtualKeyCode.NUMPAD1);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_1);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            // start real new thread 

            var cki =  Task.Run(() => Console.ReadKey(false));// Get user input
            simulator.Keyboard.KeyPress(VirtualKeyCode.NUMPAD1);

            return cki.Result;

        }

    }


}
