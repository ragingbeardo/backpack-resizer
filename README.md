# Backpack Resizer
A config based mod that allows you to modify how large or small backpacks are.

## Limitations
Due to how backpacks are handled by the game itself, any backpack that has more than one "pocket" will not be touched by this mod.
A "pocket" meaning any backpack that has a split inventory in its container view.

## Installation
1. Extract the content of the archive
2. Copy the contents into your SPT installation folder

## Configuration
Config file will generate after the server is started. 

You can technically manage the config.jsonc directly, but 
it will be infinitely easier to use the UI based configuration via the following link:
https://127.0.0.1:6969/backpackresizer

UI contains a dropdown for each backpack found by the mod sorted by resizable and not resizable.

Available options:
- Mod Enabled: checkbox
- Debug Logging: checkbox
- Override Backpack Size Limits: checkbox
- Per backpack dropdown to customize width and height
- Save/Load presets for the per backpack values