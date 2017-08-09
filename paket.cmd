@echo off
pushd %~dp0
call .paket\paket.bootstrapper.exe 5.79.3
call .paket\paket.exe %*
popd
