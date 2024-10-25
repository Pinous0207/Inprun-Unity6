using System.Collections;
using UnityEngine;

public class Hero : Character
{
    public float attackRange = 1.0f;
    public float attackSpeed = 1.0f;
    public Monster target;
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
            target = enemiesInRange[0].GetComponent<Monster>();
            if(attackSpeed >= 1.0f)
            {
                attackSpeed = 0.0f;
                AttackEnemy(target);
            }
        }
        else
        {
            target = null;
        }
    }

    void AttackEnemy(Monster enemy)
    {
        AnimatorChange("ATTACK", true);
        enemy.GetDamage(10);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
