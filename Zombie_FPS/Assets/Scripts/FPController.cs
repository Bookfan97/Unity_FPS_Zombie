using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPController : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
        _capsuleCollider = this.GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            _rigidbody.AddForce(0, 300, 0);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        transform.position += new Vector3(x * speed,0,z * speed);
    }

    bool isGrounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, _capsuleCollider.radius, 
            Vector3.down, out hit, _capsuleCollider.height/2 - _capsuleCollider.radius + 0.1f))
        {
            return true;
        }
        return false;
    }
}
