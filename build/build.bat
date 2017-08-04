@echo off
setlocal

pushd %~dp0..

set year=%date:~6,4%

set BUILD_VERSION=1.0.0
set BUILD_PRODUCT="dotnet-merge"
set BUILD_CONFIGURATION="Release"
set BUILD_OUTPUT=%cd%\artifacts\%BUILD_CONFIGURATION:"=%

set BUILD_COMPANY="WR"
set BUILD_COPYRIGHT="Copyright (c) %BUILD_COMPANY:"=% %year%"

:loop
if x%1 equ x goto done
set param=%1
if %param:~0,1% equ - goto checkParam
:paramError
echo Parameter error: %1

:next
shift /1
goto loop

:checkParam
if "%1" equ "-v" goto Version
if "%1" equ "--version" goto Version
if "%1" equ "-p" goto Product
if "%1" equ "--product" goto Product
if "%1" equ "-c" goto Configuration
if "%1" equ "--configuration" goto Configuration
if "%1" equ "-o" goto Output
if "%1" equ "--output" goto Output
if "%1" equ "-r" goto Company
if "%1" equ "--company" goto Company
goto paramError

:Version
	shift /1
	set BUILD_VERSION=%1
	goto next
:Product
	shift /1
	set BUILD_PRODUCT=%1
	goto next
:Configuration
	shift /1
	set BUILD_CONFIGURATION=%1
	set BUILD_OUTPUT=%cd%\artifacts\%BUILD_CONFIGURATION:"=%
	goto next
:Output
	shift /1
	set BUILD_OUTPUT=%1
	goto next
:Company
	shift /1
	set BUILD_COMPANY=%1
	set BUILD_COPYRIGHT="Copyright (c) %BUILD_COMPANY:"=% %year%"
	goto next
	
:done

CALL dotnet restore
CALL dotnet pack -c %BUILD_CONFIGURATION:"=% -o "%BUILD_OUTPUT:"=%" /p:Version=%BUILD_VERSION:"=%;Product="%BUILD_PRODUCT:"=%";Copyright="%BUILD_COPYRIGHT:"=%";Company="%BUILD_COMPANY:"=%";InformalVersion="v%BUILD_VERSION:"=%"

popd