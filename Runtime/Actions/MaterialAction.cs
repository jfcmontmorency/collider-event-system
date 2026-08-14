using UnityEngine;

namespace ColliderEventSystem
{
    /// <summary>
    /// Replaces every material slot on a Renderer with a different Material.
    /// </summary>
    public sealed class MaterialAction : ActionBase
    {
        public TargetMode targetMode = TargetMode.EnteringObjects;

        [Tooltip("The Renderer to modify. Used when Target Mode is Specific Object.")]
        public Renderer targetRenderer;

        [Tooltip("Applied to every material slot on the Renderer.")]
        public Material newMaterial;

        public override bool RequiresCollisionObjectData => targetMode == TargetMode.EnteringObjects;

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
            if (renderer == null || newMaterial == null) return;

            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++) materials[i] = newMaterial;
            renderer.materials = materials;
        }
    }
}
