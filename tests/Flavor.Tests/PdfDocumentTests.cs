namespace Flavor.Tests;

public class PdfDocumentTests
{
    private static readonly byte[] PdfMagicBytes = "%PDF"u8.ToArray();
    private static readonly byte[] TestPdfData = [.. PdfMagicBytes, .. "test content"u8.ToArray()];

    [Fact]
    public void Constructor_WithValidData_CreatesPdfDocument()
    {
        using var pdf = new PdfDocument(TestPdfData, 1);

        pdf.PageCount.Should().Be(1);
        pdf.Size.Should().Be(TestPdfData.Length);
    }

    [Fact]
    public void Constructor_WithNullData_ThrowsArgumentNullException()
    {
        var action = () => new PdfDocument(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToBytes_ReturnsCopyOfData()
    {
        using var pdf = new PdfDocument(TestPdfData);

        var bytes = pdf.ToBytes();

        bytes.Should().BeEquivalentTo(TestPdfData);
        bytes.Should().NotBeSameAs(TestPdfData);
    }

    [Fact]
    public void AsSpan_ReturnsDataSpan()
    {
        using var pdf = new PdfDocument(TestPdfData);

        var span = pdf.AsSpan();

        span.ToArray().Should().BeEquivalentTo(TestPdfData);
    }

    [Fact]
    public void AsMemory_ReturnsDataMemory()
    {
        using var pdf = new PdfDocument(TestPdfData);

        var memory = pdf.AsMemory();

        memory.ToArray().Should().BeEquivalentTo(TestPdfData);
    }

    [Fact]
    public void ToStream_ReturnsReadableStream()
    {
        using var pdf = new PdfDocument(TestPdfData);

        using var stream = pdf.ToStream();

        stream.CanRead.Should().BeTrue();
        stream.Length.Should().Be(TestPdfData.Length);
    }

    [Fact]
    public void ToBase64_ReturnsBase64String()
    {
        using var pdf = new PdfDocument(TestPdfData);

        var base64 = pdf.ToBase64();
        var decoded = Convert.FromBase64String(base64);

        decoded.Should().BeEquivalentTo(TestPdfData);
    }

    [Fact]
    public void ToDataUri_ReturnsValidDataUri()
    {
        using var pdf = new PdfDocument(TestPdfData);

        var dataUri = pdf.ToDataUri();

        dataUri.Should().StartWith("data:application/pdf;base64,");
    }

    [Fact]
    public async Task SaveAsync_WritesToFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            using var pdf = new PdfDocument(TestPdfData);

            await pdf.SaveAsync(tempFile);

            var savedData = await File.ReadAllBytesAsync(tempFile);
            savedData.Should().BeEquivalentTo(TestPdfData);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Save_WritesToFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            using var pdf = new PdfDocument(TestPdfData);

            pdf.Save(tempFile);

            var savedData = File.ReadAllBytes(tempFile);
            savedData.Should().BeEquivalentTo(TestPdfData);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectory_WhenNotExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tempFile = Path.Combine(tempDir, "test.pdf");
        try
        {
            using var pdf = new PdfDocument(TestPdfData);

            await pdf.SaveAsync(tempFile);

            Directory.Exists(tempDir).Should().BeTrue();
            File.Exists(tempFile).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SaveAsync_WithNullPath_ThrowsArgumentException()
    {
        var action = async () =>
        {
            using var pdf = new PdfDocument(TestPdfData);
            await pdf.SaveAsync(null!);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SaveAsync_WithEmptyPath_ThrowsArgumentException()
    {
        var action = async () =>
        {
            using var pdf = new PdfDocument(TestPdfData);
            await pdf.SaveAsync(string.Empty);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task WriteToAsync_WritesToStream()
    {
        using var pdf = new PdfDocument(TestPdfData);
        using var stream = new MemoryStream();

        await pdf.WriteToAsync(stream);

        stream.ToArray().Should().BeEquivalentTo(TestPdfData);
    }

    [Fact]
    public void WriteTo_WritesToStream()
    {
        using var pdf = new PdfDocument(TestPdfData);
        using var stream = new MemoryStream();

        pdf.WriteTo(stream);

        stream.ToArray().Should().BeEquivalentTo(TestPdfData);
    }

    [Fact]
    public void Dispose_AllowsMultipleCalls()
    {
        var pdf = new PdfDocument(TestPdfData);

        var action = () =>
        {
            pdf.Dispose();
            pdf.Dispose();
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void ToBytes_AfterDispose_ThrowsObjectDisposedException()
    {
        var pdf = new PdfDocument(TestPdfData);
        pdf.Dispose();

        var action = () => pdf.ToBytes();

        action.Should().Throw<ObjectDisposedException>();
    }
}