using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;

namespace Flavor.Security;

/// <summary>
///     Provides functionality for PDF security operations including encryption and password protection.
/// </summary>
public static class PdfSecurity
{
    /// <summary>
    ///     Encrypts a PDF document with password protection.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="password">The password required to open the document.</param>
    /// <returns>A new encrypted <see cref="PdfDocument" />.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var secured = PdfSecurity.Encrypt(pdf, "secret123");
    /// await secured.SaveAsync("secured.pdf");
    /// </code>
    /// </example>
    public static PdfDocument Encrypt(PdfDocument document, string password)
    {
        return Encrypt(document, PdfSecurityOptions.PasswordProtected(password));
    }

    /// <summary>
    ///     Encrypts a PDF document with custom security options.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="options">The security options.</param>
    /// <returns>A new encrypted <see cref="PdfDocument" />.</returns>
    /// <example>
    ///     <code>
    /// var pdf = await converter.ConvertHtmlAsync(html);
    /// var secured = PdfSecurity.Encrypt(pdf, new PdfSecurityOptions
    /// {
    ///     UserPassword = "viewPassword",
    ///     OwnerPassword = "editPassword",
    ///     AllowPrinting = true,
    ///     AllowCopyContent = false
    /// });
    /// await secured.SaveAsync("secured.pdf");
    /// </code>
    /// </example>
    public static PdfDocument Encrypt(PdfDocument document, PdfSecurityOptions options)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(options.UserPassword) && string.IsNullOrEmpty(options.OwnerPassword))
            throw new ArgumentException("At least one password (user or owner) must be specified.", nameof(options));

        using var stream = new MemoryStream(document.ToBytes());
        using var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

        var securitySettings = pdfDocument.SecuritySettings;

        if (!string.IsNullOrEmpty(options.UserPassword))
            securitySettings.UserPassword = options.UserPassword;

        if (!string.IsNullOrEmpty(options.OwnerPassword))
            securitySettings.OwnerPassword = options.OwnerPassword;

        // Set permissions
        securitySettings.PermitPrint = options.AllowPrinting;
        securitySettings.PermitExtractContent = options.AllowCopyContent;
        securitySettings.PermitModifyDocument = options.AllowModifyDocument;
        securitySettings.PermitAnnotations = options.AllowModifyAnnotations;
        securitySettings.PermitFormsFill = options.AllowFillForms;
        securitySettings.PermitAccessibilityExtractContent = options.AllowAccessibility;
        securitySettings.PermitAssembleDocument = options.AllowAssembly;
        securitySettings.PermitFullQualityPrint = options.AllowHighQualityPrint;

        using var outputStream = new MemoryStream();
        pdfDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), pdfDocument.PageCount);
    }

    /// <summary>
    ///     Encrypts a PDF document using a fluent builder.
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <param name="configure">An action to configure security options.</param>
    /// <returns>A new encrypted <see cref="PdfDocument" />.</returns>
    /// <example>
    ///     <code>
    /// var secured = PdfSecurity.Encrypt(pdf, opt => opt
    ///     .WithUserPassword("view123")
    ///     .WithOwnerPassword("edit456")
    ///     .DenyPrinting()
    ///     .DenyCopyContent());
    /// </code>
    /// </example>
    public static PdfDocument Encrypt(PdfDocument document, Action<PdfSecurityOptionsBuilder> configure)
    {
        var builder = new PdfSecurityOptionsBuilder();
        configure?.Invoke(builder);
        return Encrypt(document, builder.Build());
    }

    /// <summary>
    ///     Decrypts a password-protected PDF document.
    /// </summary>
    /// <param name="encryptedBytes">The encrypted PDF bytes.</param>
    /// <param name="password">The password to decrypt the document.</param>
    /// <returns>A decrypted <see cref="PdfDocument" />.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the password is incorrect.</exception>
    /// <example>
    ///     <code>
    /// var bytes = await File.ReadAllBytesAsync("secured.pdf");
    /// var pdf = PdfSecurity.Decrypt(bytes, "secret123");
    /// </code>
    /// </example>
    public static PdfDocument Decrypt(byte[] encryptedBytes, string password)
    {
        ArgumentNullException.ThrowIfNull(encryptedBytes);

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        try
        {
            using var stream = new MemoryStream(encryptedBytes);
            using var pdfDocument = PdfReader.Open(stream, password, PdfDocumentOpenMode.Modify);

            // Remove security
            pdfDocument.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.None;

            using var outputStream = new MemoryStream();
            pdfDocument.Save(outputStream, false);

            return new PdfDocument(outputStream.ToArray(), pdfDocument.PageCount);
        }
        catch (PdfReaderException ex) when (ex.Message.Contains("password"))
        {
            throw new UnauthorizedAccessException("Invalid password.", ex);
        }
    }

    /// <summary>
    ///     Decrypts a password-protected PDF file.
    /// </summary>
    /// <param name="filePath">The path to the encrypted PDF file.</param>
    /// <param name="password">The password to decrypt the document.</param>
    /// <returns>A decrypted <see cref="PdfDocument" />.</returns>
    public static PdfDocument DecryptFile(string filePath, string password)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);

        var bytes = File.ReadAllBytes(filePath);
        return Decrypt(bytes, password);
    }

    /// <summary>
    ///     Decrypts a password-protected PDF file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the encrypted PDF file.</param>
    /// <param name="password">The password to decrypt the document.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A decrypted <see cref="PdfDocument" />.</returns>
    public static async Task<PdfDocument> DecryptFileAsync(string filePath, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);

        var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return Decrypt(bytes, password);
    }

    /// <summary>
    ///     Checks if a PDF document is encrypted.
    /// </summary>
    /// <param name="pdfBytes">The PDF bytes to check.</param>
    /// <returns>True if the document is encrypted; otherwise, false.</returns>
    public static bool IsEncrypted(byte[] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        try
        {
            using var stream = new MemoryStream(pdfBytes);
            using var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.InformationOnly);
            return false; // If we can open without password, it's not encrypted
        }
        catch (PdfReaderException)
        {
            return true; // Encrypted or corrupted
        }
    }

    /// <summary>
    ///     Removes all security from a PDF document (requires owner password).
    /// </summary>
    /// <param name="document">The source PDF document.</param>
    /// <returns>An unsecured <see cref="PdfDocument" />.</returns>
    public static PdfDocument RemoveSecurity(PdfDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        using var stream = new MemoryStream(document.ToBytes());
        using var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

        pdfDocument.SecuritySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.None;

        using var outputStream = new MemoryStream();
        pdfDocument.Save(outputStream, false);

        return new PdfDocument(outputStream.ToArray(), pdfDocument.PageCount);
    }
}

