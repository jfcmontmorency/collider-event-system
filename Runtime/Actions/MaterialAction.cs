using System.Collections.Generic;
using UnityEngine;

namespace ColliderEventSystem
{
    /// <summary>
    /// Replaces every material slot on a Renderer with a different Material, or restores whatever was
    /// there before - e.g. a highlight applied on enter and restored on exit, without having to know or
    /// re-specify the original material.
    /// </summary>
    public sealed class MaterialAction : ActionBase
    {
        public enum Mode
        {
            Apply,
            RestoreOriginal,
        }

        public TargetMode targetMode = TargetMode.EnteringObjects;

        [Tooltip("The Renderer to modify. Used when Target Mode is Specific Object.")]
        public Renderer targetRenderer;

        public Mode mode = Mode.Apply;

        [Tooltip("Applied to every material slot on the Renderer. Used when Mode is Apply.")]
        public Material newMaterial;

        public override bool RequiresCollisionObjectData => targetMode == TargetMode.EnteringObjects;

        // Shared across every MaterialAction instance so an Apply on one (e.g. an Action) and a
        // RestoreOriginal on a different one (e.g. its Exit Action) see the same cache.
        private static readonly Dictionary<Renderer, Material[]> s_OriginalMaterials = new Dictionary<Renderer, Material[]>();

#if UNITY_EDITOR
        // Renderers are recreated (or destroyed) between Play sessions, so cached entries from a
        // previous session are never valid to restore from - clear regardless of Fast Enter Play Mode /
        // domain reload settings.
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetOriginalMaterialsCache()
        {
            s_OriginalMaterials.Clear();
        }
#endif

        public override void Execute()
        {
            Apply(targetRenderer);
        }

        public override void Execute(GameObject collidingObject)
        {
            Apply(collidingObject != null ? collidingObject.GetComponent<Renderer>() : null);
        }

        private void Apply(Renderer renderer)
        {
            if (renderer == null) return;

            if (mode == Mode.RestoreOriginal)
            {
                if (s_OriginalMaterials.TryGetValue(renderer, out Material[] original))
                {
                    renderer.materials = original;
                    s_OriginalMaterials.Remove(renderer);
                }

                return;
            }

            if (newMaterial == null) return;

            // Only remember the very first Apply - a second Apply before a RestoreOriginal (e.g. re-
            // entering before the exit fired) must not overwrite the real original with the highlight.
            if (!s_OriginalMaterials.ContainsKey(renderer))
            {
                s_OriginalMaterials[renderer] = renderer.sharedMaterials;
            }

            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++) materials[i] = newMaterial;
            renderer.materials = materials;
        }
    }
}
