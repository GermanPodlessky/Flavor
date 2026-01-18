namespace Flavor.Security;

/// <summary>
///     Options for PDF security and encryption.
/// </summary>
public class PdfSecurityOptions
{
    /// <summary>
    ///     Gets or sets the user password (required to open the document).
    /// </summary>
    public string? UserPassword { get; set; }

    /// <summary>
    ///     Gets or sets the owner password (required to change permissions).
    /// </summary>
    public string? OwnerPassword { get; set; }

    /// <summary>
    ///     Gets or sets whether printing is allowed. Default is true.
    /// </summary>
    public bool AllowPrinting { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether copying content is allowed. Default is true.
    /// </summary>
    public bool AllowCopyContent { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether modifying the document is allowed. Default is true.
    /// </summary>
    public bool AllowModifyDocument { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether modifying annotations is allowed. Default is true.
    /// </summary>
    public bool AllowModifyAnnotations { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether form filling is allowed. Default is true.
    /// </summary>
    public bool AllowFillForms { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether accessibility extraction is allowed. Default is true.
    /// </summary>
    public bool AllowAccessibility { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether document assembly is allowed. Default is true.
    /// </summary>
    public bool AllowAssembly { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether high-quality printing is allowed. Default is true.
    /// </summary>
    public bool AllowHighQualityPrint { get; set; } = true;

    /// <summary>
    ///     Creates options that only require a password to open.
    /// </summary>
    /// <param name="password">The password required to open the document.</param>
    public static PdfSecurityOptions PasswordProtected(string password)
    {
        return new PdfSecurityOptions
        {
            UserPassword = password,
            OwnerPassword = password
        };
    }

    /// <summary>
    ///     Creates options that prevent all modifications.
    /// </summary>
    /// <param name="ownerPassword">The owner password.</param>
    public static PdfSecurityOptions ReadOnly(string ownerPassword)
    {
        return new PdfSecurityOptions
        {
            OwnerPassword = ownerPassword,
            AllowModifyDocument = false,
            AllowModifyAnnotations = false,
            AllowAssembly = false
        };
    }

    /// <summary>
    ///     Creates options that prevent printing and copying.
    /// </summary>
    /// <param name="ownerPassword">The owner password.</param>
    public static PdfSecurityOptions NoPrintOrCopy(string ownerPassword)
    {
        return new PdfSecurityOptions
        {
            OwnerPassword = ownerPassword,
            AllowPrinting = false,
            AllowHighQualityPrint = false,
            AllowCopyContent = false
        };
    }
}