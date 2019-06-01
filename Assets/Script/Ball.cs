using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

	private void Start ()
    {      
        StartCoroutine(SelfDestory());
	}

    IEnumerator SelfDestory()
    {
        yield return new WaitForSecondsRealtime(5.0f);
        Destroy(gameObject);
    }
}
