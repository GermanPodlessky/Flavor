namespace Flavor.Options;

/// <summary>
///     Represents page margins for PDF generation.
/// </summary>
public readonly struct Margins : IEquatable<Margins>
{
    /// <summary>
    ///     Gets the top margin in inches.
    /// </summary>
    public double Top { get; }

    /// <summary>
    ///     Gets the right margin in inches.
    /// </summary>
    public double Right { get; }

    /// <summary>
    ///     Gets the bottom margin in inches.
    /// </summary>
    public double Bottom { get; }

    /// <summary>
    ///     Gets the left margin in inches.
    /// </summary>
    public double Left { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Margins" /> struct with uniform margins.
    /// </summary>
    /// <param name="all">The margin value for all sides in inches.</param>
    public Margins(double all) : this(all, all, all, all)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Margins" /> struct with vertical and horizontal margins.
    /// </summary>
    /// <param name="vertical">The margin value for top and bottom in inches.</param>
    /// <param name="horizontal">The margin value for left and right in inches.</param>
    public Margins(double vertical, double horizontal) : this(vertical, horizontal, vertical, horizontal)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Margins" /> struct with individual margins.
    /// </summary>
    /// <param name="top">Top margin in inches.</param>
    /// <param name="right">Right margin in inches.</param>
    /// <param name="bottom">Bottom margin in inches.</param>
    /// <param name="left">Left margin in inches.</param>
    public Margins(double top, double right, double bottom, double left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    #region Presets

    /// <summary>
    ///     No margins (0 on all sides).
    /// </summary>
    public static Margins None => new(0);

    /// <summary>
    ///     Normal margins (1 inch on all sides).
    /// </summary>
    public static Margins Normal => new(1);

    /// <summary>
    ///     Narrow margins (0.5 inch on all sides).
    /// </summary>
    public static Margins Narrow => new(0.5);

    /// <summary>
    ///     Wide margins (1.5 inch on all sides).
    /// </summary>
    public static Margins Wide => new(1.5);

    /// <summary>
    ///     Moderate margins (1 inch top/bottom, 0.75 inch left/right).
    /// </summary>
    public static Margins Moderate => new(1, 0.75);

    #endregion

    #region Factory Methods

    /// <summary>
    ///     Creates margins from values in millimeters.
    /// </summary>
    /// <param name="top">Top margin in millimeters.</param>
    /// <param name="right">Right margin in millimeters.</param>
    /// <param name="bottom">Bottom margin in millimeters.</param>
    /// <param name="left">Left margin in millimeters.</param>
    /// <returns>A new <see cref="Margins" /> instance.</returns>
    public static Margins FromMillimeters(double top, double right, double bottom, double left)
    {
        return new Margins(top / 25.4, right / 25.4, bottom / 25.4, left / 25.4);
    }

    /// <summary>
    ///     Creates uniform margins from a value in millimeters.
    /// </summary>
    /// <param name="all">The margin value for all sides in millimeters.</param>
    /// <returns>A new <see cref="Margins" /> instance.</returns>
    public static Margins FromMillimeters(double all)
    {
        return new Margins(all / 25.4);
    }

    /// <summary>
    ///     Creates margins from values in centimeters.
    /// </summary>
    /// <param name="top">Top margin in centimeters.</param>
    /// <param name="right">Right margin in centimeters.</param>
    /// <param name="bottom">Bottom margin in centimeters.</param>
    /// <param name="left">Left margin in centimeters.</param>
    /// <returns>A new <see cref="Margins" /> instance.</returns>
    public static Margins FromCentimeters(double top, double right, double bottom, double left)
    {
        return new Margins(top / 2.54, right / 2.54, bottom / 2.54, left / 2.54);
    }

    /// <summary>
    ///     Creates uniform margins from a value in centimeters.
    /// </summary>
    /// <param name="all">The margin value for all sides in centimeters.</param>
    /// <returns>A new <see cref="Margins" /> instance.</returns>
    public static Margins FromCentimeters(double all)
    {
        return new Margins(all / 2.54);
    }

    /// <summary>
    ///     Creates margins from values in pixels at specified DPI.
    /// </summary>
    /// <param name="top">Top margin in pixels.</param>
    /// <param name="right">Right margin in pixels.</param>
    /// <param name="bottom">Bottom margin in pixels.</param>
    /// <param name="left">Left margin in pixels.</param>
    /// <param name="dpi">Dots per inch (default: 96).</param>
    /// <returns>A new <see cref="Margins" /> instance.</returns>
    public static Margins FromPixels(int top, int right, int bottom, int left, int dpi = 96)
    {
        return new Margins((double)top / dpi, (double)right / dpi, (double)bottom / dpi, (double)left / dpi);
    }

    #endregion

    #region Equality

    /// <inheritdoc />
    public bool Equals(Margins other)
    {
        return Math.Abs(Top - other.Top) < 0.001 &&
               Math.Abs(Right - other.Right) < 0.001 &&
               Math.Abs(Bottom - other.Bottom) < 0.001 &&
               Math.Abs(Left - other.Left) < 0.001;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Margins other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Top, Right, Bottom, Left);
    }

    /// <summary>
    ///     Determines whether two margins are equal.
    /// </summary>
    public static bool operator ==(Margins left, Margins right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Determines whether two margins are not equal.
    /// </summary>
    public static bool operator !=(Margins left, Margins right)
    {
        return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Top: {Top}\", Right: {Right}\", Bottom: {Bottom}\", Left: {Left}\"";
    }
}