namespace MoreSocial.Hooks;

using HarmonyLib;
using Il2Cpp;
using Il2CppPantheonPersist;
using MelonLoader;

[HarmonyPatch(typeof(UIChatWindows))]
public class ChatMessageHooks
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIChatWindows.PassMessage), typeof(string), typeof(string), typeof(ChatChannelType))]
    /*
     * When we call `/who all guild` and `/guildroster`, it passes those messages through the chat window.
     * Instead, we hide those specific messages from the user.
     * I also pass those messages to the ModMain instance so proper handling of character names can be collected
     */
    private static bool Prefix(UIChatWindows __instance, string name, ref string message, ChatChannelType channel)
    {
        if (Global.ShowGuildRosterChat && Global.ShowGuildiesListChat && Global.ShowNotInGuildChat)
            return true;

        if (!Global.ShowGuildRosterChat)
        {
            if (channel == ChatChannelType.Info)
            {
                if (message.Contains("Officer") || message.Contains("Member") || message.Contains("Leader") || message.Contains("roster"))
                {
                    ModMain.Instance?.HandleGuildRosterMessage(name, message, channel);
                    return false;
                }
            }
        }

        if (!Global.ShowGuildiesListChat)
        {
            if (channel == ChatChannelType.WhoListResults)
            {
                ModMain.Instance?.HandleGuildListMessage(name, message, channel);
                return false;
            }
        }
        
        if (!Global.ShowNotInGuildChat)
        {
            if (message.Contains("Online guild members:"))
            {
                Global.IsInGuild = true;
                return false;
            }
            else if (message.Contains("You're not in a guild!"))
            {
                Global.IsInGuild = false;
                return false;
            }
            else if (channel == ChatChannelType.WhoListResults)
            {
                return false;
            }
        }
        return true;
    }
}