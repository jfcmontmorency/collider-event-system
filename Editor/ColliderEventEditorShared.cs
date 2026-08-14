using ColliderEventSystem;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ColliderEventSystem.Editor
{
    /// <summary>
    /// Field-drawing code shared by ColliderEventEditor and ConditionWatcherEditor.
    /// </summary>
    public static class ColliderEventEditorShared
    {
        public static void DrawCommonFields(SerializedProperty holdTime, SerializedProperty afterTrigger)
        {
            EditorGUILayout.PropertyField(holdTime, new GUIContent("Hold Time", holdTime.tooltip));
            EditorGUILayout.PropertyField(afterTrigger);
        }

        public static void DrawDebugLogging(SerializedProperty debugLogging)
        {
            EditorGUILayout.PropertyField(debugLogging);
        }

        /// <summary>
        /// Draws lists already built with ConditionActionListDrawer.Build() in the owning Editor's OnEnable().
        /// </summary>
        public static void DrawLists(ReorderableList conditionsList, ReorderableList actionsList, ReorderableList exitActionsList, SerializedProperty afterTrigger)
        {
            EditorGUILayout.Space(8f);
            conditionsList.DoLayoutList();

            EditorGUILayout.Space(8f);
            actionsList.DoLayoutList();

            if ((AfterTrigger)afterTrigger.enumValueIndex == AfterTrigger.ExecuteExitActions)
            {
                EditorGUILayout.Space(8f);
                exitActionsList.DoLayoutList();
            }
        }

        /// <summary>
        /// Warns when a listed Condition/Action needs the colliding GameObject, but the owner has no way
        /// to provide one (Condition Watcher has no zone/Collider).
        /// </summary>
        public static void DrawMissingCollisionDataWarning(SerializedProperty conditions, SerializedProperty actions, SerializedProperty exitActions)
        {
            if (AnyRequiresCollisionObjectData(conditions) || AnyRequiresCollisionObjectData(actions) || AnyRequiresCollisionObjectData(exitActions))
            {
                EditorGUILayout.HelpBox(
                    "One or more Conditions/Actions here need the GameObject that entered a zone, but Condition Watcher isn't tied to a zone - they'll receive no object.",
                    MessageType.Warning);
            }
        }

        private static bool AnyRequiresCollisionObjectData(SerializedProperty listProperty)
        {
            if (listProperty == null) return false;

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                Object element = listProperty.GetArrayElementAtIndex(i).objectReferenceValue;

                if (element is ConditionBase condition && condition.RequiresCollisionObjectData) return true;
                if (element is ActionBase action && action.RequiresCollisionObjectData) return true;
            }

            return false;
        }
    }
}
