#!/bin/sh

# ubuntu 16.04

wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.2

sudo apt-get update
sudo apt-get install nodejs
sudo apt-get install npm

npm install -g surge@latest