### Build (on Windows)
Follow this guide: https://rainworldmodding.miraheze.org/wiki/BepInPlugins  
The basic steps are also listed below.

Download and install Visual Studio (Community): https://visualstudio.microsoft.com/

Make sure to select the ".NET desktop development" workload, and at the individual components tab, select ".NET Framework 4.8 development tools".

Clone this repository, or download as ZIP and unzip. Open the project (.sln).

Add references to files as described in the guide above:
- [BeastMaster.dll](https://github.com/NoirCatto/BeastMaster)
- [SplitScreen Co-op.dll](https://github.com/henpemaz/RemixMods)
- [SBCameraScroll.dll](https://github.com/SchuhBaum/SBCameraScroll)
- BepInEx.dll
- HOOKS-Assembly-CSharp.dll
- Mono.Cecil.dll
- MonoMod.RuntimeDetour.dll
- MonoMod.Utils.dll
- PUBLIC-Assembly-CSharp.dll
- UnityEngine.dll
- UnityEngine.CoreModule.dll
- UnityEngine.InputLegacyModule.dll

The files can be found in the Rain World folder. You can copy and store these, for example, in a folder "references" next to the folder containing this source code.

You can build the code using the shortcut CTRL + SHIFT + B.


### Tools
[dnSpy](https://github.com/dnSpy/dnSpy) can be used to decompile PUBLIC-Assembly-CSharp.dll and find game functions.

[Here's](https://www.youtube.com/watch?v=1ckUvTtZaVY) a handy tutorial by [\[Alpha\]-0mega-](https://www.youtube.com/@0megaD).


### Contributing
Follow the "standard fork -> clone -> edit -> pull request workflow": https://github.com/firstcontributions/first-contributions
