copy /y C:\Peach3\PeachHooker.Network\bin\Debug\PeachHooker.Network.dll 
call dogac.bat 
peachhooker --network -e c:\peach3\Release\CrashableServer.exe -c "crash 127.0.0.1 4242" 
