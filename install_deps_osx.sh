#!/bin/sh

brew update
brew tap caskroom/cask
brew cask install dotnet-sdk

brew update
brew install node

npm install -g surge