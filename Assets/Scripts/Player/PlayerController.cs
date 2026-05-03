using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform targetFront;
    [SerializeField] private Transform targetBack;
    private Vector2 directionFront;
    private Vector2 directionBack;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        directionFront = targetFront.position - transform.position;
        directionBack = targetBack.position - transform.position;

       if(Input.GetKey(KeyCode.W))
        {
            rb.AddForce(directionFront);
        }
       
       if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(directionBack);
        }

       if (Input.GetKey(KeyCode.D))
        {
            rb.AddTorque(-rotationSpeed);
        }
       if (Input.GetKey(KeyCode.A))
        {
            rb.AddTorque(rotationSpeed);
        }


    }
}
