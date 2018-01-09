using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour {
    public float maxSpeed;
    public float accel;
    public float brakeFactor;

    [HideInInspector]
    public bool affectable = true;

    private new Rigidbody rigidbody;

    void Awake() {
        rigidbody = GetComponent<Rigidbody>();    
    }

    void Update () {
        if (!affectable) return;

        float force = accel * Input.GetAxis("Horizontal");
        Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        if (localVelocity.x * force < 0) force *= brakeFactor;
        rigidbody.AddForce(transform.right * force);
	}
}
