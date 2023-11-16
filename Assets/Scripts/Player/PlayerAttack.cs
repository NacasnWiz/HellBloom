using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public enum AttackType
    {
        None = 0,
        Regular = 1,
    }


    [field: SerializeField]
    public int attackDamage { get; private set; } = 1;

    public AttackType currentAttackType = AttackType.Regular;

    

    public void Attack(PlayerController attackingPlayer)
    {
        if(currentAttackType == AttackType.Regular)
        {
            HexCoord attackingPlayerPos = attackingPlayer.playerPos;
            HexCoord.Orientation attackingPlayerOrientation = attackingPlayer.playerOrientation;
            List<HexCoord> targettedTiles = new List<HexCoord>
            {
                HexCoord.GetNeighbour(attackingPlayerPos, attackingPlayerOrientation),
                HexCoord.GetNeighbour(attackingPlayerPos, attackingPlayerOrientation - 1)
            };

            GameManager.Instance.DamageTiles(targettedTiles, attackDamage);
        }
    }
}
