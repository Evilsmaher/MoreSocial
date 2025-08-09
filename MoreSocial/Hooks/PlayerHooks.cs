namespace MoreSocial.Hooks;

using HarmonyLib;
using Il2Cpp;

[HarmonyPatch(typeof(EntityPlayerGameObject), nameof(EntityPlayerGameObject.NetworkStart))]
public class PlayerNetworkStart
{
    private static void Postfix(EntityPlayerGameObject __instance)
    {
        if (__instance.NetworkId.Value == EntityPlayerGameObject.LocalPlayerId.Value)
            Global.LoggedIn = true;
    }
}