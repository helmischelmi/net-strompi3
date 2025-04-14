# net-strompi3
A very basic net-console-project with classic menu to get to know the StromPi3 (Firmware-Version 1.72 required, 1.8 as optimum) (from joy-it: https://github.com/joy-it/strompi3) using .Net 8.
It reads the status & configuration, changes the configuration and is enabled to react on events like powerfail/powerIsBack from the serial port of the stromPi3 whil doing status requests simultaneously.

It ports python-scripts 'StromPi3_Status.py', but sudo-permissions are needed to port 'RTCSerial.py' and 'serialShutdown.py' successfully,
provided by joy-it (https://strompi.joy-it.net/downloads/)

Start: sudo dotnet Strompi3Console.dll
opens the main menu:

![Menu_Strompi3](https://github.com/user-attachments/assets/55c1132e-b407-4abc-87fe-2bdbaf63f641)

No. 1 - 5 and 7 are working. Item 6 isn't implemented yet
Output 1:

![SerialPortCheck_Strompi3](https://github.com/user-attachments/assets/16b03f0b-8e70-45a9-975f-3a28ad98db2d)

Output 2:

![GetStatus_Strompi3](https://github.com/user-attachments/assets/dd7f9129-6289-4442-9989-d21810d63c8e)

No. 3 opens the configuration menu (only submenu): 

![ChangeConfigurationComplete_Strompi3](https://github.com/user-attachments/assets/8af3ca2a-07ab-4a64-8d00-6ebd845d1c51)
Here, the first No 1 works, rest is experimental


Output 4 (SyncRTC):

![SynchronizeRTC_Strompi3](https://github.com/user-attachments/assets/12d9e9a7-0970-4009-a0a4-1afe0108e4f1)


Summary: The main purpose is to provide a simple console app to help the usage of Strompi3 in .NET.

