using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    { get { if (instance == null) Debug.LogError("UIManager is null !!!"); return instance; } }

    



    private void Awake()
    {
        if(instance == null) instance = this; else { Debug.LogError("Tried to instantiate another UIManager !!"); Destroy(this); }
    }



}