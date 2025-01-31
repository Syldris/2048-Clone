using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayScriptableObject : MonoBehaviour
{
    public DataScriptableObject test;

    private void Start()
    {
        test.isturn = true;
    }

    //IEnumerator Play()
    //{
    //    yield return StartCoroutine(Stop());
    //}

    //IEnumerator Stop()
    //{

    //}
}
