#!/usr/bin/env bash

windows() { [[ -n "$WINDIR" ]]; }
if windows; then
	source scripts/init_build_windows.sh
fi

defaultProject=src/WebGL.NET.sln
defaultConfiguration=Release
defaultTreatWarningsAsErrors=true

project=${1:-$defaultProject}
configuration=${2:-$defaultConfiguration}
treatWarningsAsErrors=${3:-$defaultTreatWarningsAsErrors}
commithash=$(git rev-parse HEAD)
verbosity=n

dotnet build $project -c $configuration -v $verbosity -p:CommitHash=$commithash;TreatWarningsAsErrors=$treatWarningsAsErrors