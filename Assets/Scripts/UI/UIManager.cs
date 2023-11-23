using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    { get { if (instance == null) Debug.LogError("UIManager is null !!!"); return instance; } }

    [SerializeField] private TMP_Text playerHealthText;



    private void Awake()
    {
        if(instance == null) instance = this; else { Debug.LogError("Tried to instantiate another UIManager !!"); Destroy(this); }
    }

    private void Start()
    {
        GameManager.Instance.player.character.ev_playerHealthChanged.AddListener((health) => ActualizePlayerHealth(health));
    }


    private void ActualizePlayerHealth(int health)
    {
        playerHealthText.text = health + " HP";
    }
}