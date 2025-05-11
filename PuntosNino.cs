using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuntosNino : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (this.gameObject.tag == "LostChild")
            {

                collision.gameObject.GetComponent<SerializableUserData>().Moneda = collision.gameObject.GetComponent<SerializableUserData>().Moneda + 10;
            }
        }
    }
}
