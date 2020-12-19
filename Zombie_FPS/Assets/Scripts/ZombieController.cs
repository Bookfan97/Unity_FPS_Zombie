using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            _animator.SetBool("isWalking", true);
        }
        else if (Input.GetKey(KeyCode.R))
        {
            _animator.SetBool("isRunning", true);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _animator.SetBool("isAttacking", true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _animator.SetBool("isDead", true);
        }
        else 
        {
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isAttacking", false);
        }
    }
}
