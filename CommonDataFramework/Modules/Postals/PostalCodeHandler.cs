using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using LSPD_First_Response.Mod.API;
using Rage;

namespace CommonDataFramework.Modules.Postals;

/// <summary>
/// Handles the postal codes.
/// </summary>
internal static class PostalCodeHandler
{
    /// <summary>
    /// Gets the active postal code set.
    /// </summary>
    internal static PostalCodeSet PostalCodeSet { get; private set; } = null;

    /// <summary>
    /// The path for the postal codes xml.
    /// </summary>
    internal static string PostalXmlPath = "";

    /// <summary>
    /// Gets the postal code as a stringed number.
    /// </summary>
    /// <param name="position">The position for the code.</param>
    /// <returns>The code.</returns>
    public static string GetPostalCode(Vector3 position)
    {
        if (PostalCodeSet != null)
        {
            var code = GetNearestPostalCode(position);
            if (code != null)
            {
                return code.Code.Number;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the nearest postal code to a given position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>The nearest postal code.</returns>
    public static NearestPostalCode GetNearestPostalCode(Vector3 position)
    {
        var iter = PostalCodeSet.Codes.Where(code =>
            code.X > position.X - 500 &&
            code.X < position.X + 500 &&
            code.Y > position.Y - 500 &&
            code.Y < position.Y + 500);

        float nearestDistance = -1;
        Postal nearestCode = null;
        foreach (var entry in iter)
        {
            var codePosition = new Vector3(entry.X, entry.Y, 0);
            var codeDistance = codePosition.DistanceTo(position);
            if (nearestCode == null || codeDistance < nearestDistance)
            {
                nearestCode = entry;
                nearestDistance = codeDistance;
            }
        }

        if (nearestCode != null)
        {
            return new NearestPostalCode(nearestCode, nearestDistance);
        }

        return null;
    }

    private static void Load()
    {
        foreach (var filename in Directory.GetFiles(PostalXmlPath).Where(x => x.EndsWith(".xml")))
        {
            var postalCodeSet = PostalCodeSet.FromXML(filename);
            if (postalCodeSet != null)
            {
                PostalCodeSet = postalCodeSet;
            }
        }
    }
}
