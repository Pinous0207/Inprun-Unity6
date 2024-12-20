using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float m_Speed;
    [SerializeField] private GameObject Destroy_Particle;
    private Transform target;
    Hero parentHero;

    public void Init(Transform t, Hero hero)
    {
        target = t;
        parentHero = hero;
    }

    private void Update()
    {
        float distance = Vector2.Distance(transform.position, target.position);
        if(distance > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, m_Speed * Time.deltaTime);
        }
        else if(distance <= 0.1f)
        {
            Instantiate(Destroy_Particle, transform.position, Quaternion.identity);
            parentHero.SetDamage();
            Destroy(this.gameObject);
        }
    }
}
