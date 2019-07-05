#!/usr/bin/env bash

windows() { [[ -n "$WINDIR" ]]; }
if windows; then
	shopt -s expand_aliases
	alias msbuild='MSBuild.exe'
fi

printenv

defaultProject=src/WebGL.NET.sln
defaultRestore=true
defaultTreatWarningsAsErrors=true

project=${1:-$defaultProject}
restore=${2:-$defaultRestore}
treatWarningsAsErrors=${3:-$defaultTreatWarningsAsErrors}
commithash=$(git rev-parse HEAD)

msbuild /restore:$restore $project -p:CommitHash=$commithash -p:TreatWarningsAsErrors=$treatWarningsAsErrors