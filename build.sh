#!/bin/sh

COMMITHASH=$(git rev-parse HEAD)

msbuild WebGL.NET.sln -p:CommitHash=$COMMITHASH /restore