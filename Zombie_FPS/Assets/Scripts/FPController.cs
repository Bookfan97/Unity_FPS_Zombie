using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPController : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;
    [SerializeField] GameObject _camera = null;
    [SerializeField] float xSensitivity = 2;
    [SerializeField] float ySensitivity = 2;
    [SerializeField] private float minX = -90;
    [SerializeField] private float maxX = 90;
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private Quaternion cameraRotation;
    private Quaternion playerRotation;
    
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
        
    }

    private void FixedUpdate()
    {
        //Camera Rotation
        float xRotation = Input.GetAxis("Mouse X") * ySensitivity;
        float yRotation = Input.GetAxis("Mouse Y") * xSensitivity;

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
        float x = Input.GetAxis("Horizontal") * speed;
        float z = Input.GetAxis("Vertical") * speed;
        transform.position += _camera.transform.forward * z + _camera.transform.right * x; //new Vector3(x * speed,0,z * speed);
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
}
