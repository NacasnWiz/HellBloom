using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEPRECATEDPlayerController : MonoBehaviour
{
    [SerializeField]
    private bool ballastRight = true;

    [SerializeField]
    private float transitionSpeed = 10f;
    [SerializeField]
    private float transitionRotationSpeed = 500f;

    public Vector3 targetGridPos;
    public Vector3 prevTargetGridPos;
    public Quaternion targetRotation;

    private Quaternion leftRotation = Quaternion.Euler(0f, -60f, 0f);
    private Quaternion rightRotation = Quaternion.Euler(0f, 60f, 0f);

    public Quaternion ballastRotation { get { return ballastRight ? rightRotation : leftRotation; } }

    private PlayerInputs.ActionInputs nextMoveInputs;


    //public bool isAtRest = true;

    private bool isAtRest
    {
        get
        {
            if ((Vector3.Distance(transform.position, targetGridPos) < 0.05f) && (Quaternion.Angle(transform.rotation, targetRotation) < 0.05f))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void RotateLeft() { targetRotation *= leftRotation; }
    public void RotateRight() { targetRotation *= rightRotation; }
    public void MoveForward() { targetGridPos += HexGrid.Instance.hexSize * 2f * (ballastRotation * transform.forward); }
    public void MoveBackward() { targetGridPos -= HexGrid.Instance.hexSize * 2f * (ballastRotation * transform.forward); }
    public void MoveLeft() { targetGridPos -= HexGrid.Instance.hexSize * 2f * transform.right; }
    public void MoveRight() { targetGridPos += HexGrid.Instance.hexSize * 2f * transform.right; }
    //it's only a target

    public void TargetNextMove(PlayerInputs.ActionInputs input)
    {
        if (isAtRest)
        {
            switch (input)
            {
                case PlayerInputs.ActionInputs.Forward:
                    MoveForward();
                    break;
                case PlayerInputs.ActionInputs.Back:
                    MoveBackward();
                    break;
                case PlayerInputs.ActionInputs.Left:
                    MoveLeft();
                    break;
                case PlayerInputs.ActionInputs.Right:
                    MoveRight();
                    break;
                case PlayerInputs.ActionInputs.TurnLeft:
                    RotateLeft();
                    break;
                case PlayerInputs.ActionInputs.TurnRight:
                    RotateRight();
                    break;

                default: break;
            }
            nextMoveInputs = PlayerInputs.ActionInputs.None;
        }
        else
        {
            nextMoveInputs = input;
        }
    }

    private void Start()
    {
        targetGridPos = Vector3Int.RoundToInt(transform.position);
        targetRotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }


    private void MovePlayer()
    {
        if (true)
        {


            prevTargetGridPos = targetGridPos;
            Vector3 targetPosition = targetGridPos;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * transitionRotationSpeed);
        }
        else
        {
            targetGridPos = prevTargetGridPos;
        }
    }


}
