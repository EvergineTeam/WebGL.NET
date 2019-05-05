#!/bin/sh

rm -rf website/
cp -rf Samples/bin/Debug/netstandard2.0/ website/

surge $(realpath website)/ https://webglnet.surge.sh
