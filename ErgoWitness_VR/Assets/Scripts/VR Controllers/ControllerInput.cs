﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this class to handle the input given to me from the 
/// VR controller. Put this script on the left/right controlelr 
/// on the Camera Rig.
/// 
/// Used this guide for some help on my first Vive project:
/// https://www.raywenderlich.com/149239/htc-vive-tutorial-unity
/// 
/// Author: Ben Hoffman
/// </summary>
public class ControllerInput : MonoBehaviour
{
    private GameObject objectInHand;                // Use this to keep track of an object that we want to pick up
    private GameObject collidingObject;             // The object that is in our collider
    private SteamVR_TrackedObject trackedObj;       // The tracked object that is the controller

    private SteamVR_Controller.Device Controller    // The device property that is the controller, so that we can tell what index we are on
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    private void Awake()
    {
        // Get the tracked object componenet
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    /// <summary>
    /// Check for input from the Vive controller, and handle either allowing the player
    /// to teleport, or to grab a device object and throw it around
    /// 
    /// Author: Ben Hoffman
    /// </summary>
    private void Update()
    {
        // If we are pulling the trigger on this controller...
        if (Controller.GetHairTriggerDown())
        {
            // Handle the player pulling the trigger
            if (collidingObject)
            {
                GrabObject();
            }
        }

        // IF we release the trigger and we have something in our hand...
        if(Controller.GetHairTriggerUp() && !objectInHand)
        {
            // Release the object in our hand
            if (objectInHand)
            {
                ReleaseObject();
            }
        }

        // When we release the touch pad button, we want to teleport...
        if (Controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            // We want to allow the user to pick a location to teleport to
            Teleport();
        }
    }


    #region Trigger events
    public void OnTriggerEnter(Collider other)
    {
        // Check our colliding object
        CheckCollidingObject(other);
    }

    public void OnTriggerStay(Collider other)
    {
        // Check our colliding object
        CheckCollidingObject(other);
    }

    public void OnTriggerExit(Collider other)
    {
        // If we do not have a colliding object, return
        if (!collidingObject)
        {
            return;
        }
        // Clear our colliding object, because it has left our trigger
        collidingObject = null;
    }
    #endregion


    /// <summary>
    /// Make sure that the object we are given hsa a rigidbody, andthat we do not
    /// already have a colliding object
    /// </summary>
    /// <param name="c">Something that we are colliding with</param>
    private void CheckCollidingObject(Collider c)
    {
        // If we have a colliding object, or this object does not have a RB, then return
        if (collidingObject || !c.GetComponent<Rigidbody>())
        {
            return;
        }
        // Otherwise, we want to set our colliding object reference to this.
        collidingObject = c.gameObject;
    }


    /// <summary>
    /// Grab the object, and query elasticsearch to get more informatino about it
    /// 
    /// Author: Ben Hoffman
    /// </summary>
    private void GrabObject()
    {
        // Add a fixed joint to the object that we are collidnig with
        objectInHand = collidingObject;
        // Set our colliding object to null, because now it is in our hand
        collidingObject = null;

        // Add a fixed joint to our game object
        var joint = AddFixedJoint();
        // Connect our object in our hand to that fixed joint
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Add a fixed joint to our rigidbody
    /// </summary>
    /// <returns></returns>
    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    /// <summary>
    /// 
    /// </summary>
    private void ReleaseObject()
    {
        // If we have a fixed joint...
        if (GetComponent<FixedJoint>())
        {
            // Disconnect the fixed joint from it's conencted body
            GetComponent<FixedJoint>().connectedBody = null;
            // Destroy the fixed joint component
            Destroy(GetComponent<FixedJoint>());
            // Set our object in our hands velocity to the controllers, so that we can throw it
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            // Do the same thing with the angular velocity, so it does the right way
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
        }

        // We no longer have something in our hand, so set it to null
        objectInHand = null; 
    }

    /// <summary>
    /// Use a raycast to point where this device is looking right now
    /// Display a marker area to show where the plaeyr will be goin
    /// 
    /// 
    /// Author: Ben Hoffman
    /// </summary>
    /// <param name="location"></param>
    private void Teleport()
    {
        Debug.Log(gameObject.name + " Trackpad Press");

    }


}
