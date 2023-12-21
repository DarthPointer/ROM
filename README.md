# ROM
## Building from this repo
- Setup a folder link from `RainWrold_Data/StreamingAssets/Mods/ROM` to `Source/ROM/ROM/ROM` to "install" the mod and have the plugin `dll` and `pdb` "updated" each time you build the project.
- You can do the same for the `ROMTestObjects` if you need it.
- Both projects have debug symbols set to portable and copy their pdbs together with dlls, thus can be debugged given you convert your game install and use an IDE plugin.
