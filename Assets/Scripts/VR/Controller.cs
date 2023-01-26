using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using World;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> { }

public class Controller : MonoBehaviour
{
    public PrimaryButtonEvent primaryButtonPress;

    private bool lastAButtonState = false;
    private bool lastTriggerButtonState = false;
    private List<InputDevice> devicesWithPrimaryButton;

    public Transform LeftController;
    public Transform RightController;
    public Transform Headset;
    public GameObject WorldCreatorGO;
    public GameObject RightControllerModelGO;
    public GameObject FireEffectGO;
    protected GameObject FireEffectActive;

    Vector2 primAxisLast = new Vector2(0, 0);
    bool primAxisCenter = true;

    private void Awake()
    {
        if (primaryButtonPress == null)
        {
            primaryButtonPress = new PrimaryButtonEvent();
        }

        devicesWithPrimaryButton = new List<InputDevice>();
    }

    void OnEnable()
    {
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        devicesWithPrimaryButton.Clear();
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        bool discardedValue;
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out discardedValue))
        {
            devicesWithPrimaryButton.Add(device); // Add any devices that have a primary button.
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (devicesWithPrimaryButton.Contains(device))
            devicesWithPrimaryButton.Remove(device);
    }

    void Update()
    {
        bool tempAState = false;
        bool tempTriggerState = false;
        Vector2 primAxis = new Vector2(0, 0);
        foreach (var device in devicesWithPrimaryButton)
        {
            bool primaryButtonState = false;
            tempAState = device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonState) // A pressed
                        && primaryButtonState // the value we got
                        || tempAState; // cumulative result from other controllers

            bool triggerButtonState = false;
            tempTriggerState = device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonState) // trigger pressed
                        && triggerButtonState // the value we got
                        || tempTriggerState; // cumulative result from other controllers

            bool ret = device.TryGetFeatureValue(CommonUsages.primary2DAxis, out primAxis);
        }

        if (tempAState != lastAButtonState) // Button state changed since last frame
        {
            OnAButton(tempAState);
            lastAButtonState = tempAState;
        }

        if(primAxis != primAxisLast)
        {
            OnPrimAxisButton(primAxis - primAxisLast, primAxis);
            primAxisLast = primAxis;
        }

        if (tempTriggerState != lastTriggerButtonState) // Button state changed since last frame
        {
            OnTriggerButton(tempTriggerState);
            lastTriggerButtonState = tempTriggerState;
        }

        if (FireEffectActive)
        {
            FireEffectActive.transform.position = RightController.position;
            WorldCreator worldCreator = WorldCreatorGO.GetComponent<WorldCreator>();
            World.World world = worldCreator.worlds[0];
            world.m_vScarePosition = RightController.position;
        }
    }

    private int WhatSpawnObject()
    {
        if (RightControllerModelGO.GetComponent<MeshRenderer>().material.color == Color.red)
            return 1;
        if (RightControllerModelGO.GetComponent<MeshRenderer>().material.color == Color.blue)
            return 2;
        if (RightControllerModelGO.GetComponent<MeshRenderer>().material.color == Color.green)
            return 3;
        return 0;
    }
    private void OnPrimAxisButton(Vector2 change, Vector2 changeTo)
    {
        int spawnObj = WhatSpawnObject();
        Color nextColor = Color.red;
        if (spawnObj == 1)
            nextColor = Color.blue;
        if (spawnObj == 2)
            nextColor = Color.green;
        if (spawnObj == 3)
            nextColor = Color.red;
        if (changeTo.x > 0.5f && primAxisCenter)
        {
            RightControllerModelGO.GetComponent<MeshRenderer>().material.color = nextColor;
            primAxisCenter = false;
        }

        if (Mathf.Abs(changeTo.x) < 0.5f && !primAxisCenter)
        {
            primAxisCenter = true;
        }
    }

    private void OnTriggerButton(bool bChangeTo)
    {
        WorldCreator worldCreator = WorldCreatorGO.GetComponent<WorldCreator>();
        World.World world = worldCreator.worlds[0];

        if (!bChangeTo)
        {
            world.m_vScarePosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Destroy(FireEffectActive);
            FireEffectActive = null;
            return;
        }

        if(FireEffectActive)
        {
            Destroy(FireEffectActive);
            FireEffectActive = null;
        }
        FireEffectActive = Instantiate(FireEffectGO);

    }

    private void OnAButton(bool bChangeTo)
    {
        if (!bChangeTo)
            return;

        WorldCreator worldCreator = WorldCreatorGO.GetComponent<WorldCreator>();
        World.World world = worldCreator.worlds[0];

        int spawnObj = WhatSpawnObject();
        if (spawnObj == 1)
            world.AddFox(RightController.position);
        if (spawnObj == 2)
            world.AddRabbit(RightController.position);
        if (spawnObj == 3)
            world.AddGrass(RightController.position);

    }
}