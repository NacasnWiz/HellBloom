using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

    public float moveSpeedDuringSwing => movements.baseTransitionSpeed + demonicArm.swingSpeed;

    public HexCoord.Orientation edgeDirectionForward => ballastLeft ? playerOrientation : playerOrientation - 1;
    public HexCoord.Orientation edgeDirectionAntiForward => !ballastLeft ? playerOrientation : playerOrientation - 1;
    public HexCoord.Orientation edgeDirectionBackward => ballastLeft ? playerOrientation + 2 : playerOrientation - 3;
    public HexCoord.Orientation edgeDirectionAntiBackward => !ballastLeft ? playerOrientation + 2 : playerOrientation - 3;
    public HexCoord.Orientation edgeDirectionLeft => playerOrientation + 1;
    public HexCoord.Orientation edgeDirectionRight => playerOrientation - 2;

    [SerializeField]
    private PlayerInputs.ActionInputs currentActionInput = PlayerInputs.ActionInputs.None;
    [SerializeField]
    private PlayerInputs.ActionInputs nextActionInput = PlayerInputs.ActionInputs.None;

    [SerializeField]
    private bool mode_doubleInputReceiveMode = true;
    [SerializeField]
    private bool mode_swingTakesYouWithIt = true;

    private HexCoord targetGridPos;

    private HexCoord.Orientation targetOrientation;

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
        if(currentActionInput != PlayerInputs.ActionInputs.None)
        {
            TargetMovement(currentActionInput);

            PlayerAct();
        }
    }

    private void PlayerAct()
    {
        if (!CanAct())
        {
            return;
        }

        if (targetGridPos != playerPos)
        {
            if (!(HexGrid.Instance.isValidCoordinates(targetGridPos)))
            {
                Debug.Log("You can't move there.");
                targetGridPos = playerPos;
                currentActionInput = PlayerInputs.ActionInputs.None;
                return;
            }
            MoveTo(targetGridPos);
        }

        if (targetOrientation != playerOrientation)
        {
            RotateTo(targetOrientation);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hexOrientation">The hex orientation to rotate to</param>
    private void RotateTo(HexCoord.Orientation hexOrientation)
    {
        Quaternion targetRotation = hexOrientation.GetUnderlyingRotation();
        movements.Rotate(targetRotation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gridPos">The grid coordinates to move to</param>
    private void MoveTo(HexCoord gridPos, float customSpeed = -1f)
    {
        Vector3 targetMovePos = HexGrid.Instance.GetWorldPos(gridPos);
        movements.Move(targetMovePos, customSpeed);
    }

    private void TakeASwing()
    {
        if (!CanSwing())
        {
            currentActionInput = PlayerInputs.ActionInputs.None;
            return;
        }
        demonicArm.Swing();

        if(mode_swingTakesYouWithIt)
        {
            TargetMovement(PlayerInputs.ActionInputs.Forward, true);
            MoveTo(targetGridPos, moveSpeedDuringSwing);
        }

        StartCoroutine(SwingCooldownCoroutine());
    }

    private bool CanSwing()
    {
        if (isOnSwingCooldown)
        {
            Debug.Log("You can't swing, it's on cooldown");
            return false;
        }
        if (movements.isMoving)
        {
            Debug.Log("You can't swing while moving");
            return false;
        }
        if (movements.isRotating)
        {
            Debug.Log("You can't swing while rotating");
            return false;
        }

        return true;
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
        if (mode_swingTakesYouWithIt && demonicArm.isSwinging)
        {
            return;
        }

        if (mode_doubleInputReceiveMode)
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

    private void TargetMovement(PlayerInputs.ActionInputs instruction, bool invertedBallast = false)
    {
        switch (instruction)
        {
            case PlayerInputs.ActionInputs.Forward:
                targetGridPos = invertedBallast ? HexCoord.GetNeighbour(playerPos, edgeDirectionAntiForward) : HexCoord.GetNeighbour(playerPos, edgeDirectionForward);
                break;

            case PlayerInputs.ActionInputs.Back:
                targetGridPos = invertedBallast ? HexCoord.GetNeighbour(playerPos, edgeDirectionAntiBackward) : HexCoord.GetNeighbour(playerPos, edgeDirectionBackward);
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
