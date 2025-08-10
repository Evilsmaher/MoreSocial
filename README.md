# MoreSocial

MoreSocial 

## TODO

- Check roster and make sure the changes from guild list are actually logging off and not leaving the guild or joining the guild
- Track the previous user tab
  - When requesting Guild or friends, it clicks on their tab if the window is open (conversely, does nothing if  window is closed). Instead of changing this on the user every 30 sec, we should put it back to where it was :) 
    - May even hook on switch and if the bool `RequestGuildiesList` == true, don't tab swap 
- I need to register the user logging out and reset some of the variables. 
- I need to ensure that if I am running a guild check....the character is actually in a guild or they get a horrid message. 

## Issues

- Guildies leaving or joining the guild will get the in(appropriate) message: joining = logging in; leaving = logging off;