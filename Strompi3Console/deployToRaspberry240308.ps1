# define variables
$projectPath = "E:\Projects\NetCore\Pi\Pi.Common\Pi.HWTest_Strompi3"
$publishOutput = "$projectPathbin\bin\Debug\net8.0\publish\linux-arm"
$raspberryPiUser = "pi"
$raspberryPiPw = "raspi240308"
$raspberryPiIp = "192.168.1.197"
$raspberryPiPath = "/home/pi/apps/Pi.HWTest_Strompi3"
$pscpPath = "E:\Projects\NetCore\Pi\deploytool\pscp.exe"

# execute publish-command
dotnet publish $projectPath --no-build -c Debug -r linux-arm -o $publishOutput

# copy published files onto the raspi
& $pscpPath -pw $raspberryPiPw -v -r ${publishOutput}\*.* $raspberryPiUser@${raspberryPiIp}:$raspberryPiPath
