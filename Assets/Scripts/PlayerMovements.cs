using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    public HexCoord pos = HexCoord.zero;
    public HexCoord.Orientation pointyOrientation = 0;//always acts like an edgeDirection with a ballastRight = left;
    public bool ballastRight = true;
    public HexCoord.Orientation edgeDirectionForward { get { return ballastRight ? (pointyOrientation - 1) : pointyOrientation; } }

    [SerializeField]
    private float transitionSpeed = 10f;
    [SerializeField]
    private float transitionRotationSpeed = 500f;

    public HexCoord targetGridPos;
    public HexCoord prevTargetGridPos;
    public Vector3 targetMovePos;
    public Quaternion targetRotation;

    private Quaternion leftRotation = Quaternion.Euler(0f, -60f, 0f);
    private Quaternion rightRotation = Quaternion.Euler(0f, 60f, 0f);

    public Quaternion ballastRotation { get { return ballastRight ? rightRotation : leftRotation; } }

    private PlayerInputs.MoveInputs moveInput = PlayerInputs.MoveInputs.None;
    private PlayerInputs.MoveInputs nextMoveInput = PlayerInputs.MoveInputs.None;
    

    public bool isMoving { get; private set; } = false;
    public bool isTurning { get; private set; } = false;
    [SerializeField]
    public float moveCooldown = 0.1f;
    public bool isOnMoveCooldown { get; private set; } = false;

    private bool canMove
    {
        get
        {
            if (!isMoving && !isTurning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void TargetMovePos()
    {

    }




    private void MovePlayer()
    {
        if(canMove)
        {
            StartCoroutine(MoveCoroutine(targetMovePos));
        }
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        isMoving = true;
        float safetyTimer = 0f;
        while(Vector3.Distance(transform.position, targetPosition) > 0.05f && safetyTimer < 10f)
        {
            yield return new WaitForFixedUpdate();
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            safetyTimer += Time.deltaTime;
        }
        isMoving = false;

        //nextMoveInput

        isOnMoveCooldown = true;
        yield return new WaitForSeconds(moveCooldown);
        isOnMoveCooldown = false;


    }


    private HexTile TargetTile(PlayerInputs.MoveInputs moveTo)
    {
        HexCoord.Orientation moveToDirection;

        switch (moveTo)
        {
            case PlayerInputs.MoveInputs.Forward:
                moveToDirection = edgeDirectionForward;
                break;

            case PlayerInputs.MoveInputs.Back:
                moveToDirection = ballastRight ? pointyOrientation - 2 : pointyOrientation + 2;
                break;

            case PlayerInputs.MoveInputs.Left:
                moveToDirection = pointyOrientation + 1;
                break;

            case PlayerInputs.MoveInputs.Right:
                moveToDirection = pointyOrientation - 2;
                break;

            case PlayerInputs.MoveInputs.TurnLeft:
            case PlayerInputs.MoveInputs.TurnRight:
            default:
                return HexGrid.Instance.GetTile(pos); //Player isn't moving.
        }

        return HexGrid.Instance.GetTile(HexCoord.GetNeighbour(pos, moveToDirection));
    }



    /// <summary>
    /// Set the transform.rotation to grid orientation
    /// </summary>
    public void AdjustRotationToOrientation()
    {
        Quaternion orientationRotation = Quaternion.identity;
        for (int i = 0; i < pointyOrientation; ++i)
        {
            orientationRotation *= leftRotation;
        }
        transform.rotation = orientationRotation;
    }


}