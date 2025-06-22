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
    /// Component that manages ksMonoScripts. <see cref="ksMonoScript{Parent, Script}"/> will look for a 
    /// component when initialized.
    /// </summary>
    /// <typeparam name="Parent">The type the scripts are attached to.</typeparam>
    /// <typeparam name="Script">The script type.</typeparam>
    public class ksScriptsComponent<Parent, Script> : MonoBehaviour
        where Parent : class
        where Script : ksMonoScript<Parent, Script>
    {
        /// <summary>Attached event handler.</summary>
        public delegate void AttachedHandler();

        /// <summary>Invoked after scripts are attached.</summary>
        public event AttachedHandler OnAttached;

        /// <summary>Parent the scripts are attached to.</summary>
        public Parent ParentObject
        {
            get { return m_object; }
            set { m_object = value; }
        }
        private Parent m_object;

        /// <summary>List of attached scripts.</summary>
        public ksLinkedList<Script> Scripts
        {
            get { return m_scripts; }
        }
        private ksLinkedList<Script> m_scripts = new ksLinkedList<Script>();

        /// <summary>
        /// Are the scripts attached? If true, new scripts will be added to the script list and have
        /// <see cref="ksMonoScript{Parent, Script}.Attached"/> called when they are added to the game object.
        /// </summary>
        public bool ScriptsAttached
        {
            get { return m_scriptsAttached; }
        }
        private bool m_scriptsAttached = false;

        /// <summary>
        /// Are the scripts initialized? If true, new scripts will be initialized when they are attached.
        /// </summary>
        public bool IsInitialized
        {
            get { return m_initialized; }
        }
        private bool m_initialized = false;

        /// <summary>
        /// Finds <see cref="Script"/>s on the game object and adds them to the  script list and calls their
        /// <see cref="ksMonoScript{Parent, Script}.Attached"/> method. Then invokes the <see cref="OnAttached"/>
        /// method.
        /// </summary>
        internal void AttachScripts()
        {
            if (!m_scriptsAttached)
            {
                m_scriptsAttached = true;
                foreach (Script script in GetComponents<Script>())
                {
                    script.Component = this;
                    m_scripts.Add(script);
                    if (script.EnableOnAttach)
                    {
                        // Set enabled to true before calling Attached so if Attached sets it to false, it won't be set
                        // back to true after.
                        script.enabled = true;
                    }
                    script.Attached();
                }
                
                // Invoke the on attached event
                if (OnAttached != null)
                {
                    try
                    {
                        OnAttached();
                    }
                    catch (Exception ex)
                    {
                        ksExceptionHandler.Handle("Exception caught invoking OnAttached", ex);
                    }
                }
            }
        }

        /// <summary>Initializes scripts.</summary>
        internal void InitializeScripts()
        {
            if (!m_initialized)
            {
                foreach (Script script in m_scripts)
                {
                    ksRPCManager<ksRPCAttribute>.Instance.RegisterTypeRPCs(script.InstanceType);
                    if (script.enabled)
                    {
                        script.IsInitialized = true;
                        script.Initialize();
                    }
                }
                m_initialized = true;
            }
        }

        /// <summary>Detaches all scripts and resets the component to its uninitialized state.</summary>
        internal void CleanUp()
        {
            DetachScripts();
            m_object = null;
            m_initialized = false;
            m_scriptsAttached = false;
        }

        /// <summary>Detaches and disables all scripts when the component is destroyed.</summary>
        protected virtual void OnDestroy()
        {
            DetachScripts();
        }

        /// <summary>Detaches and disables all scripts.</summary>
        private void DetachScripts()
        {
            foreach (Script script in m_scripts)
            {
                script.Detached();
                script.IsInitialized = false;
                script.enabled = false;
                script.Component = null;
            }
            m_scripts.Clear();
        }
    }
}
