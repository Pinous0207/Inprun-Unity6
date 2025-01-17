using UnityEngine;

public class Player_Animation_Handler : MonoBehaviour
{
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void AnimationReturn() => animator.speed = 1.0f;
}
