using System.Collections;
using UnityEngine;

namespace ColliderEventSystem
{
    /// <summary>
    /// Moves, rotates, and/or scales a Transform - toggle any combination of the three on, optionally
    /// animated over a duration with easing.
    /// </summary>
    public sealed class TransformAction : ActionBase
    {
        public enum ValueSource
        {
            FixedValue,
            ReferenceTransform,
        }

        public TargetMode targetMode = TargetMode.SpecificObject;

        [Tooltip("Used when Target Mode is Specific Object. Leave empty to use this GameObject's own Transform.")]
        public Transform target;

        public bool modifyPosition;
        public Vector3 position;

        public bool modifyRotation;
        public Vector3 rotation;

        public bool modifyScale;
        public Vector3 scale = Vector3.one;

        [Tooltip("Set replaces the value outright. Additive adds to the current value.")]
        public ValueMode valueMode = ValueMode.Set;

        [Tooltip("Fixed Value uses Position/Rotation/Scale above. Reference Transform reads them from Value Reference below instead, sampled once when this Action runs.")]
        public ValueSource valueSource = ValueSource.FixedValue;

        [Tooltip("Used when Value Source is Reference Transform.")]
        public Transform valueReference;

        [Tooltip("Applies to Position and Rotation. Scale is always Local - there's no reliable way to set world scale directly.")]
        public Space space = Space.World;

        [Tooltip("If the target has a Rigidbody, makes it kinematic while this runs so physics doesn't fight the scripted move. Note this cancels its momentum, not pauses it - a kinematic Rigidbody's velocity reads as zero, so once this restores it to non-kinematic afterward, it resumes at rest (falling fresh from the new position) rather than continuing whatever motion it had before.")]
        public bool cancelPhysicsWhileMoving = true;

        [Tooltip("If false, the change applies instantly. If true, it plays out over Duration below.")]
        public bool animate;

        [Tooltip("How long the change takes, in seconds.")]
        public float duration = 1f;

        public Ease ease = Ease.Linear;

        [Tooltip("Used when Ease is Custom. Should run from (0,0) to (1,1).")]
        public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override float Duration => animate ? duration : 0f;
        public override bool RequiresCollisionObjectData => targetMode == TargetMode.EnteringObjects;

        private Coroutine m_ActiveCoroutine;
        private Rigidbody m_SuspendedRigidbody;
        private bool m_SuspendedRigidbodyWasKinematic;

        public override void Execute()
        {
            Run(ResolveTarget(null));
        }

        public override void Execute(GameObject collidingObject)
        {
            Run(ResolveTarget(collidingObject));
        }

        public override void CancelExecution()
        {
            if (m_ActiveCoroutine != null)
            {
                StopCoroutine(m_ActiveCoroutine);
                m_ActiveCoroutine = null;
            }

            RestoreRigidbody();
        }

        private Transform ResolveTarget(GameObject collidingObject)
        {
            if (targetMode == TargetMode.EnteringObjects)
            {
                return collidingObject != null ? collidingObject.transform : null;
            }

            return target != null ? target : transform;
        }

        private void Run(Transform targetTransform)
        {
            if (targetTransform == null) return;

            CancelExecution();
            SuspendRigidbody(targetTransform);

            // Sampled once up front, not re-read every frame - if Value Reference is itself moving, this
            // Action animates toward where it was the moment it started, not a live-chasing homing move.
            StartState start = GetStart(targetTransform);
            EndState end = GetEnd(start);

            if (!animate || duration <= 0f)
            {
                Apply(targetTransform, 1f, start, end);
                RestoreRigidbody();
                return;
            }

            m_ActiveCoroutine = StartCoroutine(AnimateOverTime(targetTransform, start, end));
        }

        private void SuspendRigidbody(Transform targetTransform)
        {
            if (!cancelPhysicsWhileMoving) return;

            Rigidbody rb = targetTransform.GetComponent<Rigidbody>();
            if (rb == null) return;

            m_SuspendedRigidbody = rb;
            m_SuspendedRigidbodyWasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        private void RestoreRigidbody()
        {
            if (m_SuspendedRigidbody == null) return;

            m_SuspendedRigidbody.isKinematic = m_SuspendedRigidbodyWasKinematic;
            m_SuspendedRigidbody = null;
        }

        private struct StartState
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        private struct EndState
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        private StartState GetStart(Transform t)
        {
            return new StartState
            {
                Position = space == Space.World ? t.position : t.localPosition,
                Rotation = space == Space.World ? t.rotation : t.localRotation,
                Scale = t.localScale,
            };
        }

        private EndState GetEnd(StartState start)
        {
            // A null Value Reference (Reference Transform selected but nothing assigned) falls back to
            // the current value - i.e. that property doesn't move, the same as leaving its toggle off.
            Vector3 rawPosition = position;
            Quaternion rawRotation = Quaternion.Euler(rotation);
            Vector3 rawScale = scale;

            if (valueSource == ValueSource.ReferenceTransform)
            {
                rawPosition = valueReference != null ? (space == Space.World ? valueReference.position : valueReference.localPosition) : start.Position;
                rawRotation = valueReference != null ? (space == Space.World ? valueReference.rotation : valueReference.localRotation) : start.Rotation;
                rawScale = valueReference != null ? valueReference.localScale : start.Scale;
            }

            return new EndState
            {
                Position = valueMode == ValueMode.Additive ? start.Position + rawPosition : rawPosition,
                // Additive composes quaternions instead of adding Euler angles component-wise - eulerAngles
                // isn't a stable round-trip representation (the same rotation can decompose to different
                // Euler triples once more than one axis is involved), so accumulating via Vector3 addition
                // can silently drift or even cancel out from one run to the next.
                Rotation = valueMode == ValueMode.Additive
                    ? (space == Space.World ? rawRotation * start.Rotation : start.Rotation * rawRotation)
                    : rawRotation,
                Scale = valueMode == ValueMode.Additive ? start.Scale + rawScale : rawScale,
            };
        }

        private IEnumerator AnimateOverTime(Transform t, StartState start, EndState end)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = EaseUtility.Evaluate(ease, customCurve, elapsed / duration);
                Apply(t, progress, start, end);
                yield return null;
            }

            Apply(t, 1f, start, end);
            RestoreRigidbody();
            m_ActiveCoroutine = null;
        }

        private void Apply(Transform t, float progress, StartState start, EndState end)
        {
            if (modifyPosition)
            {
                Vector3 newPosition = Vector3.Lerp(start.Position, end.Position, progress);
                if (space == Space.World) t.position = newPosition;
                else t.localPosition = newPosition;
            }

            if (modifyRotation)
            {
                Quaternion newRotation = Quaternion.Slerp(start.Rotation, end.Rotation, progress);
                if (space == Space.World) t.rotation = newRotation;
                else t.localRotation = newRotation;
            }

            if (modifyScale)
            {
                t.localScale = Vector3.Lerp(start.Scale, end.Scale, progress);
            }
        }
    }
}
