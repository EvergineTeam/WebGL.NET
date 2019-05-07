#!/bin/sh

defaultProject=WebGL.NET.sln
defaultRestore=true

PROJECT=${1:-$defaultProject}
RESTORE=${2:-$defaultRestore}
COMMITHASH=$(git rev-parse HEAD)

msbuild /restore:$RESTORE $PROJECT -p:CommitHash=$COMMITHASH