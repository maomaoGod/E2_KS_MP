using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace KS.Reactor.Client.Unity.Editor
{
    /// <summary>Custom editor for <see cref="ksOwnershipBehaviour"/>.</summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ksOwnershipBehaviour))]
    public class ksOwnershipBehaviourEditor : UnityEditor.Editor
    {
        // Option labels for scripts that can be enabled/disabled.
        private static string[] m_scriptOptions;
        // Option labels for components that cannot be enabled/disabled.
        private static string[] m_componentOptions;
        // Option labels for rigid bodies.
        private static string[] m_rigidBodyOptions;
        private static ksOwnershipBehaviour.Behaviours[] m_scriptValues;
        private static ksOwnershipBehaviour.Behaviours[] m_componentValues;

        /// <summary>Draws the inspector GUI.</summary>
        public override void OnInspectorGUI()
        {
            ksOwnershipBehaviour script = target as ksOwnershipBehaviour;
            if (script == null)
            {
                return;
            }
            ksEntityComponent entityComponent = script.GetComponentInParent<ksEntityComponent>(true);
            if (entityComponent == null)
            {
                EditorGUILayout.HelpBox(typeof(ksOwnershipBehaviour).Name + " can only be used on an object with a " +
                    typeof(ksEntityComponent).Name + " or one of its descendant objects.", MessageType.Warning);
            }

            serializedObject.Update();

            // If the script is not on the same game object as the entity component script, draw the behaviour field for
            // the game object.
            SerializedProperty sprop;
            if (entityComponent == null || entityComponent.gameObject != script.gameObject)
            {
                sprop = serializedObject.FindProperty(nameof(ksOwnershipBehaviour.GameObjectBehaviour));
                EditorGUILayout.PropertyField(sprop, new GUIContent("Game Object", sprop.tooltip));

                // If the game object behaviour is not leave unchanged, we don't need to configure the component
                // behaviours.
                if ((ksOwnershipBehaviour.Behaviours)sprop.intValue != ksOwnershipBehaviour.Behaviours.LEAVE_UNCHANGED
                    || sprop.hasMultipleDifferentValues)
                {
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }

            if (m_scriptOptions == null)
            {
                CreateOptions();
            }

            // Change the default option label to show what the default option is.
            sprop = serializedObject.FindProperty(nameof(ksOwnershipBehaviour.DefaultBehaviour));
            EditorGUILayout.PropertyField(sprop, new GUIContent("Default", sprop.tooltip));
            if (sprop.hasMultipleDifferentValues)
            {
                m_scriptOptions[0] = m_componentOptions[0] = m_rigidBodyOptions[0] = "Default";
            }
            else
            {
                ksOwnershipBehaviour.Behaviours behaviour = (ksOwnershipBehaviour.Behaviours)sprop.intValue;
                m_scriptOptions[0] = m_componentOptions[0] = "Default (" +
                    ksEnumDrawer.GetEnumDisplayName(behaviour.ToString()) + ")";
                switch (behaviour)
                {
                    case ksOwnershipBehaviour.Behaviours.DISABLE_WHEN_REMOTE:
                        m_rigidBodyOptions[0] = "Default (Make Kinematic When Remote)"; break;
                    case ksOwnershipBehaviour.Behaviours.DISABLE_WHEN_OWNED:
                        m_rigidBodyOptions[0] = "Default (Make Kinematic When Owned)"; break;
                    default:
                        m_rigidBodyOptions[0] = m_scriptOptions[0]; break;
                }
            }

            serializedObject.ApplyModifiedProperties();
            DrawComponentBehaviours();
        }

        /// <summary>Creates the option labels.</summary>
        private static void CreateOptions()
        {
            // Script options are the same as component options without the disable options, and rigid body options are
            // the same as the script options but with disable changed to make kinematic.
            Array enumValues = Enum.GetValues(typeof(ksOwnershipBehaviour.Behaviours));
            m_scriptOptions = new string[enumValues.Length + 1];
            m_scriptValues = new ksOwnershipBehaviour.Behaviours[enumValues.Length];
            m_componentOptions = new string[enumValues.Length];
            m_componentValues = new ksOwnershipBehaviour.Behaviours[enumValues.Length - 1];
            m_rigidBodyOptions = new string[m_scriptOptions.Length];
            int behaviourIndex = 0;
            int componentIndex = 0;
            m_scriptOptions[0] = m_componentOptions[0] = m_rigidBodyOptions[0] = "Default";
            foreach (object value in enumValues)
            {
                ksOwnershipBehaviour.Behaviours behaviour = (ksOwnershipBehaviour.Behaviours)value;
                m_scriptValues[behaviourIndex++] = behaviour;
                string label = ksEnumDrawer.GetEnumDisplayName(Enum.GetName(typeof(ksOwnershipBehaviour.Behaviours), value));
                m_scriptOptions[behaviourIndex] = label;
                switch (behaviour)
                {
                    case ksOwnershipBehaviour.Behaviours.DISABLE_WHEN_REMOTE:
                        m_rigidBodyOptions[behaviourIndex] = "Make Kinematic When Remote"; break;
                    case ksOwnershipBehaviour.Behaviours.DISABLE_WHEN_OWNED:
                        m_rigidBodyOptions[behaviourIndex] = "Make Kinematic When Owned"; break;
                    default:
                    {
                        m_componentValues[componentIndex++] = behaviour;
                        m_componentOptions[componentIndex] = label;
                        m_rigidBodyOptions[behaviourIndex] = label;
                        break;
                    }
                }
            }
        }

        /// <summary>Draws behaviour properties for the components on the object.</summary>
        private void DrawComponentBehaviours()
        {
            // Build the list of component arrays, with one array in the list for each target containing the components
            // that are present on all targets.
            List<Component>[] componentLists = BuildComponentLists();

            // Iterate the components of the first target and draw the behaviour properties.
            for (int i = 0; i < componentLists[0].Count; i++)
            {
                Component component = componentLists[0][i];
                string label = ksInspectorNames.Get().GetDefaultName(component.GetType());
                string[] options;
                ksOwnershipBehaviour.Behaviours[] values;

                // If the component is a behaviour, collider, or renderer, it can be enabled/disabled and we use the
                // script options.
                if (component is Behaviour || component is Collider || component is Renderer)
                {
                    options = m_scriptOptions;
                    values = m_scriptValues;
                }
                // Use the rigid body options for rigid bodies
                else if (component is Rigidbody || component is Rigidbody2D)
                {
                    options = m_rigidBodyOptions;
                    values = m_scriptValues;
                }
                // Use the component options for everything else.
                else
                {
                    options = m_componentOptions;
                    values = m_componentValues;
                }

                // Get the index of the selected option.
                int index = 0;
                if (HasMixedValues(componentLists, i))
                {
                    EditorGUI.showMixedValue = true;
                }
                else
                {
                    ksOwnershipBehaviour.Behaviours behaviour;
                    if (((ksOwnershipBehaviour)target).ComponentBehaviours.TryGetValue(component, out behaviour))
                    {
                        index = Array.IndexOf(values, behaviour) + 1;
                    }
                }

                // Draw the property.
                EditorGUI.BeginChangeCheck();
                GUIContent content = new GUIContent(label, "What happens to the " + label + "?");
                index = EditorGUILayout.Popup(content, index, options) - 1;
                EditorGUI.showMixedValue = false;

                // If the selected option changed, change the value on all targets.
                if (EditorGUI.EndChangeCheck())
                {
                    for (int j = 0; j < componentLists.Length; j++)
                    {
                        if (componentLists[j] == null)
                        {
                            continue;
                        }
                        ksOwnershipBehaviour script = (ksOwnershipBehaviour)targets[j];
                        component = componentLists[j][i];
                        if (index < 0)
                        {
                            RemoveComponentBehaviour(script, component);
                        }
                        else
                        {
                            SetComponentBehaviour(script, component, (int)values[index]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the component at the given index has different behaviour values on different targets.
        /// </summary>
        /// <param name="componentLists">List of shared component arrays for each target.</param>
        /// <param name="index">Index of the component to check.</param>
        /// <returns>
        /// True if the component at the given index has different behaviour values on different targets.
        /// </returns>
        private bool HasMixedValues(List<Component>[] componentLists, int index)
        {
            if (componentLists.Length > 1)
            {
                // Get the expected value from the first target.
                int expectedValue = -1;
                ksOwnershipBehaviour script = (ksOwnershipBehaviour)target;
                ksOwnershipBehaviour.Behaviours behaviour;
                if (script.ComponentBehaviours.TryGetValue(componentLists[0][index], out behaviour))
                {
                    expectedValue = (int)behaviour;
                }

                // Check if any of the other targets have a different value.
                for (int i = 1; i < componentLists.Length; i++)
                {
                    if (componentLists[i] == null)
                    {
                        continue;
                    }
                    script = (ksOwnershipBehaviour)targets[i];
                    int value = -1;
                    if (script.ComponentBehaviours.TryGetValue(componentLists[i][index], out behaviour))
                    {
                        value = (int)behaviour;
                    }
                    if (value != expectedValue)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Builds a list of component arrays, with one array in the list for each target containing the components
        /// that are present on all targets. Transforms and some Reactor components are excluded. Also removes
        /// destroyed components from the <see cref="ksOwnershipBehaviour.ComponentBehaviours"/> of each target.
        /// </summary>
        /// <returns>List of shared component arrays.</returns>
        private List<Component>[] BuildComponentLists()
        {
            // Build the list of configurable components for the first target.
            ksOwnershipBehaviour script0 = (ksOwnershipBehaviour)target;
            RemoveDestroyedComponents(script0);
            List<Component> components0 = GetConfigurableComponents(script0.gameObject);
            List<Component>[] componentLists = new List<Component>[targets.Length];
            componentLists[0] = components0;

            // Iterate the remaining targets and build their configurable component lists.
            for (int i = 1; i < targets.Length; i++)
            {
                ksOwnershipBehaviour script = targets[i] as ksOwnershipBehaviour;
                if (script == null)
                {
                    continue;
                }
                RemoveDestroyedComponents(script);
                List<Component> components = GetConfigurableComponents(script.gameObject);
                componentLists[i] = new List<Component>();

                // Iterate the components from the first target's list.
                for (int j = 0; j < components0.Count; j++)
                {
                    // Look for a matching component on the current target.
                    Component match = FindAndRemove(components0[j].GetType(), components);
                    if (match == null)
                    {
                        // We didn't find a match. Remove this component from all lists.
                        for (int k = 0; k < i; k++)
                        {
                            if (componentLists[k] != null)
                            {
                                componentLists[k].RemoveAt(j);
                            }
                        }
                        if (components0.Count == 0)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // Add the matching component to the current target's list.
                        componentLists[i].Add(match);
                    }
                }
            }
            return componentLists;
        }

        /// <summary>
        /// Finds and removes the first component of the given type from a list of components. The type must be an exact
        /// match.
        /// </summary>
        /// <param name="type">Type of component to look for.</param>
        /// <param name="components">Component list to find and remove component from.</param>
        /// <returns>The first component of the given <paramref name="type"/>, or null if none was found.</returns>
        private Component FindAndRemove(Type type, List<Component> components)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == type)
                {
                    Component component = components[i];
                    components.RemoveAt(i);
                    return component;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a list of components from a game object that can have their remove entity behaviour configured.
        /// Transforms and some Reactor components are excluded.
        /// </summary>
        /// <param name="gameObj">Game object to get configurable components from.</param>
        /// <returns>List of configurable components</returns>
        private List<Component> GetConfigurableComponents(GameObject gameObj)
        {
            List<Component> components = new List<Component>();
            foreach (Component component in gameObj.GetComponents<Component>())
            {
                if (component != null &&
                    component is not Transform &&
                    component is not ksOwnershipBehaviour &&
                    component is not ksEntityComponent &&
                    component is not ksProxyScript &&
                    component is not ksColliderData)
                {
                    components.Add(component);
                }
            }
            return components;
        }

        /// <summary>Sets a component's behaviour using serialized properties.</summary>
        /// <param name="script">Ownership behaviour script</param>
        /// <param name="component">Component to set behaviour for</param>
        /// <param name="behaviour">
        /// Behaviour value to set. Negative values will remove the component from 
        /// <see cref="ksOwnershipBehaviour.ComponentBehaviours"/>.
        /// </param>
        private void SetComponentBehaviour(
            ksOwnershipBehaviour script,
            Component component,
            int behaviour)
        {
            SerializedObject so = new SerializedObject(script);
            SerializedProperty listProp = FindListProperty(so);
            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elemProp = listProp.GetArrayElementAtIndex(i);
                if (elemProp.FindPropertyRelative("Key").objectReferenceValue == component)
                {
                    if (behaviour < 0)
                    {
                        listProp.DeleteArrayElementAtIndex(i);
                    }
                    else
                    {
                        elemProp.FindPropertyRelative("Value").intValue = behaviour;
                    }
                    so.ApplyModifiedProperties();
                    return;
                }
            }
            if (behaviour >= 0)
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
                SerializedProperty elemProp = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                elemProp.FindPropertyRelative("Key").objectReferenceValue = component;
                elemProp.FindPropertyRelative("Value").intValue = behaviour;
                so.ApplyModifiedProperties();
            }
        }

        /// <summary>Removes a component's behaviour using serialized properties.</summary>
        /// <param name="script">Ownership behaviour script</param>
        /// <param name="component">Component to remove behaviour for</param>
        private void RemoveComponentBehaviour(ksOwnershipBehaviour script, Component component)
        {
            SetComponentBehaviour(script, component, -1);
        }

        /// <summary>
        /// Finds the <see cref="ksOwnershipBehaviour.ComponentBehaviours"/> list property.
        /// </summary>
        /// <param name="so">Serialized object to get property from</param>
        /// <returns>Serialized list property</returns>
        private SerializedProperty FindListProperty(SerializedObject so)
        {
            return so.FindProperty("m_componentBehaviours").FindPropertyRelative("m_list");
        }

        /// <summary>
        /// Checks if a <see cref="ksOwnershipBehaviour"/> has any behaviours for destroyed components.
        /// </summary>
        /// <param name="script">Ownership behaviour script to check for destroyed components.</param>
        /// <returns>True if the script has destroyed components.</returns>
        private bool HasDestroyedComponent(ksOwnershipBehaviour script)
        {
            foreach (Component component in script.ComponentBehaviours.Keys)
            {
                if (component == null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes destroyed components from a <see cref="ksOwnershipBehaviour"/> using serialized properties.
        /// </summary>
        /// <param name="script">Ownership behaviour script to remove destroyed components from.</param>
        private void RemoveDestroyedComponents(ksOwnershipBehaviour script)
        {
            if (!HasDestroyedComponent(script))
            {
                return;
            }
            SerializedObject so = new SerializedObject(script);
            SerializedProperty listProp = FindListProperty(so);
            for (int i = listProp.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty elemProp = listProp.GetArrayElementAtIndex(i);
                if (elemProp.FindPropertyRelative("Key").objectReferenceValue == null)
                {
                    listProp.DeleteArrayElementAtIndex(i);
                }
            }
            so.ApplyModifiedProperties();
        }
    }
}
