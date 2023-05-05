# ROM
## Building from this repo
- Copy all the dependecny .dll's needed from the game into the `lib` folder.
- Create an env var `rwinstall` that points at the root folder of the game instance you want to debug the build plugin with.
- In that instance, create a folder link or a copy `RainWorld_dbg_Data` of the original `RainWolrd_Data`. Using an independent copy folder requires few extra steps.
  * You need a renamed .exe file to start the game - `RainWolrd_dbg.exe`.
  * You have to alter the data folder references in the `doorstop.config` in the game root so that BepInEx actually uses the dbg data folder to load plugins from.
- The project is set up to build and copy the plugin .dll with a "portable" .pdb that is ready to be used for debugging via the MVS Unity debugger.
