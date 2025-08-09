namespace MoreSocial.Hooks;

using HarmonyLib;
using Il2Cpp;

/*
 * When we request guildies or friends, we don't want to show the SocialWindowPanel
 * We do boolean checks to see if our mod is showing it or not
 */
[HarmonyPatch(typeof(UIWindowPanel), nameof(UIWindowPanel.Show))]
[HarmonyPatch(new Type[] { typeof(bool) })]
public class UIPanelHooks
{
    private static bool Prefix(UIWindowPanel __instance, bool fadeIn)
    {
        if (__instance.name == "Panel_Social")
        {
            if (Global.RequestFriendsList)
            {
                Global.RequestFriendsList = false;
                return false;
            }

            if (Global.RequestGuildiesList)
            {
                Global.RequestGuildiesList = false;
                return false;
            }
        }
        return true;
    }
}