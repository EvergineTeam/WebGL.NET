#!/usr/bin/env bash

if [ "$#" -ne 1 ]; then
    echo "usage:" `basename "$0"` "folder_to_copy_packages"
	exit
fi

WASM_DROP_URL=https://jenkins.mono-project.com/job/test-mono-mainline-wasm/label=ubuntu-1804-amd64/lastSuccessfulBuild/Azure/
WASM_HTML_CONTENT=$(curl -L $WASM_DROP_URL)
WASM_SDK=$(echo $WASM_HTML_CONTENT | perl -ne 'print $1 if /(processDownloadRequest\/(.*?)\/ubuntu-1804-amd64\/sdks\/wasm\/mono-wasm-(.*?)\.zip)/s')

curl -L -o mono_wasm_sdk.zip $WASM_DROP_URL/$WASM_SDK
unzip -o -qq mono_wasm_sdk.zip 'packages/*' -d $1
rm -rf mono_wasm_sdk.zip

nuget locals all -clear