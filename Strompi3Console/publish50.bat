Rem �r = f�r eigenst�ndige Bereitstellung
Rem dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true
Rem f�r framework-dependent bereitstellung
dotnet publish -c Debug --no-build  
rem Sichert das aktuelle Verzeichnis f�r POPD und wechselt dann zum angegebenen Verzeichnis
pushd .\bin\Debug\netcoreapp3.1\publish
rem Secure copy client: -pw=password -v=verbose �r=folder-recursive .\* = source [user]@host:folder
C:\Projects\00_Pscp\pscp -pw raspi -v -r .\* pi@raspi3bp:/home/pi/Strompi3/
Rem wechselt zu dem Verzeichnis, dass durch PUSHD gespeichert wurde
popd
