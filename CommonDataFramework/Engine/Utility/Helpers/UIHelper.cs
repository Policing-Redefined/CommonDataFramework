namespace CommonDataFramework.Engine.Utility.Helpers;

internal static class UIHelper
{
    internal static void DisplayNotification(string subtitle, string text, bool isWarning = false)
    {
        string txtDict = isWarning ? "helicopterhud" : "3dtextures";
        string txtName = isWarning ? "targetlost" : "mpgroundlogo_cops";
        Game.DisplayNotification(txtDict, txtName, "Policing Redefined", subtitle, text);
    }
    
    internal static void DisplayErrorNotification()
    {
        DisplayNotification("~r~An error occured", "Please go to your GTA 5 main directory and provide us your ~b~RagePluginHook.log~s~ file via discord.", true);
    }
}