# WebGL.NET

[![Build Status](https://dev.azure.com/webglnet/WebGL.NET/_apis/build/status/WaveEngine.WebGL.NET?branchName=master)](https://dev.azure.com/webglnet/WebGL.NET/_build/latest?definitionId=2&branchName=master)

C# wrapper for WebGL through WebAssembly.

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

## TODO

- Unit Tests: mostly for marshalling, to assure we don't drive back while touching the transpiler