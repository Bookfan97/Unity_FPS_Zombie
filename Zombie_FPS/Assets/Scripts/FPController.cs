using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class FPController : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;
    [SerializeField] GameObject _camera = null;
    [SerializeField] float xSensitivity = 2;
    [SerializeField] float ySensitivity = 2;
    [SerializeField] private float minX = -90;
    [SerializeField] private float maxX = 90;
    [SerializeField] private Animator _animator = null;
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private Quaternion cameraRotation;
    private Quaternion playerRotation;
    private float x;
    private float z;
    private bool isCursorLocked = true;
    private bool lockCursor = true;
    
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
        _capsuleCollider = this.GetComponent<CapsuleCollider>();
        cameraRotation = _camera.transform.localRotation;
        playerRotation = this.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _animator.SetBool("arm", !_animator.GetBool("arm"));
        }
        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger("fire");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            _animator.SetTrigger("reload");
        }

        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!_animator.GetBool("walking"))
            {
                _animator.SetBool("walking", true);
            }
        }
        else if(_animator.GetBool("walking"))
        {
            _animator.SetBool("walking", false);
        }
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
        
        //Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            _rigidbody.AddForce(0, 300, 0);
        }

        //Moving
        x = Input.GetAxis("Horizontal") * speed;
        z = Input.GetAxis("Vertical") * speed;
        transform.position += _camera.transform.forward * z + _camera.transform.right * x; //new Vector3(x * speed,0,z * speed);
        UpdateCursorLock();
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
}
