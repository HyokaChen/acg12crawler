@echo off
@echo. 正在启动服务端脚本...
start .\start_server_detail.bat
start .\start_server_download.bat
@echo. 正在编译运行.net core代码...
cd spider/
dotnet run

