#!/bin/sh

rm -rf website/
cp -rf Samples/bin/Debug/netstandard2.0/ website/

npm install --save-dev surge

surge $(realpath website)/ https://webglnet.surge.sh
