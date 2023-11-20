using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshRenderer))]
public class DemonicArm : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps_readyToSwing;

    [SerializeField] private MeshRenderer _mr;

    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material exhaustedMaterial;

    private bool isOnLeft = false;//not physically linked to PlayerController.ballastLeft

    [SerializeField] private Vector3 leftAnchor = new(1f, 0.35f, 0f);
    [SerializeField] private Vector3 rightAnchor = new(-1f, 0.35f, 0f);

    public bool isSwinging { get; private set; }
    [field: SerializeField]
    public float swingSpeed { get; private set; } = 10f;


    public readonly UnityEvent doneSwinging = new UnityEvent();


    private void Reset()
    {
        _mr = gameObject.GetComponent<MeshRenderer>();
    }

    public void Swing()
    {
        Vector3 targetSwingPosition = isOnLeft ? leftAnchor : rightAnchor;
        StartCoroutine(SwingCoroutine(targetSwingPosition));
    }

    /// <summary>
    /// Moves the arm and actualizes local position
    /// </summary>
    /// <param name="targetSwingPosition"></param>
    /// <returns></returns>
    private IEnumerator SwingCoroutine(Vector3 targetSwingPosition)
    {
        isSwinging = true;
        float safetyTimer = 0f;
        while (Vector3.Distance(transform.localPosition, targetSwingPosition) > 0.01f && safetyTimer < 5f)
        {
            yield return null; //new WaitForFixedUpdate();

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetSwingPosition, swingSpeed * Time.deltaTime);
            safetyTimer += Time.deltaTime;
        }
        transform.localPosition = targetSwingPosition;
        isSwinging = false;
        isOnLeft = !isOnLeft;

        doneSwinging.Invoke();
    }

    public void ChangeAppearance(bool readyToSwing)
    {
        if(readyToSwing)
        {
            ps_readyToSwing.Play();
        }
        else
        {
            ps_readyToSwing.Stop();
        }

        _mr.material = readyToSwing ? baseMaterial : exhaustedMaterial;
    }

}