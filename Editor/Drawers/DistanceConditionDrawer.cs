using ColliderEventSystem;
using ColliderEventSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace ColliderEventSystem.Editor.Drawers
{
    /// <summary>
    /// A flat field list (Distance Condition has no conditional fields of its own), plus the shared Hint
    /// Material section every Condition type gets.
    /// </summary>
    public static class DistanceConditionDrawer
    {
        public static void Draw(Rect rect, SerializedObject so)
        {
            Layout(rect, so, true);
        }

        public static float GetHeight(SerializedObject so)
        {
            return Layout(default, so, false);
        }

        private static float Layout(Rect rect, SerializedObject so, bool draw)
        {
            var cursor = new RectLayoutCursor(rect);

            SerializedProperty targetProp = so.FindProperty("target");
            DrawField(cursor.NextField(targetProp), targetProp, draw);

            SerializedProperty otherProp = so.FindProperty("other");
            DrawField(cursor.NextField(otherProp), otherProp, draw);

            SerializedProperty operatorProp = so.FindProperty("operatorValue");
            DrawField(cursor.NextField(operatorProp), operatorProp, draw);

            SerializedProperty valueProp = so.FindProperty("value");
            DrawField(cursor.NextField(valueProp), valueProp, draw);

            ConditionHintDrawer.Layout(ref cursor, so, draw);

            return cursor.ConsumedHeight;
        }

        private static void DrawField(Rect rect, SerializedProperty property, bool draw)
        {
            if (draw) EditorGUI.PropertyField(rect, property);
        }
    }
}
