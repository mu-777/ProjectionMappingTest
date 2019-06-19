using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

    public float degPerSec = 50;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {
        this.transform.Rotate(Vector3.up, degPerSec * Time.deltaTime, Space.World);
    }
}
