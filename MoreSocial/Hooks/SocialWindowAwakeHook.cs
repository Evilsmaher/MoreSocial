namespace MoreSocial.Hooks;

using HarmonyLib;
using Il2Cpp;


[HarmonyPatch(typeof(UISocialWindow))]
public class SocialWindowHooks
{
    /*
     * A reference to `SocialWindow` is needed to call `/who all guild` and `/guildroster`
     */
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UISocialWindow.Awake))]
    private static void Postfix(UISocialWindow __instance)
    {
        if (__instance.name == "Panel_Social")
            Global.SocialWindow = __instance;
    }

    /*
     * When we request who list, we want to make sure that the list of all guildies are not listed
     * We must know when this occurs, so we set some booleans
     */
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UISocialWindow.RequestWhoList), typeof(string), typeof(bool) )]
    private static bool Prefix(string command, bool force)
    {
        if (Global.RequestGuildRoster)
        {
            Global.ShowGuildRosterChat = false;
            Global.RequestGuildRoster = false;
        }

        return true;
    }
}