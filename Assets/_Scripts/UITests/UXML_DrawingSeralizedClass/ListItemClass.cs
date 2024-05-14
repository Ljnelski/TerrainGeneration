using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ListItemClass : ScriptableObject
{
    [SerializeField] private float _floatData1;
    [SerializeField] private float _floatData2;
    [SerializeField] private string _stringData1;
    [SerializeField] private int _intData1;

    public void Logic()
    {
        Debug.Log("Doing Logic");
        Debug.Log("Float1: " + _floatData1);
        Debug.Log("Float2: " + _floatData2);
        Debug.Log("String1: " + _stringData1);
        Debug.Log("Int1: " + _intData1);
    }
}
