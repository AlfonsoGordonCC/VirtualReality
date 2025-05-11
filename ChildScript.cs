using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildScript : MonoBehaviour
{
    public GameObject nino;
    private bool juegoEmpezado;
    private int tiempoJuego = 120;

    private void Start()
    {
        nino.SetActive(false);
        juegoEmpezado = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (juegoEmpezado)
        {
            if (Time.deltaTime > tiempoJuego)
            {
                juegoEmpezado = false;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            nino.SetActive(true);
            juegoEmpezado = true;
        }
    }
}
