using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClassWithList : MonoBehaviour
{
    public List<ListItemClass> list;

    public void LoadMockData()
    {
        list = new List<ListItemClass>();

        list.Add(ListItemClass.CreateInstance<ListItemClass>());
        list.Add(ListItemClass.CreateInstance<ListItemClass>());
    }
}
