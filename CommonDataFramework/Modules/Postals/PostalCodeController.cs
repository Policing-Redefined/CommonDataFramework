using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonDataFramework.Modules.Postals;

/// <summary>
/// Handles the postal codes.
/// </summary>
internal static class PostalCodeController
{
    /// <summary>
    /// Gets the active postal code set.
    /// </summary>
    internal static PostalCodeSet PostalCodeSet { get; private set; } = null;

    /// <summary>
    /// Gets the active postal code set.
    /// </summary>
    public static PostalCodeSet ActivePostalCodeSet { get; private set; }

    /// <summary>
    /// Gets a list of the installed postal code sets.
    /// </summary>
    public static List<PostalCodeSet> PostalCodeSets { get; private set; } = [];

    /// <summary>
    /// The path for the postal codes xml.
    /// </summary>
    internal const string PostalXmlPath = DefaultPluginFolder + "/PostalXMLs";

    /// <summary>
    /// Gets the postal code as a stringed number.
    /// </summary>
    /// <param name="position">The position for the code.</param>
    /// <returns>The code.</returns>
    public static string GetPostalCode(Vector3 position)
    {
        if (PostalCodeSet == null) return "";
        NearestPostalCode code = GetNearestPostalCode(position);
        return code != null ? code.Code.Number : string.Empty;
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

    public static string[] GetAllPostalCodeSetNames() => PostalCodeSets.Select(i => i.Name).ToArray();

    public static void SetActivePostalCodeSet(string name)
    {
        ActivePostalCodeSet = PostalCodeSets.FirstOrDefault(i => i.Name == name);
    }

    internal static void Load()
    {
        foreach (string filename in Directory.GetFiles(PostalXmlPath).Where(x => x.EndsWith(".xml")))
        {
            PostalCodeSet postalCodeSet = PostalCodeSet.FromXML(filename);
            if (postalCodeSet != null)
            {
                PostalCodeSets.Add(postalCodeSet);
            }
        }

        LogDebug($"Loaded {PostalCodeSets.Count} set(s) of postal codes.");
        foreach (PostalCodeSet entry in PostalCodeSets)
        {
            LogDebug($"... {entry.Name} containing {entry.Codes.Count} entries.");
        }

        if (PostalCodeSets.Count != 0)
        {
            ActivePostalCodeSet = PostalCodeSets.FirstOrDefault(x => x.Name == CDFSettings.PostalsSet);
            ActivePostalCodeSet ??= PostalCodeSets[0];
            return;
        }
        
        Game.DisplaySubtitle("~b~[PR]~s~ Could ~r~not~s~ load any postal codes.");
    }
}
