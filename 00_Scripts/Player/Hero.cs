using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Hero : Character
{
    Hero_Holder parent_holder;
    public int ATK;
    public float attackRange = 1.0f;
    public float attackSpeed = 1.0f;
    public NetworkObject target;
    public LayerMask enemyLayer;
    public Hero_Scriptable m_Data;

    public void Initalize(HeroData obj, Hero_Holder holder)
    {
        parent_holder = holder;
        ATK = obj.heroATK;
        attackRange = obj.heroRange;
        attackSpeed = obj.heroATK_Speed;
        GetInitCharacter(obj.heroName);
    }

    private void Update()
    {
        CheckForEnemies();
    }

    void CheckForEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(parent_holder.transform.position, attackRange, enemyLayer);
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
                monster.GetDamage(ATK);
            }
        }
    }
}
