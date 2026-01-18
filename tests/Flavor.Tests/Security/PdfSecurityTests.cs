using Flavor.Security;

namespace Flavor.Tests.Security;

public class PdfSecurityTests
{
    private static readonly byte[] SimplePdfBytes = CreateSimplePdf();

    private static byte[] CreateSimplePdf()
    {
        using var doc = new PdfSharpCore.Pdf.PdfDocument();
        doc.AddPage();
        using var stream = new MemoryStream();
        doc.Save(stream, false);
        return stream.ToArray();
    }

    [Fact]
    public void Encrypt_WithNullDocument_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => PdfSecurity.Encrypt(null!, "password");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Encrypt_WithPassword_ReturnsEncryptedDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var result = PdfSecurity.Encrypt(doc, "secret123");

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(1);
        result.ToBytes().Should().StartWith("%PDF"u8.ToArray());
    }

    [Fact]
    public void Encrypt_WithOptions_ReturnsEncryptedDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);
        var options = new PdfSecurityOptions
        {
            UserPassword = "view123",
            OwnerPassword = "edit456",
            AllowPrinting = true,
            AllowCopyContent = false
        };

        // Act
        var result = PdfSecurity.Encrypt(doc, options);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(1);
    }

    [Fact]
    public void Encrypt_WithBuilder_ReturnsEncryptedDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var result = PdfSecurity.Encrypt(doc, opt => opt
            .WithPassword("secret")
            .DenyPrinting()
            .DenyCopyContent());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Encrypt_WithNoPassword_ThrowsArgumentException()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var act = () => PdfSecurity.Encrypt(doc, new PdfSecurityOptions());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*password*");
    }

    [Fact]
    public void RemoveSecurity_ReturnsUnsecuredDocument()
    {
        // Arrange
        var doc = new PdfDocument(SimplePdfBytes, 1);

        // Act
        var result = PdfSecurity.RemoveSecurity(doc);

        // Assert
        result.Should().NotBeNull();
        result.PageCount.Should().Be(1);
    }

    [Fact]
    public void IsEncrypted_WithUnencryptedPdf_ReturnsFalse()
    {
        // Act
        var result = PdfSecurity.IsEncrypted(SimplePdfBytes);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void PdfSecurityOptions_PasswordProtected_SetsBothPasswords()
    {
        // Act
        var options = PdfSecurityOptions.PasswordProtected("test");

        // Assert
        options.UserPassword.Should().Be("test");
        options.OwnerPassword.Should().Be("test");
    }

    [Fact]
    public void PdfSecurityOptions_ReadOnly_DeniesModifications()
    {
        // Act
        var options = PdfSecurityOptions.ReadOnly("owner");

        // Assert
        options.OwnerPassword.Should().Be("owner");
        options.AllowModifyDocument.Should().BeFalse();
        options.AllowModifyAnnotations.Should().BeFalse();
        options.AllowAssembly.Should().BeFalse();
    }

    [Fact]
    public void PdfSecurityOptions_NoPrintOrCopy_DeniesThose()
    {
        // Act
        var options = PdfSecurityOptions.NoPrintOrCopy("owner");

        // Assert
        options.AllowPrinting.Should().BeFalse();
        options.AllowHighQualityPrint.Should().BeFalse();
        options.AllowCopyContent.Should().BeFalse();
    }
}