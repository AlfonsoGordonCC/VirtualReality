using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;


namespace Photon.Pun.Demo.PunBasics
{
    public class Launcher : MonoBehaviourPunCallbacks
    {

        [SerializeField]
        private Text infoSala;

        [SerializeField]
        private byte maxPlayersPerRoom = 4;
        string gameVersion = "1";

        [Space(5)]
        public Text connectionStatus;

        public static string playerName = "";
        public static string roomName = "";

        public static int arena { get; set; }
        private bool roomJoined;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            connectionStatus.text = "Conectando...";
            PlayerPrefs.DeleteAll();
            arena = 0;
            roomJoined = false;
            ConnectToPhoton();
        }

        private void ConnectToPhoton()
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            connectionStatus.text = "Conectando con Photon...";

        }

        private void Update()
        {

            if (roomName.Contains("door"))
            {

                if (OVRInput.GetDown(OVRInput.RawButton.A))
                {
                    JoinRoom();
                }

                if (OVRInput.GetDown(OVRInput.RawButton.B))
                {
                    LoadArena();
                }

            }
        }

        public override void OnConnected()
        {
            base.OnConnected();
            connectionStatus.text = "Conectado a Photon!";
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            connectionStatus.text = "Desconectado a Photon!";
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                infoSala.text = "Eres el master de la sala\nUsuario: " + playerName + "\nSala: " + roomName;

            }

            else
            {
                infoSala.text = "Eres cliente de la sala\nUsuario: " + playerName + "\nSala: " + roomName;
            }

        }

        public static void SetRoomName(string name)
        {
            roomName = name;
        }

        public static void SetPlayerName(string name)
        {
            playerName = name;
        }

        public void JoinRoom()
        {
            if (!PhotonNetwork.IsConnected)
            {
                connectionStatus.text = "La conexion con Photon no ha sido posible";
                this.roomJoined = false;
            }
            else
            {
                //voy a crear el cliente con un usuario aleatorio, para evitar fallos con el PlayerPrefs
                PhotonNetwork.LocalPlayer.NickName = playerName;
                RoomOptions ro = new RoomOptions();
                ro.MaxPlayers = maxPlayersPerRoom;
                TypedLobby tl = new TypedLobby(roomName, LobbyType.Default);

                //voy a crear la sala
                PhotonNetwork.JoinOrCreateRoom(roomName, ro, tl);

                roomJoined = true;
            }
        }

        public void LoadArena()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.LoadLevel(roomName);

            }
        }

    }
}