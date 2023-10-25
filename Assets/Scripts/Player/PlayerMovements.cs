using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovements : MonoBehaviour
{
    //public HexCoord pos = HexCoord.zero;
    //public HexCoord.Orientation orientation = 0;//always acts like an edgeDirection with a ballastRight = left;
    //public bool ballastLeft = false;

    //public HexCoord.Orientation edgeDirectionForward => ballastLeft ? orientation : orientation - 1;
    //public HexCoord.Orientation edgeDirectionBackward => ballastLeft ? orientation + 2 : orientation - 3;
    //public HexCoord.Orientation edgeDirectionLeft => orientation + 1;
    //public HexCoord.Orientation edgeDirectionRight => orientation - 2;

    [SerializeField]
    private float transitionSpeed = 10f;
    [SerializeField]
    private float transitionRotationSpeed = 500f;

    //private HexCoord targetGridPos;
    //private Vector3 targetMovePos;

    //private HexCoord.Orientation targetOrientation;
    //private Quaternion targetRotation;

    //[SerializeField]
    //private PlayerInputs.ActionInputs actionInput = PlayerInputs.ActionInputs.None;
    //[SerializeField]
    //private PlayerInputs.ActionInputs nextActionInput = PlayerInputs.ActionInputs.None;

    //[SerializeField]
    //private bool doubleInputReceiveMode = false;

    public bool isMoving { get; private set; } = false;
    public bool isRotating { get; private set; } = false;
    //public bool isOnActionCooldown { get; private set; } = false;

    //[field: SerializeField]
    //public float moveCooldown { get; private set; } = 0.05f;
    //[field: SerializeField]
    //public float rotateCooldown { get; private set; } = 0.1f;



    public readonly UnityEvent doneMoving = new UnityEvent();
    public readonly UnityEvent doneRotating = new UnityEvent();


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

    //private void FixedUpdate()
    //{
    //    PlayerAct();
    //}

    //private bool CanAct()
    //{
    //    return (!isMoving && !isRotating && !isOnActionCooldown);

    //    if (!isMoving && !isRotating && !isOnActionCooldown)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    /// <summary>
    /// Actualise actionInput or nextActionInput to provided input parameter
    /// </summary>
    /// <param name="input"></param>
    /// <param name="highPriority">Add to buffer?</param>
    //public void ReceiveActionInput(PlayerInputs.ActionInputs input, bool highPriority = false)
    //{
    //    if (doubleInputReceiveMode)
    //    {
    //        if (isMoving && highPriority)
    //        {
    //            nextActionInput = input;
    //        }

    //        if (actionInput == PlayerInputs.ActionInputs.None && input != PlayerInputs.ActionInputs.None)
    //        {
    //            actionInput = input;
    //        }
    //    }
    //    else
    //    {
    //        actionInput = input;
    //    }

    //}

    //private void PlayerAct()
    //{
    //    if (CanAct() && actionInput != PlayerInputs.ActionInputs.None)
    //    {

    //        if (actionInput == PlayerInputs.ActionInputs.Swing)
    //        {
    //            //Swing();
    //        }

    //        DeduceMovementFromInput();

    //        if (targetGridPos != pos)
    //        {
    //            targetMovePos = HexGrid.Instance.GetWorldPos(targetGridPos);
    //            Move(targetMovePos);
    //        }

    //        if (targetOrientation != orientation)
    //        {
    //            targetRotation = targetOrientation.GetUnderlyingRotation();
    //            Rotate(targetRotation);
    //        }
    //    }
    //}

    public void Move(Vector3 targetPosition)
    {
        StartCoroutine(MoveCoroutine(targetPosition));
    }

    public void Rotate(Quaternion targetRotation)
    {
        StartCoroutine(RotateCoroutine(targetRotation));
    }

    private IEnumerator RotateCoroutine(Quaternion targetRotation)
    {
        isRotating = true;
        float safetyTimer = 0f;
        while (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRotation)) > 0.05f && safetyTimer < 10f)
        {
            yield return null; //new WaitForFixedUpdate();
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, transitionRotationSpeed * Time.deltaTime);
            safetyTimer += Time.deltaTime;
        }
        transform.rotation = targetRotation;
        //orientation = targetOrientation;
        isRotating = false;

        doneRotating.Invoke();
        //StartCoroutine(EndAction(rotateCooldown));
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        isMoving = true;
        float safetyTimer = 0f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f && safetyTimer < 10f)
        {
            yield return null; //new WaitForFixedUpdate();
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            safetyTimer += Time.deltaTime;
        }
        transform.position = targetPosition;
        //pos = targetGridPos;
        isMoving = false;

        doneMoving.Invoke();

        //StartCoroutine(EndAction(moveCooldown));
    }

    //private IEnumerator EndAction(float endActionCooldown)
    //{
    //    actionInput = nextActionInput;
    //    nextActionInput = PlayerInputs.ActionInputs.None;

    //    isOnActionCooldown = true;
    //    yield return new WaitForSeconds(endActionCooldown);
    //    isOnActionCooldown = false;
    //}

    //private void DeduceMovementFromInput()
    //{
    //    switch (actionInput)
    //    {
    //        case PlayerInputs.ActionInputs.Forward:
    //            targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionForward);
    //            break;

    //        case PlayerInputs.ActionInputs.Back:
    //            targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionBackward);
    //            break;

    //        case PlayerInputs.ActionInputs.Left:
    //            targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionLeft);
    //            break;

    //        case PlayerInputs.ActionInputs.Right:
    //            targetGridPos = HexCoord.GetNeighbour(pos, edgeDirectionRight);
    //            break;

    //        case PlayerInputs.ActionInputs.TurnLeft:
    //            targetOrientation = orientation + 1;
    //            break;
    //        case PlayerInputs.ActionInputs.TurnRight:
    //            targetOrientation = orientation - 1;
    //            break;

    //        default:

    //            return;
    //    }
    //}

}