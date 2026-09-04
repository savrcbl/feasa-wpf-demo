# FeasaWpfDemo

A Windows desktop app demonstrating the [`FeasaLedAnalyser`](https://github.com/savrcbl/feasa-led-analyser)
NuGet package — connect to a Feasa LED analyser, capture a measurement, and read
every supported measurement type, with a live connection log.

This is a reference example, installed the same way any consumer of the package
would: via `PackageReference`, not a project reference. There's no library source
in this repo — just a client of it.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![WPF](https://img.shields.io/badge/UI-WPF-0078D4)

## Prerequisites

- Windows (WPF is Windows-only)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- A Feasa LED analyser connected over USB/serial, with its driver installed

## Running it

```
git clone https://github.com/savrcbl/feasa-wpf-demo.git
cd feasa-wpf-demo
dotnet run
```

Or open `FeasaWpfDemo.csproj` in Visual Studio and hit F5.

## What it demonstrates

- `FeasaClient(port, baudRate)` construction, with `"auto"` as the default port
- `ConnectAsync()` / `Disconnect()`
- `CaptureAsync()`
- Every `Get*Async()` measurement method the library exposes: RGBI, HSI, xy, xyi,
  CCT, UV, CIE XYZ, wavelength+intensity, WSI, signal level, wavelength, intensity,
  and absolute intensity
- The `OnLog` event, wired up to a live-updating log panel

Each reading is displayed using its own `ToString()` output, so the UI works the
same way regardless of which measurement type is selected — no need to know each
reading type's specific fields ahead of time.

## Related

- [feasa-led-analyser](https://github.com/savrcbl/feasa-led-analyser) — the library
  this demo consumes
- [`FeasaLedAnalyser` on NuGet](https://www.nuget.org/packages/FeasaLedAnalyser)

## License

[MIT](LICENSE)
