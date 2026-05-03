using UnityEngine;
using Pathfinding;

public class ZombieController : MonoBehaviour
{
public AIPath path;
public SpriteRenderer renderer;

[SerializeField] private Patrol ps;
[SerializeField] private AIDestinationSetter ds;
[SerializeField] private GameObject target;

[SerializeField] private float myDistance = 3f;

void Update()  {
    if (path.desiredVelocity.x >= 0.01f)  
    {
        renderer.flipX = false;
    }
     else if (path.desiredVelocity.x <= 0.01f) 
    {
        renderer.flipX = true;
    }
}

// [RequireComponent(typeof(Rigidbody2D))]
//public class ZombieController : MonoBehaviour
    // public Transform target;    // ціль
    // public float speed = 3f;    // швидкість

    // private Rigidbody2D rb;

    void Start()
    {
        // rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);

        if(distance < myDistance)
        {
            ps.enabled = false;
            ds.enabled = true;
        }
        else
        {
            ds.enabled = false;
            ps.enabled = true;
        }
    }
}