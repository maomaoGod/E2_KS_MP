using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Unity;

namespace KS.Reactor.Client.Unity
{
    /// <summary>
    /// Controls what happens to components and/or child objects when the local player owns or doesn't own the entity.
    /// Attach this script to an object with a <see cref="ksEntityComponent"/> or one of its descendants.
    /// </summary>
    public class ksOwnershipBehaviour : MonoBehaviour
    {
        /// <summary>Object behaviours to apply based on entity ownership.</summary>
        public enum Behaviours
        {
            /// <summary>Do nothing to the object.</summary>
            LEAVE_UNCHANGED = 0,
            /// <summary>
            /// Disable the object or script for entities the local player does not own. It will be re-enabled if the
            /// local player gains ownership of the entity. If the component is a rigid body, it will be set to
            /// kinematic instead of disabled.
            /// </summary>
            DISABLE_WHEN_REMOTE = 1,
            /// <summary>
            /// Disable the object or script for entities the local player does owns. It will be re-enabled if the local
            /// player loses ownership of the entity. If the component is a rigid body, it will be set to kinematic
            /// instead of disabled.
            /// </summary>
            DISABLE_WHEN_OWNED = 2,
            /// <summary>
            /// Destroy the object or script for entities the local player does not own. If will not be recreated if
            /// the local player gains ownership of the entity.
            /// </summary>
            DESTROY_WHEN_REMOTE = 3,
            /// <summary>
            /// Destroy the object or script for entities the local player owns. If will not be recreated if the local
            /// player loses ownership of the entity.
            /// </summary>
            DESTROY_WHEN_OWNED = 4
        }

        /// <summary>
        /// When happens to the game object. This is only applied if the game object is not the root of the entity.
        /// </summary>
        [ksEnum]
        [Tooltip("What happens to the game object?")]
        public Behaviours GameObjectBehaviour;

        /// <summary>What happens to components by default.</summary>
        [ksEnum]
        [Tooltip("What happens to components by default?")]
        public Behaviours DefaultBehaviour;

        /// <summary>
        /// Maps components on this game object to behaviours to apply. Components that aren't in the dictionary will
        /// use <see cref="DefaultBehaviour"/>.
        /// </summary>
        public ksSerializableDictionary<Component, Behaviours> ComponentBehaviours
        {
            get { return m_componentBehaviours; }
        }
        [SerializeField]
        private ksSerializableDictionary<Component, Behaviours> m_componentBehaviours = 
            new ksSerializableDictionary<Component, Behaviours>();

        private ksEntityComponent m_component;
        private ksEntity m_entity;
        private bool m_isEntityRoot;
        private List<UnityEngine.Object> m_disabledObjects = new List<UnityEngine.Object>();

        /// <summary>
        /// Looks for a <see cref="ksEntityComponent"/> script on this object or one of its ancestors and applies the
        /// behaviours to the game object/components if an entity is found. Registers a handler to reapply behaviours
        /// when ownership changes.
        /// </summary>
        private void Awake()
        {
            m_component = GetComponentInParent<ksEntityComponent>();
            if (m_component == null)
            {
                return;
            }
            if (m_component.Entity == null)
            {
                // The entity isn't spawned yet. Initialize once it is spawned.
                m_component.OnAttached += Attached;
                return;
            }
            Initialize();
        }

        /// <summary>
        /// Called when the <see cref="ksEntityComponent"/> is attached to an entity. Applies the behaviours to the
        /// game object/components. Registers a handler to reapply the behaviours when ownership changes.
        /// </summary>
        private void Attached()
        {
            m_component.OnAttached -= Attached;
            Initialize();
        }

        /// <summary>
        /// Applies the behaviours to the game object/components. Registers a handler to reapply the behaviours when
        /// ownership changes.
        /// </summary>
        private void Initialize()
        {
            m_isEntityRoot = m_component.gameObject == gameObject;
            m_entity = m_component.Entity;
            m_entity.OnOwnershipChange += OwnershipChanged;
            ApplyBehaviours(m_entity.OwnerId == m_entity.Room.LocalPlayerId);
        }

        /// <summary>
        /// Called when the entity's owner or permissions change. Reapplies the behaviours to the game object/
        /// components if the local player gained or lost ownership.
        /// </summary>
        /// <param name="oldOwner">Old owner id</param>
        /// <param name="newOwner">New owner id</param>
        /// <param name="oldPermissions">Old permissions</param>
        /// <param name="newPermissions">New permissions</param>
        private void OwnershipChanged(
            uint oldOwner,
            uint newOwner,
            ksOwnerPermissions oldPermissions,
            ksOwnerPermissions newPermissions)
        {
            bool wasOwner = oldOwner == m_entity.Room.LocalPlayerId;
            bool isOwner = newOwner == m_entity.Room.LocalPlayerId;
            if (wasOwner == isOwner)
            {
                return;
            }
            EnableDisabledObjects();
            ApplyBehaviours(isOwner);
        }

