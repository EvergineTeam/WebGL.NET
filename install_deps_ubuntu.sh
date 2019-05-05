#!/bin/sh

# ubuntu 18.04

nodejs () 
{
	curl -sL https://deb.nodesource.com/setup_10.x | sudo bash -
	sudo apt-get -y install nodejs
}

dotnet ()
{
	wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list
	sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
	sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys EB3E94ADBE1229CF

	sudo apt-get -y update
	sudo apt-get -y install dotnet-sdk-2.2
}

mono ()
{
	echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
	sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys A6A19B38D3D831EF

	sudo apt-get -y update
	sudo apt-get -y install mono-devel
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
