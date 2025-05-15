@echo off

cd /d %~dp0
powershell -ExecutionPolicy RemoteSigned .\export.ps1 -ConfigPath .\ProjectTemplate\config.json

@echo on