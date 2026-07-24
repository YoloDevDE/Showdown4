# Decompiled Zeepkist

C:\Users\TEute\Desktop\Projects\Zeepkist Mods\_Zeepkist

# ZeepSDK URL

## Github

https://github.com/donderjoekel/ZeepSDK

https://donderjoekel.github.io/ZeepSDK/api/index.html

### GUI

https://github.com/Thundernerd/Imui/tree/feat/zeepkist

This one is for the top toolbar https://donderjoekel.github.io/ZeepSDK/articles/zeep-toolbar-drawer.html

https://donderjoekel.github.io/ZeepSDK/articles/zeep-gui-drawer.html

# Meine Mods

C:\Users\TEute\Desktop\Projects\Zeepkist Mods

# Language Policy

Everything in this project is done in English. However, the user communicates with the agent in German.

# Zeepkist Modding Knowledge

Detailed documentation can be found in [README.md](C:\Users\TEute\Desktop\Projects\Zeepkist%20Modding%20Docs\README.md).

## Critical API Signatures (ZeepSDK)

* **RacingApi.CrossedFinishLine**: `delegate void CrossedFinishLineDelegate(float time)`
* **RacingApi.PassedCheckpoint**: `delegate void PassedCheckpointDelegate(float time)`
* **RacingApi.Crashed**: `delegate void CrashedDelegate(CrashReason reason)`
* **RacingApi.LevelLoaded**: `delegate void LevelLoadedDelegate()`
* **RacingApi.RoundStarted**: `delegate void RoundStartedDelegate()`
* **RacingApi.RoundEnded**: `delegate void RoundEndedDelegate()`
* **RacingApi.PlayerSpawned**: `delegate void PlayerSpawnedDelegate()`
* **MultiplayerApi.PlayerJoined**: `delegate void PlayerJoinedDelegate(ZeepkistNetworkPlayer player)`
* **MultiplayerApi.CreatedRoom**: `delegate void CreatedRoomDelegate()`
* **LevelEditorApi.EnteredLevelEditor**: `delegate void EnteredLevelEditorDelegate()`
* **LevelEditorApi.EnteredTestMode**: `delegate void EnteredTestModeDelegate()`

# Showdown4 Project Knowledge

This section captures architecture knowledge and conventions worked out with the user, so future changes stay
consistent.

## Building & Verifying

* **Do not use the full `build` target to verify code.** The project has a PostBuild target that copies `Showdown4.dll`
  into the Steam Zeepkist BepInEx plugins folder. While the game is running this fails with "access denied" — that error
  is **not** code-related.
* **To verify compilation, use:** `dotnet build Showdown4.csproj -t:Compile -v:minimal`. This compiles both production
  and test code without the PostBuild copy step.
* Always aim for 0 errors / 0 warnings and keep changed files lint-clean.
* Target framework: `net472`, C# 14.

## State Machine Architecture

* Located in `src/States`. Two machines: the **Master** state machine (`src/States/Master`, e.g. `StateMasterOn`/
  `StateMasterOff`) and the **Showdown** state machine (`src/States/Showdown`).
* `ShowdownStateMachine` is a `MonoBehaviour`. It is created via `GameObject.AddComponent` (see `StateMasterOn.Enter()`)
  so Unity drives its `Update()` loop, which forwards keyboard input to `CurrentState.HandleInput()`. States implement
  `IState` (`Enter`/`Exit` + `Finished` event + `InvokeFinish()`) and override `HandleInput()` when they need input.
