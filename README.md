# WebGL.NET

C# wrapper for WebGL through WebAssembly.

See the samples running [here](https://marcoscobena.com/tmp/webgldotnet/) (they may not be updated with current status).

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
