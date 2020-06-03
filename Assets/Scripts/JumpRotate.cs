using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpRotate : MonoBehaviour {

	[SerializeField]
	private float rotateSpeed;
	[SerializeField]
	private float jumpSpeed;
	[SerializeField]
	private float jumpOffset;

	private bool dirUp;

	private float startPosition, topEndPosition, bottomEndPosition;
	// Use this for initialization
	void Start () {
		startPosition = transform.position.y;
		topEndPosition = startPosition + jumpOffset;
		bottomEndPosition = startPosition - jumpOffset;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(new Vector3(Time.deltaTime * rotateSpeed,0,0));

		if (dirUp)
			transform.Translate(Vector2.right * jumpSpeed * Time.deltaTime);
		else
			transform.Translate(-Vector2.right * jumpSpeed * Time.deltaTime);

		if (transform.position.y >= topEndPosition)
		{
			dirUp = false;
		}

		if (transform.position.y <= bottomEndPosition)
		{
			dirUp = true;
		}
	}
}
