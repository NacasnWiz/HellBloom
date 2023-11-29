using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCharacter), typeof(PlayerController))]//I'm far from sure this architechture is optimal.
public class Player : MonoBehaviour
{
    [field: SerializeField] public PlayerController controller { get; private set; }
    [field: SerializeField] public PlayerCharacter character { get; private set; }

    private void Reset()
    {
        controller = gameObject.GetComponent<PlayerController>();
        character = gameObject.GetComponent<PlayerCharacter>();
    }

    private void Start()
    {
        Enemy.ev_attack.AddListener((attackedPos, damage) => { if (controller.playerPos == attackedPos) TakeDamage(damage); });
    }

    private void Update()
    {
        if(character.isAlive)
        {
            if(character.hasDemonInside)
                controller.TryToSwing();
            controller.TryToAct();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.Pause();
        }
    }

    public int TakeDamage(int amount)
    {
        Debug.Log("Player took " + amount + " points of damage!");
        character.Health -= amount;

        if(character.Health <= 0)
            character.Die();

        return character.Health;
    }

    public int GetHealth() { return character.Health; }
}
