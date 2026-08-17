using ColliderEventSystem;
using UnityEditor;
using UnityEngine;

namespace ColliderEventSystem.Editor
{
    /// <summary>
    /// Shared "Hint Material" section appended to every Condition's own drawer - every Condition type
    /// gets it from ConditionBase, so it's drawn once here instead of duplicated per drawer. Target Mode
    /// only shows when the owner has a ColliderEvent (Condition Watcher has no entering object); the
    /// Renderer/Material fields only show once Show Hint Material is on.
    /// </summary>
    public static class ConditionHintDrawer
    {
        public static void Layout(ref RectLayoutCursor cursor, SerializedObject so, bool draw)
        {
            cursor.Spacer();

            SerializedProperty showProp = so.FindProperty("showHintMaterial");
            Rect showRect = cursor.NextField(showProp);
            if (draw) EditorGUI.PropertyField(showRect, showProp, new GUIContent("Hint Material"));

            if (!showProp.boolValue) return;

            EditorGUI.indentLevel++;

            SerializedProperty targetModeProp = so.FindProperty("hintTargetMode");
            bool supportsEnteringObjects = TargetModeGui.SupportsEnteringObjects(so);

            if (supportsEnteringObjects)
            {
                Rect targetModeRect = cursor.NextField(targetModeProp);
                if (draw) EditorGUI.PropertyField(targetModeRect, targetModeProp, new GUIContent("Target Mode"));
            }
            else
            {
                targetModeProp.enumValueIndex = (int)TargetMode.SpecificObject;
            }

            if (!supportsEnteringObjects || (TargetMode)targetModeProp.enumValueIndex == TargetMode.SpecificObject)
            {
                SerializedProperty rendererProp = so.FindProperty("hintRenderer");
                Rect rendererRect = cursor.NextField(rendererProp);
                if (draw) EditorGUI.PropertyField(rendererRect, rendererProp, new GUIContent("Target"));
            }

            SerializedProperty materialProp = so.FindProperty("hintMaterial");
            Rect materialRect = cursor.NextField(materialProp);
            if (draw) EditorGUI.PropertyField(materialRect, materialProp, new GUIContent("Material"));

            EditorGUI.indentLevel--;
        }
    }
}
