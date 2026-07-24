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

---

## What a Good Project AGENT.md Should Add (checklist, not filled in here)

This base file only covers external knowledge. Each individual project should have its own AGENT.md (or a
project-specific section) that adds, at minimum:

- **Build & verify command (s)** — the exact command to compile/check the project, and any known false-positive errors
  to ignore (e.g. a PostBuild step that fails while the game is running).
- **Architecture overview** — the 4–6 sentence mental model of how the major pieces fit together.
- **Conventions** — naming, where shared/common logic lives, how config values are declared and bound.
- **Ground rules agreed with the user** — e.g. "refactors are behavior-neutral by default," anything the user has
  explicitly corrected the agent on before.
- **Known fragilities** — things that look fine but silently break (string-based type comparisons, manual event
  unsubscription, etc.).

Keep it factual and current — remove notes once they're no longer true, don't let it grow into a changelog.

# 4  <:Showdown:1437119940933976306>  THE SHOWDOWN - MAIN EVENT

# 📜 MATCH RULES

> ## 🔹 **Core Rules**
> - Format: **2 vs 2** / Single Elimination
> - Match type: **Best of 3** (First to 2)
> - Each round lasts **5 minutes**
> - Both teams' players set their best time; team **average time** determines the result
> - A **round win ** gains your team **1 point** for the whole match
> - First team with **2 points** wins the match
> ## 🔹 **Special Rules (Ties)**
> ### __ Exact Time Tie – Picked Map__
> - If both teams have **identical average times**  
>   → The **OPPONENT** of the team that **PICKED** the map wins the round cause the Map Picker is expected to win. If
    they don't win, they lose the tie.
> ### __Exact Time Tie – Randomized Map__
> - If the map was **randomly selected**, and the result is a perfect tie  
>   → The winner is the team with the **better Qualifier Average Time**  
>   (the team that originally held **initiative**).
> - However if the **Qualifier Average Time** is also **tied**, we replay the match 🆕

# 🧮 MATCH PROCEDURE

> ## 🔸 **Core Procedure**
> 1. **Pre Match**
>   1. Players join (Code in https://discord.com/channels/1127321762686836798/1137826678542966875 !! DO NOT JOIN UNTIL
       PINGED !!)
>   1. Players link to their team
> 1. **Match **
>   1. Draft Phase I
>   1. Round I
>   1. Round II
>   1. Intermission
>      1. Winner determined?
>         1. Yes <:Yes:761601587986432072> (2:0 or 0:2)
>             1. Match Ends
>         1. No  <:No:761601587843432539> (1:1)
>             1. Draft Phase II
>             1. Round III
> 1. **Post Match**
>   1. Players leave the lobby
>   1. (optional) join us on stage for an interview! 🎙️
>
> ## 🔸 **Procedure Details & Sequence**
> ### __**Draft Phase I**__
> - Team with the better **Qualifier Average Time** gets **initiative**
> - Teams have **consumables** that they use over the entire match:
>   - **2 bans** 🟥
>   - **1 pick**  🟩
> - Draft pattern: **ABAB**
>
> ### __Draft Phase I__ - Sequence
> 1. **Initiative team starts** (pick or ban)
> 2. Teams alternate
> 3. If one team **picks**, the other team must **also pick**
> 4. Once **two maps are picked**, the first two rounds begin
>    ⚠️ **IMPORTANT NOTE** ⚠️
> 5. **90 seconds** per decision
>    - **Timer runs out → the other team gets their action** (this also happens when you use **!pass**)
>    - If the other team has no consumables → action becomes **randomized**
> 6. repeat from 2. until **2** maps are picked. you **cannot __not__** use pick in **Draft I**. That's in the nature of
     this system.
>
> ## __**Rounds I & II**__
> - Played on the **two picked maps** in pick-order
> - Round Length: **5 minutes**
> - Winner team is determined by **better average time**
>
> ## __**Draft Phase II**__
> - Triggered when the first two rounds are **tied** (1:1)
> - Previously **banned** maps **__re-enter__** the pool
> - Previously **played** maps are **__removed __**→ **5 maps remain**
> - 🔃 **Initiative switches** to the other team
> ### Draft Phase II Sequence
> 1. The new Initiative team starts the **draft**
> 2. Teams alternate
> 2. If no more consumables remain → map is **randomized** by [Showdown]
>
> ## __**Round III (Tiebreaker)**__
> - Played on the map selected in **Draft Phase II**
> - **Tie rules** apply here as well
>   - Picked map → picker loses
>   - Random map → better Qualifier wins ⬅️ https://discord.com/channels/1127321762686836798/1134182342278258789 ⬅️
      ➡️ https://discord.com/channels/1127321762686836798/1134183381077340161 ➡️