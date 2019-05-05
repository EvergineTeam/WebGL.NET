#!/bin/sh

COMMITHASH=$(git rev-parse HEAD)

dotnet build WebGL.NET.sln -p:CommitHash=$COMMITHASH