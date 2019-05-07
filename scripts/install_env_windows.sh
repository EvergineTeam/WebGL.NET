# Enable Developer Mode in windows 10 (https://blogs.windows.com/buildingapps/2016/12/02/symlinks-windows-10/#wbtYV5KMKtH03zrl.97)
# or 
# Grant create symbolic links to your user (https://superuser.com/questions/124679/how-do-i-create-a-link-in-windows-7-home-premium-as-a-regular-user/125981#125981)

VSWHEREPATH="$(echo $PROGRAMFILES)"" (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
if [ ! -f "$VSWHEREPATH" ]; then
	echo "Visual Studio 2017 version 15.2 or greater required"
	exit 1
fi

mkdir -p ./tools
if [ ! -f "./tools/msbuild" ]; then
	MSBUILDPATH=$("$VSWHEREPATH" -latest -requires Microsoft.Component.MSBuild -find MSBuild/**/Bin/MSBuild.exe)
	cmd <<< "mklink .\tools\msbuild \"$MSBUILDPATH\"" > /dev/null
fi
