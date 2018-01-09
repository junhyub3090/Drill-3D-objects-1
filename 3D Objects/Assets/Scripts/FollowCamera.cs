using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {
    public Transform target;
    private Vector3 offset;

    private void Start() {
        offset = transform.position;
    }

    void Update () {
        transform.position = target.position + offset;
	}
}
