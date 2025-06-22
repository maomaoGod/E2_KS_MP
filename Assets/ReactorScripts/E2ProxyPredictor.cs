using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

[CreateAssetMenu(menuName = ksMenuNames.REACTOR + "E2ProxyPredictor", order = ksMenuGroups.SCRIPT_ASSETS)]
public class E2ProxyPredictor : ksPredictor
{
    private ksReadOnlyTransformState m_serverState;
    private float m_serverProperty;
    private bool m_teleported = false;
    private Vector3 m_velocity;
    private float m_rotationSpeed;
    private float m_propertySpeed;

    // Initializes the predictor. Return false if initialization fails.
    public override bool Initialize()
    {
        return true;
    }

    // Called when the predictor is removed. Performs clean up.
    public override void Detached()
    {
        // The base method clears the Entity/Room reference.
        base.Detached();
    }

    // Return true if the predictor will predict the property value.
    public override bool IsPredictedProperty(uint propertyId)
    {
        // Predict property 0
        return propertyId == 0;
    }

    // Called once per server frame. Perform any relevant calculations with server frame data here, and/or store
    // the relevant server frame data to be used in calculating future client state in the ClientUpdate method.
    // If it returns false and idle is true, this function will not be called again until the server transform
    // or a server property changes.
    public override bool ServerUpdate(ksReadOnlyTransformState state, Dictionary<uint, ksMultiType> properties, bool teleport, bool idle)
    {
        m_serverState = state;
        m_teleported = teleport;

        // Properties is null if none of the predicted properties changed.
        if (properties != null)
        {
            // Get the server property value.
            ksMultiType property;
            if (properties.TryGetValue(0, out property))
            {
                m_serverProperty = property;
            }
        }
        // State is null if the predictor is predicting room or player properties.
        if (!teleport && state != null && Time.ServerUnscaledDelta > 0f)
        {
            // Calculate rotation interpolation speed.
            m_rotationSpeed = ksQuaternion.DeltaDegrees(state.Rotation, Entity.GameObject.transform.rotation) / Time.ServerUnscaledDelta;
        }
        if (Controller != null)
        {
            // Move the controller to the server position.
            Controller.Transform.Position = state.Position;
            Controller.Transform.Rotation = state.Rotation;
            Controller.Transform.Scale = state.Scale;
            if (Controller.Properties.Contains(0))
            {
                Controller.Properties[0] = m_serverProperty;
            }
            // If there's a controller we want to receive all server updates so we can reposition the controller every time a
            // server update arrives.
            return true;
        }
        // Only call this function when the server values change.
        return false;
    }

    // Called once per client render frame to update the client transform and smoothed properties.
    // If it returns false, this function will not be called again until the server transform or a
    // server property changes.
    public override bool ClientUpdate(ksTransformState state, Dictionary<uint, ksMultiType> properties)
    {
        // Properties is null if there are no predicted properties.
        if (properties != null)
        {
            if (m_teleported)
            {
                // Teleport to the server value.
                properties[0] = m_serverProperty;
                m_propertySpeed = 0f;
            }
            else if (Time.UnscaledDelta > 0f)
            {
                // Move towards the controller property value if there is a controller. Otherwise move towards the server value.
                float targetValue = Controller == null ? m_serverProperty : Controller.Properties[0];
                properties[0] = Mathf.SmoothDamp(properties[0], targetValue, ref m_propertySpeed, Time.UnscaledDelta);
            }
        }
        // State is null if the predictor is predicting room or player properties.
        if (state == null)
        {
            return false;
        }
        state.Scale = m_serverState.Scale;
        if (m_teleported)
        {
            // Teleport to the server position.
            m_teleported = false;
            state.Position = m_serverState.Position;
            state.Rotation = m_serverState.Rotation;
            m_velocity = ksVector3.Zero;
            return Controller != null;
        }
        if (Time.UnscaledDelta > 0f)
        {
            // Move towards the controller position if there is a controller. Otherwise move towards the server position.
            ksVector3 targetPosition = Controller == null ? m_serverState.Position : Controller.Transform.Position;
            ksQuaternion targetRotation = Controller == null ? m_serverState.Rotation : Controller.Transform.Rotation;

            // Smooth damp the position.
            state.Position = Vector3.SmoothDamp(state.Position, targetPosition, ref m_velocity, Time.UnscaledDelta);

            // Linearly interpolate rotation.
            state.Rotation = ksQuaternion.RotateTowards(state.Rotation, targetRotation, m_rotationSpeed * Time.UnscaledDelta);
        }
        // If there's a controller or we haven't reached the server position, we need more client updates.
        return Controller != null || state.Position != m_serverState.Position || state.Rotation != m_serverState.Rotation;
    }

    // Called when a new frame of input is generated. Only called for entities with player controllers.
    public override void InputUpdate(ksInput input)
    {
        // Update the player controller.
        ksPredictorUtils.UpdateController(Entity, Time.UnscaledDelta, Time.TimeScale, input);
        input.CleanUp();
        if (!m_teleported && Time.ServerDelta > 0f)
        {
            // Calculate rotation interpolation speed.
            m_rotationSpeed = Math.Max(m_rotationSpeed,
                ksQuaternion.DeltaDegrees(Controller.Transform.Rotation, Entity.GameObject.transform.rotation) / Time.ServerUnscaledDelta);
        }
    }
}