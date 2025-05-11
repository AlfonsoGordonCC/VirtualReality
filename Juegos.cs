using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;
using UnityEngine.SceneManagement;

public class Juegos : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (this.gameObject.tag.Contains("door"))
        {
            Launcher.roomName = this.gameObject.tag;
        }
        if (OVRInput.GetDown(OVRInput.RawButton.A) && this.gameObject.tag.Contains("door"))
        {
            string[] hilos = this.gameObject.tag.Split('_');
            SceneManager.LoadScene(hilos[1]);
        }
    }
}
