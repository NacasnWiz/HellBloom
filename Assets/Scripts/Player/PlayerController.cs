using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovements))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerMovements movements;

    public enum Actions
    {
        Move = 1,
        Rotate = 2,
        Swing = 3,
    }


    public HexCoord playerPos { get; private set; } = HexCoord.zero;
    public HexCoord.Orientation playerOrientation { get; private set; } = 0;//always acts like an edgeDirection with a ballastRight = left;
    public bool ballastLeft { get; private set; } = false;

    public HexCoord.Orientation edgeDirectionForward => ballastLeft ? playerOrientation : playerOrientation - 1;
    public HexCoord.Orientation edgeDirectionBackward => ballastLeft ? playerOrientation + 2 : playerOrientation - 3;
    public HexCoord.Orientation edgeDirectionLeft => playerOrientation + 1;
    public HexCoord.Orientation edgeDirectionRight => playerOrientation - 2;

    [SerializeField]
    private PlayerInputs.ActionInputs currentActionInput = PlayerInputs.ActionInputs.None;
    [SerializeField]
    private PlayerInputs.ActionInputs nextActionInput = PlayerInputs.ActionInputs.None;

    [SerializeField]
    private bool doubleInputReceiveMode = true;

    private HexCoord targetGridPos;
    private Vector3 targetMovePos;

    private HexCoord.Orientation targetOrientation;
    private Quaternion targetRotation;

    [field: SerializeField]
    public float moveEndCooldown { get; private set; } = 0.05f;
    [field: SerializeField]
    public float rotateEndCooldown { get; private set; } = 0.1f;
    [field: SerializeField]
    public float swingEndCooldown { get; private set; } = 0.3f;

    public bool isOnActionCooldown { get; private set; } = false;


    private void Start()
    {
        //movements.doneMoving.AddListener((coords) => Debug.Log(coords));
        movements.doneMoving.AddListener(() => OnEndAction(Actions.Move));
        movements.doneRotating.AddListener(() => OnEndAction(Actions.Rotate));

    }

    private void Reset()
    {
        movements = gameObject.GetComponent<PlayerMovements>();
    }

    private void FixedUpdate()
    {
        PlayerAct();
    }

    private void PlayerAct()
    {
        if (CanAct() && currentActionInput != PlayerInputs.ActionInputs.None)
        {

            if (currentActionInput == PlayerInputs.ActionInputs.Swing)
            {
                //Swing();
            }

            DeduceMovementFromInput();

            if (targetGridPos != playerPos)
            {
                if (!(HexGrid.Instance.isValidCoordinates(targetGridPos)))
                {
                    Debug.Log("You can't move there.");
                    targetGridPos = playerPos;
                    currentActionInput = PlayerInputs.ActionInputs.None;
                    return;
                }
                targetMovePos = HexGrid.Instance.GetWorldPos(targetGridPos);
                movements.Move(targetMovePos);
            }

            if (targetOrientation != playerOrientation)
            {
                targetRotation = targetOrientation.GetUnderlyingRotation();
                movements.Rotate(targetRotation);
            }
        }
    }

    private bool CanAct()
    {
        if (!movements.isMoving && !movements.isRotating && !isOnActionCooldown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Actualise actionInput or nextActionInput to provided input parameter
    /// </summary>
    /// <param name="input"></param>
    /// <param name="highPriority">Add to buffer?</param>
    public void ReceiveActionInput(PlayerInputs.ActionInputs input, bool highPriority = false)
    {
        if (doubleInputReceiveMode)
        {
            if (movements.isMoving && highPriority)
            {
                nextActionInput = input;
            }

            if (currentActionInput == PlayerInputs.ActionInputs.None && input != PlayerInputs.ActionInputs.None)
            {
                currentActionInput = input;
            }
        }
        else
        {
            currentActionInput = input;
        }
    }



    private void OnEndAction(Actions actionEnded)
    {
        float actionEndCooldown = 0f;
        switch(actionEnded)
        {
            case Actions.Move:
                actionEndCooldown = moveEndCooldown;
                playerPos = targetGridPos;
                break;
            case Actions.Rotate:
                actionEndCooldown = rotateEndCooldown;
                playerOrientation = targetOrientation;
                break;
            case Actions.Swing:
                actionEndCooldown = swingEndCooldown;
                break;

            default:
                break;
        }

        StartCoroutine(EndAction(actionEndCooldown));

    }

    private IEnumerator EndAction(float endActionCooldown)
    {
        currentActionInput = nextActionInput;
        nextActionInput = PlayerInputs.ActionInputs.None;

        isOnActionCooldown = true;
        yield return new WaitForSeconds(endActionCooldown);
        isOnActionCooldown = false;
    }

    private void DeduceMovementFromInput()
    {
        switch (currentActionInput)
        {
            case PlayerInputs.ActionInputs.Forward:
                targetGridPos = HexCoord.GetNeighbour(playerPos, edgeDirectionForward);
                break;

            case PlayerInputs.ActionInputs.Back:
                targetGridPos = HexCoord.GetNeighbour(playerPos, edgeDirectionBackward);
                break;

            case PlayerInputs.ActionInputs.Left:
                targetGridPos = HexCoord.GetNeighbour(playerPos, edgeDirectionLeft);
                break;

            case PlayerInputs.ActionInputs.Right:
                targetGridPos = HexCoord.GetNeighbour(playerPos, edgeDirectionRight);
                break;

            case PlayerInputs.ActionInputs.TurnLeft:
                targetOrientation = playerOrientation + 1;
                break;
            case PlayerInputs.ActionInputs.TurnRight:
                targetOrientation = playerOrientation - 1;
                break;

            default:

                return;
        }
    }
}
