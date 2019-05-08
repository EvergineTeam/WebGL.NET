windows() { [[ -n "$WINDIR" ]]; }
if windows; then
	export PATH=$(realpath ./tools/):$PATH
	export MSYS_NO_PATHCONV=1
fi
