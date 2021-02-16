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
	* Added Auto-save detection to try and avoid crashes due to write durning saveing.
	* Added background color for the "Map Dropper"
	* Added a bit more code and a log file to try and cath exception.
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
	* The "dodo.txt" file will now be constantly update if you have Regenerator running.
	* Added "Dodo Helper" to Regenerator to restore island crash/disconnect.
      * Thanks to Rydog, Red, Coolguy for the teleportation code. And thanks to berichan's ACNHOrders implementation.
    
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
