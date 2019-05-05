#!/bin/sh

# ubuntu 18.04

# dotnet
wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list
sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list

sudo apt-get -y update
sudo apt-get -y install dotnet-sdk-2.2

# mono
sudo apt -y install gnupg ca-certificates
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt -y update

sudo apt -y install mono-devel

# nodejs
sudo apt-get -y update
sudo apt-get -y install nodejs
sudo apt-get -y install npm

sudo npm install -g surge