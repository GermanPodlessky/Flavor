using PdfSharpCore.Pdf.IO;

namespace Flavor.Operations;

/// <summary>
///     Provides functionality to split PDF documents.
/// </summary>
public static class PdfSplitter
{
    /// <summary>
    ///     Splits a PDF document into individual pages.
    /// </summary>
    /// <param name="document">The PDF document to split.</param>
    /// <returns>A collection of <see cref="PdfDocument" /> objects, one per page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var pages = PdfSplitter.SplitToPages(pdf);
    /// for (int i = 0; i &lt; pages.Count; i++)
    /// {
    ///     await pages[i].SaveAsync($"page-{i + 1}.pdf");
    /// }
    /// </code>
    /// </example>
    public static IReadOnlyList<PdfDocument> SplitToPages(PdfDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        using var stream = new MemoryStream(document.ToBytes());
        using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

        var pages = new List<PdfDocument>();

        for (var i = 0; i < inputDocument.PageCount; i++)
        {
            using var outputDocument = new PdfSharpCore.Pdf.PdfDocument();
            outputDocument.AddPage(inputDocument.Pages[i]);

            using var outputStream = new MemoryStream();
            outputDocument.Save(outputStream, false);

            pages.Add(new PdfDocument(outputStream.ToArray(), 1));
        }

        return pages;
    }

    /// <summary>
    ///     Extracts a range of pages from a PDF document.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="startPage">The starting page number (1-based).</param>
    /// <param name="endPage">The ending page number (1-based, inclusive).</param>
    /// <returns>A new <see cref="PdfDocument" /> containing the specified pages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when page numbers are invalid.</exception>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var firstThreePages = PdfSplitter.ExtractPages(pdf, 1, 3);
    /// await firstThreePages.SaveAsync("first-three.pdf");
    /// </code>
    /// </example>
    public static PdfDocument ExtractPages(PdfDocument document, int startPage, int endPage)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (startPage < 1)
            throw new ArgumentOutOfRangeException(nameof(startPage), "Start page must be at least 1.");

        if (endPage < startPage)
            throw new ArgumentOutOfRangeException(nameof(endPage), "End page must be greater than or equal to start page.");

        using var stream = new MemoryStream(document.ToBytes());
        using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

        if (startPage > inputDocument.PageCount)
            throw new ArgumentOutOfRangeException(nameof(startPage),
                $"Start page {startPage} exceeds document page count {inputDocument.PageCount}.");

        if (endPage > inputDocument.PageCount)
            throw new ArgumentOutOfRangeException(nameof(endPage), $"End page {endPage} exceeds document page count {inputDocument.PageCount}.");

        using var outputDocument = new PdfSharpCore.Pdf.PdfDocument();

        for (var i = startPage - 1; i < endPage; i++) outputDocument.AddPage(inputDocument.Pages[i]);

        using var outputStream = new MemoryStream();
        outputDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), endPage - startPage + 1);
    }

    /// <summary>
    ///     Extracts specific pages from a PDF document.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="pageNumbers">The page numbers to extract (1-based).</param>
    /// <returns>A new <see cref="PdfDocument" /> containing the specified pages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document or pageNumbers is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any page number is invalid.</exception>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var selectedPages = PdfSplitter.ExtractPages(pdf, 1, 3, 5, 7);
    /// await selectedPages.SaveAsync("selected.pdf");
    /// </code>
    /// </example>
    public static PdfDocument ExtractPages(PdfDocument document, params int[] pageNumbers)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(pageNumbers);

        if (pageNumbers.Length == 0)
            throw new ArgumentException("At least one page number is required.", nameof(pageNumbers));

        using var stream = new MemoryStream(document.ToBytes());
        using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

        foreach (var pageNum in pageNumbers)
            if (pageNum < 1 || pageNum > inputDocument.PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageNumbers), $"Page number {pageNum} is out of range (1-{inputDocument.PageCount}).");

        using var outputDocument = new PdfSharpCore.Pdf.PdfDocument();

        foreach (var pageNum in pageNumbers) outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);

        using var outputStream = new MemoryStream();
        outputDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), pageNumbers.Length);
    }

    /// <summary>
    ///     Splits a PDF document into chunks of a specified size.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="pagesPerChunk">The number of pages per chunk.</param>
    /// <returns>A collection of <see cref="PdfDocument" /> chunks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pagesPerChunk is less than 1.</exception>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html); // 10 pages
    /// var chunks = PdfSplitter.SplitByPageCount(pdf, 3);
    /// // Result: 4 PDFs with 3, 3, 3, and 1 page(s)
    /// </code>
    /// </example>
    public static IReadOnlyList<PdfDocument> SplitByPageCount(PdfDocument document, int pagesPerChunk)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (pagesPerChunk < 1)
            throw new ArgumentOutOfRangeException(nameof(pagesPerChunk), "Pages per chunk must be at least 1.");

        using var stream = new MemoryStream(document.ToBytes());
        using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

        var chunks = new List<PdfDocument>();

        for (var i = 0; i < inputDocument.PageCount; i += pagesPerChunk)
        {
            using var outputDocument = new PdfSharpCore.Pdf.PdfDocument();

            var endIndex = Math.Min(i + pagesPerChunk, inputDocument.PageCount);
            for (var j = i; j < endIndex; j++) outputDocument.AddPage(inputDocument.Pages[j]);

            using var outputStream = new MemoryStream();
            outputDocument.Save(outputStream, false);

            chunks.Add(new PdfDocument(outputStream.ToArray(), endIndex - i));
        }

        return chunks;
    }

    /// <summary>
    ///     Removes specific pages from a PDF document.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="pageNumbers">The page numbers to remove (1-based).</param>
    /// <returns>A new <see cref="PdfDocument" /> without the specified pages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document or pageNumbers is null.</exception>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var withoutCover = PdfSplitter.RemovePages(pdf, 1);
    /// await withoutCover.SaveAsync("no-cover.pdf");
    /// </code>
    /// </example>
    public static PdfDocument RemovePages(PdfDocument document, params int[] pageNumbers)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(pageNumbers);

        using var stream = new MemoryStream(document.ToBytes());
        using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

        var pagesToRemove = new HashSet<int>(pageNumbers);
        var pagesToKeep = Enumerable.Range(1, inputDocument.PageCount)
            .Where(p => !pagesToRemove.Contains(p))
            .ToArray();

        if (pagesToKeep.Length == 0)
            throw new InvalidOperationException("Cannot remove all pages from a document.");

        return ExtractPages(document, pagesToKeep);
    }
}