using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovements))]
[RequireComponent (typeof(PlayerInputs))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerMovements movements;
    [SerializeField] PlayerInputs inputs;

    public enum Actions
    {
        Move = 1,
        Rotate = 2,
        Swing = 3,
    }

    [SerializeField] DemonicArm demonicArm;



    public HexCoord playerPos { get; private set; } = HexCoord.zero;
    public HexCoord.Orientation playerOrientation { get; private set; } = 0;//always acts like an edgeDirection with a ballastLeft = true;
    [field: SerializeField]
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
    //private Vector3 targetMovePos;

    private HexCoord.Orientation targetOrientation;
    //private Quaternion targetRotation;

    [field: SerializeField]
    public float moveEndCooldown { get; private set; } = 0.05f;
    [field: SerializeField]
    public float rotateEndCooldown { get; private set; } = 0.1f;
    [field: SerializeField]
    public float swingEndCooldown { get; private set; } = 0.15f;
    [field: SerializeField]
    public float swingCooldown { get; private set; } = 2f;

    public bool isOnActionCooldown { get; private set; } = false;
    public bool isOnSwingCooldown { get; private set; } = false;


    private void Start()
    {
        //movements.doneMoving.AddListener((coords) => Debug.Log(coords));
        movements.doneMoving.AddListener(() => OnEndAction(Actions.Move));
        movements.doneRotating.AddListener(() => OnEndAction(Actions.Rotate));
        inputs.wantSwing.AddListener(() => TakeASwing());
        demonicArm.doneSwinging.AddListener(() => OnEndAction(Actions.Swing));

        Application.targetFrameRate = 60;

    }

    private void Reset()
    {
        movements = gameObject.GetComponent<PlayerMovements>();
        inputs = gameObject.GetComponent<PlayerInputs>();
        demonicArm = gameObject.GetComponent<DemonicArm>();
    }

    private void FixedUpdate()
    {
        PlayerAct();
    }

    private void PlayerAct()
    {
        if (CanAct() && currentActionInput != PlayerInputs.ActionInputs.None)
        {
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
                Vector3 targetMovePos = HexGrid.Instance.GetWorldPos(targetGridPos);
                movements.Move(targetMovePos);
            }

            if (targetOrientation != playerOrientation)
            {
                Quaternion targetRotation = targetOrientation.GetUnderlyingRotation();
                movements.Rotate(targetRotation);
            }
        }
    }

    private void TakeASwing()
    {
        if (!CanSwing())
        {
            Debug.Log("You can't swing, it's on cooldown");
            currentActionInput = PlayerInputs.ActionInputs.None;
            return;
        }
        demonicArm.Swing();
        StartCoroutine(SwingCooldownCoroutine());
    }

    private bool CanSwing()
    {
        return !isOnSwingCooldown;
    }

    private bool CanAct()
    {
        if (!movements.isMoving && !movements.isRotating && !isOnActionCooldown && !demonicArm.isSwinging)
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
                playerPos = targetGridPos;
                actionEndCooldown = moveEndCooldown;
                break;
            case Actions.Rotate:
                playerOrientation = targetOrientation;
                actionEndCooldown = rotateEndCooldown;
                break;
            case Actions.Swing:
                ballastLeft = !ballastLeft;
                actionEndCooldown = swingEndCooldown;
                break;

            default:
                break;
        }

        StartCoroutine(EndActionCoroutine(actionEndCooldown));

    }

    private IEnumerator EndActionCoroutine(float endActionCooldown)
    {
        currentActionInput = nextActionInput;
        nextActionInput = PlayerInputs.ActionInputs.None;

        isOnActionCooldown = true;
        yield return new WaitForSeconds(endActionCooldown);
        isOnActionCooldown = false;
    }

    private IEnumerator SwingCooldownCoroutine()
    {
        isOnSwingCooldown = true;
        demonicArm.ChangeAppearance(!isOnSwingCooldown);
        yield return new WaitForSeconds(swingCooldown + swingEndCooldown);
        isOnSwingCooldown = false;
        demonicArm.ChangeAppearance(!isOnSwingCooldown);
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
