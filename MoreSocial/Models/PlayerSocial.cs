namespace MoreSocial.Models;

using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppPantheonPersist;

/*
 * Class to track:
    * friends
    * new and old guildies
    * the guild roster
 */
public class PlayerSocial
{
    private Il2CppReferenceArray<WhoListEntry>? _previousFriends = null; 
    
    private List<string> _newGuildies = new List<string>();
    public List<string> NewGuildies => _newGuildies;
    public List<string> TwoCyclesAgoPreviousGuildies = new List<string>();
    public List<string> OneCycleAgoPreviousGuildies = new List<string>();
    
    private List<string> _newRoster = new List<string>();
    public List<string> NewRoster => _newRoster;
    
    /*
     * When running `/guildroster`, it handles these messages
     * Since they come one at a time, we need to ensure we reset the list. We do this by using the alphabet
     */
    public void ProcessGuildRosterMessage(string name, string message, ChatChannelType channel)
    {
        if (string.IsNullOrWhiteSpace(message))
            return; 

        string charName = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

        if (_newRoster.Count > 0)
        {
            char firstCharofLastItemInOldRoster = char.ToLower(_newRoster[^1][0]);
            char firstCharOfNewName = char.ToLower(charName[0]);

            if (firstCharOfNewName < firstCharofLastItemInOldRoster)
                _newRoster.Clear();
        }

        _newRoster.Add(charName);
    }
    
    /*
     * When running `/who all guild`, it handles these messages
     * Since they come one at a time, we need to ensure we reset the list. We do this by using the alphabet
     */
    public void ProcessGuildListMessage(string name, string message, ChatChannelType channel)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        string charName = ExtractName(message);

        /*
         * One of the chat windows that DOES pop up is "Online Guild Members: " so we have to account for that here.
         */
        if (string.IsNullOrEmpty(charName))
            return;

        if (_newGuildies.Count > 0)
        {
            string lastGuildie = _newGuildies[^1];
            if (!string.IsNullOrEmpty(lastGuildie))
            {
                char firstCharofLastItemInOldRoster = char.ToLower(_newGuildies[^1][0]);
                char firstCharOfNewName = char.ToLower(charName[0]);

                if (firstCharOfNewName < firstCharofLastItemInOldRoster)
                    _newGuildies.Clear();
            }
        }

        _newGuildies.Add(charName);
    }

    /*
     * Func to find current friends list and compare against previous friends list. Any changes mean new offline or online friends
     */
    public (List<string> onlineFriends, List<string> offlineFriends)? CheckFriendStatus(Il2CppReferenceArray<WhoListEntry>? newFriends)
    {
        if (newFriends == null)
            return null;

        var onlineFriends = new List<string>();
        var offlineFriends = new List<string>();
        
        if (_previousFriends == null)
        {
            if (newFriends.Count > 0)
                _previousFriends = newFriends;
            return (onlineFriends, offlineFriends);
        }
        
        var previousFriendData = new Dictionary<string, string>(_previousFriends.Count);
        foreach (var previousFriend in _previousFriends)
            previousFriendData[CleanName(previousFriend.CharacterName)] = previousFriend.ZoneName;

        foreach (var friend in newFriends)
        {
            var name = CleanName(friend.CharacterName);
            if (!previousFriendData.TryGetValue(name, out var previousZone))
                continue; 

            var newZone = friend.ZoneName;
            if (previousZone == newZone)
                continue; 

            if (previousZone == "offline")
                onlineFriends.Add(name);
            
            if (newZone == "offline")
                offlineFriends.Add(name);
        }
        
        _previousFriends = newFriends;
        return (onlineFriends, offlineFriends);
    }
    
    /* ------------------------- HELPER Functions ------------------------- */ 
    
    /*
     * Names can come with a `(Grp)` tag at the end, so we remove this.
     */
    private string CleanName(string name)
    {
        return name.Replace(" (Grp)", "", StringComparison.OrdinalIgnoreCase).Trim();
    }
    
    /*
     * The guild messages do `[cleric 47] Azmor info info info`
     * This function extracts just `Azmor` from the above example
     */
    string ExtractName(string message)
    {
        int bracketIndex = message.IndexOf(']');
        if (bracketIndex == -1 || bracketIndex + 1 >= message.Length)
            return "";
        
        string afterBracket = message.Substring(bracketIndex + 1).Trim();

        string[] parts = afterBracket.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return "";

        return parts[0];
    }
}