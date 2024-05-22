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
internal static class PostalCodeController
{
    /// <summary>
    /// The path for the postal codes xml.
    /// </summary>
    private const string PostalXmlPath = DefaultPluginFolder + "/Postals.xml";
    
    /// <summary>
    /// Gets the active postal code set.
    /// </summary>
    internal static PostalCodeSet PostalCodeSet { get; private set; }

    /// <summary>
    /// Gets the postal code as a stringed number.
    /// </summary>
    /// <param name="position">The position for the code.</param>
    /// <returns>The code.</returns>
    public static string GetPostalCode(Vector3 position)
    {
        if (PostalCodeSet != null)
        {
            NearestPostalCode code = GetNearestPostalCode(position);
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
        IEnumerable<Postal> iter = PostalCodeSet.Codes.Where(code =>
            code.X > position.X - 500 &&
            code.X < position.X + 500 &&
            code.Y > position.Y - 500 &&
            code.Y < position.Y + 500);

        float nearestDistance = -1;
        Postal nearestCode = null;
        foreach (Postal entry in iter)
        {
            Vector3 codePosition = new(entry.X, entry.Y, 0);
            float codeDistance = codePosition.DistanceTo(position);
            if (nearestCode != null && codeDistance > nearestDistance) continue;
            nearestCode = entry;
            nearestDistance = codeDistance;
        }

        return nearestCode != null ? new NearestPostalCode(nearestCode, nearestDistance) : null;
    }

    internal static void Load()
    {
        PostalCodeSet postalCodeSet = PostalCodeSet.FromXML(PostalXmlPath);
        LogDebug($"PostalCodeSet: Null: {postalCodeSet == null}.");
        if (postalCodeSet != null)
        {
            PostalCodeSet = postalCodeSet;
        }
    }
}
