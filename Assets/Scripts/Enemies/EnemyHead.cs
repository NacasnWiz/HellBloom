using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHead : MonoBehaviour
{
    [SerializeField] private GameObject leftEye;
    [SerializeField] private GameObject rightEye;
    [SerializeField] private GameObject horn;

    private Material m_leftEyeMaterial;
    private Material m_rightEyeMaterial;

    private void Start()
    {
        m_leftEyeMaterial = leftEye.GetComponent<MeshRenderer>().material;
        m_rightEyeMaterial = rightEye.GetComponent<MeshRenderer>().material;
    }

    public IEnumerator AttackCoroutine()
    {
        //m_head.transform.rotation *= Quaternion.Euler(5f, 0f, 0f);
        horn.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        //m_head.transform.rotation *= Quaternion.Euler(-5f, 0f, 0f);
        horn.SetActive(false);
    }

    public IEnumerator FlickerEyesCoroutine()
    {
        float timer = 0f;
        int blazeEyes = 0;
        while (timer < 0.55f)
        {
            m_leftEyeMaterial.SetFloat("_Blazing", blazeEyes);
            m_rightEyeMaterial.SetFloat("_Blazing", blazeEyes);
            blazeEyes = 1 - blazeEyes;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }
    }

}
