namespace MoreSocial;

using Il2Cpp;

public static class Global
{
    public static bool LoggedIn = false;

    public static bool IsInGuild = false;
    public static bool RequestIsInGuild = false;
    public static bool ShowNotInGuildChat = true;
    
    public static bool RequestFriendsList = false;
    
    public static bool RequestGuildiesList = false;
    public static bool ShowGuildiesListChat = true;
    
    public static bool RequestGuildRoster = false;
    public static bool ShowGuildRosterChat = true;
    
    public static UISocialWindow? SocialWindow;
}