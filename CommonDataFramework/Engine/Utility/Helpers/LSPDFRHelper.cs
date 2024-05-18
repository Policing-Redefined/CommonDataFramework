using LSPD_First_Response;

namespace CommonDataFramework.Engine.Utility.Helpers;

internal static class LSPDFRHelper
{
    internal static Gender GetPedGender(Ped ped) => ped.IsMale ? Gender.Male : Gender.Female;
}