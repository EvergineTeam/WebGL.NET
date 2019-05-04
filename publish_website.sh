#!/bin/sh

rm -rf website/
cp -rf Samples/bin/Debug/netstandard2.0/ website/

npm install --global surge

cd website/
surge

