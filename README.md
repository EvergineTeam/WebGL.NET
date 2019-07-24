# WebGL.NET

[![Build Status](https://dev.azure.com/webglnet/WebGL.NET/_apis/build/status/WaveEngine.WebGL.NET?branchName=master)](https://dev.azure.com/webglnet/WebGL.NET/_build/latest?definitionId=2&branchName=master)
[![Run Tests](https://img.shields.io/badge/tests-run%20now-orange.svg)](https://webglnet.surge.sh/tests)

.NET binding for WebGL through WebAssembly.

We've published a preview [NuGet](https://www.nuget.org/packages/WebGLDotNET) *but* you need a copy of:
- this [packages](https://github.com/WaveEngine/WebGL.NET/tree/master/src/packages) folder which contains mandatory NuGets from Mono still not published in nuget.org, and
- this [NuGet.config](https://github.com/WaveEngine/WebGL.NET/blob/master/src/NuGet.config) along your SLN/CSPROJ

See the samples running [here](https://webglnet.surge.sh).

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
