# WebGL.NET

[![Build status](https://ci.appveyor.com/api/projects/status/mtl68763xqr4wkuk?svg=true)](https://ci.appveyor.com/project/jacano/webgl-net)
[![Build Status](https://dev.azure.com/webglnet/WebGL.NET/_apis/build/status/WaveEngine.WebGL.NET?branchName=master)](https://dev.azure.com/webglnet/WebGL.NET/_build/latest?definitionId=2&branchName=master)

C# wrapper for WebGL through WebAssembly.

See the samples running [here](https://webglnet.surge.sh).

(As of May 7th 2019, if you're in macOS please use Visual Studio Code; its brother Visual Studio for Mac currently doesn't support the latests .NET Core SDKs.)

## Features

- WebGL 1 & 2 support
- API C#-ified to make .NET Developers feel at home

## Debugging

Run this in browser's Console to monitor how many bridged objects there are in such moment:

```
BINDING.mono_wasm_object_registry
```

## Thanks

- [@kjpou1](https://github.com/kjpou1), for your PRs improving performance overall, along with nice comments on how stuff work underneath
