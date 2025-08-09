# MoreSocial

MoreSocial 

## TODO

- Check roster and make sure the changes from guild list are actually logging off and not leaving the guild or joining the guild
- Track the previous user tab
  - When requesting Guild or friends, it clicks on their tab if the window is open (conversely, does nothing if  window is closed). Instead of changing this on the user every 30 sec, we should put it back to where it was :) 
    - May even hook on switch and if the bool `RequestGuildiesList` == true, don't tab swap 

## Issues

- Two messages are logged if the person logging off is both a guildie and a friend
- When zone, it could show that someone logged off:
  - I currently use a longer timer (30sec) to try and avoid repeat timers that would definitely register as "zoning". (If it was every 5secs, people would definitely show as "Offline" when zoning)
- Guildies leaving or joining the guild will get the in(appropriate) message: joining = logging in; leaving = logging off;