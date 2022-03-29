using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {
    public GameObject anchor;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
        this.transform.rotation = anchor.transform.rotation;
        this.transform.position = anchor.transform.position;
    }
}
