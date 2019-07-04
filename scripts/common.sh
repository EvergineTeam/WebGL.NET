#!/usr/bin/env bash

windows() { [[ -n "$WINDIR" ]]; }
if windows; then
	export MSYS_NO_PATHCONV=1
	export PATH=$(realpath ./tools/):$PATH
fi
