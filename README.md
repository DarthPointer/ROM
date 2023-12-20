# ROM
## Building from this repo
- Copy all the dependecny .dll's needed from the game into the `lib` folder.
- Create an env var `rwinstall` that points at the root folder of the game instance you want to send the mod to.
- Create a `RainWorld_dbg_Data` symlink to `RainWorld_Data` or create such a folder as a debug-oriented copy of `RainWorld_Data` (switching between debug and non-debug builds will requrie swapping `UnityPlayer.dll` `doorstop_config.ini` and `BepInEx/config/BepInEx.cfg` in that case).
- The project is set up to build and copy the plugin .dll with a "portable" .pdb that is ready to be used for debugging via the MVS Unity debugger.
