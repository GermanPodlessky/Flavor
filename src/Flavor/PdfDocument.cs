namespace Flavor;

/// <summary>
///     Represents a generated PDF document.
/// </summary>
public sealed class PdfDocument : IDisposable
{
    private byte[] _data;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PdfDocument" /> class.
    /// </summary>
    /// <param name="data">The raw PDF data.</param>
    /// <param name="pageCount">The number of pages in the document.</param>
    internal PdfDocument(byte[] data, int pageCount = 0)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        PageCount = pageCount;
    }

    /// <summary>
    ///     Gets the number of pages in the PDF document.
    ///     Note: This may be 0 if page count detection is not available.
    /// </summary>
    public int PageCount { get; }

    /// <summary>
    ///     Gets the size of the PDF document in bytes.
    /// </summary>
    public long Size => _data.Length;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _data = [];
        _disposed = true;
    }

    /// <summary>
    ///     Returns the PDF data as a byte array.
    /// </summary>
    /// <returns>A copy of the PDF data.</returns>
    public byte[] ToBytes()
    {
        ThrowIfDisposed();
        var copy = new byte[_data.Length];
        Array.Copy(_data, copy, _data.Length);
        return copy;
    }

    /// <summary>
    ///     Returns a read-only span over the PDF data.
    /// </summary>
    /// <returns>A read-only span of the PDF data.</returns>
    public ReadOnlySpan<byte> AsSpan()
    {
        ThrowIfDisposed();
        return _data.AsSpan();
    }

    /// <summary>
    ///     Returns a read-only memory over the PDF data.
    /// </summary>
    /// <returns>A read-only memory of the PDF data.</returns>
    public ReadOnlyMemory<byte> AsMemory()
    {
        ThrowIfDisposed();
        return _data.AsMemory();
    }

    /// <summary>
    ///     Creates a stream from the PDF data.
    /// </summary>
    /// <returns>A new <see cref="MemoryStream" /> containing the PDF data.</returns>
    public MemoryStream ToStream()
    {
        ThrowIfDisposed();
        return new MemoryStream(_data, false);
    }

    /// <summary>
    ///     Saves the PDF document to a file.
    /// </summary>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when file path is null or empty.</exception>
    public async Task SaveAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(filePath, _data, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Saves the PDF document to a file synchronously.
    /// </summary>
    /// <param name="filePath">The file path to save to.</param>
    /// <exception cref="ArgumentException">Thrown when file path is null or empty.</exception>
    public void Save(string filePath)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllBytes(filePath, _data);
    }

    /// <summary>
    ///     Writes the PDF document to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(stream);
        await stream.WriteAsync(_data, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Writes the PDF document to a stream synchronously.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    public void WriteTo(Stream stream)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(stream);
        stream.Write(_data);
    }

    /// <summary>
    ///     Returns the PDF as a Base64-encoded string.
    /// </summary>
    /// <returns>The PDF data as a Base64 string.</returns>
    public string ToBase64()
    {
        ThrowIfDisposed();
        return Convert.ToBase64String(_data);
    }

    /// <summary>
    ///     Returns the PDF as a data URI suitable for embedding in HTML.
    /// </summary>
    /// <returns>A data URI string.</returns>
    public string ToDataUri()
    {
        ThrowIfDisposed();
        return $"data:application/pdf;base64,{ToBase64()}";
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}