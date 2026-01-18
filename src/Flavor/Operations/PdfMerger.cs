using PdfSharpCore.Pdf.IO;

namespace Flavor.Operations;

/// <summary>
///     Provides functionality to merge multiple PDF documents into one.
/// </summary>
public static class PdfMerger
{
    /// <summary>
    ///     Merges multiple PDF documents into a single PDF.
    /// </summary>
    /// <param name="documents">The PDF documents to merge.</param>
    /// <returns>A new <see cref="PdfDocument" /> containing all pages from the input documents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when documents is null.</exception>
    /// <exception cref="ArgumentException">Thrown when documents collection is empty.</exception>
    /// <example>
    ///     <code>
    /// var pdf1 = await converter.ConvertHtmlAsync(html1);
    /// var pdf2 = await converter.ConvertHtmlAsync(html2);
    /// var merged = PdfMerger.Merge(pdf1, pdf2);
    /// await merged.SaveAsync("combined.pdf");
    /// </code>
    /// </example>
    public static PdfDocument Merge(params PdfDocument[] documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        if (documents.Length == 0)
            throw new ArgumentException("At least one document is required.", nameof(documents));

        return Merge(documents.AsEnumerable());
    }

    /// <summary>
    ///     Merges multiple PDF documents into a single PDF.
    /// </summary>
    /// <param name="documents">The PDF documents to merge.</param>
    /// <returns>A new <see cref="PdfDocument" /> containing all pages from the input documents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when documents is null.</exception>
    /// <exception cref="ArgumentException">Thrown when documents collection is empty.</exception>
    public static PdfDocument Merge(IEnumerable<PdfDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var docList = documents.ToList();
        if (docList.Count == 0)
            throw new ArgumentException("At least one document is required.", nameof(documents));

        using var outputDocument = new PdfSharpCore.Pdf.PdfDocument();

        var totalPages = 0;
        foreach (var doc in docList)
        {
            using var stream = new MemoryStream(doc.ToBytes());
            using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            for (var i = 0; i < inputDocument.PageCount; i++)
            {
                var page = inputDocument.Pages[i];
                outputDocument.AddPage(page);
                totalPages++;
            }
        }

        using var outputStream = new MemoryStream();
        outputDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), totalPages);
    }

    /// <summary>
    ///     Merges multiple PDF byte arrays into a single PDF.
    /// </summary>
    /// <param name="pdfBytes">The PDF byte arrays to merge.</param>
    /// <returns>A new <see cref="PdfDocument" /> containing all pages.</returns>
    public static PdfDocument Merge(params byte[][] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        if (pdfBytes.Length == 0)
            throw new ArgumentException("At least one PDF is required.", nameof(pdfBytes));

        using var outputDocument = new PdfSharpCore.Pdf.PdfDocument();

        var totalPages = 0;
        foreach (var bytes in pdfBytes)
        {
            using var stream = new MemoryStream(bytes);
            using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            for (var i = 0; i < inputDocument.PageCount; i++)
            {
                var page = inputDocument.Pages[i];
                outputDocument.AddPage(page);
                totalPages++;
            }
        }

        using var outputStream = new MemoryStream();
        outputDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), totalPages);
    }

    /// <summary>
    ///     Merges multiple PDF files into a single PDF.
    /// </summary>
    /// <param name="filePaths">The paths to the PDF files to merge.</param>
    /// <returns>A new <see cref="PdfDocument" /> containing all pages.</returns>
    public static PdfDocument MergeFiles(params string[] filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        if (filePaths.Length == 0)
            throw new ArgumentException("At least one file path is required.", nameof(filePaths));

        using var outputDocument = new PdfSharpCore.Pdf.PdfDocument();

        var totalPages = 0;
        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDF file not found.", filePath);

            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);

            for (var i = 0; i < inputDocument.PageCount; i++)
            {
                var page = inputDocument.Pages[i];
                outputDocument.AddPage(page);
                totalPages++;
            }
        }

        using var outputStream = new MemoryStream();
        outputDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), totalPages);
    }

    /// <summary>
    ///     Merges multiple PDF files into a single PDF asynchronously.
    /// </summary>
    /// <param name="filePaths">The paths to the PDF files to merge.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A new <see cref="PdfDocument" /> containing all pages.</returns>
    public static async Task<PdfDocument> MergeFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        var fileList = filePaths.ToList();
        if (fileList.Count == 0)
            throw new ArgumentException("At least one file path is required.", nameof(filePaths));

        var pdfBytesList = new List<byte[]>();
        foreach (var filePath in fileList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDF file not found.", filePath);

            var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            pdfBytesList.Add(bytes);
        }

        return Merge(pdfBytesList.ToArray());
    }
}