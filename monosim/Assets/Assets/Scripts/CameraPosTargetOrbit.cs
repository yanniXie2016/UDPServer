using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosTargetOrbit : MonoBehaviour {

    protected float _CameraDistance = 10f;
    protected float _CameraX = 0;
    protected float _CameraY = 0f;

    public float ScrollSensitvity = 2f;
    private float ScrollAmount = 0;

    // Use this for initialization
    void Start () {
        _CameraDistance = -this.transform.localPosition.z;
    }

    // Update is called once per frame
    void Update () {
        bool doUpdate = false;
        
        // Zooming Input from our Mouse Scroll Wheel
        if (Input.GetMouseButton(1))
        {
            ScrollAmount = Input.GetAxis("Mouse Y") * 0.2f;
            doUpdate = true;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitvity;
            doUpdate = true;
        }
        if (doUpdate)
        { 
            ScrollAmount *= (this._CameraDistance * 0.3f);

            this._CameraDistance += ScrollAmount;

            this._CameraDistance = Mathf.Clamp(this._CameraDistance, 1.5f, 100f);
            //Actual Camera Rig Transformations
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, -_CameraDistance);

            print("Rotation orbit: " + this.transform.parent.localRotation.x + ", " + this.transform.parent.localRotation.y);
        }
    }
}
