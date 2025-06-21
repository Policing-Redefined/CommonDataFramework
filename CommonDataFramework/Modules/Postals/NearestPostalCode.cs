namespace CommonDataFramework.Modules.Postals;

/// <summary>
/// Basically a struct for storing the nearest postal code.
/// </summary>
public class NearestPostalCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NearestPostalCode"/> class.
    /// </summary>
    /// <param name="code">The postal code.</param>
    /// <param name="distance">Distance to whatever Vector3 was supplied.</param>
    public NearestPostalCode(Postal code, float distance)
    {
        Code = code;
        Distance = distance;
    }

    /// <summary>
    /// Gets a caption for the ResText.
    /// </summary>
    public string Caption => $"~w~{Code.Number} (~g~{Distance:.02}m~w~)";

    /// <summary>
    /// Gets the postal code.
    /// </summary>
    public Postal Code { get; private set; }

    /// <summary>
    /// Gets the distance.
    /// </summary>
    public float Distance { get; private set; }
}
