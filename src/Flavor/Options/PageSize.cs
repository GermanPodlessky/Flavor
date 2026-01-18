namespace Flavor.Options;

/// <summary>
///     Represents standard paper sizes for PDF generation.
/// </summary>
public readonly struct PageSize : IEquatable<PageSize>
{
    /// <summary>
    ///     Gets the width in inches.
    /// </summary>
    public double Width { get; }

    /// <summary>
    ///     Gets the height in inches.
    /// </summary>
    public double Height { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PageSize" /> struct with custom dimensions.
    /// </summary>
    /// <param name="width">Width in inches.</param>
    /// <param name="height">Height in inches.</param>
    public PageSize(double width, double height)
    {
        Width = width;
        Height = height;
    }

    #region Standard Sizes

    /// <summary>
    ///     US Letter size (8.5 x 11 inches).
    /// </summary>
    public static PageSize Letter => new(8.5, 11);

    /// <summary>
    ///     US Legal size (8.5 x 14 inches).
    /// </summary>
    public static PageSize Legal => new(8.5, 14);

    /// <summary>
    ///     US Tabloid size (11 x 17 inches).
    /// </summary>
    public static PageSize Tabloid => new(11, 17);

    /// <summary>
    ///     US Ledger size (17 x 11 inches).
    /// </summary>
    public static PageSize Ledger => new(17, 11);

    /// <summary>
    ///     ISO A0 size (33.1 x 46.8 inches).
    /// </summary>
    public static PageSize A0 => new(33.1, 46.8);

    /// <summary>
    ///     ISO A1 size (23.4 x 33.1 inches).
    /// </summary>
    public static PageSize A1 => new(23.4, 33.1);

    /// <summary>
    ///     ISO A2 size (16.54 x 23.4 inches).
    /// </summary>
    public static PageSize A2 => new(16.54, 23.4);

    /// <summary>
    ///     ISO A3 size (11.7 x 16.54 inches).
    /// </summary>
    public static PageSize A3 => new(11.7, 16.54);

    /// <summary>
    ///     ISO A4 size (8.27 x 11.7 inches). Most common international standard.
    /// </summary>
    public static PageSize A4 => new(8.27, 11.7);

    /// <summary>
    ///     ISO A5 size (5.83 x 8.27 inches).
    /// </summary>
    public static PageSize A5 => new(5.83, 8.27);

    /// <summary>
    ///     ISO A6 size (4.13 x 5.83 inches).
    /// </summary>
    public static PageSize A6 => new(4.13, 5.83);

    #endregion

    #region Factory Methods

    /// <summary>
    ///     Creates a page size from dimensions in millimeters.
    /// </summary>
    /// <param name="widthMm">Width in millimeters.</param>
    /// <param name="heightMm">Height in millimeters.</param>
    /// <returns>A new <see cref="PageSize" /> instance.</returns>
    public static PageSize FromMillimeters(double widthMm, double heightMm)
    {
        return new PageSize(widthMm / 25.4, heightMm / 25.4);
    }

    /// <summary>
    ///     Creates a page size from dimensions in centimeters.
    /// </summary>
    /// <param name="widthCm">Width in centimeters.</param>
    /// <param name="heightCm">Height in centimeters.</param>
    /// <returns>A new <see cref="PageSize" /> instance.</returns>
    public static PageSize FromCentimeters(double widthCm, double heightCm)
    {
        return new PageSize(widthCm / 2.54, heightCm / 2.54);
    }

    /// <summary>
    ///     Creates a page size from dimensions in pixels at 96 DPI.
    /// </summary>
    /// <param name="widthPx">Width in pixels.</param>
    /// <param name="heightPx">Height in pixels.</param>
    /// <param name="dpi">Dots per inch (default: 96).</param>
    /// <returns>A new <see cref="PageSize" /> instance.</returns>
    public static PageSize FromPixels(int widthPx, int heightPx, int dpi = 96)
    {
        return new PageSize((double)widthPx / dpi, (double)heightPx / dpi);
    }

    #endregion

    #region Conversions

    /// <summary>
    ///     Gets the width in millimeters.
    /// </summary>
    public double WidthMm => Width * 25.4;

    /// <summary>
    ///     Gets the height in millimeters.
    /// </summary>
    public double HeightMm => Height * 25.4;

    /// <summary>
    ///     Returns a landscape version of this page size (swaps width and height if needed).
    /// </summary>
    public PageSize Landscape => Width > Height ? this : new PageSize(Height, Width);

    /// <summary>
    ///     Returns a portrait version of this page size (swaps width and height if needed).
    /// </summary>
    public PageSize Portrait => Height > Width ? this : new PageSize(Height, Width);

    #endregion

    #region Equality

    /// <inheritdoc />
    public bool Equals(PageSize other)
    {
        return Math.Abs(Width - other.Width) < 0.001 && Math.Abs(Height - other.Height) < 0.001;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is PageSize other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    /// <summary>
    ///     Determines whether two page sizes are equal.
    /// </summary>
    public static bool operator ==(PageSize left, PageSize right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Determines whether two page sizes are not equal.
    /// </summary>
    public static bool operator !=(PageSize left, PageSize right)
    {
        return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Width}\" x {Height}\"";
    }
}