#!/bin/sh

rm -rf website/
cp -rf Samples/bin/Debug/netstandard2.0/ website/

npm install --global surge

WEBSITE_DIR=$(realpath website)
surge $WEBSITE_DIR/ https://webglnet.surge.sh

