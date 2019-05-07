# ubuntu 18.04

echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys A6A19B38D3D831EF

sudo apt-get -y update
sudo apt-get -y install mono-devel