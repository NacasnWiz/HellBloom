using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovements))]
public class PlayerInputs : MonoBehaviour
{
    [SerializeField]
    private PlayerMovements movements;

    public enum MoveInputs
    {
        None = 0,
        Forward = 1,
        Back = -1,
        Left = -2,
        Right = 2,
        TurnLeft = -3,
        TurnRight = 3,
    }

    public MoveInputs moveInput;
    
    public KeyCode forward = KeyCode.W;
    public KeyCode back = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode turnLeft = KeyCode.Q;
    public KeyCode turnRight = KeyCode.E;

    private void Reset()
    {
        movements = gameObject.GetComponent<PlayerMovements>();
    }

    private void Update()
    {
        if (moveInput == MoveInputs.None)
        {
            if (Input.GetKey(forward)) moveInput = MoveInputs.Forward;
            if (Input.GetKey(back)) moveInput = MoveInputs.Back;
            if (Input.GetKey(left)) moveInput = MoveInputs.Left;
            if (Input.GetKey(right)) moveInput = MoveInputs.Right;
            if (Input.GetKey(turnLeft)) moveInput = MoveInputs.TurnLeft;
            if (Input.GetKey(turnRight)) moveInput = MoveInputs.TurnRight;
        }

        if (Input.GetKeyDown(forward)) moveInput = MoveInputs.Forward;     //controller.MoveForward();
        if (Input.GetKeyDown(back)) moveInput = MoveInputs.Back;           //controller.MoveBackWard();
        if (Input.GetKeyDown(left)) moveInput = MoveInputs.Left;           //controller.MoveLeft();
        if (Input.GetKeyDown(right)) moveInput = MoveInputs.Right;         //controller.MoveRight();
        if (Input.GetKeyDown(turnLeft)) moveInput = MoveInputs.TurnLeft;   //controller.RotateLeft();
        if (Input.GetKeyDown(turnRight)) moveInput = MoveInputs.TurnRight; //controller.RotateRight();

        movements.ReceiveMoveInput(moveInput);
        moveInput = MoveInputs.None;
    }

}
