Rem –r = für eigenständige Bereitstellung
Rem dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true
Rem für framework-dependent bereitstellung
dotnet publish -c Release --no-build  
rem Sichert das aktuelle Verzeichnis für POPD und wechselt dann zum angegebenen Verzeichnis
pushd .\bin\Release\netcoreapp3.0\publish
rem Secure copy client: -pw=password -v=verbose –r=folder-recursive .\* = source [user]@host:folder
C:\Projects\00_Pscp\pscp -pw raspi -v -r .\* pi@raspi3bp:/home/pi/Strompi3/
Rem wechselt zu dem Verzeichnis, dass durch PUSHD gespeichert wurde
popd
