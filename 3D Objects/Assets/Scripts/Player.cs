using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Roll))]
public class Player : MonoBehaviour
{
    public GameObject drill;
    public Transform drillPoint;
    public float maxDrillExposeDistance = 1;
    private Vector3 drillDefaultLocalPosition;

    public GameObject legGroup;
    public Animator leftForwardLeg;
    public Animator rightForwardLeg;
    public Animator leftBackwardLeg;
    public Animator rightBackwardLeg;
    public float animationSpeed = .5f;

    public GameObject steamGroup;
    private ParticleSystem[] steams;

    public float forwardSpeed;
    public float backwardSpeedFactor;
    public float brakeFactor;
    public float stopThreshold;

    private bool isLegsOut = true;
    private Roll roll;

    private Vector3 movement;
    private Vector3 lastAddedVelocity = Vector3.zero;
    private new Rigidbody rigidbody;

    private bool braking = false;

    void Awake()
    {
        roll = GetComponent<Roll>();
        roll.affectable = false;

        rigidbody = GetComponent<Rigidbody>();

        steams = steamGroup.GetComponentsInChildren<ParticleSystem>();
    }

    private void Start()
    {
        drillDefaultLocalPosition = drill.transform.localPosition;
    }

    void Update()
    {
        if (braking) return;

        if (isLegsOut)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                MoveForward(true);
                movement.z = 1;
            }
            else if (Input.GetKeyUp(KeyCode.W))
            {
                MoveForward(false);
                movement.z = 0;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                MoveBackWard(true);
                movement.z = -backwardSpeedFactor;
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                MoveBackWard(false);
                movement.z = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isLegsOut)
            {
                legGroup.SetActive(false);
                isLegsOut = false;
                roll.affectable = true;
            }
            else
            {
                transform.Translate(transform.up * .2f);
                legGroup.SetActive(true);
                isLegsOut = true;
                roll.affectable = false;
                StartCoroutine("StartBraking");
            }
        }

        if (Input.GetMouseButton(0))
            ExposeDrill();
        else MoveDrillLocal(0);
    }

    private void FixedUpdate()
    {
        if (isLegsOut)
        {
            rigidbody.velocity -= lastAddedVelocity;
            lastAddedVelocity = movement * forwardSpeed;
            rigidbody.velocity += lastAddedVelocity;
        }
    }

    void MoveForward(bool isForward)
    {
        leftForwardLeg.speed = forwardSpeed * animationSpeed;
        rightForwardLeg.speed = forwardSpeed * animationSpeed;
        leftBackwardLeg.speed = forwardSpeed * animationSpeed;
        rightBackwardLeg.speed = forwardSpeed * animationSpeed;

        leftForwardLeg.SetBool("cw", isForward);
        rightForwardLeg.SetBool("ccw", isForward);
        leftBackwardLeg.SetBool("cw", isForward);
        rightBackwardLeg.SetBool("ccw", isForward);
    }

    void MoveBackWard(bool isBackward)
    {
        leftForwardLeg.speed = backwardSpeedFactor * forwardSpeed * animationSpeed;
        rightForwardLeg.speed = backwardSpeedFactor * forwardSpeed * animationSpeed;
        leftBackwardLeg.speed = backwardSpeedFactor * forwardSpeed * animationSpeed;
        rightBackwardLeg.speed = backwardSpeedFactor * forwardSpeed * animationSpeed;

        leftForwardLeg.SetBool("ccw", isBackward);
        rightForwardLeg.SetBool("cw", isBackward);
        leftBackwardLeg.SetBool("ccw", isBackward);
        rightBackwardLeg.SetBool("cw", isBackward);
    }

    IEnumerator StartBraking()
    {
        foreach (ParticleSystem ps in steams)
        {
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
        }
        braking = true;
        Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        int sign = localVelocity.x > 0 ? -1 : 1;
        while (true)
        {
            Quaternion q = transform.localRotation;
            q.x = 0;
            q.z = 0;
            transform.localRotation = q;
            localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
            if (localVelocity.x <= stopThreshold && localVelocity.x >= -stopThreshold)
                break;
            if (localVelocity.x * sign > 0)
                break;

            localVelocity.x += sign * (brakeFactor * Time.deltaTime);
            rigidbody.velocity = transform.TransformDirection(localVelocity);
            yield return null;
        }
        localVelocity.x = 0;
        rigidbody.velocity = transform.TransformDirection(localVelocity);
        braking = false;

        foreach (ParticleSystem ps in steams)
        {
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = false;
        }
    }

    void ExposeDrill()
    {
        RaycastHit hit;
        float distance = maxDrillExposeDistance;
        if (Physics.Raycast(drillPoint.position, transform.forward, out hit, maxDrillExposeDistance))
            distance = hit.distance;
        MoveDrillLocal(distance);
    }

    void MoveDrillLocal(float distance)
    {
        drill.transform.localPosition = drillDefaultLocalPosition + Vector3.forward * distance;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            other.gameObject.SetActive(false);
        }

    }
}
