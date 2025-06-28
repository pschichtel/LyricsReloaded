Ask any questions you might have here:
https://getmusicbee.com/forum/index.php?topic=36952.0 

Installation
============
- Inside MusicBee, go to 'Edit->Edit Preferences->Plugins->Add Plugin'.
  Then locate and select the 'mb_LyricsReloaded.dll' file.


Optional
========
- Download and extract the 'providers' zip file into the "...\MusicBee\AppData\mb_LyricsReloaded\providers" folder.
  Inside MusicBee, go to "Help > Support > Open AppData" if you cannot locate the above folder.

- In addition to the built-in providers ... Genius, Musixmatch, Deezer ...
  the contents of this zip file - called YAML configuration files - will appear as additional sources in
  'Edit->Edit Preferences->Tags (2)->auto-tagging->lyrics' , where you can reorder their priority.


Advanced
========
- Want your retrieved lyrics to indicate which source they came from?
  Then open the 'Settings.ini' file located in the plugin's AppData folder.
  Change the 'enabled' value of the <showSource> setting from 'false' to 'true'.

- By default, the source will appear at the bottom of the lyrics when the above setting is enabled.
  To have it appear at the top instead, change the 'position' attribute from 'bottom' to 'top'.

- Follow this link to learn more on how to make custom YAML files:
  https://getmusicbee.com/forum/index.php?topic=25406.msg145769#msg145769
  https://getmusicbee.com/forum/index.php?topic=36952.msg204745#msg204745