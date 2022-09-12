using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ImageProcessingUtils;

SIMD.LoadAssembly();

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());

SIMD.UnloadAssembly();