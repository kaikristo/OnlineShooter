using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
namespace KaiKristo.Shooter
{
    public class NetworkManager : MonoBehaviourPunCallbacks, ILobbyCallbacks
    {

        [Tooltip("Максимальное количество игроков")]
        [Range(1, 4)]
        [SerializeField]
        byte maxPlayers = 4;


        [SerializeField]
        GameObject JoinBtn, CreateBtn, ExitBtn;

        [SerializeField]
        Text LogText;

        [SerializeField]
        GameObject Loading;



        private void Awake()
        {
            Connect();
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 1;
        }


        #region Connection Methods
        public void Join()
        {
            PhotonNetwork.JoinRandomRoom();

        }

        private void Connect()
        {
            JoinBtn.gameObject.SetActive(false);
            CreateBtn.gameObject.SetActive(false);
            ExitBtn.gameObject.SetActive(false);
            Loading.SetActive(true);

            PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
            Log(PhotonNetwork.NickName);

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.ConnectUsingSettings();


        }

        public void CreateRoom()
        {
            PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = maxPlayers });
        }
        #endregion

        #region  Callbacks
        public override void OnCreatedRoom()
        {
            Log("Create");
        }

        public override void OnConnectedToMaster()
        {
            Log("Connect to master");
            JoinBtn.gameObject.SetActive(true);
            CreateBtn.gameObject.SetActive(true);
            ExitBtn.gameObject.SetActive(true);
            Loading.SetActive(false);
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            Log("Disconnected");
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Log("random join failed. creating new one");
            CreateRoom();

        }
        public override void OnJoinedRoom()
        {
            Log("Join");
            PhotonNetwork.LoadLevel("StartLevel");
        }


        #endregion

        public void Exit()
        {
            PhotonNetwork.Disconnect();
            Application.Quit();

        }

        private void Log(string message)
        {
            Debug.Log(message);
            LogText.text += "\n";
            LogText.text += message;
        }

    }
}
