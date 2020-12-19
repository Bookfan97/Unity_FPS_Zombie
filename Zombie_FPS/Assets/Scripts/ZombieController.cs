using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int IsDead = Animator.StringToHash("isDead");

    // Start is called before the first frame update
    void Start()
    {
        _animator = this.GetComponent<Animator>();
        _navMeshAgent = this.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
       
        _navMeshAgent.SetDestination(player.transform.position);
        if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
        {
             _animator.SetBool(IsWalking, true);
             _animator.SetBool(IsAttacking, false);
        }
        else
        {
            _animator.SetBool(IsWalking, false);
            _animator.SetBool(IsAttacking, true);
        }
    }
}
