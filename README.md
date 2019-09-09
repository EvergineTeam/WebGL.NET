# WebGL.NET

[![Build Status](https://dev.azure.com/webglnet/WebGL.NET/_apis/build/status/WaveEngine.WebGL.NET?branchName=master)](https://dev.azure.com/webglnet/WebGL.NET/_build/latest?definitionId=2&branchName=master)
[![Run Tests](https://img.shields.io/badge/tests-run%20now-orange.svg)](https://webglnet.surge.sh/tests)
[![WebGLDotNET](https://img.shields.io/nuget/v/WebGLDotNET.svg?label=WebGLDotNET)](https://www.nuget.org/packages/WebGLDotNET)

.NET binding for WebGL through WebAssembly. See the samples running [here](https://webglnet.surge.sh).

If you want to jump in quickly, [Your first WebGL.NET app](https://geeks.ms/xamarinteam/2019/08/28/your-first-webgldotnet-app/) will guide you step by step.

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
