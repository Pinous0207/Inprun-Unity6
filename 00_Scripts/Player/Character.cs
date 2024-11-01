using Unity.Netcode;
using UnityEngine;

public class Character : NetworkBehaviour
{
    protected Animator animator;
    protected SpriteRenderer renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    protected void AnimatorChange(string temp, bool Trigger)
    {
        if (Trigger)
        {
            animator.SetTrigger(temp);
        }
        else animator.SetBool(temp, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
