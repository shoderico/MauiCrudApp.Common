@echo off

cd /d %~dp0
powershell -ExecutionPolicy RemoteSigned .\export.ps1 -ConfigPath .\AddFeature\config.json

@echo on