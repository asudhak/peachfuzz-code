SET GACUTIL="c:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\gacutil.exe"

%GACUTIL% /u EasyHook
%GACUTIL% /u PeachHooker
%GACUTIL% /u PeachHooker.Network
%GACUTIL% /i EasyHook.dll
%GACUTIL% /i PeachHooker.exe
%GACUTIL% /i PeachHooker.Network.dll
