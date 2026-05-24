using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumpIn : MonoBehaviour
{
    private AIPath aipath;
    private Animator animator;
    public int zombieHP;

    // Start is called before the first frame update
    void Start()
    {
        aipath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (zombieHP <= 0)
        {
            animator.Play("death");
            aipath.maxSpeed = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && zombieHP >= 0)
        {
            zombieHP -= 10;
            StartCoroutine(Stun());      
        }
    }

    IEnumerator Stun()
    {
        aipath.canMove = false;
        animator.Play("stun");
        yield return new WaitForSeconds(3f);
        aipath.canMove = true;
        animator.Play("walk");
    }
}
