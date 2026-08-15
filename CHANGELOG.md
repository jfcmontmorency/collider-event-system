# Changelog

All notable changes to this package are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.1.5]

### Added

- Demo's notification sound now also ships as a lossless `.wav`, alongside the original `.mp3` (same file
  name), per Asset Store guidelines against lossy-only audio.

## [0.1.4]

### Fixed

- Demo sample: a misconfigured Shader Graph material, a mislabeled scene object, and a non-functional
  prefab reference, all in the demo scene itself.

## [0.1.3]

### Changed

- Demo sample's on-screen label switched from TextMeshPro to the legacy UI Text, to avoid the one-time
  "Import TMP Essentials" prompt Unity shows the first time TextMeshPro is used in a project - no import
  step of any kind is needed to try the Demo now. Uses the same bundled Inter font (a plain .ttf, no
  TMP-specific font asset needed).

## [0.1.2]

### Fixed

- Demo sample's materials (Demo Dark, Demo Grid, Demo Condition Met, Demo Transparent) now use custom
  Shader Graphs targeting both URP and Built-In, instead of a URP-only shader - no more pink materials or
  running the Render Pipeline Converter on a Built-In project.
- Demo sample now bundles its own font (Inter, SIL Open Font License) instead of referencing the Editor's
  TMP Essentials font, which doesn't exist in a project that has never imported TextMeshPro before.

## [0.1.1]

### Fixed

- `LICENSE` and `CHANGELOG.md` were missing `.meta` files, so Unity ignored them (with a console warning)
  when the package was installed via git URL.

## [0.1.0]

### Added

- **Collider Event** - zone-based trigger built on any native Collider.
- **Condition Watcher** - always-on equivalent with no physical zone, for state-based waits (e.g. "player
  has 3 keys").
- **Conditions**: Looking At, Input, Variable, Distance, Rotation - foldable left-to-right with an And/Or
  operator per condition.
- **Actions**: Animation, Audio, Scene, GameObject, Material, Rigidbody, Transform, Variable, Invoke
  Events, Instantiate Prefab.
- **Variables**: Float/Int/Bool/String assets, optionally persisted to disk between play sessions.
- **Target Mode** (Entering Objects / Specific Object) on every Action that acts on a GameObject, with
  Entering Objects automatically unavailable on a Condition Watcher (no entering object to give it).
- Custom Inspector tooling throughout: grouped "Target"/"Effect" sections, conditional field visibility,
  a "+" button to create a Variable inline, and inline validation warnings for non-obvious silent failures.
- Starter prefabs (`Prefabs/`) for Cube, Sphere, and Capsule trigger zones.
- **Demo** sample scene demonstrating Conditions, Actions, and Variables together.