        /// <summary>Applies behaviours to the game object and its scripts based on ownership state.</summary>
        /// <param name="isOwner">Is the local player the owner of the entity?</param>
        private void ApplyBehaviours(bool isOwner)
        {
            // Only apply the game object behaviour if this is not the root of the entity. If the behaviour is not
            // leave unchanged, we don't have to apply script behaviours.
            if (!m_isEntityRoot)
            {
                switch (GameObjectBehaviour)
                {
                    case Behaviours.DISABLE_WHEN_REMOTE:
                    {
                        if (!isOwner)
                        {
                            m_disabledObjects.Add(gameObject);
                            gameObject.SetActive(false);
                        }
                        return;
                    }
                    case Behaviours.DISABLE_WHEN_OWNED:
                    {
                        if (isOwner)
                        {
                            m_disabledObjects.Add(gameObject);
                            gameObject.SetActive(false);
                        }
                        return;
                    }
                    case Behaviours.DESTROY_WHEN_REMOTE:
                    {
                        if (!isOwner)
                        {
                            Destroy(gameObject);
                        }
                        return;
                    }
                    case Behaviours.DESTROY_WHEN_OWNED:
                    {
                        if (isOwner)
                        {
                            Destroy(gameObject);
                        }
                        return;
                    }
                }
            }
            // Apply remote script behaviours.
            foreach (Component component in GetComponents<Component>())
            {
                Behaviours behaviour;
                if (!m_componentBehaviours.TryGetValue(component, out behaviour))
                {
                    behaviour = DefaultBehaviour;
                }
                switch (behaviour)
                {
                    case Behaviours.DISABLE_WHEN_REMOTE:
                    {
                        if (!isOwner && SetEnabled(component, false))
                        {
                            m_disabledObjects.Add(component);
                        }
                        break;
                    }
                    case Behaviours.DISABLE_WHEN_OWNED:
                    {
                        if (isOwner && SetEnabled(component, false))
                        {
                            m_disabledObjects.Add(component);
                        }
                        break;
                    }
                    case Behaviours.DESTROY_WHEN_REMOTE:
                    {
                        if (!isOwner)
                        {
                            Destroy(component);
                        }
                        break;
                    }
                    case Behaviours.DESTROY_WHEN_OWNED:
                    {
                        if (isOwner)
                        {
                            Destroy(component);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>Enables objects that were disabled when we applied remote behaviours.</summary>
        private void EnableDisabledObjects()
        {
            foreach (UnityEngine.Object uobj in m_disabledObjects)
            {
                SetEnabled(uobj, true);
            }
            m_disabledObjects.Clear();
        }

        /// <summary>
        /// Sets an object to be enabled or disabled. If the object is a rigid body, sets the kinematic flag to true
        /// for disabled and false for enabled.
        /// </summary>
        /// <param name="uobj">Object to enable or disable.</param>
        /// <param name="enabled">Enabled state to set.</param>
        /// <returns>True if the enabled state changed.</returns>
        private bool SetEnabled(UnityEngine.Object uobj, bool enabled)
        {
            ksEntityScript script = uobj as ksEntityScript;
            if (script != null)
            {
                if (script.EnableOnAttach == enabled)
                {
                    return false;
                }
                script.EnableOnAttach = enabled;
                if (script.IsAttached)
                {
                    script.enabled = enabled;
                }
                return true;
            }
            Behaviour behaviour = uobj as Behaviour;
            if (behaviour != null)
            {
                if (behaviour.enabled == enabled)
                {
                    return false;
                }
                behaviour.enabled = enabled;
                return true;
            }
            Collider collider = uobj as Collider;
            if (collider != null)
            {
                if (collider.enabled == enabled)
                {
                    return false;
                }
                collider.enabled = enabled;
                return true;
            }
            Renderer renderer = uobj as Renderer;
            if (renderer != null)
            {
                if (renderer.enabled == enabled)
                {
                    return false;
                }
                renderer.enabled = enabled;
                return true;
            }
            Rigidbody rigidbody = uobj as Rigidbody;
            if (rigidbody != null)
            {
                if (rigidbody.isKinematic != enabled)
                {
                    return false;
                }
                rigidbody.isKinematic = !enabled;
                return true;
            }
            Rigidbody2D rigidbody2D = uobj as Rigidbody2D;
            if (rigidbody2D != null)
            {
                if (rigidbody2D.isKinematic != enabled)
                {
                    return false;
                }
                rigidbody2D.isKinematic = !enabled;
                return true;
            }
            GameObject gameObj = uobj as GameObject;
            if (gameObj != null)
            {
                if (gameObj.activeSelf == enabled)
                {
                    return false;
                }
                gameObj.SetActive(enabled);
                return true;
            }
            return false;
        }
    }
}