using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentBehaviour : MonoBehaviour
{
    [SerializeField] public List<ChildBehaviour> childBehaviours;

    private void Awake()
    {
        getChildren();
    }

    public void getChildren()
    {
        childBehaviours.Clear();
        childBehaviours = new List<ChildBehaviour>(GetComponents<ChildBehaviour>());

    }
}
