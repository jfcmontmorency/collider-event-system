using UnityEngine;

namespace ColliderEventSystem
{
    /// <summary>
    /// Base class for all Conditions. Inherit from this to create a new condition type -
    /// it will automatically appear in the "Add Condition" list in the Inspector.
    /// </summary>
    [AddComponentMenu("")]
    public abstract class ConditionBase : MonoBehaviour
    {
        /// <summary>
        /// How this condition combines with the previous condition in the list. Ignored on the first condition.
        /// </summary>
        public LogicOperator Operator;

        [Tooltip("Optional. Applies a material to the target while this Condition is false, and restores the original once it becomes true (or once the zone is left before it does) - a hint that an interaction is available.")]
        public bool showHintMaterial;

        [Tooltip("Used when Show Hint Material is on. Entering Objects highlights whatever triggered the zone; Specific Object always highlights the same Renderer below.")]
        public TargetMode hintTargetMode = TargetMode.EnteringObjects;

        [Tooltip("Used when Hint Target Mode is Specific Object.")]
        public Renderer hintRenderer;

        [Tooltip("Applied to every material slot on the target Renderer while this Condition is false.")]
        public Material hintMaterial;

        private Renderer m_ActiveHintRenderer;

        /// <summary>
        /// If true, Evaluate(GameObject) is called instead of Evaluate() so this condition can use the
        /// GameObject that triggered the zone. Only meaningful under ColliderEvent, not ConditionWatcher.
        /// A Hint Material targeting Entering Objects needs it too, regardless of the condition's own logic.
        /// </summary>
        public virtual bool RequiresCollisionObjectData => showHintMaterial;

        /// <summary>
        /// Returns whether this condition is currently met.
        /// </summary>
        public abstract bool Evaluate();

        /// <summary>
        /// Same as Evaluate(), but with access to the GameObject that triggered the zone.
        /// Only called when RequiresCollisionObjectData is true.
        /// </summary>
        public virtual bool Evaluate(GameObject collidingObject)
        {
            bool met = Evaluate();

            if (showHintMaterial)
            {
                UpdateHintMaterial(met, collidingObject);
            }

            return met;
        }

        /// <summary>
        /// Called once when the game starts, before any Evaluate calls. Use this for one-time setup.
        /// </summary>
        public virtual void OnAwake() { }

        /// <summary>
        /// Called when the owning ColliderEvent/ConditionWatcher resets after firing, and also when it
        /// stops checking without ever firing (e.g. the colliding object left the zone) - both are places
        /// where an active Hint Material needs to be restored, since Evaluate() won't be called again to
        /// notice the condition went away.
        /// </summary>
        public virtual void ResetState()
        {
            if (m_ActiveHintRenderer != null)
            {
                RendererMaterialOverride.Restore(m_ActiveHintRenderer);
                m_ActiveHintRenderer = null;
            }
        }

        private void UpdateHintMaterial(bool conditionMet, GameObject collidingObject)
        {
            Renderer renderer = conditionMet || hintMaterial == null ? null : ResolveHintRenderer(collidingObject);

            if (renderer == m_ActiveHintRenderer) return;

            if (m_ActiveHintRenderer != null) RendererMaterialOverride.Restore(m_ActiveHintRenderer);
            if (renderer != null) RendererMaterialOverride.Apply(renderer, hintMaterial);

            m_ActiveHintRenderer = renderer;
        }

        private Renderer ResolveHintRenderer(GameObject collidingObject)
        {
            return hintTargetMode == TargetMode.EnteringObjects
                ? (collidingObject != null ? collidingObject.GetComponent<Renderer>() : null)
                : hintRenderer;
        }

        /// <summary>
        /// Optional inline validation shown in the Inspector as a warning box. Return true and set message
        /// when something required is missing (e.g. an empty reference field).
        /// </summary>
        public virtual bool TryGetValidationWarning(out string message)
        {
            message = null;
            return false;
        }
    }
}
