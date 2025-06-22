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
using UnityEngine;

namespace KS.Reactor.Client.Unity
{
    /// <summary>
    /// Factory for creating game objects for entities. Use this class to implement your own entity factory
    /// and assign it to <see cref="ksRoom.EntityFactory"/>.
    /// </summary>
    public class ksEntityFactory
    {
        /// <summary>
        /// Gets the game object for an entity. The returned game object must have a <see cref="ksEntityComponent"/> 
        /// and must not be assigned to another non-destroyed entity.
        /// </summary>
        /// <param name="entity">The entity to get the game object for.</param>
        /// <param name="prefab">Prefab associated with the entity's asset Id.</param>
        /// <returns>Game object representing the entity.</returns>
        public virtual GameObject GetGameObject(ksEntity entity, GameObject prefab)
        {
            GameObject gameObject;
            if (prefab == null)
            {
                if (entity.AssetId == 0)
                {
                    ksLog.Warning(this, "No scene object found for non-prefab entity with asset id 0.");
                }
                gameObject = new GameObject("Entity");
                gameObject.AddComponent<ksEntityComponent>();
            }
            else
            {
                gameObject = GameObject.Instantiate<GameObject>(prefab);
                gameObject.name = prefab.name;
            }
            return gameObject;
        }

        [Obsolete("Use the overload that takes a ksEntity argument instead.")]
        public virtual GameObject GetGameObject(uint assetId, string entityType, GameObject prefab)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use the GetGameObject overload that takes a ksEntity argument instead.")]
        public virtual void InitializeEntity(ksEntity entity)
        {

        }

        /// <summary>
        /// Called when an entity is destroyed and <see cref="ksEntity.DestroyWithServer"/> is true.
        /// Override this to implement your own destruction logic.
        /// </summary>
        /// <param name="entity">Entity to release the game object from.</param>
        public virtual void ReleaseGameObject(ksEntity entity)
        {
            GameObject.Destroy(entity.GameObject);
        }
    }
}
