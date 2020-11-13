**Update November 13th, 2020**

After noticing performance issues with this approach, we have moved to [Emscripten's EGL one](https://emscripten.org/docs/porting/multimedia_and_graphics/EGL-Support-in-Emscripten.html) through static linking –which has resulted much faster–, using our existing [OpenGL.NET](https://github.com/WaveEngine/OpenGL.NET) binding. [Wave Engine 3.0 preview 4](https://geeks.ms/waveengineteam/2020/08/28/wave-engine-3-0-preview-4/) already contains these changes.

Our efforts will go in this direction and, surely, we will not maintain this repository as much as before. We are also working on [WebGPU.NET](https://github.com/WaveEngine/WebGPU.NET), as the replacement for everything WebGL. However, WebGL.NET is still valid whenever performance is not the key ([this article](https://marcoscobena.com/?i=wave-engine-web-performance) draws a red line on it).

# WebGL.NET

[![Build Status](https://dev.azure.com/waveengineteam/Wave.Engine/_apis/build/status/WaveEngine.WebGL.NET?branchName=master)](https://dev.azure.com/waveengineteam/Wave.Engine/_build/latest?definitionId=42&branchName=master)
[![Run Tests](https://img.shields.io/badge/tests-run%20now-orange.svg)](https://webgldotnet.surge.sh/tests)
[![NuGet](https://img.shields.io/nuget/v/WebGLDotNET.svg?label=NuGet)](https://www.nuget.org/packages/WebGLDotNET)

.NET binding for WebGL through WebAssembly. See the samples running [here](https://webgldotnet.surge.sh).

<img src="LoadGLTFSample.gif" width="509" />

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
- [Wave Engine's glTF viewer](http://gltf.waveengine.net): self-explanatory O:-)

## Debugging

Run this in browser's Console to monitor how many bridged objects there are in such moment:

```
BINDING.mono_wasm_object_registry
```

## Thanks

- [@kjpou1](https://github.com/kjpou1), for your PRs improving performance overall, along with nice comments on how stuff work underneath
