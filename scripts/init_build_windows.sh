#!/usr/bin/env bash

if [ ! -x "$(command -v msbuild)" ] ; then

	VSWHEREPATH=$(echo $(cygpath -F 42))"/Microsoft Visual Studio/Installer/vswhere.exe"
	if [ ! -f "$VSWHEREPATH" ]; then
		echo "Visual Studio 2017 version 15.2 or greater required"
		exit 1
	fi

	MSBUILDWINPATH=$("$VSWHEREPATH" -latest -requires Microsoft.Component.MSBuild -find MSBuild/**/Bin/MSBuild.exe)
	MSBUILDWINPATH=${MSBUILDWINPATH%"MSBuild.exe"}
	MSBUILDPATH=$(echo $(cygpath -u $MSBUILDWINPATH))
	
	export PATH=$MSBUILDPATH:$PATH
	alias msbuild=MSBuild.exe
fi




