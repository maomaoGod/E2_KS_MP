using System;
using System.Collections.Generic;
using E2MultiPlayer;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor;

public class E2ClientAnimSync : ksEntityScript
{
    // If true, will also sync the animation states of each layer. If your layer states are driven by animation
    // parameters, leave this false. If your layer states are set programmatically, set this to true.
    public bool SyncLayerStates = false;
    // The cross fade duration in seconds when changing layer states. This is only used when SyncLayerStates is
    // true.
    public float CrossFadeDuration = .2f;

    private Animator m_animator;
    private int[] m_states;
    private ksMultiType[] m_parameterValues;
    private uint[] m_parameterChangedFlags;

    // Does the local player control the entity's animations?
    public bool IsOwner
    {
        get { return m_states != null; }
    }

    private uint m_OwnerId;
    
    // Use Start instead of Initialize to ensure the ceClientAuthority script is already initialized.
    public void InitAnimSync(Animator animator)
    {
        var clientAuthority = GetComponent<E2ClientEntityScript>();
        if (clientAuthority == null)
        {
            Destroy(this);
            return;
        }
        
        m_animator = animator;
        if (m_animator == null)
        {
            Log.Error("E2ClientAnimSync animator is null");
            Destroy(this);
            return;
        }

        m_OwnerId = clientAuthority.OwnerId;
        Log.Info($"E2ClientAnimSync::InitAnimSync {m_OwnerId} {clientAuthority.IsOwner}");
        if (clientAuthority.IsOwner)
        {
            m_states = new int[m_animator.layerCount];
            m_parameterValues = new ksMultiType[m_animator.parameterCount];
            m_parameterChangedFlags = new uint[1 + (m_animator.parameterCount - 1) / 32];
            clientAuthority.OnSendUpdate += SendChanges;
        }
        else
        {
            if (SyncLayerStates)
            {
                // Bind property change event handers for animation layer states.
                for (int i = 0; i < m_animator.layerCount; i++)
                {
                    BindLayerState(i);
                }
            }
        }
    }

    // Called when the script is detached.
    public override void Detached()
    {
        if (IsOwner)
        {
            var clientAuthority = GetComponent<E2ClientEntityScript>();
            if (clientAuthority != null)
            {
                clientAuthority.OnSendUpdate -= SendChanges;
            }
        }
    }

    // Called every frame.
    private void Update()
    {
        if (IsOwner)
        {
            // Sync animation triggers. These must be checked every frame since they are only true for one frame.
            for (int i = 0; i < m_animator.parameterCount; i++)
            {
                AnimatorControllerParameter parameter = m_animator.GetParameter(i);
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    if (m_animator.GetBool(parameter.nameHash))
                    {
                        Entity.CallRPC(Consts.RPC.ANIMATION_TRIGGER, m_OwnerId, i);
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner)
        {
            var cnt = m_animator.parameterCount;
            for (int i = 0; i < cnt; i++)
            {
                uint propertyId = Consts.Prop.ANIMATION_PARAMS + (uint)i;
                var prop = Entity.Properties[propertyId];
                SetParameterValue(i, prop);
            }
        }
    }


    private void SendChanges()
    {

        if (!IsOwner)
        {
            return;
        }
        
        if (SyncLayerStates)
        {
            // Sync changed animation layer states
            for (int i = 0; i < m_animator.layerCount; i++)
            {
                int state = m_animator.GetCurrentAnimatorStateInfo(i).fullPathHash;
                //if (state != m_states[i])
                {
                    m_states[i] = state;
                    Entity.CallRPC(Consts.RPC.ANIMATION_STATE, m_OwnerId, i, state);
                }
            }
        }

        // Send changed animation parameters
        for (int i = 0; i < m_animator.parameterCount; i++)
        {
            AnimatorControllerParameter parameter = m_animator.GetParameter(i);
            if (parameter.type != AnimatorControllerParameterType.Trigger)
            {
                ksMultiType value = GetParameterValue(parameter);
                //if (!value.Equals(m_parameterValues[i]))
                {
                    // Set a flag indicating this parameter changed.
                    SetParameterChanged(i);
                    m_parameterValues[i] = value;
                   
                }
            }
        }

        // Send all changed parameters in one RPC.
        //args.Insert(0, m_parameterChangedFlags);
        Entity.CallRPC(Consts.RPC.ANIMATION_PARAM, m_parameterValues);
        ClearParameterChangedFlags();
    }

    private ksMultiType GetParameterValue(AnimatorControllerParameter parameter)
    {
        switch (parameter.type)
        {
            case AnimatorControllerParameterType.Trigger:
            case AnimatorControllerParameterType.Bool:
            {
                return m_animator.GetBool(parameter.nameHash);
            }
            case AnimatorControllerParameterType.Float:
            {
                return m_animator.GetFloat(parameter.nameHash);
            }
            case AnimatorControllerParameterType.Int:
            {
                return m_animator.GetInteger(parameter.nameHash);
            }
        }
        return 0;
    }

    private void SetParameterValue(int index, ksMultiType value)
    {
        AnimatorControllerParameter parameter = m_animator.GetParameter(index);
        
        switch (value.Type)
        {
            case ksMultiType.Types.BOOL:
            {
                m_animator.SetBool(parameter.nameHash, value);
                break;
            }
            case ksMultiType.Types.INT:
            {
                m_animator.SetInteger(parameter.nameHash, value);
                break;
            }
            case ksMultiType.Types.FLOAT:
            {
                if (parameter.nameHash == -574273453 ||
                    parameter.nameHash == 699414757 ||
                    parameter.nameHash == 699414757 ||
                    parameter.nameHash == -531067169 ||
                    parameter.nameHash == -434599977)
                {
                    break;
                }
                m_animator.SetFloat(parameter.nameHash, value);
                break;
            }
        }
    }

    // Sets a flag to indicate a parameter changed.
    private void SetParameterChanged(int index)
    {
        int flagIndex = index / 32;
        int bitIndex = index % 32;
        m_parameterChangedFlags[flagIndex] |= 1u << bitIndex;
    }

    // Gets a flag indicating if a parameter changed.
    private bool GetParameterChanged(int index)
    {
        int flagIndex = index / 32;
        int bitIndex = index % 32;
        return (m_parameterChangedFlags[flagIndex] & (1u << bitIndex)) != 0;
    }

    private void ClearParameterChangedFlags()
    {
        for (int i = 0; i < m_parameterChangedFlags.Length; i++)
        {
            m_parameterChangedFlags[i] = 0;
        }
    }

    private void BindLayerState(int index)
    {
        uint propertyId = Consts.Prop.ANIMATION_STATES + (uint)index;
        Entity.OnPropertyChange[propertyId] += (ksMultiType oldValue, ksMultiType newValue) =>
            m_animator.CrossFade(newValue.Int, CrossFadeDuration, index);
        m_animator.Play(Properties[propertyId].Int, index);
    }

    [ksRPC(Consts. RPC.ANIMATION_TRIGGER)]
    private void OnTrigger(uint ownerId, int index)
    {
        if (!IsOwner)
        {
            AnimatorControllerParameter parameter = m_animator.GetParameter(index);
            m_animator.SetTrigger(parameter.nameHash);
        }
    }
}