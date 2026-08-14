using UnityEngine;

namespace ColliderEventSystem
{
    /// <summary>
    /// Compares a Transform's rotation on one axis, in degrees, against a threshold value. Useful for
    /// things like "this door has swung open past 80 degrees" or "this object has tipped over".
    /// </summary>
    public sealed class RotationCondition : ConditionBase
    {
        public enum Axis
        {
            X,
            Y,
            Z,
        }

        [Tooltip("The Transform to check.")]
        public Transform target;

        [Tooltip("Measures rotation relative to this Transform instead of world space. Leave empty for world space.")]
        public Transform relativeTo;

        public Axis axis = Axis.Y;

        public ComparisonOperator operatorValue = ComparisonOperator.GreaterThan;

        public float value;

        public override bool Evaluate()
        {
            if (target == null) return false;

            return ComparisonUtility.Compare(GetCurrentAngle(), value, operatorValue);
        }

        private float GetCurrentAngle()
        {
            Vector3 euler = relativeTo != null
                ? (Quaternion.Inverse(relativeTo.rotation) * target.rotation).eulerAngles
                : target.eulerAngles;

            switch (axis)
            {
                case Axis.X: return euler.x;
                case Axis.Y: return euler.y;
                default: return euler.z;
            }
        }
    }
}
