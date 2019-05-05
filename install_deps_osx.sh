#!/bin/sh

nodejs () 
{
	brew update
	brew install node
}

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
		nodejs
		dotnet
		mono
else
	for arg in $@
	do
		eval $(echo $arg)
	done
fi
