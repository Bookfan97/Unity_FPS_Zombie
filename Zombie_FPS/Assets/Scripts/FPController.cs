﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Random = UnityEngine.Random;

public class FPController : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;
    [SerializeField] GameObject _camera = null;
    [SerializeField] private Transform shotDirection = null;
    [SerializeField] float xSensitivity = 2;
    [SerializeField] float ySensitivity = 2;
    [SerializeField] private float minX = -90;
    [SerializeField] private float maxX = 90;
    [SerializeField] int ammoToAdd = 10;
    [SerializeField] int healthToAdd = 10;
    [SerializeField] int damageAmount = 10;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private AudioClip jump = null;
    [SerializeField] private AudioClip land = null;
    [SerializeField] private AudioClip ammoEmpty = null;
    [SerializeField] private AudioClip death = null;
    [SerializeField] private AudioClip ammoPickupSound = null;
    [SerializeField] private AudioClip healthPickupSound = null;
    [SerializeField] private AudioClip reloadSound = null;
    [SerializeField] private AudioClip[] footsteps = null;
    [SerializeField] private GameObject fullBodyPrefab = null;
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private Quaternion cameraRotation;
    private Quaternion playerRotation;
    private float x;
    private float z;
    private bool isCursorLocked = true;
    private bool lockCursor = true;
    private bool playingWalking = false;
    private bool previouslyGrounded = true;

    private AudioSource playerAudioSource;

    //Inventory
    private int ammo = 0;
    private int health = 0;
    private int ammoMax = 50;
    private int healthMax = 100;
    private int ammoClip = 0;

    private int ammoClipMax = 10;

    // Start is called before the first frame update
    void Start()
    {
        health = healthMax;
        _rigidbody = this.GetComponent<Rigidbody>();
        _capsuleCollider = this.GetComponent<CapsuleCollider>();
        cameraRotation = _camera.transform.localRotation;
        playerRotation = this.transform.localRotation;
        playerAudioSource = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _animator.SetBool("arm", !_animator.GetBool("arm"));
        }

        if (Input.GetMouseButtonDown(0) && !_animator.GetBool("fire"))
        {
            if (ammo > 0)
            {
                _animator.SetTrigger("fire");
                //ammo = Mathf.Max(ammo - 1 , 0, ammo-1);
                ammo--;
                ProcessZombieHit();
                Debug.Log("Ammo: " + ammo);
            }
            else if (_animator.GetBool("arm"))
            {
                playFPCSound(ammoEmpty);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && _animator.GetBool("arm") && ammo > 0)
        {
            _animator.SetTrigger("reload");
            int amountNeed = ammoClipMax - ammoClip;
            int ammoAvailable = amountNeed < ammo ? amountNeed : ammo;
            ammo -= ammoAvailable;
            ammoClip += ammoAvailable;
            Debug.Log("Ammo Left: " + ammo);
            Debug.Log("Ammo in clip: " + ammoClip);
            playFPCSound(reloadSound);
        }

        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!_animator.GetBool("walking"))
            {
                _animator.SetBool("walking", true);
                InvokeRepeating("PlayFootstepAudio", 0, 0.4f);
                playingWalking = true;
            }
        }
        else if (_animator.GetBool("walking"))
        {
            _animator.SetBool("walking", false);
            CancelInvoke("PlayFootstepAudio");
            playingWalking = false;
        }

        //Jumping
        bool grounded = isGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            _rigidbody.AddForce(0, 300, 0);
            playFPCSound(jump);
            if (_animator.GetBool("walking"))
            {
                CancelInvoke("PlayFootstepAudio");
                playingWalking = false;
            }
        }
        else if (!previouslyGrounded & grounded)
        {
            playFPCSound(land);
        }

        previouslyGrounded = grounded;
    }

    void ProcessZombieHit()
    {
        RaycastHit hit;
        if (Physics.Raycast(shotDirection.position, shotDirection.forward, out hit, 200))
        {
            GameObject hitZombie = hit.collider.gameObject;
            if (hitZombie.tag == "Zombie")
            {
                if (Random.Range(0, 10) < 5)
                {
                    GameObject ragdollPrefab = hitZombie.GetComponent<ZombieController>().GetRagdoll();
                    GameObject newRagDoll = Instantiate(ragdollPrefab, hitZombie.transform.position,
                        hitZombie.transform.rotation);
                    newRagDoll.transform.Find("Hips").GetComponent<Rigidbody>()
                        .AddForce(shotDirection.forward * 10000);
                    Destroy(hitZombie);
                }
                else
                {
                    hitZombie.GetComponent<ZombieController>().KillZombie();
                }
            }
        }
    }

    public void playFPCSound(AudioClip sound)
    {
        playerAudioSource.Stop();
        playerAudioSource.clip = sound;
        playerAudioSource.Play();
    }

    void PlayFootstepAudio()
    {
        int n = Random.Range(1, footsteps.Length);
        playFPCSound(footsteps[n]);
        footsteps[n] = footsteps[0];
        footsteps[0] = playerAudioSource.clip;
        playingWalking = true;
    }

    private void FixedUpdate()
    {
        //Camera Rotation
        float xRotation = Input.GetAxis("Mouse Y") * xSensitivity;
        float yRotation = Input.GetAxis("Mouse X") * ySensitivity;

        cameraRotation *= Quaternion.Euler(-xRotation, 0, 0);
        playerRotation *= Quaternion.Euler(0, yRotation, 0);
        cameraRotation = ClampRotationAroundXAxis(cameraRotation);
        this.transform.localRotation = playerRotation;
        _camera.transform.localRotation = cameraRotation;

        //Moving
        x = Input.GetAxis("Horizontal") * speed;
        z = Input.GetAxis("Vertical") * speed;
        transform.position +=
            _camera.transform.forward * z + _camera.transform.right * x; //new Vector3(x * speed,0,z * speed);
        UpdateCursorLock();
    }

    bool isGrounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, _capsuleCollider.radius,
            Vector3.down, out hit, _capsuleCollider.height / 2 - _capsuleCollider.radius + 0.1f))
        {
            return true;
        }

        return false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ammo" && ammo < ammoMax)
        {
            playFPCSound(ammoPickupSound);
            ammo = Mathf.Clamp(ammo + ammoToAdd, 0, ammoMax);
            Debug.Log("Ammo: " + ammo);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "Health" && health < healthMax)
        {
            playFPCSound(healthPickupSound);
            health = Mathf.Clamp(health + healthToAdd, 0, healthMax);
            Debug.Log("Health: " + health);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "Lava")
        {
            health = Mathf.Clamp(health - damageAmount, 0, healthMax);
            Debug.Log("Health: " + health);
            playFPCSound(death);
        }
        else if (isGrounded())
        {
            if (_animator.GetBool("walking") && !playingWalking)
            {
                InvokeRepeating("PlayFootstepAudio", 0, 0.4f);
            }
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion quaternion)
    {
        quaternion.x /= quaternion.w;
        quaternion.y /= quaternion.w;
        quaternion.z /= quaternion.w;
        quaternion.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(quaternion.x);
        angleX = Mathf.Clamp(angleX, minX, maxX);
        quaternion.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return quaternion;
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
        {
            InternalLockUpdate();
        }
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            isCursorLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isCursorLocked = true;
        }

        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public void TakeHit(float amount)
    {
        health = (int) Mathf.Clamp(health - amount, 0, healthMax);
        if (health<=0)
        {
            PlayerGameOver("Death");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Safe")
        {
            PlayerGameOver("Dance");
        }
    }
    
    private void PlayerGameOver(string triggerName)
    {
        Vector3 position = new Vector3(this.transform.position.x,
            Terrain.activeTerrain.SampleHeight(this.transform.position), 
            this.transform.position.z);
        GameObject fullBody = Instantiate(fullBodyPrefab, position, this.transform.rotation);
        fullBody.GetComponent<Animator>().SetTrigger(triggerName);
        GameManager.gameOver = true;
        Destroy(this.gameObject);
    }
}