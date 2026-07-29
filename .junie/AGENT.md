# Zeepkist Modding — AGENT.md

This file gives the agent background knowledge about the Zeepkist modding ecosystem and its dependencies. It
intentionally contains no project-specific architecture — that belongs in a separate, project-local AGENT.md (see note
at the bottom).

## Language Policy

Everything in this project is done in English (code, comments, commit messages). The user communicates with the agent in
German.

## Zeepkist & Dependencies — Reference Links

### Zeepkist (the game)

- Decompiled game assemblies: `C:\Users\TEute\Desktop\Projects\Zeepkist Mods\_Zeepkist` — fill in per machine, not
  universal.

### ZeepSDK (main modding SDK)

- GitHub: https://github.com/donderjoekel/ZeepSDK
- API docs: https://donderjoekel.github.io/ZeepSDK/api/index.html
- GUI toolkit (Imui, Zeepkist-flavored fork): https://github.com/Thundernerd/Imui/tree/feat/zeepkist
- Toolbar Drawer article: https://donderjoekel.github.io/ZeepSDK/articles/zeep-toolbar-drawer.html
- GUI Drawer article: https://donderjoekel.github.io/ZeepSDK/articles/zeep-gui-drawer.html

### BepInEx (mod loader/patcher framework)

- GitHub: https://github.com/BepInEx/BepInEx
- Docs: https://docs.bepinex.dev
- Plugin creation walkthrough & Harmony usage: linked from the docs index above

### HarmonyLib (runtime method patching, ships with BepInEx)

- GitHub: https://github.com/pardeike/Harmony
- Docs: https://harmony.pardeike.net
- Utilities (AccessTools, Traverse, etc.): https://harmony.pardeike.net/articles/utilities.html

### Other Zeepkist modders (reference for idiomatic SDK/API usage)

Useful when unsure how to use a ZeepSDK feature — check whether an established modder already solved it:

- metalted — prolific modder, many mods under the `com.metalted.zeepkist.*` namespace: https://github.com/metalted
- donderjoekel — ZeepSDK author, also has other Zeepkist projects: https://github.com/donderjoekel
- Kilandor (Jason Booth) — several mods, some forked from metalted's work: https://github.com/Kilandor
- Zeepkist community org (backend/API/tooling, less relevant for in-game GUI): https://github.com/zeepkist
- Search GitHub for "<name> zeepkist" for anyone not listed here — don't guess URLs.

Workflow when stuck on SDK usage: web-search `<modder> zeepkist github <feature>`, open the matching repo, and compare
against the approach you're about to take. A pattern that shows up in more than one of these repos is a stronger signal
of "the" idiomatic way than a single example.