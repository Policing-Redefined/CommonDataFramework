using LSPD_First_Response.Engine.Scripting.Entities;

namespace CommonDataFramework.Utility.Extensions;

internal static class PersonaExtensions
{
    internal static Persona Clone(this Persona persona) =>
        new(persona.Forename, persona.Surname, persona.Gender, persona.Birthday)
        {
            AdvisoryText = persona.AdvisoryText,
            Citations = persona.Citations,
            ELicenseState = persona.ELicenseState,
            RuntimeInfo = persona.RuntimeInfo,
            TimesStopped = persona.TimesStopped,
            Wanted = persona.Wanted,
            WantedInformation =
            {
                CanBeSearchedInManhunt = persona.WantedInformation.CanBeSearchedInManhunt,
                CarriesEvidence = persona.WantedInformation.CarriesEvidence,
                EscapedInVehicle = persona.WantedInformation.EscapedInVehicle,
                GetawayCar = persona.WantedInformation.GetawayCar,
                IsWantedInManhunt = persona.WantedInformation.IsWantedInManhunt,
                LastSeenPosition = persona.WantedInformation.LastSeenPosition,
                LastSeenUtc = persona.WantedInformation.LastSeenUtc
            }
        };
}