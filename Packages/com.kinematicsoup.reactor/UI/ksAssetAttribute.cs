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

using UnityEngine;

namespace KS.Reactor.Client.Unity
{
    /// <summary>
    /// Attribute to indicate a scriptable object browser should only show the assets tab and not
    /// the scene tab. Unity by default opens the scene tab when browsing for scriptable objects,
    /// which is unfortunate because scriptable objects can't exist in the scene.
    /// </summary>
    public class ksAssetAttribute : PropertyAttribute
    {
    }
}
