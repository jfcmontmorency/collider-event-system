# Collider Event System

Attach ordered **Conditions** and **Actions** to a GameObject to build zone triggers and always-on
condition watchers, without writing code.

## Components

- **Collider Event** - add alongside any native Collider (mark it "Is Trigger"). Waits for something to
  enter the zone, then checks the Conditions. Optionally restrict which Tags can trigger it - leave every
  box unchecked to let anything with a Collider trigger it.
- **Condition Watcher** - no Collider needed. Checks the Conditions continuously from the moment the
  scene starts, for things that don't happen at a place (e.g. "wait until the player has 3 keys").

## Quick start

1. Add a Collider (e.g. Box Collider) to a GameObject, tick "Is Trigger", then add **Collider Event**.
2. Under Conditions, click "Add Condition" and pick one (e.g. Distance).
3. Under Actions, click "Add Action" and pick one (e.g. Invoke Event).
4. Enter Play Mode and walk into the zone.

## Conditions

Looking At, Input, Variable, Distance, Rotation.

Each condition after the first has an **And/Or** selector relating it to the one above it, read strictly
top to bottom (e.g. "A Or B And C" reads as "(A Or B) And C").

## Actions

Animation, Audio, Scene, GameObject, Material, Rigidbody, Transform, Variable, Invoke Events, Instantiate
Prefab.

## Variables

Instead of typing a string key, create a Variable asset (**Assets > Create > Collider Event System >
Variables**) and drag it into a Variable Condition or Set Variable Action - or click the "+" next to the
field to create one on the spot. Toggle **Persistent** on a Variable to have it saved to disk and
reloaded automatically between play sessions.

Note: a Variable asset is shared - every GameObject that references the same one reads/writes the same
value. There's currently no built-in way to give each instance of a prefab (e.g. each enemy) its own
independent value like HP without writing a script for it.

## Base options

- **Hold Time** - how many seconds the Conditions must stay true, without interruption, before Actions run.
- **After Trigger** - what happens once Actions have run (deactivate, destroy, do nothing and reset, or
  wait and run Exit Actions once the Conditions/zone go inactive again).

## Credits

Initially built starting from the concept behind Alexander Scott's
[Enhanced Trigger Box](https://github.com/alexanderscott/Enhanced-Trigger-Box) - a trigger volume driven by
composable conditions and actions. This package has since diverged into its own architecture (Conditions,
Actions, and Variables as separate composable components, a Condition Watcher for non-physical triggers,
custom Inspector tooling, etc.), but the original idea is worth crediting.
