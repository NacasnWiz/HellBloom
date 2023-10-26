using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovements : MonoBehaviour
{
    [SerializeField]
    private float transitionSpeed = 10f;
    [SerializeField]
    private float transitionRotationSpeed = 500f;

    public bool isMoving { get; private set; } = false;
    public bool isRotating { get; private set; } = false;

    [SerializeField]
    private bool test_weirdAlliasedMovementMode = false;

    public readonly UnityEvent doneMoving = new UnityEvent();
    public readonly UnityEvent doneRotating = new UnityEvent();

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
            if (test_weirdAlliasedMovementMode)
                yield return new WaitForFixedUpdate();
            else
                yield return null; //new WaitForFixedUpdate();

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, transitionRotationSpeed * Time.deltaTime);
            safetyTimer += Time.deltaTime;
        }
        transform.rotation = targetRotation;
        isRotating = false;

        doneRotating.Invoke();
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        isMoving = true;
        float safetyTimer = 0f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f && safetyTimer < 10f)
        {
            if (test_weirdAlliasedMovementMode)
                yield return new WaitForFixedUpdate();
            else
                yield return null; //new WaitForFixedUpdate();

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            safetyTimer += Time.deltaTime;
        }
        transform.position = targetPosition;
        isMoving = false;

        doneMoving.Invoke();
    }

}