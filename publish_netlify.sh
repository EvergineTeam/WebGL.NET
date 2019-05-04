#!/bin/sh

wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.2

source build.sh

rm -rf website/
cp -rf Samples/bin/Debug/netstandard2.0/ website/

