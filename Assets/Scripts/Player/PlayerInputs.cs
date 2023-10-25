using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
//[RequireComponent(typeof(PlayerMovements))]
public class PlayerInputs : MonoBehaviour
{
    [SerializeField]
    private PlayerController controller;

    //[SerializeField]
    //private PlayerMovements movements;

    public enum ActionInputs
    {
        None = 0,
        Forward = 1,
        Back = -1,
        Left = -2,
        Right = 2,
        TurnLeft = -3,
        TurnRight = 3,
        Swing = 5
    }

    public ActionInputs actionInput;

    public KeyCode forward = KeyCode.W; 
    public KeyCode back = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode turnLeft = KeyCode.Q;
    public KeyCode turnRight = KeyCode.E;
    public KeyCode swing = KeyCode.Space;

    private void Reset()
    {
        controller = gameObject.GetComponent<PlayerController>();
        //movements = gameObject.GetComponent<PlayerMovements>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(forward)) actionInput = ActionInputs.Forward;
        if (Input.GetKeyDown(back)) actionInput = ActionInputs.Back;
        if (Input.GetKeyDown(left)) actionInput = ActionInputs.Left;
        if (Input.GetKeyDown(right)) actionInput = ActionInputs.Right;
        if (Input.GetKeyDown(turnLeft)) actionInput = ActionInputs.TurnLeft;
        if (Input.GetKeyDown(turnRight)) actionInput = ActionInputs.TurnRight;
        //if (Input.GetKeyDown(swing)) actionInput = ActionInputs.Swing;
        if (actionInput != ActionInputs.None)
        {
            controller.ReceiveActionInput(actionInput, true);
            //movements.ReceiveActionInput(actionInput, true);
        }
        else
        {
            if (Input.GetKey(forward)) actionInput = ActionInputs.Forward;
            if (Input.GetKey(back)) actionInput = ActionInputs.Back;
            if (Input.GetKey(left)) actionInput = ActionInputs.Left;
            if (Input.GetKey(right)) actionInput = ActionInputs.Right;
            if (Input.GetKey(turnLeft)) actionInput = ActionInputs.TurnLeft;
            if (Input.GetKey(turnRight)) actionInput = ActionInputs.TurnRight;

            controller.ReceiveActionInput(actionInput);
            //movements.ReceiveActionInput(actionInput);
        }

        actionInput = ActionInputs.None;
    }

}
