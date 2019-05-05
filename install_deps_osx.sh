#!/bin/sh

dotnet ()
{
	brew update
	brew tap caskroom/cask
	brew cask install dotnet-sdk
}

mono ()
{
	brew update
	brew tap caskroom/cask
	brew cask install mono-mdk
}

if [ $# -eq 0 ]
	then
		dotnet
		mono
else
	for arg in $@
	do
		eval $(echo $arg)
	done
fi
