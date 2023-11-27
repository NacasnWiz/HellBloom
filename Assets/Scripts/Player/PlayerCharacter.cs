using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 10;
    private int _currentHp = 100;
    public int Health
    {
        get { return _currentHp; }
        set
        {
            if (_currentHp != value)
            {
                _currentHp = value;

                ev_playerHealthChanged.Invoke(_currentHp);
            }
            else { return; }
        }
    }

    [field: SerializeField]
    public bool isAlive { get; private set; } = true;
    [field: SerializeField]
    public bool hasDemonInside { get; private set; } = false;

    public UnityEvent ev_playerDied = new();
    public UnityEvent<int> ev_playerHealthChanged = new();


    private void Start()
    {
        GameManager.Instance.ev_playerPickedUpDemon.AddListener(() => hasDemonInside = true);

        Health = _maxHealth;
    }

    public void Die()
    {
        if(!isAlive)
        {
            Debug.Log("Player is already dead.");
            return;
        }

        isAlive = false;
        Debug.Log("Player has died.");
        ev_playerDied.Invoke();
    }
}
