# Changelog

All notable changes to this package are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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
