using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceToForget = 20;
    [SerializeField] private float walkingSpeed =1;
    [SerializeField] private float runningSpeed=2;
    [SerializeField] private GameObject ragDoll = null;
    [SerializeField] private float damageAmount = 5;
    [SerializeField] private AudioClip[] splats = null;
    public Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private AudioSource playerAudioSource;
  enum STATE
    {
        IDLE,
        WANDER,
        ATTACK,
        CHASE,
        DEAD
    };

    private STATE State = STATE.IDLE;
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = this.GetComponent<Animator>();
        _navMeshAgent = this.GetComponent<NavMeshAgent>();
        playerAudioSource = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.P))
        {
            if (Random.Range(0, 10) < 5)
            {
                GameObject newRagDoll = Instantiate(ragDoll, this.transform.position, this.transform.rotation);
                newRagDoll.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 100000);
                Destroy(this.gameObject);
            }
            else
            {
                TurnOffTriggers();
                _animator.SetBool(IsDead, true);
                State = STATE.DEAD;
            }
            return;
        }*/
        if (player == null && GameManager.gameOver == false)
        {
            player = GameObject.FindWithTag("Player");
            return;
        }
        switch (State)
        {
            case STATE.IDLE:
                if (CanSeePlayer())
                {
                    State = STATE.CHASE;
                }
                else if (Random.Range(0, 5000) < 5)
                {
                    State = STATE.WANDER;    
                }
                break;
            case STATE.WANDER:
                if (!_navMeshAgent.hasPath)
                {
                    float newX = this.transform.position.x + Random.Range(-5, 5);
                    float newZ = this.transform.position.z + Random.Range(-5, 5);
                    float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 destination = new Vector3(newX, newY, newZ);
                    _navMeshAgent.SetDestination(destination);
                    _navMeshAgent.stoppingDistance = 0;
                    TurnOffTriggers();
                    _navMeshAgent.speed = walkingSpeed;
                    _animator.SetBool(IsWalking, true);
                }
                if (CanSeePlayer())
                {
                    State = STATE.CHASE;
                }
                else if (Random.Range(0, 5000) < 5)
                {
                    State = STATE.IDLE;
                    TurnOffTriggers();
                    _navMeshAgent.ResetPath();
                }
                break;
            case STATE.CHASE:
                if (GameManager.gameOver)
                {
                    TurnOffTriggers();
                    State = STATE.WANDER;
                    return;
                }
                _navMeshAgent.SetDestination(player.transform.position);
                _navMeshAgent.stoppingDistance = 2;
                TurnOffTriggers();
                _navMeshAgent.speed = runningSpeed;
                _animator.SetBool(IsRunning, true);
                if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.pathPending)
                {
                    State = STATE.ATTACK;
                }
                if (ForgetPlayer())
                {
                    State = STATE.WANDER;
                    _navMeshAgent.ResetPath();
                }
                break;
            case STATE.ATTACK:
                if (GameManager.gameOver)
                {
                    TurnOffTriggers();
                    State = STATE.WANDER;
                    return;
                }
                TurnOffTriggers();
                _animator.SetBool(IsAttacking, true);
                this.transform.LookAt(player.transform.position);
                if (DistanceToPlayer() > _navMeshAgent.stoppingDistance + 2)
                {
                    State = STATE.CHASE;
                }
                break;
            case STATE.DEAD:
                /*TurnOffTriggers();
                _animator.SetBool(IsDead, true);*/
                Destroy(_navMeshAgent);
                this.GetComponent<Sink>().StartSink();
                break;
        }
    }

    public void TurnOffTriggers()
    {
        _animator.SetBool(IsWalking, false);
        _animator.SetBool(IsAttacking, false);
        _animator.SetBool(IsRunning, false);
        _animator.SetBool(IsDead, false);
    }

    bool CanSeePlayer()
    {
        if (DistanceToPlayer() < 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private float DistanceToPlayer()
    {
        if (GameManager.gameOver)
        {
            return Mathf.Infinity;
        }
        else
        {
            return Vector3.Distance(player.transform.position, this.transform.position);
        }
    }

    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > distanceToForget)
        {
            return true;
        }

        return false;
    }

    public void DamagePlayer()
    {
        if (player != null)
        {
            player.GetComponent<FPController>().TakeHit(damageAmount);
            int n = Random.Range(1, splats.Length);
            playZombieSound(splats[n]);
            splats[n] = splats[0];
            splats[0] = playerAudioSource.clip;
        }
    }

    public void playZombieSound(AudioClip sound)
    {
        playerAudioSource.Stop();
        playerAudioSource.clip = sound;
        playerAudioSource.Play();
    }

    
    public void KillZombie()
    {
        TurnOffTriggers();
        _animator.SetBool(IsDead, true);
        State = STATE.DEAD;
    }

    public GameObject GetRagdoll()
    {
        return ragDoll;
    }
}
