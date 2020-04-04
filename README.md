# net-strompi3
A very basic net-project of StromPi3 (Firmware-Version 1.72 required) (from joy-it: https://github.com/joy-it/strompi3) using .Net Core 3.1.

Ports python-scripts 'StromPi3_Status.py', 'RTCSerial.py' and 'serialShutdown.py'
provided by joy-it (https://strompi.joy-it.net/downloads/)

Start: sudo dotnet Strompi3Console.dll
opens the main menu: https://github.com/helmischelmi/net-strompi3/blob/master/assets/Strompi3Console_MainMenu.png






Output ReadStatus:
https://github.com/helmischelmi/net-strompi3/blob/master/rpi_StromPi3_Status.png

Output SyncRTC:
https://github.com/helmischelmi/net-strompi3/blob/master/rpi_StromPi3_SyncRTC.png

Output serialShutdown:
https://github.com/helmischelmi/net-strompi3/blob/master/rpi_StromPi3_SerialShutdown.png

Attention: Publishxx.bat runs on post-build, to transfer files to the raspi.
Adopt publishxx.bat according to your needs or use WinSCP to transfer the files.

Roadmap: 
Potential next steps: 
