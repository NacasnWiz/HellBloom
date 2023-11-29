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

    [SerializeField] Camera playerCamera;

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

    public float moveSpeedDuringSwing => movements.baseTransitionSpeed / 2f + demonicArm.swingSpeed;

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
    private HexCoord.Orientation targetMoveDirection;
    [SerializeField] private float bumpTime;
    [SerializeField] private AnimationCurve bumpEffectCurve;
    [SerializeField, Range(0f, 0.5f)] private float bumpAmplitude;

    [field: SerializeField]
    public float moveEndCooldown { get; private set; } = 0.025f;
    [field: SerializeField]
    public float rotateEndCooldown { get; private set; } = 0.05f;
    [field: SerializeField]
    public float swingEndCooldown { get; private set; } = 0.15f;
    [field: SerializeField]
    public float swingCooldown { get; private set; } = 2f;

    public bool isBumpingUnwalkableTile { get; private set; } = false;

    public bool isOnActionCooldown { get; private set; } = false;
    public bool isOnSwingCooldown { get; private set; } = false;

    private void Reset()
    {
        movements = gameObject.GetComponent<Movements>();
        //inputs = gameObject.GetComponent<PlayerInputs>();
        demonicArm = gameObject.GetComponentInChildren<DemonicArm>();
        attack = gameObject.GetComponent<PlayerAttack>();

        playerCamera = gameObject.GetComponentInChildren<Camera>();
    }


    private void Start()
    {
        //movements.doneMoving.AddListener((coords) => Debug.Log(coords));

        movements.doneMoving.AddListener(() => OnEndMovement(Actions.Move));
        movements.doneRotating.AddListener(() => OnEndMovement(Actions.Rotate));
        demonicArm.doneSwinging.AddListener(() => OnEndMovement(Actions.Swing));
    }

    public void TryToSwing()
    {
        if (currentActionInput == PlayerInputs.ActionInputs.Swing || nextActionInput == PlayerInputs.ActionInputs.Swing)
        {
            TakeASwing();
        }
    }

    public void TryToAct()
    {
        if ((currentActionInput != PlayerInputs.ActionInputs.None || nextActionInput != PlayerInputs.ActionInputs.None) && (currentActionInput != PlayerInputs.ActionInputs.Swing && nextActionInput != PlayerInputs.ActionInputs.Swing))
        {
            Act();
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
        if (IsInMovement() || isBumpingUnwalkableTile)
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
        if (IsInMovement() || isOnActionCooldown || isBumpingUnwalkableTile)
        {
            return false;
        }

        return true;
    }

    private void Act()
    {
        if (!CanAct())
        {
            return;
        }

        if (nextActionInput != PlayerInputs.ActionInputs.None)
        {
            TargetMovement(nextActionInput);
            nextActionInput = PlayerInputs.ActionInputs.None;
        }
        else if (currentActionInput != PlayerInputs.ActionInputs.None)
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
        if (!CanMoveTo(gridPos))
        {
            Debug.Log("You can't move there.");
            StartCoroutine(BumpUnwalkableTileEffect(targetMoveDirection));
            targetGridPos = playerPos;
            currentActionInput = PlayerInputs.ActionInputs.None;
            return;
        }
        Vector3 targetMovePos = GameManager.Instance.hexGrid.GetWorldPos(gridPos);
        movements.Move(targetMovePos, customSpeed);
    }

    private IEnumerator BumpUnwalkableTileEffect(HexCoord.Orientation direction)
    {
        isBumpingUnwalkableTile = true;

        Vector3 cameraInitialPosition = playerCamera.transform.position;
        Vector3 bumpDirection = (direction * Quaternion.Euler(0f, -30f, 0f) * Vector3.forward).normalized;

        float safetyTimer = 0f;
        float bumpTimer = 0f;

        float t = bumpTimer / bumpTime;

        while (t < 1 && safetyTimer < 4f)
        {
            yield return null;
            playerCamera.transform.position = cameraInitialPosition + bumpDirection * bumpEffectCurve.Evaluate(t) * bumpAmplitude;

            bumpTimer += Time.deltaTime;
            t = bumpTimer / bumpTime;
            safetyTimer += Time.deltaTime;
        }
        if (safetyTimer >= 4f)
            Debug.LogWarning("exited via safetyTimer from BumpUnwalkableCoroutine");

        playerCamera.transform.position = cameraInitialPosition;
        isBumpingUnwalkableTile = false;
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

        if (mode_swingTakesYouWithIt)
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
        if(input == PlayerInputs.ActionInputs.Swing)
        {
            if(mode_swingTakesYouWithIt && demonicArm.isSwinging || isOnSwingCooldown)
            {
                return;
            }
        }

        if (mode_doubleInputReceiveMode)
        {
            //if (nextActionInput == PlayerInputs.ActionInputs.Swing) return;

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
                targetMoveDirection = invertedBallast ? edgeDirectionAntiForward : edgeDirectionForward;
                targetGridPos = HexCoord.GetNeighbour(playerPos, targetMoveDirection);
                //targetGridPos = invertedBallast ? HexCoord.GetNeighbour(playerPos, edgeDirectionAntiForward) : HexCoord.GetNeighbour(playerPos, edgeDirectionForward);
                break;

            case PlayerInputs.ActionInputs.Back:
                targetMoveDirection = invertedBallast ? edgeDirectionAntiBackward : edgeDirectionBackward;
                targetGridPos = HexCoord.GetNeighbour(playerPos, targetMoveDirection);
                //targetGridPos = invertedBallast ? HexCoord.GetNeighbour(playerPos, edgeDirectionAntiBackward) : HexCoord.GetNeighbour(playerPos, edgeDirectionBackward);
                break;

            case PlayerInputs.ActionInputs.Left:
                targetMoveDirection = edgeDirectionLeft;
                targetGridPos = HexCoord.GetNeighbour(playerPos, targetMoveDirection);
                //targetGridPos = HexCoord.GetNeighbour(playerPos, edgeDirectionLeft);
                break;

            case PlayerInputs.ActionInputs.Right:
                targetMoveDirection = edgeDirectionRight;
                targetGridPos = HexCoord.GetNeighbour(playerPos, targetMoveDirection);
                //targetGridPos = HexCoord.GetNeighbour(playerPos, edgeDirectionRight);
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

        //targetMoveDirection = HexCoord.GetCorrespondingOrientation(playerPos - targetGridPos);
    }

    public bool CanMoveTo(HexCoord coord)
    {
        if (!GameManager.Instance.hexGrid.IsValidMoveCoordinates(coord))
            return false;
        return !EnemiesManager.Instance.AnEnemyIsMovingToThisTile(coord);
    }
}
