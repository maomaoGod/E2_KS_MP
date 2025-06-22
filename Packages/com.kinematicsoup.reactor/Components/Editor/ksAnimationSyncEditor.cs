using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace KS.Reactor.Client.Unity.Editor
{
    /// <summary>Custom editor for <see cref="ksAnimationSync"/>.</summary>
    [CustomEditor(typeof(ksAnimationSync))]
    public class ksAnimationSyncEditor : ksEntityScriptEditor
    {
        private static GUIContent m_animationPropertiesLabel;

        /// <summary>Draws the inspector GUI.</summary>
        public override void OnInspectorGUI()
        {
            ksAnimationSync sync = target as ksAnimationSync;
            if (sync == null)
            {
                return;
            }

            // Check for entity component and animator.
            Validate();
            if (GetAnimator(sync) == null)
            {
                EditorGUILayout.HelpBox("No Animator component found. You must add an Animator to this game object " +
                    "or assign one from a different game object to the Animator field.", MessageType.Warning);
            }

            serializedObject.Update();
            bool syncLayerStates = false;
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                switch (iterator.name)
                {
                    case "m_Script": continue;
                    case "m_syncLayerStates":
                    {
                        EditorGUILayout.PropertyField(iterator);
                        syncLayerStates = iterator.boolValue || iterator.hasMultipleDifferentValues;
                        break;
                    }
                    case "m_crossFadeDuration":
                    {
                        if (syncLayerStates)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(iterator);
                            EditorGUI.indentLevel--;
                        }
                        break;
                    }
                    case "m_animationPropertiesStart":
                    {
                        if (m_animationPropertiesLabel == null)
                        {
                            m_animationPropertiesLabel = new GUIContent("Animation Property Ids",
                                "The range of ids used to sync animation properties. Make sure you do not use " +
                                "property ids in this range for other properties or your properties will be " +
                                "overwritten. It is recommended to leave some unused properties after the end of " +
                                "this range so if you add more animation properties, they won't collide with your " +
                                "other property ids.");
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(iterator, m_animationPropertiesLabel);
                        int count = GetPropertyCount();
                        if (count >= 0)
                        {
                            EditorGUILayout.LabelField("to " + (iterator.intValue + count));
                        }
                        else
                        {
                            EditorGUILayout.LabelField("to -");
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                    default:
                    {
                        EditorGUILayout.PropertyField(iterator);
                        break;
                    }
                }

                if (GUI.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        /// <summary>
        /// Gets the animator for a <see cref="ksAnimationSync"/>. If the <paramref name="sync"/> has an 
        /// <see cref="ksAnimationSync.Animator"/> returns it. If not, looks for a <see cref="ksEntityComponent"/> on
        /// the same game object and if it finds one that has a <see cref="ksEntityComponent.SyncTarget"/>, looks for an
        /// animator on the sync target. Otherwise looks for an animator on the same game object as the 
        /// <paramref name="sync"/>.
        /// </summary>
        /// <param name="sync">Script to get animator.</param>
        /// <returns>Animator</returns>
        private Animator GetAnimator(ksAnimationSync sync)
        {
            if (sync == null)
            {
                return null;
            }
            if (sync.Animator != null)
            {
                return sync.Animator;
            }
            ksEntityComponent entityComponent = sync.GetComponent<ksEntityComponent>();
            if (entityComponent != null && entityComponent.SyncTarget != null)
            {
                return entityComponent.SyncTarget.GetComponent<Animator>();
            }
            return sync.GetComponent<Animator>();
        }

        /// <summary>
        /// Gets the number of properties the animator will use to sync. If there are multiple animators using different
        /// numbers of properties or if there are no animators, returns negative one.
        /// </summary>
        /// <returns>
        /// The number of properties the animator will use to sync, or negative one if there are multiple animators
        /// using different numbers of properties or if there are no animators.
        /// </returns>
        private int GetPropertyCount()
        {
            int count = -1;
            for (int i = 0; i < targets.Length; i++)
            {
                ksAnimationSync sync = targets[i] as ksAnimationSync;
                Animator animator = GetAnimator(sync);
                if (animator == null)
                {
                    continue;
                }
                int num = GetPropertyCount(animator, sync.SyncLayerStates);
                if (count < 0)
                {
                    count = num;
                }
                else if (count != num)
                {
                    return -1;
                }
            }
            return count;
        }

        /// <summary>Gets the number of properties an animator will use to sync.</summary>
        /// <param name="animator">Animator to get property count from.</param>
        /// <param name="includeLayerStates">If true, includes a property for each layer.</param>
        /// <returns>The number of properties the animator will use to sync.</returns>
        private int GetPropertyCount(Animator animator, bool includeLayerStates)
        {
            // If the animator is not part of a prefab, we can get the parameter and layer count directly from the
            // animator.
            int count;
            if (!PrefabUtility.IsPartOfPrefabAsset(animator))
            {
                 count = animator.parameterCount;
                if (includeLayerStates)
                {
                    count += animator.layerCount;
                }
                return count;
            }
            // Parameter and layer count do not work when the animator is part of a prefab, so we have to get the counts
            // from the animator controller, which we get from the animator's serialized properties.
            SerializedObject so = new SerializedObject(animator);
            SerializedProperty sprop = so.FindProperty("m_Controller");
            AnimatorController controller = sprop.objectReferenceValue as AnimatorController;
            if (controller == null)
            {
                return 0;
            }
            count = controller.parameters.Length;
            if (includeLayerStates)
            {
                count += controller.layers.Length;
            }
            return count;
        }
    }
}
