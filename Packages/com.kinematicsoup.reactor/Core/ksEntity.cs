﻿/*
KINEMATICSOUP CONFIDENTIAL
 Copyright(c) 2014-2024 KinematicSoup Technologies Incorporated 
 All Rights Reserved.

NOTICE:  All information contained herein is, and remains the property of 
KinematicSoup Technologies Incorporated and its suppliers, if any. The 
intellectual and technical concepts contained herein are proprietary to 
KinematicSoup Technologies Incorporated and its suppliers and may be covered by
U.S. and Foreign Patents, patents in process, and are protected by trade secret
or copyright law. Dissemination of this information or reproduction of this
material is strictly forbidden unless prior written permission is obtained from
KinematicSoup Technologies Incorporated.
*/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KS.Reactor.Client.Unity
{
    /// <summary>
    /// Unity implementation of ksBaseEntity. Entities are objects create, updated, and destroyed by a 
    /// server and replicated to clients.
    /// </summary>
    public class ksEntity : ksBaseEntity
    {
        /// <summary>GameObject that represents this entity.</summary>
        public GameObject GameObject
        {
            get { return m_gameObject; }
        }
        private GameObject m_gameObject;

        /// <summary>
        /// The transform to sync. By default this is the transform from the <see cref="ksEntityComponent"/>'s game
        /// object. Setting it to null will set it back to the transform from the <see cref="ksEntityComponent"/>'s
        /// game object.
        /// </summary>
        public Transform SyncTarget
        {
            get { return m_syncTarget; }
            set
            {
                m_syncTarget = value == null && m_gameObject != null ? m_gameObject.transform : value;
            }
        }
        private Transform m_syncTarget;

        /// <summary>Room the entity belongs to.</summary>
        public ksRoom Room
        {
            get { return (ksRoom)BaseRoom; }
        }

        /// <summary>
        /// If true then the Unity game object will be destroyed when this entity is destroyed by the server.
        /// </summary>
        public bool DestroyWithServer
        {
            get { return m_component == null ? true : m_component.DestroyWithServer; }
            set { if (m_component != null) { m_component.DestroyWithServer = value; } }
        }

        /// <summary>
        /// If false, the entity's transform update are not applied to the game object. You can still access the
        /// smoothed entity transform using <see cref="ksBaseEntity.Transform"/> or the server transform using
        /// <see cref="ksBaseEntity.ServerTransform"/>.
        /// </summary>
        public bool ApplyTransformUpdates
        {
            get { return m_applyTransformUpdates; }
            set 
            {
                if (m_applyTransformUpdates == value)
                {
                    return;
                }
                m_applyTransformUpdates = value;
                if (value)
                {
                    UpdatePosition();
                    UpdateRotation();
                    UpdateScale();
                }
            }
        }
        private bool m_applyTransformUpdates = true;

        /**
         * The entity's rigid body. Null if the entity does not have a rigid body.
         */
        public new ksUnityRigidBody RigidBody
        {
            get { return (ksUnityRigidBody)base.RigidBody; }
            set { base.RigidBody = value; }
        }

        /**
         * The entity's 2D rigid body. Null if the entity does not have a 2D rigid body.
         */
        public new ksUnityRigidBody2DView RigidBody2D
        {
            get { return (ksUnityRigidBody2DView)base.RigidBody2D; }
            set { base.RigidBody2D = value; }
        }

        /**
         * Was this entity part of the initial scene? If false, the entity was spawned at runtime.
         */
        public bool IsSceneEntity
        {
            get { return m_isSceneEntity; }
        }
        private bool m_isSceneEntity = false;
        private ksEntityComponent m_component;

        /// <summary>Was the game object for this entity destroyed?</summary>
        public override bool IsDestroyedLocally
        {
            get { return m_gameObject == null && (object)m_gameObject != null; }
        }

        private CharacterController m_unityCharacterController = null;

        /// <summary>Initializes a permanent entity.</summary>
        /// <param name="component">Entity component with entity data.</param>
        /// <param name="room">Room the entity belongs to.</param>
        internal void InitializePermanentEntity(ksEntityComponent component, ksBaseRoom room)
        {
            m_component = component;
            m_gameObject = component.gameObject;
            m_syncTarget = m_gameObject.transform;
            Type = component.Type;
            m_isSceneEntity = true;

            ksTransformState transform = new ksTransformState();
            transform.Position = component.transform.position;
            transform.Rotation = component.transform.rotation;
            transform.Scale = component.transform.localScale;

            Initialize(component.EntityId, component.AssetId, transform, room);
        }

        /// <summary>
        /// Creates or finds the game object for the entity if it is not already set, and initializes the entity with
        /// data from the game object's entity component and physics components.
        /// </summary>
        protected override void Initialize()
        {
            InitializeGameObject();
            m_component.ParentObject = this;
            m_component.EntityId = Id;
            CollisionFilter = m_component.GetCollisionFilter();
            if (m_syncTarget == null)
            {
                m_syncTarget = m_component.SyncTarget == null ? m_gameObject.transform : m_component.SyncTarget;
            }

            m_unityCharacterController = m_gameObject.GetComponent<CharacterController>();
            if (m_unityCharacterController != null)
            {
                ksColliderData colliderData = m_component.GetColliderData(m_unityCharacterController);
                if (colliderData != null && colliderData.Existence != ksExistenceModes.SERVER_ONLY)
                {
                    CharacterController = new ksUnityCharacterController(this, m_unityCharacterController);
                }
            }

            Rigidbody rigidbody = m_gameObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                if (m_component.RigidBodyExistence == ksExistenceModes.SERVER_ONLY)
                {
                    Component.Destroy(rigidbody);
                }
                else
                {
                    List<ksIUnityCollider> tempTriggers = null;
                    if (m_component.RigidBodyExistence == ksExistenceModes.CLIENT_AND_SERVER)
                    {
                        // Make non-simulation and client-only colliders into triggers temporarily so they are excluded
                        // from center of mass calculations.
                        tempTriggers = new List<ksIUnityCollider>();
                        foreach (ksIUnityCollider collider in m_component.GetColliders())
                        {
                            if (!collider.IsTrigger &&
                                (!collider.IsSimulationCollider || collider.Existance == ksExistenceModes.CLIENT_ONLY))
                            {
                                collider.IsTrigger = true;
                                tempTriggers.Add(collider);
                            }
                        }
                    }
                    // Constructing the wrapper will make the rigidbody kinematic, which must be done after we make the
                    // non-simulation/client-only colliders into triggers in order for center of mass to be updated.
                    ksRigidBodyModes rigidBodyType = m_component.PhysicsOverrides.RigidBodyMode;
                    if (rigidBodyType == ksRigidBodyModes.DEFAULT)
                    {
                        rigidBodyType = ksBaseUnityRigidBody.DefaultType;
                    }
                    if (rigidBodyType == ksRigidBodyModes.RIGID_BODY_2D)
                    {
                        RigidBody2D = new ksUnityRigidBody2DView(rigidbody, 
                            m_component.RigidBodyExistence == ksExistenceModes.CLIENT_AND_SERVER);
                    }
                    else
                    {
                        RigidBody = new ksUnityRigidBody(rigidbody,
                            m_component.RigidBodyExistence == ksExistenceModes.CLIENT_AND_SERVER);
                    }

                    if (tempTriggers != null)
                    {
                        for (int i = 0; i < tempTriggers.Count; i++)
                        {
                            tempTriggers[i].IsTrigger = false;
                        }
                    }
                }
            }

            // Destroy server-only colliders.
            foreach (ksColliderData colliderData in m_gameObject.GetComponents<ksColliderData>())
            {
                if (colliderData.Collider == null)
                {
                    Component.Destroy(colliderData);
                }
                else if (colliderData.Existence == ksExistenceModes.SERVER_ONLY)
                {
                    Component.Destroy(colliderData.Collider);
                    Component.Destroy(colliderData);
                }
            }

            // Destroy all joints connected to the world (null) or to other game objects with ksEntityComponents
            Joint[] joints = m_gameObject.GetComponents<Joint>();
            if (joints != null)
            {
                foreach (Joint joint in joints)
                {
                    if (joint.connectedBody == null || joint.connectedBody.GetComponents<ksEntityComponent>() != null)
                    {
                        GameObject.Destroy(joint);
                    }
                }
            }

            // Initialize transform
            UpdatePosition();
            UpdateRotation();
            UpdateScale();
        }

        /// <summary>
        /// Finds <see cref="ksEntityScript"/>s on the entity's game object and adds them to the 
        /// <see cref="ksScriptsComponent{Parent, Script}"/> and calls their
        /// <see cref="ksMonoScript{Parent, Script}.Attached"/> method.
        /// </summary>
        protected override void AttachScripts()
        {
            if (m_component != null)
            {
                m_component.AttachScripts();
            }
        }

        /// Finds or creates the game object for this entity. First checks the <see cref="ksEntityLinker"/> for a game
        /// object in the scene with an entity component with the same id as the entity. If none is found, gets an
        /// entity factory from the room for this entity's type and uses it to create the game object.
        /// </summary>
        private void InitializeGameObject()
        {
            if (m_gameObject != null)
            {
                return;
            }

            Type = ksReactor.PrefabCache.GetEntityType(AssetId);

            // Check if there's a game object in the scene for this entity
            if (Room.IsSceneLoaded)
            {
                m_gameObject = ksReactor.GetEntityLinker(Room.GameObject.scene).GetGameObjectForEntity(Id);
                if (ValidateGameObject() && m_component.AssetId == AssetId)
                {
                    m_isSceneEntity = true;
                    m_gameObject.transform.parent = Room.GameObject.transform;
                    return;
                }
                m_gameObject = null;
            }

            m_isSceneEntity = false;
            ksEntityFactory factory;

            // Find and use a factory to create the gameobject.
            try
            {
                GameObject prefab = ksReactor.PrefabCache.GetPrefab(AssetId, Type);
                factory = Room.GetEntityFactory(AssetId, Type);
                m_gameObject = factory.GetGameObject(this, prefab);
            }
            catch (Exception e)
            {
                ksLog.Error(this, "Error creating entity game object with id " + Id, e);
            }

            if (!ValidateGameObject())
            {
                string type = string.IsNullOrEmpty(Type) ? AssetId.ToString() : Type;
                m_gameObject = new GameObject("Missing Entity (" + type + ")");
                m_component = m_gameObject.AddComponent<ksEntityComponent>();
            }
            if (m_gameObject.transform.parent == null)
            {
                m_gameObject.transform.parent = Room.GameObject.transform;
            }
            m_component.IsPermanent = false;
        }

        /// <summary>
        /// Validates the entity's game object. If the entity is valid, sets the component to the entity component from
        /// the game object. To be valid, the game object must:
        /// - not be null.
        /// - not be a prefab.
        /// - not be assigned to a non-destroyed entity.
        /// - have a <see cref="ksEntityComponent"/> component.
        /// </summary>
        /// <returns>True if the game object is a valid entity</returns>
        private bool ValidateGameObject()
        {
            if (m_gameObject == null)
            {
                return false;
            }
            m_component = m_gameObject.GetComponent<ksEntityComponent>();
            if (m_component == null)
            {
                ksLog.Error(this, "Entity GameObject '" + m_gameObject.name + "' does not have a ksEntityComponent. " +
                    "Your entity factory must return game objects with ksEntityComponents.");
                GameObject.Destroy(m_gameObject);
                return false;
            }
            if (m_component.Entity != null && !m_component.Entity.IsDestroyed)
            {
                ksLog.Error(this, "Entity GameObject '" + m_gameObject.name + "' belongs to another entity. Your " +
                    "entity factory cannot return game objects that belong to non-destroyed entities.");
                return false;
            }
            if (m_gameObject.scene.rootCount == 0 || ksReactor.PrefabCache.IsPseudoPrefab(m_gameObject))
            {
                ksLog.Error(this, "Entity GameObject '" + m_gameObject.name + "' is a prefab. Your entity factory " +
                    "must return instantiated game objects.");
                return false;
            }
            return true;
        }

        /// <summary>Initializes all <see cref="ksEntityScript"/> attached to this entity.</summary>
        protected override void InitializeScripts()
        {
            if (m_component != null)
            {
                m_component.InitializeScripts();
            }
        }

        /// <summary>
        /// Cleans up the entity and returns it to a pool of reusable entity resources.
        /// </summary>
        /// <param name="isSyncGroupRemoval">
        /// True when the entity is destroyed because it is in a different sync group than the local player.
        /// </param>
        protected override void Destroy(bool isSyncGroupRemoval)
        {
            if (!DestroyWithServer && Room.IsConnected)
            {
                return;
            }
            if (m_component != null && !m_component.IsPermanent)
            {
                if (!m_isSceneEntity)
                {
                    Room.GetEntityFactory(AssetId, Type).ReleaseGameObject(this);
                }
                else
                {
                    ksReactor.GetEntityLinker(m_gameObject.scene).TrackEntity(m_component);
                }
            }
            Recycle();
        }

        /// <summary>
        /// Clears entity data and returns it to the object pool for reuse. Does nothing if the entity is not
        /// destroyed. You should only need to call this function if you have <see cref="DestroyWithServer"/> set to
        /// false and you want to disconnect the entity from the game object and reuse it without destroying the game
        /// object.
        /// </summary>
        public void Recycle()
        {
            // Check if component is null to prevent double recycling.
            if (!IsDestroyed || (object)m_component == null)
            {
                return;
            }
            m_component.CleanUp();
            m_component = null;
            if (!ksObjectPool<ksEntity>.Instance.IsFull)
            {
                CleanUp();
                m_gameObject = null;
                m_syncTarget = null;
                ksObjectPool<ksEntity>.Instance.Return(this);
            }
        }

        /// <summary>
        /// Get the collider data component for a collider. Optionally creates collider data is none is found and
        /// <paramref name="collider"/> is one of the supported collider types.
        /// </summary>
        /// <param name="collider">Collider to get collider data for.</param>
        /// <param name="create">
        /// True to create the collider data if it is not found and <paramref name="collider"/> is one of the supported
        /// collider types.
        /// </param>
        /// <returns>
        /// Collider data for the collider. Null if the collider is not one of the supported types or
        /// <paramref name="create"/> was false and none was found.
        /// </returns>
        public ksColliderData GetColliderData(Collider collider, bool create = true)
        {
            return m_component == null ? null : m_component.GetColliderData(collider, create);
        }

        /// <summary>Destroys a collider and its associated <see cref="ksColliderData"/>.</summary>
        /// <param name="collider">Collider to destroy</param>
        public void DestroyCollider(Collider collider)
        {
            if (m_component != null)
            {
                m_component.DestroyCollider(collider);
            }
        }

        /// <summary>Applies the entity's position to the entity's game object.</summary>
        protected override void UpdatePosition()
        {
            if (m_syncTarget == null || !m_applyTransformUpdates)
            {
                return;
            }
            if (m_unityCharacterController != null)
            {
                m_unityCharacterController.enabled = false;
            }
            m_syncTarget.position = Transform.Position;
            if (m_unityCharacterController != null)
            {
                m_unityCharacterController.enabled = true;
            }
        }

        /// <summary>Applies the entity's rotation to the entity's game object.</summary>
        protected override void UpdateRotation()
        {
            if (m_syncTarget != null && m_applyTransformUpdates)
            {
                m_syncTarget.rotation = Transform.Rotation;
            }
        }

        /// <summary>Applies the entity's scale to the entity's game object.</summary>
        protected override void UpdateScale()
        {
            if (m_syncTarget != null && m_applyTransformUpdates)
            {
                m_syncTarget.localScale = Transform.Scale;
            }
        }

        /// <summary>
        /// Updates <paramref name="transform"/> with the values from the game object's transform, setting dirty flags
        /// for values that are different. This is called on locally-owned entities with the 
        /// <see cref="ksOwnerPermissions.TRANSFORM"/> permission to get a transform update to send to the server.
        /// </summary>
        /// <param name="transform">Transform to update.</param>
        protected override void GetTransformUpdate(ksTransformState transform)
        {
            if (m_syncTarget != null)
            {
                // Calling the transform setters will set a dirty flag even if the value did not change, so we check if
                // the values changed before calling the setter.
                if (transform.Position != m_syncTarget.position)
                {
                    transform.Position = m_syncTarget.position;
                }
                if (transform.Rotation != m_syncTarget.rotation)
                {
                    transform.Rotation = m_syncTarget.rotation;
                }
                if (transform.Scale != m_syncTarget.localScale)
                {
                    transform.Scale = m_syncTarget.localScale;
                }
            }
        }

        /// <summary>
        /// Creates a predictor to use when the entity does not have a player controller with input prediction enabled.
        /// If <see cref="ksEntityComponent.OverridePredictor"/> is true, creates an instance from 
        /// <see cref="ksEntityComponent.Predictor"/>. Otherwise if the room type exists, uses 
        /// <see cref="ksRoomType.DefaultEntityPredictor"/>. Otherwise calls the base function to return a
        /// <see cref="ksLinearPredictor"/>.
        /// </summary>
        /// <param name="predictor">The predictor created</param>
        /// <returns>False if the predictor could not be created because the component is null.</returns>
        protected override bool CreateNonInputPredictor(out ksIPredictor predictor)
        {
            if (m_component == null)
            {
                predictor = null;
                return false;
            }
            ksPredictor predictorAsset;
            if (m_component.OverridePredictor)
            {
                predictorAsset = m_component.Predictor;
            }
            else if (Room.RoomType != null)
            {
                predictorAsset = Room.RoomType.DefaultEntityPredictor;
            }
            else
            {
                return base.CreateNonInputPredictor(out predictor);
            }
            if (predictorAsset != null)
            {
                predictor = predictorAsset.CreateInstance();
                // If the input predictor is not assigned and it uses the same predictor asset, assign the
                // instance to be the input predictor as well.
                if (!IsInputPredictorAssigned)
                {
                    ksPredictor controllerPredictor = m_component.OverrideControllerPredictor ?
                        m_component.ControllerPredictor :
                        Room.RoomType == null ? null : Room.RoomType.DefaultEntityControllerPredictor;
                    if (predictorAsset == controllerPredictor)
                    {
                        InputPredictor = predictor;
                    }
                }
                return true;
            }
            predictor = null;
            return true;
        }

        /// <summary>
        /// Creates a predictor to use when the entity has a player controller with input prediction enabled.
        /// If <see cref="ksEntityComponent.OverrideControllerPredictor"/> is true, creates an instance from 
        /// <see cref="ksEntityComponent.ControllerPredictor"/>. Otherwise if the room type exists, uses 
        /// <see cref="ksRoomType.DefaultEntityControllerPredictor"/>. Otherwise calls the base function to return a
        /// <see cref="ksLinearPredictor"/>.
        /// </summary>
        /// <param name="predictor">The predictor created.</param>
        /// <returns>False if the predictor could not be created because the component is null.</returns>
        protected override bool CreateInputPredictor(out ksIPredictor predictor)
        {
            if (m_component == null)
            {
                predictor = null;
                return false;
            }
            ksPredictor predictorAsset;
            if (m_component.OverrideControllerPredictor)
            {
                predictorAsset = m_component.ControllerPredictor;
            }
            else if (Room.RoomType != null)
            {
                predictorAsset = Room.RoomType.DefaultEntityControllerPredictor;
            }
            else
            {
                return base.CreateInputPredictor(out predictor);
            }
            if (predictorAsset != null)
            {
                predictor = predictorAsset.CreateInstance();
                // If the non input predictor is not assigned and it uses the same predictor asset, assign the
                // instance to be the non input predictor as well.
                if (!IsNonInputPredictorAssigned)
                {
                    ksPredictor noControllerPredictor = m_component.OverridePredictor ? m_component.Predictor :
                        Room.RoomType == null ? null : Room.RoomType.DefaultEntityPredictor;
                    if (predictorAsset == noControllerPredictor)
                    {
                        NonInputPredictor = predictor;
                    }
                }
                return true;
            }
            predictor = null;
            return true;
        }

        /// <summary>
        /// Our non-standard colliders generate collision meshes which are attached as hidden children to the
        /// game object. We attach this indicator to those child game objects to indicate that we should check
        /// the parent to find the <see cref="ksEntityComponent"/> for that game object.
        /// </summary>
        public class Indicator : MonoBehaviour
        {
            /// <summary>Collider that created the child game object this indicator is attached to.</summary>
            public ksIUnityCollider Collider;
        }

        /// <summary>Invoke managed RPCs on all entity scripts.</summary>
        /// <param name="rpcId">ID of the RPC to invoke</param>
        /// <param name="rpcArgs">Arguments to pass to the RPC handlers</param>
        /// <returns>True if the method invoked any RPC handler</returns>
        protected override bool InvokeRPC(uint rpcId, ksMultiType[] rpcArgs)
        {
            bool called = false;
            if (m_component != null && ksRPCManager<ksRPCAttribute>.Instance.HasRegisteredRPC(rpcId))
            {
                foreach (ksEntityScript script in m_component.Scripts)
                {
                    called |= ksRPCManager<ksRPCAttribute>.Instance.InvokeRPC(
                        rpcId, script, script.InstanceType, Room, rpcArgs);
                }
            }
            return called;
        }
    }
}
