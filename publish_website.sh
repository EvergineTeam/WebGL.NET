#!/bin/sh

rm -rf website/
cp -rf Samples/bin/Debug/netstandard2.0/ website/

npm install -g npm@latest
npm install -g surge@latest

surge $(realpath website)/ https://webglnet.surge.sh

