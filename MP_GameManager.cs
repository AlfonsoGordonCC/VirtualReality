using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class MP_GameManager : MonoBehaviour
{
    public GameObject spawn;
    public GameObject cebo;
    public GameObject ceboLoc;

    private GameObject p1;
    private GameObject p2;
    private GameObject p3;
    private GameObject p4;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            p1 = PhotonNetwork.Instantiate("Player", spawn.transform.position, spawn.transform.rotation);

        }
        else
        {
            p2 = PhotonNetwork.Instantiate("Player", spawn.transform.position, spawn.transform.rotation);
            p3 = PhotonNetwork.Instantiate("Player", spawn.transform.position, spawn.transform.rotation);
            p4 = PhotonNetwork.Instantiate("Player", spawn.transform.position, spawn.transform.rotation);
        }
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Instantiate(cebo, ceboLoc.transform.position + new Vector3(UnityEngine.Random.Range(0, 6), 0, UnityEngine.Random.Range(0, 6)), ceboLoc.transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
