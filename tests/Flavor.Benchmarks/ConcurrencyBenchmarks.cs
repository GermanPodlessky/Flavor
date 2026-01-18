using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Flavor.Benchmarks;

/// <summary>
///     Benchmarks for concurrent PDF generation with different pool sizes.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[RankColumn]
public class ConcurrencyBenchmarks
{
    private const string Html = """
                                <!DOCTYPE html>
                                <html>
                                <head>
                                    <style>
                                        body { font-family: Arial, sans-serif; margin: 40px; }
                                        h1 { color: #333; }
                                        p { line-height: 1.6; }
                                    </style>
                                </head>
                                <body>
                                    <h1>Document Title</h1>
                                    <p>This is a sample document for concurrent PDF generation testing.</p>
                                    <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>
                                </body>
                                </html>
                                """;

    private FlavorConverter _converter = null!;

    [Params(1, 2, 4)]
    public int PoolSize { get; set; }

    [Params(4, 8)]
    public int ConcurrentRequests { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _converter = new FlavorConverter(options => { options.PoolSize = PoolSize; });
        await _converter.WarmupAsync();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _converter.DisposeAsync();
    }

    [Benchmark(Description = "Concurrent Generation")]
    public async Task ConcurrentPdfGeneration()
    {
        var tasks = Enumerable.Range(0, ConcurrentRequests)
            .Select(_ => _converter.ConvertHtmlAsync(Html));

        await Task.WhenAll(tasks);
    }
}