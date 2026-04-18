using UnityEngine;
using Pathfinding;

public class ZombieController : MonoBehaviour
{
public AIPath path;
public SpriteRenderer renderer;

void Update()  {
    if (path.desiredVelocity.x >= 0.01f)  {
        renderer.flipX = true;
    } else if (path.desiredVelocity.x <= 0.01f)  {
    renderer.flipX = false;
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
        // if (target == null) return;

        // Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        // float distance = Vector2.Distance(rb.position, target.position);

        // // рухаємося тільки якщо не на місці
        // if (distance > 0.05f)
        // {
        //     rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        // }
    }
}