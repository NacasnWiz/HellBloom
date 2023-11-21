using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movements))]
[RequireComponent(typeof(PlayerAttack))]
//[RequireComponent (typeof(PlayerInputs))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] Movements movements;
    //[SerializeField] PlayerInputs inputs;
    [SerializeField] PlayerAttack attack;

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

    public float moveSpeedDuringSwing => movements.baseTransitionSpeed/2f + demonicArm.swingSpeed;

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

    public HexCoord targetGridPos { get; private set; }

    private HexCoord.Orientation targetOrientation;

    [field: SerializeField]
    public float moveEndCooldown { get; private set; } = 0.025f;
    [field: SerializeField]
    public float rotateEndCooldown { get; private set; } = 0.05f;
    [field: SerializeField]
    public float swingEndCooldown { get; private set; } = 0.15f;
    [field: SerializeField]
    public float swingCooldown { get; private set; } = 2f;

    public bool isOnActionCooldown { get; private set; } = false;
    public bool isOnSwingCooldown { get; private set; } = false;


    private void Start()
    {
        //movements.doneMoving.AddListener((coords) => Debug.Log(coords));

        movements.doneMoving.AddListener(() => OnEndMovement(Actions.Move));
        movements.doneRotating.AddListener(() => OnEndMovement(Actions.Rotate));
        demonicArm.doneSwinging.AddListener(() => OnEndMovement(Actions.Swing));


        Application.targetFrameRate = 60;
    }

    private void Reset()
    {
        movements = gameObject.GetComponent<Movements>();
        //inputs = gameObject.GetComponent<PlayerInputs>();
        demonicArm = gameObject.GetComponentInChildren<DemonicArm>();
        attack = gameObject.GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        if(currentActionInput == PlayerInputs.ActionInputs.Swing || nextActionInput == PlayerInputs.ActionInputs.Swing)
        {
            TakeASwing();
        }
        else if (currentActionInput != PlayerInputs.ActionInputs.None || nextActionInput != PlayerInputs.ActionInputs.None)
        {
            PlayerAct();
        }
    }

    private bool IsInMovement()
    {
        return (movements.isMoving || movements.isRotating);
    }

    private bool CanSwing()
    {
        if (isOnSwingCooldown)
        {
            Debug.Log("You can't swing, it's on cooldown");
            return false;
        }
        if (IsInMovement())
        {
            Debug.Log("You can't swing while in movement.");
            return false;
        }
        if (isOnActionCooldown)
        {
            Debug.Log("You're on action cooldown.");
            return false;
        }

        return true;
    }

    private bool CanAct()
    {
        if (IsInMovement() || isOnActionCooldown)
        {
            return false;
        }
        
        return true;
    }

    private void PlayerAct()
    {
        if (!CanAct())
        {
            return;
        }

        if(nextActionInput != PlayerInputs.ActionInputs.None)
        {
            TargetMovement(nextActionInput);
            nextActionInput = PlayerInputs.ActionInputs.None;
        }
        else if(currentActionInput != PlayerInputs.ActionInputs.None)
        {
            TargetMovement(currentActionInput);
        }

        if (targetGridPos != playerPos)
        {
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
        if (!CanMoveTo(targetGridPos))
        {
            Debug.Log("You can't move there.");
            targetGridPos = playerPos;
            currentActionInput = PlayerInputs.ActionInputs.None;
            return;
        }
        Vector3 targetMovePos = GameManager.Instance.hexGrid.GetWorldPos(gridPos);
        movements.Move(targetMovePos, customSpeed);
    }

    private void TakeASwing()
    {
        if (!CanSwing())
        {
            return;
        }

        demonicArm.Swing();
        attack.Attack(this);
        ballastLeft = !ballastLeft;

        if(mode_swingTakesYouWithIt)
        {
            TargetMovement(PlayerInputs.ActionInputs.Forward);
            MoveTo(targetGridPos, moveSpeedDuringSwing);
        }
        
        StartCoroutine(SwingCooldownCoroutine());
        currentActionInput = PlayerInputs.ActionInputs.None;
        nextActionInput = PlayerInputs.ActionInputs.None;
    }

    private void OnEndMovement(Actions actionEnded)
    {
        float actionEndCooldown = 0f;
        switch (actionEnded)
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
                currentActionInput = PlayerInputs.ActionInputs.None;
                actionEndCooldown = swingEndCooldown;
                break;

            default:
                break;
        }

        StartCoroutine(EndMovementCoroutine(actionEndCooldown));
    }

    private IEnumerator EndMovementCoroutine(float endActionCooldown)
    {
        isOnActionCooldown = true;
        yield return new WaitForSeconds(endActionCooldown);
        isOnActionCooldown = false;

        yield return new WaitForEndOfFrame();
        currentActionInput = PlayerInputs.ActionInputs.None;
    }

    private IEnumerator SwingCooldownCoroutine()
    {
        isOnSwingCooldown = true;
        demonicArm.ChangeAppearance(!isOnSwingCooldown);
        yield return new WaitForSeconds(swingCooldown);
        isOnSwingCooldown = false;
        demonicArm.ChangeAppearance(!isOnSwingCooldown);
    }

    /// <summary>
    /// Actualise actionInput or nextActionInput to provided input parameter
    /// </summary>
    /// <param name="input"></param>
    /// <param name="highPriority">Add to buffer?</param>
    public void ReceiveActionInput(PlayerInputs.ActionInputs input, bool highPriority = false)
    {
        if (mode_swingTakesYouWithIt && demonicArm.isSwinging) //Take no input while swinging.
        {
            return;
        }

        if (mode_doubleInputReceiveMode)
        {
            if (nextActionInput == PlayerInputs.ActionInputs.Swing) return;

            if ((!CanAct()) && highPriority)
            {
                nextActionInput = input;
            }
            else if (input == PlayerInputs.ActionInputs.None)
            {
                StartCoroutine(NoneInputCoroutine());
            }
            else
            {
                currentActionInput = input;
            }
            
        }
        else
        {
            currentActionInput = input;
        }
    }

    private IEnumerator NoneInputCoroutine()
    {
        yield return new WaitForEndOfFrame();
        currentActionInput = PlayerInputs.ActionInputs.None;
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

    public bool CanMoveTo(HexCoord coord)
    {
        if (!GameManager.Instance.hexGrid.IsValidMoveCoordinates(coord))
            return false;
        return !EnemiesManager.Instance.IsMovedOn(coord);
    }
}