/// <summary>
///     Builder for configuring PDF security options fluently.
/// </summary>
public class PdfSecurityOptionsBuilder
{
    private readonly PdfSecurityOptions _options = new();

    /// <summary>Sets the user password (required to open).</summary>
    public PdfSecurityOptionsBuilder WithUserPassword(string password)
    {
        _options.UserPassword = password;
        return this;
    }

    /// <summary>Sets the owner password (required to change permissions).</summary>
    public PdfSecurityOptionsBuilder WithOwnerPassword(string password)
    {
        _options.OwnerPassword = password;
        return this;
    }

    /// <summary>Sets both user and owner passwords to the same value.</summary>
    public PdfSecurityOptionsBuilder WithPassword(string password)
    {
        _options.UserPassword = password;
        _options.OwnerPassword = password;
        return this;
    }

    /// <summary>Allows printing.</summary>
    public PdfSecurityOptionsBuilder AllowPrinting()
    {
        _options.AllowPrinting = true;
        _options.AllowHighQualityPrint = true;
        return this;
    }

    /// <summary>Denies printing.</summary>
    public PdfSecurityOptionsBuilder DenyPrinting()
    {
        _options.AllowPrinting = false;
        _options.AllowHighQualityPrint = false;
        return this;
    }

    /// <summary>Allows copying content.</summary>
    public PdfSecurityOptionsBuilder AllowCopyContent()
    {
        _options.AllowCopyContent = true;
        return this;
    }

    /// <summary>Denies copying content.</summary>
    public PdfSecurityOptionsBuilder DenyCopyContent()
    {
        _options.AllowCopyContent = false;
        return this;
    }

    /// <summary>Allows modifying the document.</summary>
    public PdfSecurityOptionsBuilder AllowModifications()
    {
        _options.AllowModifyDocument = true;
        _options.AllowModifyAnnotations = true;
        _options.AllowAssembly = true;
        return this;
    }

    /// <summary>Denies all modifications.</summary>
    public PdfSecurityOptionsBuilder DenyModifications()
    {
        _options.AllowModifyDocument = false;
        _options.AllowModifyAnnotations = false;
        _options.AllowAssembly = false;
        return this;
    }

    /// <summary>Allows form filling.</summary>
    public PdfSecurityOptionsBuilder AllowFormFilling()
    {
        _options.AllowFillForms = true;
        return this;
    }

    /// <summary>Denies form filling.</summary>
    public PdfSecurityOptionsBuilder DenyFormFilling()
    {
        _options.AllowFillForms = false;
        return this;
    }

    /// <summary>Makes the document read-only (view only).</summary>
    public PdfSecurityOptionsBuilder ReadOnly()
    {
        _options.AllowModifyDocument = false;
        _options.AllowModifyAnnotations = false;
        _options.AllowAssembly = false;
        _options.AllowFillForms = false;
        return this;
    }

    /// <summary>Builds the security options.</summary>
    public PdfSecurityOptions Build()
    {
        return _options;
    }
}