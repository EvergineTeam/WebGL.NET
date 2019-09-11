# WebGL.NET

[![Build Status](https://dev.azure.com/waveengineteam/Wave.Engine/_apis/build/status/WaveEngine.WebGL.NET?branchName=master)](https://dev.azure.com/waveengineteam/Wave.Engine/_build/latest?definitionId=42&branchName=master)
[![Run Tests](https://img.shields.io/badge/tests-run%20now-orange.svg)](https://webgldotnet.surge.sh/tests)
[![NuGet](https://img.shields.io/nuget/v/WebGLDotNET.svg?label=NuGet)](https://www.nuget.org/packages/WebGLDotNET)

.NET binding for WebGL through WebAssembly. See the samples running [here](https://webgldotnet.surge.sh).

If you want to jump in quickly, [Your first WebGL.NET app](https://geeks.ms/xamarinteam/2019/08/28/your-first-webgldotnet-app/) will guide you step by step.

### Installation instructions

Apart from adding the [NuGet](https://www.nuget.org/packages/WebGLDotNET) package, make a local copy of:
- this [packages](https://github.com/WaveEngine/WebGL.NET/tree/master/src/packages) folder, which contains mandatory NuGets from Mono still not published in nuget.org; and
- this [NuGet.config](https://github.com/WaveEngine/WebGL.NET/blob/master/src/NuGet.config) along your SLN/CSPROJ

## Features

- WebGL 1 & 2 support
- API C#-ified to make .NET Developers feel at home

## Who's using it?

Apart from serving Wave Engine 3.0 for the Web, it's being used in some other really nice projects, such like:
- [Jazz² Resurrection](http://deat.tk/jazz2/wasm/): a "reimplementation of the game Jazz Jackrabbit 2 released in 1998"

## Debugging

Run this in browser's Console to monitor how many bridged objects there are in such moment:

```
BINDING.mono_wasm_object_registry
```

## Thanks

- [@kjpou1](https://github.com/kjpou1), for your PRs improving performance overall, along with nice comments on how stuff work underneath
