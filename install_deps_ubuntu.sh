#!/bin/sh

# ubuntu 18.04

sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys EB3E94ADBE1229CF
wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list
sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list

sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys A6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list

sudo apt-get -y update

# nodejs
sudo apt-get -y install nodejs
sudo apt-get -y install npm

# dotnet
sudo apt-get -y install dotnet-sdk-2.2

# mono
sudo apt-get -y install mono-devel