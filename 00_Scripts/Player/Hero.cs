using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Hero : Character
{
    public float attackRange = 1.0f;
    public float attackSpeed = 1.0f;
    public NetworkObject target;
    public LayerMask enemyLayer;

    private void Update()
    {
        CheckForEnemies();
    }

    void CheckForEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        attackSpeed += Time.deltaTime;
        if(enemiesInRange.Length > 0)
        {
            target = enemiesInRange[0].GetComponent<NetworkObject>();
            if(attackSpeed >= 1.0f)
            {
                attackSpeed = 0.0f;
                AnimatorChange("ATTACK", true);
                AttackMonsterServerRpc(target.NetworkObjectId);
            }
        }
        else
        {
            target = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackMonsterServerRpc(ulong monsterId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(monsterId, out var spawnedObject))
        {
            Monster monster = spawnedObject.GetComponent<Monster>();
            if(monster != null)
            {
                monster.GetDamage(10);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
