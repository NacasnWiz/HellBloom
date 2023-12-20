using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[RequireComponent(typeof(PlayerController))]
public class PlayerInputs : MonoBehaviour
{
    [SerializeField]
    private PlayerController controller;

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

    [field: SerializeField]
    public ActionInputs actionInput { get; private set; }
    [SerializeField]
    private List<KeyCode> lastPressed;

    public KeyCode forward = KeyCode.W;
    public KeyCode back = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode turnLeft = KeyCode.Q;
    public KeyCode turnRight = KeyCode.E;
    public KeyCode swing = KeyCode.Space;

    public KeyCode alt_forward = KeyCode.Keypad8;
    public KeyCode alt_back = KeyCode.Keypad5;
    public KeyCode alt_left = KeyCode.Keypad4;
    public KeyCode alt_right = KeyCode.Keypad6;
    public KeyCode alt_turnLeft = KeyCode.Keypad7;
    public KeyCode alt_turnRight = KeyCode.Keypad9;
    public KeyCode alt_swing = KeyCode.Keypad0;



    private void Reset()
    {
        controller = gameObject.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(forward) || Input.GetKeyDown(alt_forward)) { actionInput = ActionInputs.Forward; lastPressed.Add(forward); }
        if (Input.GetKeyDown(back) || Input.GetKeyDown(alt_back)) { actionInput = ActionInputs.Back; lastPressed.Add(back); }
        if (Input.GetKeyDown(left) || Input.GetKeyDown(alt_left)) { actionInput = ActionInputs.Left; lastPressed.Add(left); }
        if (Input.GetKeyDown(right) || Input.GetKeyDown(alt_right)) { actionInput = ActionInputs.Right; lastPressed.Add(right); }
        if (Input.GetKeyDown(turnLeft) || Input.GetKeyDown(alt_turnLeft)) { actionInput = ActionInputs.TurnLeft; lastPressed.Add(turnLeft); }
        if (Input.GetKeyDown(turnRight) || Input.GetKeyDown(alt_turnRight)) { actionInput = ActionInputs.TurnRight; lastPressed.Add(turnRight); }
        if (Input.GetKeyDown(swing) || Input.GetKeyDown(alt_swing)) actionInput = ActionInputs.Swing;

        if (actionInput != ActionInputs.None)
        {
            controller.ReceiveActionInput(actionInput, true);
        }
        else
        {
            if ((Input.GetKey(forward) || Input.GetKey(alt_forward)) && lastPressed.LastOrDefault() == forward) actionInput = ActionInputs.Forward;
            if ((Input.GetKey(back) || Input.GetKey(alt_back)) && lastPressed.LastOrDefault() == back) actionInput = ActionInputs.Back;
            if ((Input.GetKey(left) || Input.GetKey(alt_left)) && lastPressed.LastOrDefault() == left) actionInput = ActionInputs.Left;
            if ((Input.GetKey(right) || Input.GetKey(alt_right)) && lastPressed.LastOrDefault() == right) actionInput = ActionInputs.Right;
            if ((Input.GetKey(turnLeft) || Input.GetKey(alt_turnLeft)) && lastPressed.LastOrDefault() == turnLeft) actionInput = ActionInputs.TurnLeft;
            if ((Input.GetKey(turnRight) || Input.GetKey(alt_turnRight)) && lastPressed.LastOrDefault() == turnRight) actionInput = ActionInputs.TurnRight;

            controller.ReceiveActionInput(actionInput);
        }

        if (Input.GetKeyUp(forward) || Input.GetKeyUp(alt_forward)) lastPressed.Remove(forward);
        if (Input.GetKeyUp(back) || Input.GetKeyUp(alt_back)) lastPressed.Remove(back);
        if (Input.GetKeyUp(left) || Input.GetKeyUp(alt_left)) lastPressed.Remove(left);
        if (Input.GetKeyUp(right) || Input.GetKeyUp(alt_right)) lastPressed.Remove(right);
        if (Input.GetKeyUp(turnLeft) || Input.GetKeyUp(alt_turnLeft)) lastPressed.Remove(turnLeft);
        if (Input.GetKeyUp(turnRight) || Input.GetKeyUp(alt_turnRight)) lastPressed.Remove(turnRight);

        actionInput = ActionInputs.None;
    }

}
