BuildingTools
=====
*From the Depths experience enhancements.*


Compatible FtD version : **2.4.4+**

GitHub repository: [https://github.com/Why7090/BuildingTools](https://github.com/Why7090/BuildingTools)


Features: (more will be added)
-----
- **3D Hologram Projector**  
	Render a 3D object from an .obj file and associated material & textures

- **Block Search Tool**  
	Search among all blocks in your inventory

- **Block Counter**  
	View the amount of each block on your vehicle

- **Calculator**  
	Advanced scientific calculator accessible with a single keystroke

- **Armor Visualizer**  
	View the armor strength of your vehicle in real time

- **CapsLock Restore**  
	Stop accidentally writing cAPITALIZED tEXT (only works on Windows)

- **Other enhancements**  
	Unlimited prefab size
	Orbit Camera zoom out (max distance changed from 50m to 300m)

Installation
=====

With Automatic Update
-----
You only have to install this once, and FtdModManager will automatically update when a new version is available

1. Install [FtdModManager](https://github.com/Why7090/FtdModManager)
1. Open FtD
1. Press `Ctrl`+`M`
1. Paste `https://raw.githubusercontent.com/Why7090/BuildingTools/master/modmanifest.json` into *Mod Manifest URL* field
1. Click *Install Mod*
1. Restart FtD

Manual
-----
Don't forget to return here often to check new updates !

1. Download [the archive of the latest version](https://github.com/Why7090/BuildingTools/archive/master.zip)
1. Delete any existing installation of BuildingTools in `Documents\From The Depths\Mods\` directory
1. Extract the content into the `Documents\From The Depths\Mods` directory
1. Make sure that `BuildingTools.dll` is directly under `Documents\From The Depths\Mods\BuildingTools-master`

Feature Details
=====

3D Hologram Projector
-----

Can be found in Decoration tab, above the Hologram Projector

You can find WoWS ship models on these websites
- [https://gamemodels3d.com/games/worldofwarships/](https://gamemodels3d.com/games/worldofwarships/) (Requires paid account)
- [https://sketchfab.com/max_romash/collections](https://sketchfab.com/max_romash/collections) (Free)

The model path must end with .obj and textures and material file must be located in a **sub directory named `textures`** or **in the same directory** as the model file
**Do not forget to unzip everything! The model files must not be zipped.**

Settings:

[![](https://i.imgur.com/OYwLA1dl.jpg)](https://i.imgur.com/OYwLA1d.jpg) [![](https://i.imgur.com/eCpfUVPl.jpg)](https://i.imgur.com/eCpfUVP.jpg)

Available shaders: Hologram (Glass), Transparent, Solid:

[![](https://i.imgur.com/UpMmzjHm.jpg)](https://i.imgur.com/UpMmzjH.jpg) [![](https://i.imgur.com/XzBF9m0m.jpg)](https://i.imgur.com/XzBF9m0.jpg) [![](https://i.imgur.com/2x3sBhGm.jpg)](https://i.imgur.com/2x3sBhG.jpg)

Building with Hologram:

[![](https://i.imgur.com/hhDehMdl.jpg)](https://i.imgur.com/hhDehMd.jpg)


Block Search Tool
-----

Press `` ` `` (Back Quote key, usually above `Tab`) in Build Mode to activate
Type your search query in the text box and the results will be displayed in real time
Click on an item to close the window and select it
When the query is empty, it will display the items you previously selected
Press `` ` `` again to close the window manually

Typing screenshot:

[![](https://i.imgur.com/iLmL9ZGl.gif)](https://i.imgur.com/iLmL9ZG.gif)


Calculator
-----

Press `Insert` (usually on the right of `Backspace`) to toggle window
Type expression in the text box, then press <Enter> to evaluate
Use `↑` `↓` to navigate previous expressions
Type `help()` to view a list of functions and variables
You can copy the results in the output text area

Example usage of Calculator:

[![](https://i.imgur.com/sTeMcoel.png)](https://i.imgur.com/sTeMcoe.png)


Armor Visualizer
-----

<ins>SAVE ALL CHANGES BEFORE USING THIS TOOL, YOU HAVE TO RESTART FTD TO DEACTIVATE THIS VIEW</ins>
Press `Home` (usually on the right of `Insert`) when focusing on a Vehicle / Sub-Object to activate the tool
From weakest to strongest : Blue, Green, Yellow, Orange, Red
Use `W` `A` `S` `D` `Space` `LeftAlt` to move the camera
Hold `LeftShift` for faster movement, `LeftControl` for slower movement
Hold mouse left / right button to zoom (change the field of view)
You may notice strange effects when you get too close to the object, it's a problem of the ray-box intersection algorithm used.
Press `Home` again to restart the game (Unfortunately it's way too complicated to run this parallel to FtD, so the script simply deletes everything in the scene and activates itself)

Armor Visualizer in action:

[![](https://i.imgur.com/c6CAhDtm.png)](https://i.imgur.com/c6CAhDt.png) [![](https://i.imgur.com/Zr7DwQbm.png)](https://i.imgur.com/Zr7DwQb.png)

[Changelog](https://github.com/Why7090/BuildingTools/blob/master/changelog.txt)
===
