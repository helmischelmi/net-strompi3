# net-strompi3
A very basic net-project of StromPi3 (Firmware-Version 1.72 required, 1.8 as optimum) (from joy-it: https://github.com/joy-it/strompi3) using .Net 8.

Ports python-scripts 'StromPi3_Status.py', 'RTCSerial.py' and 'serialShutdown.py'
provided by joy-it (https://strompi.joy-it.net/downloads/)

Start: sudo dotnet Strompi3Console.dll
opens the main menu: https://github.com/helmischelmi/net-strompi3/blob/master/assets/Strompi3Console_MainMenu.png
No. 1 - 5 and 0 are working. Item 6 isn't implemented yet

No. 3 opens the configuration menu (only submenu): 
https://github.com/helmischelmi/net-strompi3/blob/master/assets/Strompi3Console_ConfigMenu.png
Here, the first No 1 and 2 work, rest (3-0) is experimental

Output ReadPorts:
https://github.com/helmischelmi/net-strompi3/blob/master/assets/Strompi3Console_ReadPorts.png

Output ReadConfiguration:
https://github.com/helmischelmi/net-strompi3/blob/master/assets/Strompi3Console_ReadConfiguration.png

Output SyncRTC:
https://github.com/helmischelmi/net-strompi3/blob/master/assets/Strompi3Console_SyncRTC.png

Output (Test) Poll for PowerFailure:
Showing 1.Power fails, 2. power is back and 3. power fails und countdown is run until shutdown is initialised.
https://github.com/helmischelmi/net-strompi3/blob/master/assets/Strompi3Console_TestPollForPowerFailure.png

Attention: Publishxx.bat runs on post-build, to transfer files to the raspi.
Adopt publishxx.bat according to your needs or use WinSCP to transfer the files.

Summary: The main purpose is to provide a .net core library to manage (some parts I use) of the Strompi3.
See https://github.com/helmischelmi/net-strompi3/blob/master/assets/StromPi3LibDiagram.png


