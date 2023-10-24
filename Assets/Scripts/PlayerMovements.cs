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

    public HexCoord targetGridPos;
    public Vector3 targetMovePos;

    public HexCoord.Orientation targetOrientation;
    public Quaternion targetRotation;

    private Quaternion leftRotation = Quaternion.Euler(0f, -60f, 0f);
    private Quaternion rightRotation = Quaternion.Euler(0f, 60f, 0f);

    public Quaternion ballastRotation { get { return ballastLeft ? leftRotation : rightRotation; } }

    [SerializeField]
    private PlayerInputs.MoveInputs moveInput = PlayerInputs.MoveInputs.None;
    private PlayerInputs.MoveInputs nextMoveInput = PlayerInputs.MoveInputs.None;
    

    public bool isMoving { get; private set; } = false;
    public bool isRotating { get; private set; } = false;
    [SerializeField]
    public float moveCooldown = 0.1f;
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

    public void ReceiveMoveInput(PlayerInputs.MoveInputs input)
    {
        moveInput = input;
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
        //Debug.Log("Rotating to " + targetRotation.eulerAngles);
        isRotating = true;
        float safetyTimer = 0f;
        while (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRotation)) > 0.05f && safetyTimer < 10f)
        {
            yield return new WaitForFixedUpdate();
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, transitionRotationSpeed * Time.deltaTime);
            safetyTimer += Time.deltaTime;
        }
        orientation = targetOrientation;
        //Debug.Log("Done rotating");
        isRotating = false;

    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        isMoving = true;
        float safetyTimer = 0f;

        //float lerpTimer = 0f;

        //while (lerpTimer <= 1f)
        //{
        //    transform.position = Vector3.MoveTowards(transform.position, Vector3.Lerp(transform.position, targetPosition, lerpTimer), Time.deltaTime * transitionSpeed);
        //    lerpTimer += Time.fixedDeltaTime;
        //    yield return null;
        //}

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f && safetyTimer < 10f)
        {
            yield return new WaitForFixedUpdate();
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            safetyTimer += Time.deltaTime;
        }
        transform.position = targetPosition;

        //Debug.Log("Done moving");
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