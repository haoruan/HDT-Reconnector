# HDT-Reconnector
HDT-Reconnector is a plugin for [Hearthstone Deck Tracker](https://github.com/HearthSim/Hearthstone-Deck-Tracker) that allows you to quickly disconnect and reconnect to the game. It can be used in any game mode, but especially for the battleground to skip the battle animation.  
 
HDT-Reconnector controls Hearthstone's TCP connections to disconnect the game, so it's faster than the firewall rule solution, and it's more suitable for you if you can't control the firewall.

![HDT-Reconnector](images/sample.png?raw=true)

# Installation
1. Download the latest plugin from [Releases page](https://github.com/haoruan/HDT-Reconnector/releases)
2. Unzip and put the dll and localization folders in `%AppData%/HearthstoneDeckTracker/Plugins`,  you can click the following button in the Hearthstone Deck Tracker options menu to open the folder: `Options -> Tracker -> Plugins -> Plugins Folder`
3. Restart HDT with administrator privileges
4. Enable the plugin in `Options -> Tracker -> Plugins`.

# Usage

1. Plugin is automatically enabled after loading the plugin, to quickly enable/disable the plugin, you can check/uncheck the menu item: `Plugins -> Reconnector`
2. In game, when you need to disconnect the game, just click the Reconnect button.
3. Enjoy it!

# Settings
1. Move/Resize button - Unlock the ui in `Options -> Overlay -> General -> Unlock Overlay`. Sometimes HDT doesn't show the unlock ui properly and you're unable to move the button, in this case just lock and unlock it.

# Localization 
- English
- Chinese

# Known issues
After several disconnect and reconnect, the game may crash. It's not a plugin issue, but a Hearthstone's bug.

# License
Released under the Apache 2.0 license.
