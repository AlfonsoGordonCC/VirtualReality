using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawn : MonoBehaviour
{
    public GameObject objecto;
    private GameObject aux;
    public Transform posInit;

    // Update is called once per frame
    void Update()
    {
        if (objecto.transform.position != posInit.position)
        {
            aux = Instantiate(objecto, posInit);
            objecto = aux;
            posInit = aux.transform;
        }
    }
}
