namespace MoreSocial;

using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

public static class SocialFinder
{
    /*
     * Finds friends from the `SocialWindow` since the methods are attached to `UISocialWindow`
     * Friends are returned as `WhoListEntry` objects;
     */
    public static Il2CppReferenceArray<WhoListEntry>? FindFriends(UISocialWindow socialWindow)
    {
        socialWindow.RequestFriendsList();
        Il2CppReferenceArray<WhoListEntry>? friends = socialWindow.friendsListEntries;

        if (friends == null || friends.Length == 0)
        {
            return new Il2CppReferenceArray<WhoListEntry>(0); // return an empty array instead of nothing
        }

        return friends;
    }
    
    /*
     * Finds the online Guild List from the `UISocialWindow`.
     * Five seconds are used to give time to get guild members before `callback()` is handled.
        * (When this is called, the ChatMessageHooks` are called then I have to process each one. So, before I continue, I give it time to handle this.
     */
    public static System.Collections.IEnumerator FindGuildListCoroutine(UISocialWindow socialWindow, System.Action callback)
    {
        socialWindow.RequestWhoList("/who all guild", true);
        
        yield return new WaitForSeconds(5f);
        
        callback();
    }
    
    /*
     * Finds the entire Guild Roster from the `UISocialWindow`.
     * Five seconds are used to give time to get guild members before `callback()` is handled.
     * (When this is called, the ChatMessageHooks` are called then I have to process each one. So, before I continue, I give it time to handle this.
     */
    public static System.Collections.IEnumerator FindGuildiesCoroutine(UISocialWindow socialWindow, System.Action callback)
    {
        socialWindow.RequestWhoList("/guildroster", true);

        yield return new WaitForSeconds(5f);

        callback();
    }
}