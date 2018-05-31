using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCenter : MonoBehaviour {

    protected Vector3 _LocalRotation;
    public float MouseSensitivity = 4f;

    // Use this for initialization
    void Start () {
        _LocalRotation.y = this.transform.localRotation.eulerAngles.x;
        _LocalRotation.x = this.transform.localRotation.eulerAngles.y;
        print("Start rot: " + _LocalRotation.x + ", " + _LocalRotation.y);
    }

    // Update is called once per frame
    void Update () {

        //Rotation of the Camera based on Mouse Coordinates
        if (Input.GetMouseButton(0))
        {
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                _LocalRotation.x += 0.5f * Input.GetAxis("Mouse X") * MouseSensitivity;
                _LocalRotation.y -= 0.5f * Input.GetAxis("Mouse Y") * MouseSensitivity;

                print("localrot: " + _LocalRotation.x + ", " + _LocalRotation.y);

                //Clamp the y Rotation to horizon and not flipping over at the top
                if (_LocalRotation.y < 0f)
                    _LocalRotation.y = 0f;
                else if (_LocalRotation.y > 80f)
                    _LocalRotation.y = 80f;
            }
            this.transform.localRotation = Quaternion.Euler(_LocalRotation.y, _LocalRotation.x, 0);
            print("Rot center: " + this.transform.localRotation.x + ", " + this.transform.localRotation.y);
        }
    }
}
