namespace MoreSocial;

using System.Collections;
using Il2Cpp;
using MelonLoader;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppPantheonPersist;
using MoreSocial.Models;
using UnityEngine;

public class ModMain : MelonMod
{
    public const string ModVersion = "1.0.0";
    private PlayerSocial _player = new();
    
    public static ModMain Instance { get; private set; }
    public ModMain()
    {
        Instance = this;
        MelonLogger.Msg("[MoreSocial] ModMain instance created");
    }
    
    public override void OnInitializeMelon()
    {
        MelonCoroutines.Start(PeriodicCheckFriendsAndGuildStatus());
    }
    
    /*
     * This func runs on a 30 sec timer to check friends and guildies
     * It will show logins and logoffs to the user's first chat window (which cannot be changed by VR so we should always have access to it)
     */
    private System.Collections.IEnumerator PeriodicCheckFriendsAndGuildStatus()
    {
        while (true)
        {
            if (Global.LoggedIn)
            {
                Il2CppReferenceArray<WhoListEntry>? friends = FindFriends();
                var friendStatus = _player.CheckFriendStatus(friends); 
                if (friendStatus.HasValue)
                {
                    var (onlineFriends, offlineFriends) = friendStatus.Value;
                    foreach (var friend in onlineFriends)
                    {
                        string loggedInMessage = $"{friend} has logged in.";
                        if (Global.FirstChatTab != null)
                            Global.FirstChatTab.AddMessage("", loggedInMessage, ChatChannelType.LevelUp);
                        else 
                            MelonLogger.Msg($"Chat is null -- on");
                    }

                    foreach (var friend in offlineFriends)
                    {
                        string loggedInMessage = $"{friend} has logged off.";
                        if (Global.FirstChatTab != null)
                            Global.FirstChatTab.AddMessage("", loggedInMessage, ChatChannelType.LevelUp);
                        else 
                            MelonLogger.Msg($"Chat is null - off");
                    }
                }
                
                FindGuildies();
            }
            yield return new WaitForSeconds(30f);
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
    private void FindGuildies()
    {
        if (Global.SocialWindow != null)
        {
            Global.RequestGuildiesList = true;
            Global.RequestGuildRoster = true;
            Global.ShowGuildRosterChat = false;
            Global.ShowGuildiesListChat = false;

            MelonCoroutines.Start(StatusCheckOnlineAndOfflineGuildies());
        }
    }
    
    private IEnumerator StatusCheckOnlineAndOfflineGuildies()
    {
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

        if (_player.PreviousGuildies.Count < 1 && curGuildies.Count > 1)
            _player.PreviousGuildies = new List<string>(curGuildies);
        
        else
        {
            var loggedInList = curGuildies.Except(_player.PreviousGuildies).ToList();
            var loggedOffList = _player.PreviousGuildies.Except(curGuildies).ToList();

            _player.PreviousGuildies = new List<string>(curGuildies);

            foreach (var loggedInChar in loggedInList)
            {
                string loggedInMessage = $"{loggedInChar} has logged in.";
                if (Global.FirstChatTab != null)
                    Global.FirstChatTab.AddMessage("", loggedInMessage, ChatChannelType.LevelUp);
                else 
                    MelonLogger.Msg($"Chat is null -- on");
            }

            foreach (var loggedOffChar in loggedOffList)
            {
                string loggedInMessage = $"{loggedOffChar} has logged off.";
                if (Global.FirstChatTab != null)
                    Global.FirstChatTab.AddMessage("", loggedInMessage, ChatChannelType.LevelUp);
                else 
                    MelonLogger.Msg($"Chat is null - off");
            }
        }
    }
}