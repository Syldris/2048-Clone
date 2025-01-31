using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class DataScriptableObject : ScriptableObject
{
    public int[] numbers;
    public bool isturn;
}
