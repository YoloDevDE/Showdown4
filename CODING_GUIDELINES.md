# Showdown4 Coding Guidelines

## Purpose

These guidelines keep Showdown4 small, understandable, and safe to maintain while it integrates with Zeepkist through
BepInEx, ZeepSDK, Harmony, and decompiled game assemblies. Prefer a direct solution over a flexible framework unless a
real variation or repeated responsibility requires the extra structure.

## Core Principles

- Prefer clarity, small focused types, and explicit data flow over cleverness.
- Apply KISS first. Do not introduce architecture for anticipated requirements.
- Keep related behavior together and give every class one clear, domain-level responsibility.
- Use composition and direct calls for clear one-to-one dependencies.
- Add an abstraction only when it protects an external boundary, supports a real alternative implementation or test
  double, or removes an established duplicate responsibility.
- Do not add a generic event bus, service locator, mediator, repository, or singleton pattern without a concrete,
  documented need.

## Project Structure

- Keep the existing technical folders as the primary structure: `Commands`, `Config`, `Entities`, `Managers`, `States`,
  and `Utils`.
- Place new code by its responsibility, not by a speculative new architecture.
- A feature may span several technical folders when that makes each class easier to find and understand.
- Reorganize folders only to resolve a concrete maintenance problem; do not move code for style alone.
- Do not use `Utils` or `Helpers` as a catch-all. A utility type must have one narrow, well-named purpose.

## Naming, Language, and Style

- Write code, comments, log messages, and technical documentation in English.
- Use `PascalCase` for namespaces, types, files, and public or protected members.
- Use `_camelCase` for private instance fields and `camelCase` for parameters and local variables.
- Choose descriptive domain names over abbreviations and type-based names.
- Use `var` only when the assigned expression makes the type immediately obvious.
- New and substantially changed code follows these rules. Do not rename existing code solely to modernize its style.
- Comments explain intent, constraints, or non-obvious reasoning; they do not narrate self-evident code.

## Responsibilities and Patterns

### Plugin Bootstrap

`Plugin` is the BepInEx entry point, comparable to `Main`. It contains only lifecycle work and one-time composition:

- initialize and dispose runtime components;
- register and unregister commands, UI, and patches; and
- start the application's state machinery.

Do not place game rules, Showdown decisions, calculations, or feature-specific workflow in `Plugin`.

### Approved Patterns

- Use the existing state pattern for longer-lived, mutually exclusive application or Showdown phases.
- Use the command pattern for chat-command parsing and execution.
- Use a small adapter or service for external integration boundaries.
- Use events only when several independent consumers genuinely need notification. Do not introduce a global event bus.
- Prefer a direct call when there is one clear caller and one clear receiver.

### Dependencies and Global State

- Pass dependencies to new business logic explicitly through a constructor or clear initialization method.
- `Plugin.Instance` may be used only for BepInEx/Unity lifecycle needs and unavoidable framework access.
- Do not add globally accessible services or use `Plugin.Instance` as a general service locator.

## External Integration Boundary

ZeepSDK, Unity/BepInEx, Harmony, and decompiled Zeepkist types are external, version-sensitive boundaries.

- Create a thin project-owned adapter/API layer for decompiled game access, Harmony interaction, and recurring or
  fragile SDK/Unity calls.
- Translate external types and implementation details into small, clear project-facing methods or models where useful.
- Keep domain decisions and calculations independent of Unity, ZeepSDK, Harmony, and game-assembly types.
- A simple, one-off, stable SDK call may remain direct at its natural integration point, such as command or GUI
  registration in `Plugin`.
- Do not create an interface merely to wrap one stable implementation. Add one only for a real alternative, test double,
  or independently variable boundary.

## DRY and Extraction

- Do not duplicate knowledge: configuration rules, external API quirks, game rules, and message text must have one
  authoritative home.
- Extract similar code into a common, domain-named method or class when it has two genuine uses.
- Do not force extraction when the cases have different responsibilities or their differences are more important than
  their similarity.
- Prefer a focused name that describes the responsibility; avoid vague names such as `Common`, `Helper`, or `Utility`.

## Harmony, Lifecycle, and Failure Handling

- Keep each Harmony patch minimal; it should validate the integration boundary and delegate meaningful work to project
  code.
- Document the patched target type, method, game version tested, and any decompilation-dependent assumption where it is
  not obvious.
- Validate nullable, missing, and unexpected external data at the boundary before it reaches domain logic.
- Handle expected integration failures defensively: log enough context to diagnose the issue and disable or safely end
  the affected optional behavior without disrupting the game session.
- Do not catch broad exceptions merely to hide a programming error. Catch only failures that can be safely handled at
  that boundary.
- Pair every registration with its matching cleanup: commands, UI drawers, event subscriptions, and Harmony patches must
  be unregistered or unpatched during teardown when the API requires it.

## Quality Checks

- Build the project after code changes.
- For Unity, ZeepSDK, BepInEx, Harmony, or decompiled-game changes, perform a focused manual in-game check of the
  affected flow when the game environment is available.
- Add automated tests only for critical logic, regression-prone defects, or code whose correctness cannot be adequately
  established by a build and focused manual verification.
- When a decompiled API assumption or known fragility is important to future maintenance, document it next to the
  integration code or in the relevant project documentation.

## Change Checklist

Before submitting a change, confirm:

1. The code has one clear responsibility and is located in the appropriate existing folder.
2. No new abstraction, global service, or pattern was added without a concrete need.
3. Duplicate domain knowledge and external API details have one authoritative location.
4. External calls are contained at an appropriate boundary and failure paths are observable in logs.
5. Registrations have matching teardown, and the project builds.
6. The changed game-facing flow was manually checked when applicable.