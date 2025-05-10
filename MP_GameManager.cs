using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;


namespace Photon.Pun.Demo.PunBasics
{
    public class MP_GameManager : MonoBehaviourPunCallbacks
    {
        public GameObject p1Spawn;
        public GameObject p2Spawn;
        public GameObject p3Spawn;
        public GameObject p4Spawn;
        //public GameObject[] spawnPoints;

        private GameObject p1;
        private GameObject p2;
        private GameObject p3;
        private GameObject p4;
        // private GameObject[] players;

        
        
        private void Start()
        {

            if (SceneManager.GetActiveScene().name == "WeriumStandAloneShip_2")
            {
                /*for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.LocalPlayer.IsMasterClient)
                    {
                        players[i] = PhotonNetwork.Instantiate("Plane_Barco", spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
                    }
                    else
                    {
                        players[i] = PhotonNetwork.Instantiate("Plane_Barco", spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
                    }

                }*/
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    p1 = PhotonNetwork.Instantiate("Plane_Barco", p1Spawn.transform.position, p1Spawn.transform.rotation);

                }
                else
                {
                    p2 = PhotonNetwork.Instantiate("Plane_Barco", p2Spawn.transform.position, p2Spawn.transform.rotation);
                    p3 = PhotonNetwork.Instantiate("Plane_Barco", p3Spawn.transform.position, p3Spawn.transform.rotation);
                    p4 = PhotonNetwork.Instantiate("Plane_Barco", p4Spawn.transform.position, p4Spawn.transform.rotation);
                }
            }
            else
            {
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    p1 = PhotonNetwork.Instantiate("Plane", p1Spawn.transform.position, p1Spawn.transform.rotation);
                }
                else
                {
                    p2 = PhotonNetwork.Instantiate("Plane", p2Spawn.transform.position, p2Spawn.transform.rotation);
                    p3 = PhotonNetwork.Instantiate("Plane", p3Spawn.transform.position, p3Spawn.transform.rotation);
                    p4 = PhotonNetwork.Instantiate("Plane", p4Spawn.transform.position, p4Spawn.transform.rotation);
                }

            }
        }

        //public override void OnPlayerLeftRoom(Player otherPlayer)
        //{
        //    if (otherPlayer.IsMasterClient)
        //    {
        //        SceneManager.LoadScene(0);
        //    }

        //}

    }
}