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

    [SerializeField]
    private ActionInputs actionInput;
    [SerializeField]
    private List<KeyCode> lastPressed;

    public KeyCode forward = KeyCode.W;
    public KeyCode back = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode turnLeft = KeyCode.Q;
    public KeyCode turnRight = KeyCode.E;
    public KeyCode swing = KeyCode.Space;


    //public readonly UnityEvent wantSwing = new UnityEvent();


    private void Reset()
    {
        controller = gameObject.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(forward)) { actionInput = ActionInputs.Forward; lastPressed.Add(forward); }
        if (Input.GetKeyDown(back)) { actionInput = ActionInputs.Back; lastPressed.Add(back); }
        if (Input.GetKeyDown(left)) { actionInput = ActionInputs.Left; lastPressed.Add(left); }
        if (Input.GetKeyDown(right)) { actionInput = ActionInputs.Right; lastPressed.Add(right); }
        if (Input.GetKeyDown(turnLeft)) { actionInput = ActionInputs.TurnLeft; lastPressed.Add(turnLeft); }
        if (Input.GetKeyDown(turnRight)) { actionInput = ActionInputs.TurnRight; lastPressed.Add(turnRight); }
        if (Input.GetKeyDown(swing)) actionInput = ActionInputs.Swing;
        if (actionInput != ActionInputs.None)
        {
            controller.ReceiveActionInput(actionInput, true);
        }
        else
        {
            if (Input.GetKey(forward) && lastPressed.LastOrDefault() == forward) actionInput = ActionInputs.Forward;
            if (Input.GetKey(back) && lastPressed.LastOrDefault() == back) actionInput = ActionInputs.Back;
            if (Input.GetKey(left) && lastPressed.LastOrDefault() == left) actionInput = ActionInputs.Left;
            if (Input.GetKey(right) && lastPressed.LastOrDefault() == right) actionInput = ActionInputs.Right;
            if (Input.GetKey(turnLeft) && lastPressed.LastOrDefault() == turnLeft) actionInput = ActionInputs.TurnLeft;
            if (Input.GetKey(turnRight) && lastPressed.LastOrDefault() == turnRight) actionInput = ActionInputs.TurnRight;

            controller.ReceiveActionInput(actionInput);
        }

        if (Input.GetKeyUp(forward)) lastPressed.Remove(forward);
        if (Input.GetKeyUp(back)) lastPressed.Remove(back);
        if (Input.GetKeyUp(left)) lastPressed.Remove(left);
        if (Input.GetKeyUp(right)) lastPressed.Remove(right);
        if (Input.GetKeyUp(turnLeft)) lastPressed.Remove(turnLeft);
        if (Input.GetKeyUp(turnRight)) lastPressed.Remove(turnRight);

        actionInput = ActionInputs.None;
    }

}
