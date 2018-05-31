using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follow : MonoBehaviour {

    private Vector3 _oldPos;

    public GameObject followObject;
    public GameObject lookAt;
    public float tension;
    private float speed;

    // Use this for initialization
    void Start () {
        _oldPos = this.transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 diffVec = followObject.transform.position - this.transform.position;
        speed = (this.transform.position - _oldPos).magnitude / Time.deltaTime;
        Vector3 diffUnitVec = diffVec.normalized;

        float acceleration = tension * diffVec.magnitude - Mathf.Sqrt(2 * tension) * speed;

        Vector3 newPos = this.transform.position + (speed + acceleration * Time.deltaTime) * diffUnitVec * Time.deltaTime;
        _oldPos = newPos;

        this.transform.position = newPos;
        this.transform.LookAt(lookAt.transform);
    }
}
