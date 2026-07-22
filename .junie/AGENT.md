# Decompiled Zeepkist

C:\Users\TEute\Desktop\Projects\Zeepkist Mods\_Zeepkist

# ZeepSDK URL

## Github

https://github.com/donderjoekel/ZeepSDK

https://donderjoekel.github.io/ZeepSDK/api/index.html

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
* `ShowdownStateMachine` is a `MonoBehaviour`. States implement `IState` (`Enter`/`Execute`/`Exit` + `Finished` event +
  `InvokeFinish()`).
* **`ShowdownStateBase`** (`src/States/Showdown/ShowdownStateBase.cs`) is the abstract base for every Showdown state. It
  centralizes the boilerplate that used to be copy-pasted: storing the `StateMachine`, exposing strongly typed helpers
  `Showdown`, `Match`, `CurrentDraft`, the `Finished` event and `InvokeFinish()`, and a virtual `HandleInput()`. **New
  Showdown states should inherit from it** instead of re-implementing the boilerplate. Always call `InvokeFinish()`,
  never `Finished?.Invoke()` directly (except in Master states that don't derive from the base).
* Transitions are declared in `ShowdownStateMachine.InitTransitions()`.

### Known weak spot: `Execute()` is NOT a loop

* In `TransitionTo`, `Execute()` is called **exactly once**, right after `Enter()` and **before**
  `SubStateMachine?.Start()`. There is no per-frame `Update()` pumping it. So `Execute()` is effectively a "second Enter
  phase", not a tick — the name is misleading.
* Because there is no loop, some states manually re-call `Execute()` from timer callbacks to force a re-render (e.g.
  `StateMatchEnd.UpdateCountdownMessage`, `State_DraftIncomplete`). Treat `Execute()` as a "render me" hook in the
  current design.
* **Recommended future refactor (Option A, behavior-neutral, agreed but NOT yet implemented):** dissolve `Execute()`;
  move its body into `Enter()` (one-time setup + first message), keep `InvokeFinish()` on the triggering event, and
  introduce an explicit `Render()`/`Refresh()` for the few countdown states that redraw. A real per-frame tick (Option
  B) is discouraged because states are event/timer-driven, not frame-driven.
* Other fragilities to keep in mind: transitions compare `GetType().Name` strings (fragile against renames — prefer
  `Type`/`is`), and `InitTransitions()` rebuilds the list and `new StateX(this)` on every transition.

## Configuration (`MyConfig`)

* All tunable values live in `MyConfig.cs` as BepInEx `[Entry]` fields (season number, draft/ready-check/race/match-end
  timings, spin loops, lobby time). A source generator turns each `[Entry]` field `Foo` into a `FooConfig.Value`
  accessor.
* **Never read `MyConfig.*Config.Value` directly in states.** The config file is user-editable, so raw values can be
  negative/zero/huge. Read through the validated, clamped entry point **`MyConfig.Validated.*`** (partial class in
  `src/MyConfig.Validated.cs`), which clamps every numeric setting into safe bounds in one place (e.g. `SeasonNumber`
  1-3999, `DraftTime` 1-3600). Season number is 1-3999 because it is rendered as a Roman numeral.
* When adding a new tunable value: add an `[Entry]` field in `MyConfig.cs` with a description that documents its clamp
  range, add a matching clamped property in `MyConfig.Validated`, and consume `MyConfig.Validated.X` from the states.

## Draft Flow Conventions

* Draft rules are owned by the `Draft` entity (`PickLevel`/`BanLevel` with validation) — states should go through it,
  not mutate `PickedLevels` directly.
* The draft is split across two states: **`StateDrafting`** (normal pick/ban flow) and **`StateDraftIncomplete`** (
  waiting for host + random-map roulette when no picks happened). `HandleDraftComplete()` in `StateDrafting` decides the
  transition.
* **Draftphase II auto-pick:** when only one level remains in draftphase II, `StateDrafting` auto-picks the last
  remaining level instead of going to random selection.
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


