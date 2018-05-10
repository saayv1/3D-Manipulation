/*
Copyright ©2017. The University of Texas at Dallas. All Rights Reserved. 

Permission to use, copy, modify, and distribute this software and its documentation for 
educational, research, and not-for-profit purposes, without fee and without a signed 
licensing agreement, is hereby granted, provided that the above copyright notice, this 
paragraph and the following two paragraphs appear in all copies, modifications, and 
distributions. 

Contact The Office of Technology Commercialization, The University of Texas at Dallas, 
800 W. Campbell Road (AD15), Richardson, Texas 75080-3021, (972) 883-4558, 
otc@utdallas.edu, https://research.utdallas.edu/otc for commercial licensing opportunities.

IN NO EVENT SHALL THE UNIVERSITY OF TEXAS AT DALLAS BE LIABLE TO ANY PARTY FOR DIRECT, 
INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING 
OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF THE UNIVERSITY OF TEXAS AT 
DALLAS HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

THE UNIVERSITY OF TEXAS AT DALLAS SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT 
NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS 
PROVIDED "AS IS". THE UNIVERSITY OF TEXAS AT DALLAS HAS NO OBLIGATION TO PROVIDE 
MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
*/

using UnityEngine;
using System.Collections;

public class VirtualHand : MonoBehaviour
{

    // Enumerate states of virtual hand interactions
    public enum VirtualHandState
    {
        Open,
        Touching,
        Holding,
        Closed,
        Destroy
    };

    // Inspector parameters
    [Tooltip("The tracking device used for tracking the real hand.")]
    public CommonTracker tracker;
    
    [Tooltip("The tracking device used for tracking the right hand.")]
    public CommonTracker rightTracker;
    
    [Tooltip("The interactive used to represent the  virtual left hand.")]
    public Affect hand;

    [Tooltip("The interactive used to represent the  virtual right hand.")]
    public Affect rightHand;

    [Tooltip("The button required to be pressed to grab objects.")]
    public CommonButton button;
    
    [Tooltip("The button required to be pressed to destroy objects. This is the button on the right tracker")]
    public CommonButton rightButton;

    [Tooltip("The collider which represents the object which is currently being interacted with")]
    public Collider left_target;
    
    [Tooltip("The speed amplifier for thrown objects. One unit is physically realistic.")]
    public float speed = 1.0f;

    // Private interaction variables
    VirtualHandState state;
    FixedJoint grasp;

    // Called at the end of the program initialization
    void Start()
    {
        // Set initial state to open
        state = VirtualHandState.Open;

        // Ensure hand interactive is properly configured
        hand.type = AffectType.Virtual;
        rightHand.type = AffectType.Virtual;
    }

    // FixedUpdate is not called every graphical frame but rather every physics frame
    void FixedUpdate()
    {

        // If state is open
        if (state == VirtualHandState.Open)
        {

            // If the hand is touching something
            if (hand.triggerOngoing)
            {

                // Change state to touching
                state = VirtualHandState.Touching;
            }
            //If the hand is not touching anything but the trigger button is pressed
            else if (button.GetPress())
            {
                //change the hand interactive to physical
                hand.type = AffectType.Physical;

                //change the state to closed
                state = VirtualHandState.Closed;
            }

            // Process current open state
            else
            {

                // Nothing to do for open
            }
        }

        // If state is touching
        else if (state == VirtualHandState.Touching)
        {

            // If the hand is not touching something
            if (!hand.triggerOngoing)
            {

                // Change state to open
                state = VirtualHandState.Open;
            }

            // If the hand is touching something and the button is pressed
            else if (hand.triggerOngoing && button.GetPress())
            {

                // Fetch touched target
                left_target = hand.ongoingTriggers[0];
                // Create a fixed joint between the hand and the target
                grasp = left_target.gameObject.AddComponent<FixedJoint>();
                // Set the connection
                grasp.connectedBody = hand.gameObject.GetComponent<Rigidbody>();
                // Change state to holding
                state = VirtualHandState.Holding;
            }
            // Process current touching state
            else
            {

                //Nothing to do for touching

            }
        }

        // If state is holding
        else if (state == VirtualHandState.Holding)
        {

            // If grasp has been broken
            if (grasp == null)
            {

                // Update state to open
                state = VirtualHandState.Open;
            }

            // If button has been released and grasp still exists
            else if (!button.GetPress() && grasp != null)
            {

                // Get rigidbody of grasped target
                Rigidbody target = grasp.GetComponent<Rigidbody>();
                // Break grasp
                DestroyImmediate(grasp);

                // Apply physics to target in the event of attempting to throw it
                target.velocity = hand.velocity * speed;
                target.angularVelocity = hand.angularVelocity * speed;

                // Update state to open
                state = VirtualHandState.Open;
            }
            // Process current holding state
            else
            {
                if (rightHand.triggerOngoing)
                {
                    Collider t = rightHand.ongoingTriggers[0];
                    if (GameObject.ReferenceEquals(left_target.gameObject, t.gameObject))
                    {
                        if (rightButton.GetPress())
                        {
                            state = VirtualHandState.Destroy;
                        }
                    }
                }
            }
        }

        //if the state is destroy
        else if (state == VirtualHandState.Destroy)
        {
            Collider t = rightHand.ongoingTriggers[0];
            DestroyImmediate(grasp);
            Destroy(t.gameObject);
            //Destroy(left_target.gameObject);

            //changing back to open
            state = VirtualHandState.Open;
        }

        else if (state == VirtualHandState.Closed)
        {
            //if the user is not pressing the button
            if (!button.GetPress())
            {
                //update hand interactive to virtual
                hand.type = AffectType.Virtual;

                //Update state to open
                state = VirtualHandState.Open;

            }
            //Process Current closed state
            else
            {
                //Nothing to do for closed   
            }
        }

    }
}
