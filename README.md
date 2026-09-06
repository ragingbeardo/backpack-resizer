# Backpack Resizer
A config based mod that allows you to modify how large or small backpacks are.

Backpacks that come from mods will be automatically detected assuming they have been loaded before this mod. So let me know
if you find a backpack not being discovered by the mod itself and I will look into it.

## Features
- Change the width and height of backpacks that don't have a split inventory grid
- Save and Load presets for backpack sizes

## Presets Included
- Original Values - this preset will have the original found values for each backpack as it's found by the mod
- Better Backpacks - this sizes of this mod follow the sizes of the Better Backpacks mod with the inclusion of WTT Content Backport. 
  - content backport not required. any backpack in the preset but not found in the game will be ignored

## Limitations
Due to how backpacks are handled by the game itself, any backpack that has more than one "pocket" will not be touched by this mod.
A "pocket" meaning any backpack that has a split inventory in its container view.

## Installation
1. Extract the content of the archive
2. Copy the contents into your SPT installation folder

## Uninstall
1. Delete the RagingBeardo-BackpackResizer folder from your server mod directory

## Configuration
Config file will generate after the server is started. 

You can technically manage the config.jsonc directly, but 
it will be infinitely easier to use the UI based configuration via the following link:
https://127.0.0.1:6969/backpackresizer

You should definitely opt for the UI though. 

UI contains a dropdown for each backpack found by the mod grouped by resizable and not resizable.

Available configuration options:
- Mod Enabled: checkbox
- Debug Logging: checkbox
- Override Backpack Size Limits: checkbox
- Per backpack dropdown to customize width and height
- Save/Load presets for the per backpack values