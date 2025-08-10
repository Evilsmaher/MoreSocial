namespace MoreSocial;

using System.Collections;
using Il2Cpp;
using MelonLoader;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppPantheonPersist;
using Models;
using UnityEngine;

public class ModMain : MelonMod
{
    public const string ModVersion = "1.0.0";
    private PlayerSocial _player = new();
    
    public static ModMain Instance { get; private set; }
    public ModMain()
    {
        Instance = this;
    }
    
    public override void OnInitializeMelon()
    {
        MelonCoroutines.Start(PeriodicCheckFriendsAndGuildStatus());
    }
    
    /*
     * This func runs on a 6 (seemed smoother than 10) sec timer to check friends and guildies 
     * It will show logins and logoffs to the user's first chat window (which cannot be changed by VR so we should always have access to it)
     */
    private System.Collections.IEnumerator PeriodicCheckFriendsAndGuildStatus()
    {
        while (true)
        {
            if (Global.LoggedIn)
            {
                List<string> friendOnline = new();
                List<string> friendOffline = new();
                List<string> guildieOnline = new();
                List<string> guildieOffline = new();
                
                
                Il2CppReferenceArray<WhoListEntry>? friends = FindFriends();
                if (friends != null) {
                    var friendStatus = _player.CheckFriendStatus(friends);
                    if (friendStatus.HasValue)
                    {
                        var (onlineFriends, offlineFriends) = friendStatus.Value;
                        friendOnline.AddRange(onlineFriends);
                        friendOffline.AddRange(offlineFriends);
                    }
                }
                
                CheckIfInGuild();
                yield return new WaitForSeconds(1f);
                
                if (Global.IsInGuild)
                    yield return MelonCoroutines.Start(FindGuildies(guildieOnline, guildieOffline));

                if (UIChatWindows.Instance != null && UIChatWindows.Instance.mainWindow != null &&
                    UIChatWindows.Instance.mainWindow.chats != null)
                {
                    foreach (UIChatWindow.ChatAndTab chatAndTab in UIChatWindows.Instance.mainWindow.chats)
                    {
                        UIChat chat = chatAndTab.Chat;

                        if (chat != null)
                        {
                            foreach (string guildieLogin in guildieOnline)
                                chat.AddMessage("", $"{guildieLogin} has logged in.", ChatChannelType.Guild,
                                    CombatLogDirectionalFilter.All, CombatLogFilter.Both, CombatLogPlayerFilter.All,
                                    false,
                                    false);

                            foreach (string guildieLogout in guildieOffline)
                                chat.AddMessage("", $"{guildieLogout} has logged out.", ChatChannelType.Guild,
                                    CombatLogDirectionalFilter.All, CombatLogFilter.Both, CombatLogPlayerFilter.All,
                                    false,
                                    false);

                            friendOnline = friendOnline.Except(guildieOnline).ToList();
                            foreach (string friendLogin in friendOnline)
                                chat.AddMessage("", $"{friendLogin} has logged in.", ChatChannelType.ReplyWhisper,
                                    CombatLogDirectionalFilter.All, CombatLogFilter.Both, CombatLogPlayerFilter.All,
                                    false,
                                    false);

                            friendOffline = friendOffline.Except(guildieOffline).ToList();
                            foreach (string friendLogout in friendOffline)
                                chat.AddMessage("", $"{friendLogout} has logged out.", ChatChannelType.ReplyWhisper,
                                    CombatLogDirectionalFilter.All, CombatLogFilter.Both, CombatLogPlayerFilter.All,
                                    false,
                                    false);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(6f);
        }
    }
    
    /*
     * Intermediary for the `PlayerSocial` class to handle Guild Roster messages
     */
    public void HandleGuildRosterMessage(string name, string message, ChatChannelType channel)
    {
        _player.ProcessGuildRosterMessage(name, message, channel);
    }
    
    /*
     * Intermediary for the `PlayerSocial` class to handle Guild List messages
     */
    public void HandleGuildListMessage(string name, string message, ChatChannelType channel)
    {
        _player.ProcessGuildListMessage(name, message, channel);
    }

    /*
     * Some methods execute Guild functions; if the character is not in a guild, it will continually spam "You're not in a guild!". Instead, we check to see if the member is in the guild first using a quick `/who all guild`
     */
    public void CheckIfInGuild()
    {
        Global.RequestIsInGuild = true;
        Global.ShowNotInGuildChat = false;

        if (Global.SocialWindow != null)
        {
            MelonCoroutines.Start(SocialFinder.FindGuildListCoroutine(Global.SocialWindow, () =>
            {
                Global.ShowNotInGuildChat = false;
            }));
        }
    }

    /*
     * Func that calls the `FindFriends` from `SocialFinder` to find the friends from the `SocialWindow`
     * Error handles and sets booleans for ensuring windows don't show and `/who` panels don't appear
     */
    private Il2CppReferenceArray<WhoListEntry>? FindFriends()
    {
        Global.RequestFriendsList = true;

        if (Global.SocialWindow != null)
        {
            Il2CppReferenceArray<WhoListEntry>? friends = SocialFinder.FindFriends(Global.SocialWindow);
            return friends;
        }

        Global.RequestFriendsList = false;

        return null;
    }
    
    /*
     * Func that calls the `FindGuildies` from `SocialFinder` to find the friends from the `SocialWindow`
     * Error handles and sets booleans for ensuring windows don't show and `/who` panels don't appear
     *
     * I use 2 Coroutines to asynchronously determine when the roster vs. list are done. (Moderately async -- I use a hard-coded timer; described in SocialFinder.cs)
     * When they are both complete, I can compare them (TODO in README.md) and determine if someone left guild vs. actually logged off
     */
    private IEnumerator FindGuildies(List<string> guildieOnline, List<string> guildieOffline)
    {
        if (Global.SocialWindow != null)
        {
            Global.RequestGuildiesList = true;
            Global.RequestGuildRoster = true;
            Global.ShowGuildRosterChat = false;
            Global.ShowGuildiesListChat = false;

            bool guildRosterDone = false;
            bool guildListDone = false;

            if (Global.SocialWindow != null)
            {
                MelonCoroutines.Start(SocialFinder.FindGuildiesCoroutine(Global.SocialWindow, () =>
                {
                    Global.ShowGuildRosterChat = true;
                    guildRosterDone = true;
                }));

                MelonCoroutines.Start(SocialFinder.FindGuildListCoroutine(Global.SocialWindow, () =>
                {
                    Global.ShowGuildiesListChat = true;
                    guildListDone = true;
                }));
            }

            yield return new WaitUntil((Il2CppSystem.Func<bool>)(() => guildRosterDone && guildListDone));

            List<string> curRoster = _player.NewRoster;
            List<string> curGuildies = _player.NewGuildies;

            if (_player.TwoCyclesAgoPreviousGuildies.Count < 1 && curGuildies.Count > 1)
                _player.TwoCyclesAgoPreviousGuildies = new List<string>(curGuildies);
            
            else if (_player.OneCycleAgoPreviousGuildies.Count < 1 && curGuildies.Count > 1)
                _player.OneCycleAgoPreviousGuildies = new List<string>(curGuildies);
            
            else
            {
                var loggedInList = curGuildies.Except(_player.OneCycleAgoPreviousGuildies).ToList();
                /*
                 * For each zone, the `Player@(Owner)` object is destroyed and then remade in the new zone
                 * The assumption is that someone will zone within X secs (in this case, 6). If they did not zone, they will have logged off
                 */
                var loggedOffList = _player.TwoCyclesAgoPreviousGuildies
                    .Except(_player.OneCycleAgoPreviousGuildies)
                    .Except(curGuildies)
                    .ToList();

                _player.TwoCyclesAgoPreviousGuildies = new List<string>(_player.OneCycleAgoPreviousGuildies);
                _player.OneCycleAgoPreviousGuildies = new List<string>(curGuildies);

                guildieOnline.AddRange(loggedInList);
                guildieOffline.AddRange(loggedOffList);
            }
        }
    }
}