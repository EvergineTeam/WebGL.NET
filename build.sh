#!/bin/sh

windows() { [[ -n "$WINDIR" ]]; }
if windows; then

	mkdir -p ./tools
	export PATH=./tools/:$PATH

	VSWHEREPATH="$(cmd //c 'echo %ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe')"
	if [ ! -f "$VSWHEREPATH" ]; then
		echo "Visual Studio 2017 version 15.2 or greater required"
		exit 1
	fi
	
	if [ ! -f "./tools/msbuild" ]; then
		MSBUILDPATH=$("$VSWHEREPATH" -latest -requires Microsoft.Component.MSBuild -find MSBuild/**/Bin/MSBuild.exe)
		cmd <<< "mklink .\tools\msbuild \"$MSBUILDPATH\"" > /dev/null
	fi
	
	if [ ! -f "./tools/nuget" ]; then
		curl -L https://dist.nuget.org/win-x86-commandline/latest/nuget.exe --output ./tools/nuget
	fi

fi

nuget restore WebGL.NET.sln
msbuild WebGL.NET.sln