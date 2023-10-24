using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    public HexCoord pos = HexCoord.zero;
    public HexCoord.Orientation orientation = 0;//always acts like an edgeDirection with a ballastRight = left;
    public bool ballastLeft = false;

    public HexCoord.Orientation edgeDirectionForward => ballastLeft ? orientation : orientation - 1;
    public HexCoord.Orientation edgeDirectionBackward => ballastLeft ? orientation + 2 : orientation - 3;
    public HexCoord.Orientation edgeDirectionLeft => orientation + 1;
    public HexCoord.Orientation edgeDirectionRight => orientation - 2;

    [SerializeField]
    private float transitionSpeed = 10f;
    [SerializeField]
    private float transitionRotationSpeed = 500f;

    private HexCoord targetGridPos;
    private Vector3 targetMovePos;

    private HexCoord.Orientation targetOrientation;
    private Quaternion targetRotation;

    [SerializeField]
    private PlayerInputs.MoveInputs moveInput = PlayerInputs.MoveInputs.None;
    [SerializeField]
    private PlayerInputs.MoveInputs nextMoveInput = PlayerInputs.MoveInputs.None;

    [SerializeField]
    private bool doubleInputReceiveMode = false;

    public bool isMoving { get; private set; } = false;
    public bool isRotating { get; private set; } = false;
    [field: SerializeField]
    public float moveCooldown { get; private set; } = 0.05f;
    [field: SerializeField]
    public float rotateCooldown { get; private set; } = 0.1f;
    public bool isOnMoveCooldown { get; private set; } = false;



    private void Start()
    {
        //Debug.Log("edgeDirectionForward = " + edgeDirectionForward);
        //Debug.Log("edgeDirectionBackward = " + edgeDirectionBackward);
        //Debug.Log(pos + " + " + HexCoord.EdgeDirections[edgeDirectionForward] + " is " + (pos + HexCoord.EdgeDirections[5]));

        //Debug.Log("current rotation = " + transform.rotation.eulerAngles);
        //Debug.Log("leftRotation =" + leftRotation.eulerAngles);
        //Debug.Log("angle between current and left rotations : " + Quaternion.Angle(transform.rotation, leftRotation));
        //Debug.Log("current rotation * leftRotation = " + (transform.rotation * leftRotation).eulerAngles);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private bool CanMove()
    {
        if (!isMoving && !isRotating && !isOnMoveCooldown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ReceiveMoveInput(PlayerInputs.MoveInputs input, bool highPriority = false)
    {
        if (doubleInputReceiveMode)
        {
            if (isMoving && highPriority)
            {
                nextMoveInput = input;
            }

            if (moveInput == PlayerInputs.MoveInputs.None && input != PlayerInputs.MoveInputs.None)
            {
                moveInput = input;
            }
        }
        else
        {
            moveInput = input;
        }
        
    }

    private void MovePlayer()
    {
        if(CanMove() && moveInput != PlayerInputs.MoveInputs.None)
        {
            DeduceMoveFromInput();
            
            if(targetGridPos != pos)
            {
                targetMovePos = HexGrid.Instance.GetWorldPos(targetGridPos);
                StartCoroutine(MoveCoroutine(targetMovePos));
            }

            if(targetOrientation != orientation)
            {
                targetRotation = targetOrientation.GetUnderlyingRotation();
                StartCoroutine(RotateCoroutine(targetRotation));
            }
        }
    }

    private IEnumerator RotateCoroutine(Quaternion targetRotation)
    {
        isRotating = true;
        float safetyTimer = 0f;
        while (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRotation)) > 0.05f && safetyTimer < 10f)
        {
            yield return new WaitForFixedUpdate();
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, transitionRotationSpeed * Time.deltaTime);
            safetyTimer += Time.deltaTime;
        }
        transform.rotation = targetRotation;
        orientation = targetOrientation;
        isRotating = false;

        moveInput = nextMoveInput;
        nextMoveInput = PlayerInputs.MoveInputs.None;

        isOnMoveCooldown = true;
        yield return new WaitForSeconds(rotateCooldown);
        isOnMoveCooldown = false;

    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        isMoving = true;
        float safetyTimer = 0f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f && safetyTimer < 10f)
        {
            yield return new WaitForFixedUpdate();
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            safetyTimer += Time.deltaTime;
        }
        transform.position = targetPosition;
        isMoving = false;
        pos = targetGridPos;

        moveInput = nextMoveInput;
        nextMoveInput = PlayerInputs.MoveInputs.None;

        isOnMoveCooldown = true;
        yield return new WaitForSeconds(moveCooldown);
        isOnMoveCooldown = false;
    }



    private void DeduceMoveFromInput()
    {
        switch (moveInput)
        {
            case PlayerInputs.MoveInputs.Forward:
                targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionForward);
                break;

            case PlayerInputs.MoveInputs.Back:
                targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionBackward);
                break;

            case PlayerInputs.MoveInputs.Left:
                targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionLeft);
                break;

            case PlayerInputs.MoveInputs.Right:
                targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionRight);
                break;

            case PlayerInputs.MoveInputs.TurnLeft:
                targetOrientation = orientation + 1;
                break;
            case PlayerInputs.MoveInputs.TurnRight:
                targetOrientation = orientation - 1;
                break;

            default:

                return;
        }
    }

}