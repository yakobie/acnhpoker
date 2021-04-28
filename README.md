# ACNHPoker

   1. Spawns items for you on Animal Crossing New Horizons using [sys-botbase](https://github.com/olliz0r/sys-botbase) or [USB-Botbase](https://github.com/fishguy6564/USB-Botbase).
   2. Manipulate in-game critter spawn rate.
   3. Contorl turnip buying/selling price.
   4. Uncover your weather seed for [MeteoNook](https://wuffs.org/acnh/weather/)
   5. And more

## Pre Requisites
   1. A nintendo switch capable of running unsigned code
   2. [sys-botbase](https://github.com/olliz0r/sys-botbase) or [USB-Botbase](https://github.com/fishguy6564/USB-Botbase) installed on your switch.
   3. A copy of Animal Crossing™: New Horizons for the Nintendo Switch ?
   4. (Extra) A USB Type-C to USB-A cable or USB Type-C to Type-C cable to connect your switch to your PC if you are using [USB-Botbase](https://github.com/fishguy6564/USB-Botbase)

## Installation

Windows:

```sh
Just run the exe :)
```

OS X & Linux:

https://github.com/KingLycosa/acnhpoker_linux



## Usage example

https://youtu.be/-zztRgmtXig


## Original Release History

* 0.1
    * Work in progress
* 0.2
   * Added some quality of life updates
* 0.5
   * Added inventory viewer. No more ugly buttons :)
* 1.0
   * Added a lotta stuff, check the releases for a full update.

## My Release History

* R1
    * Added support for spawning in second character's inventory.
    * Added a basic variation spawning option.(experimental)
* R2
    * Added a recipe.csv file dedicated for DIY recipes.
    * Added a mode for DIY recipes spawning.
* R3
    * Fixed a typo related to image display.
* R4
    * Added keyboard shotcut for item spawning.
    * Fixed a few more image displaying bug.
* R5
    * A new item.csv file has been parsed and contain every item in game version 1.2.0.
    * Added a flowers.csv file dedicated for spawning perfect flowers with correct gene.
    * Added a mode for flower spawning.
* R6
    * Added support for spawning in third and forth character's inventory.
    * Added cheat support for character stamina.
    * Added support for displaying island name.
    * Added more keyboard shotcut for delete and copy.
    * Added mouse right click for copying item.
    * Added auto refresh setting.
    * Added basic save & loading function for entire inventory.(experimental)
    * Added a Hex option for more advance item spawning.
      * This release has a large portion of code rewritten. Auto merge with original version is no longer possible.
* R7
    * Added support for spawning in recycling bin and first character's Home storage.
    * Added support for wrapping items
    * Added support for controlling turnip buying/selling price.
    * Added support for changing first character's Reaction wheel
* R7.1
    * Update for Game version number 1.2.1
    * Rewrite Save & Load to support .nhi files.
* R8
    * Added support for USB-botbase.
    * Added support for sys-botbase v1.5.
    * Added more cheats for sys-botbase v1.5.
    * Added a variation.csv file dedicated for item variation.
    * Added support for spawning & displaying item variation.
    * Added information bubble for mouse hovering.
    * Added support for displaying flower gene in Numeric value.
* R8.1
    * Added support for displaying weather seed.
* R9
    * Update for Game version number 1.3.0
    * Added support for critter spawn rate manipulation.
    * Added new items and recipes of Game version number 1.3.0
    * Added fill remain option for only spawning in empty inventory.
* R9.1
    * Update for Game version number 1.3.1
    * Fixed sea creature spawn to inculde "Pearl"
* R9.2
    * Update for Game version number 1.4.0
    * Fixed broken copy function of R9.
    * Added gene selection boxes for flowers.
    * Added Wrap All Item and Unwrap All Item in right click menu.
    * Major bug fix for item spawn and image display.
* R9.3
	* Update for Game version number 1.4.1
* R9.4
	* Update for Game version number 1.4.2
	* Added checkbox to retain wrapped item name.
	* ANCHPoker will now work as a .nhi editor if you do not connect to your switch.
* R10
	* Added "Villager Booter" for villager editing.
* R10.1
	* Update for Game version number 1.5.0
	* Due to villager size change, villager now use .nhv2 files. All villager dump will now be .nhv2 files.
	* Item wrapping no longer works on non-wrappable item.
* R10.2
	* Fixed the display bug of "Moving out"
	* Added support for all 8 players inventories and houses. 
* R11
	* Redesign the csv files to support other languages.
	* Added a configuration page which allow user to enable address override.
	* Redo the friendship setting of villager booter.
* R11.1
	* Update for Game version number 1.6.0
	* Increase the max turnip price setting to 999,999,999 bells. 
	* Change to support new spawn rate
* R11.2
	* Added "Redd's wall display" and "Shop's wall display" for displaying wall-mount iem.
* R12
	* Added "Favorite" tab. Allow user to create their own favourite list.
	* Added "Map Dropper" for map editing.
	* Added "Map Regenerator" for treasure map hosting.
* R12.1
	* Added hotkey for map dropper. [Shift + Left Click] = Drop item & [Alt + Left Click] = Delete item
	* Fixed the bug of the regenerator that dropped item might incorrectly restocked.
	* Added a count down for the regenerator pause.
	* Added a basic Layer 2 support to the map dropper.
	* Redo the .nhi loading to ignore empty space.
	* Added .nhi support to the map dropper.
	* Added option to disable all sound.
* R12.2
	* Added last save and last load location. Saving/Loading should open your last active directory.
	* Fixed the favorite mtab to allow item witrh same ID.
	* Added logging to the regenerator.
	* Added extra option in the regenerator to limit the "ignore empty tiles" area.
	* Redo the "Map Dropper" with mini map control.
* R12.3
	* Added "Remove item..." option to bulk spawn item
	* Added Auto-save detection to try and avoid crashes due to write during auto save.
	* Added background color for the "Map Dropper"
	* Added a bit more code and a log file to try and catch exception.
* R13
	* Update for Game version number 1.7.0
* R13.1
	* All three moveout buttons can now cancel any invited villager
	* A "dodo.txt" file containing your current dodo code will now be generated in the /save folder if you start the regenerator with the gate already open.
	* Redo most of the popup message boxes
	* Added the option to type in the spawn coordinate in the bulk spawn menu
	* Fixed the "Timeout" error while the regenerator is running in the background.
* R13.2
	* Added "Clear Grid" button to map dropper to remove items within the 7x7 grid.
	* Added "Right Click" -> "Replace Item" to map dropper to replace items within the 7x7 grid.
	* Added "Ctrl + Left Click" to map dropper to select area corners. You can use "Ctrl + Left Click" on the big spawn button to fill the selected area.
	* Added "Right Click" -> "Copy Area" to map dropper to copy the selected area. You can then use "Ctrl + Left Click" to move the area around.
	* Added "Right Click" -> "Paste Area" to map dropper to paste the copied area.
	* Added up & down arrows to Flag and Hex Value in map dropper.
	* Added "Keep Village State" to Regenerator. It will reset the moveout state after they have been invited.
	* Fixed the program crash due to incomplete images download.
	* The "dodo.txt" file will now be constantly updated if you have Regenerator running.
	* Added "Dodo Helper" to Regenerator to restore island crash/disconnect.
      * Thanks to Rydog, Red, Coolguy for the teleportation code. And thanks to berichan's ACNHOrders implementation
* R13.3
	* Added "Shift + Left Click" and "Alt + Left Click" to inventory spawn. However, how they work may or may not be random.
	* Added the option to change the "dodo.txt" path.
	* Added the "Abort" button if you want to stop the dodo restore in mid-sequence.
	* Added more output during the restore sequence.
      * If it's stuck. Please report with the output included.
	* Removed the "no best friend" option and change the button sequence to work in both cases.
	* Added loading check to prevent regen when some dum-dum silently left with "-" button.
	* Added "idle emote" which will randomly do a emote after some idle time.
      * Do NOT use if you have discord bot dropping item.
	* Fixed the program crash due to incomplete villager images download(Again).
* R14
	* Update for Game version number 1.8.0
	* Added webhook support to let the dodo helper to post the dodo code to a discord channel.
      * You need a "webhook.txt" file in /save folder containing two lines : 1) The Webhook URL & 2) what the bot will say (You can ping someone here)
	* Added a "villager.txt" containing the villager list on the island.
      * Only updated when you press the button to START the regen.
* R14.1
	* Added option to speed up Orville's dialog.
	* Added handle to the dodo helper to try to finish Isabelle's announcement.
	* Added error message when Sys-botbase/socket 6000 is not responding.
	* Villager and turnip write will now also write to the save buffer.
	* Rework the forever untouched "Image Downloader".
	* Added the "Image Downloader" to the setting.
	* Yeeted some of the offensive error messages of regenerator.
	* Added the missing new recipes to .nhbs.
	* Other minor adjustments have been made to enhance the user experience.
* R14.2
	* Added a standalone mode for "Dodo Helper"
      * This will run "Dodo Helper" or "Twitch Drop" WITHOUT any regen.
      * This also means you can run things in many combinations. Like running "Dodo Helper" + "Twitch Drop" without regen.
	* Added "Twitch Drop" Twitch Chat item ordering. (You will need to set up a "twitch.json" file in the /save folder. See the "twitch[template].json" file for the info)
      * This uses the channel point reward system of Twitch and you can change the point needed & cooldown if you want.
	* Added an "Order Display" with a green screen showing the last 3 ordered items of "Twitch Drop".
      * You can use a chroma key to show the item order on your stream. Letting the viewer see what they have ordered.
	* Added a "visitor.txt" containing names of visitors currently on the island. (Only activate when regen is running)
      * Empty slot will show "[Empty]". And the last line will show the "Num of Visitor". Resize the control if you want to hide some of the information on stream.
	* Added "Map Zero" to Setting. You can override it when the update happens.
	* Added Max turnip price button.
      * You are setting all to "999,999,999 bells" anyways...
	* Added Moveout All buttons.
      * You are putting them all in "Irregular Move out" anyways...
	* As of game version 1.7.0, "Irregular Move out" no longer seems to work like intended. Changing the description of it.
	* Moved code into folders for better management. This will break the diff most likely... and screw some people over.
* R15
	* Update for Game version number 1.9.0
* R15.1
	* Fixed the villager inject game crash.
	* Fixed the turnip prices game crash.
	* Added option for "Twitch Drop" to use command instead of channel points.
      * See the new "twitch[template].json" file for settings.
* R15.2
	* Added "PocketCham's Timer". A standalone timer that you could once again chroma-key to your stream.
      * So that you do not need to use the in-game one risking it being dropped by the drop bot.
	* Reduce the regen rate of regenerator. It will now only regen when people fly-in, fly-out or the player go in & out of building.
	* Reduce the status output of "Dodo Helper".
	* Added keyboard control to "Dodo Helper". This allow you to control your character using your keyboard if needed.
      * WASD = Left stick control (8 directional movement)
      * IJKL = XYBA buttons (Hold and release)
      * R = "-" button (Minus button)
	  * Y = "+" button (Plus button)
	  * Q = ZL button
	  * O = ZR button
	  * However, do note that Sys-botbase cannot handle too many commands at once. So when the regen is running (The green bar is filling up) or item/villager inject is in process, the keyboard control will be wonky. Please wait patiently for other process to finish first before using the keyboard control.
	* Also to prevent triggering anti-virus software detecting keylogging behavior. Keyboard control only works when the "Dodo Helper"(Dutch Sailors) window is in focus.
	  * Which mean if you want to use the keyboard control, you must first use your mouse to click the form so that it is in focus.
* R16
	* Update for Game version number 1.10.0	  
## Contributing

1. Fork it (<https://github.com/KingLycosa/acnhpoker/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

## Donate

If you are considering donation please keep in mind that the donation doesn't get you anything other than my love. 
* https://ko-fi.com/myshilingstar
* https://www.paypal.me/myshilingstar

Please also consider supporting the original author of ACNHPoker @kinglycosa
* https://ko-fi.com/chad0001

## Meta

Distributed under the BSD 2 license. See ``LICENSE`` for more information.