* **`ShowdownStateBase`** (`src/States/Showdown/ShowdownStateBase.cs`) is the abstract base for every Showdown state. It
  centralizes the boilerplate that used to be copy-pasted: storing the `StateMachine`, exposing strongly typed helpers
  `Showdown`, `Match`, `CurrentDraft`, the `Finished` event and `InvokeFinish()`, and a virtual `HandleInput()`. **New
  Showdown states should inherit from it** instead of re-implementing the boilerplate. Always call `InvokeFinish()`,
  never `Finished?.Invoke()` directly (except in Master states that don't derive from the base).
* Transitions are declared in `ShowdownStateMachine.InitTransitions()`.

### State lifecycle: no `Execute()` (removed)

* `Execute()` has been **removed** from the state lifecycle. `TransitionTo` now calls `Enter()` and then
  `SubStateMachine?.Start()` — there is no separate `Execute` phase. Per-frame input is delegated by the
  `ShowdownStateMachine.Update()` MonoBehaviour hook to `CurrentState.HandleInput()` (the old separate `StateManager`
  component has been removed).
* One-time setup + the first message now live entirely in `Enter()`. For `StateMasterOn`, the `SubStateMachine`
  must still be created inside `Enter()` because `TransitionTo` starts it right after `Enter()`.
* States that redraw on timer/command callbacks now use an explicit private render method instead of re-calling the
  lifecycle: `Render()` (`StateDrafting`, `StateDraftIncomplete`, `StateDraftCompleted`, `StateReadyCheck`) or an
  existing message builder such as `SendServerMessage()` / `ServerMessageThing()` (`StateMatchEnd`, `StateSetupMatch`,
  `StateSelectInitiative`). States whose first render must happen immediately call that method at the end of `Enter()`.
* Other fragilities to keep in mind: transitions compare `GetType().Name` strings (fragile against renames — prefer
  `Type`/`is`), and `InitTransitions()` rebuilds the list and `new StateX(this)` on every transition.

## Configuration (`MyConfig`)

* All tunable values live in `MyConfig.cs` as BepInEx `ConfigEntry<T>` fields (season number,
  draft/ready-check/race/match-end timings, spin loops, lobby time). They are bound manually in `MyConfig.Register(...)`
  and exposed as `FooConfig.Value`.
* Values are read directly via `MyConfig.*Config.Value`. The config file is user-editable, so raw values could be
  negative/zero/huge. Instead of a hand-written clamping layer, each numeric entry is bound with a BepInEx
  `AcceptableValueRange<int>` (via `ConfigDescription`) so BepInEx keeps the value inside safe bounds automatically
  (e.g. `SeasonNumber` 1-3999, `DraftTime` 1-3600). Season number is 1-3999 because it is rendered as a Roman numeral.
* When adding a new tunable value: declare a `public static ConfigEntry<T> FooConfig;` field in `MyConfig.cs`, bind it
  in
  `Register(...)` with a `ConfigDescription` + `AcceptableValueRange` when it needs bounds, and consume
  `MyConfig.FooConfig.Value` from the states.

## Draft Flow Conventions

* Draft rules are owned by the `Draft` entity (`PickLevel`/`BanLevel` with validation) — states should go through it,
  not mutate `PickedLevels` directly.
* The draft is split across four states: **`StatePreDraft`** (intro animation + step-by-step pick/ban/pass instructions,
  runs before every draft), **`StateDrafting`** (normal pick/ban flow), **`StateDraftIncomplete`**
  (random-map roulette when the draft ends with no picks and more than one map open) and **`StateDraftCompleted`**
  (locks in the playlist, shows the "complete" banner + countdown, then hands over to the ready check).
  `HandleDraftComplete()` in `StateDrafting` decides the transition; the state graph is
  `SelectInitiative/PostRacing -> PreDraft -> Drafting -> (DraftIncomplete) -> DraftCompleted -> ReadyCheck`.
* **Auto-pick of the last map:** when only one level remains after the last action, `StateDrafting` auto-picks it with a
  short visual reveal countdown (`MyConfig.AutoPickRevealCountdownConfig.Value`, default 3s) before locking it in.
* **No `/sd random` command:** the random selection for an incomplete draft now runs automatically in
  `StateDraftIncomplete.Enter()` instead of being triggered by a chat command.
* **`!pass` command** (`CommandPass`) hands the current team's action over to the other team (behaves like a voluntary
  turn timeout) via the `OnPass` hook on `StateDrafting`.
* Domain helpers live on the entities: `Match.IsDraftphaseTwo`, `Match.DraftphaseName`, `Team.CreateShowdownTeam()`.

## UI / Formatting Conventions

* **Colors are centralized in `ShowdownColors`** — do not hardcode hex literals (`#00ff00`, etc.) in states; use the
  palette (`Green`, `Red`, `Cyan`, `Yellow`, `Gold`, `White`).
* Shared message layout lives in `ShowdownMessages` (e.g. `AppendPickedMaps`, used by ReadyCheck and PreRacing) and
  `ServerMessage`/`MessageFormatter`. `ServerMessage.ShowdownHeader` builds the Roman-numeral season header via
  `ToRomanNumeral`.
* Keep message-building/formatting out of the state lifecycle logic where possible.

## Refactoring Ground Rules (as agreed with the user)

* Refactors are **behavior-neutral by default** — keep messages, timings and public state signatures identical unless
  the user explicitly asks to fix a bug.
* Prefer reducing duplication by moving shared logic to the base class, `ShowdownColors`, `ShowdownMessages`, or the
  domain entities (`Match`/`Team`/`Draft`).


