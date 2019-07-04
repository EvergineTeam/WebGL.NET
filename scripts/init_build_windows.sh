#!/usr/bin/env bash

export MSYS_NO_PATHCONV=1

if [ ! -x "$(command -v msbuild)" ] ; then

	VSCOMMUNITY2017='/c/Program Files (x86)/Microsoft Visual Studio/2017/Community/MSBuild/15.0/Bin/'
	VSCOMMUNITY2019='/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/'

	VSENTERPRISE2017='/c/Program Files (x86)/Microsoft Visual Studio/2017/Enterprise/MSBuild/15.0/Bin/'
	VSENTERPRISE2019='/c/Program Files (x86)/Microsoft Visual Studio/2019/Enterprise/MSBuild/Current/Bin/'

	export PATH=$VSCOMMUNITY2017:$PATH
	export PATH=$VSCOMMUNITY2019:$PATH

	export PATH=$VSENTERPRISE2017:$PATH
	export PATH=$VSENTERPRISE2019:$PATH

	alias msbuild=msbuild.exe
fi



