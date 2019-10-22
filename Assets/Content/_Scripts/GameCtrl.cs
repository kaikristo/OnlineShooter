using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace KaiKristo.Shooter
{
    public class GameCtrl : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static GameCtrl instance;

        [SerializeField]
        GameObject playerPrefab;

        [Header("Точки возрождения")]
        [SerializeField]
        GameObject[] spawnPoints;


        private GameObject player; //игрок которым управляем локальный пользователь

        #region Константы событий    
        private const byte CodeHit = 100; //Попадание
        private const byte CodeShoot = 101; // Выстрел
        private const byte CodeKill = 102; // Килл
        private const byte CodeChangeWeapon = 103;//Смена Оружия
        #endregion

        private int killCount = 0;

        void Awake()
        {
            if (instance != null)
            {
                if (instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
        }


        private void Start()
        {
            CreatePlayer();
            PhotonNetwork.AddCallbackTarget(this);

        }


        private void CreatePlayer()
        {

            GameObject spawn = spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount];
            player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.transform.position, spawn.transform.rotation);
            player.name = PhotonNetwork.NickName;

        }

      

        //возрождение после смерти идёт на случайной позиции
        public void RespawnAtRandomPosition(PlayerCtrl player)
        {
            player.transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
        }

        //отправка всем игроками, что другой игрок выстрелил
        public void SendShootCommand(int ActorNumber, Vector3 position, Quaternion rotation)
        {
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            object[] info = new object[] {ActorNumber, position , rotation};
            PhotonNetwork.RaiseEvent(CodeShoot, info, options, sendOptions);
            Debug.Log("Player " + info + " shooting");
        }



        //отправка "по тебе попали" плееру
        public void SendHitCommand(GameObject target, GameObject shooter, int damage)
        {
            int targetActorNumber = target.GetComponent<PhotonView>().Owner.ActorNumber;
            if (target.GetComponent<PlayerCtrl>().HitPoints - damage <= 0 && !target.GetComponent<PlayerCtrl>().IsDead)
            {
                SendKillCommand(shooter);
            }
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            byte[] info = new byte[2];
            info[0] = (byte)targetActorNumber;
            info[1] = (byte)damage;
            PhotonNetwork.RaiseEvent(CodeHit, info, options, sendOptions);

        }

        //отправка "ты убил кого то" плееру
        private void SendKillCommand(GameObject shooter)
        {
            int actorNumber = shooter.GetComponent<PhotonView>().Owner.ActorNumber;

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            byte info = (byte)actorNumber;

            PhotonNetwork.RaiseEvent(CodeKill, info, options, sendOptions);

        }

        internal void SendChangeWeaponCommand(GameObject target, int weaponIndex)
        {
            int actorNumber = target.GetComponent<PhotonView>().Owner.ActorNumber;

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            byte[] info = new byte[2];
            info[0] = (byte)actorNumber;
            info[1] = (byte)weaponIndex;
            PhotonNetwork.RaiseEvent(CodeChangeWeapon, info, options, sendOptions);
        }


        /*  
         *  приём кастомных событий:
         *  попадание 
         *  выстрел
         *  убийство
         */

        public void OnEvent(EventData photonEvent)
        {
            int playersCount = 0;

            foreach (var playerCtrl in GameObject.FindObjectsOfType<PlayerCtrl>())
                playersCount++;
            //синхронизация еще не заверешена.
            if (PhotonNetwork.PlayerList.Length != playersCount) return;


            switch (photonEvent.Code)
            {
                case CodeHit:
                    {
                        byte[] info = (byte[])photonEvent.CustomData;
                        if (info[0] == player.GetComponent<PhotonView>().Owner.ActorNumber)
                        {
                            player.GetComponent<PlayerCtrl>().Hit((int)info[1]);
                        }
                        break;
                    }
                case CodeShoot:
                    {
                        object[] info = (object[])photonEvent.CustomData;
                        //кто стрелял
                        int ActorNumber = (int)info[0];
                        //откуда
                        Vector3 position = (Vector3)info[1];
                        Quaternion rotation = (Quaternion)info[2];

                        foreach (var playerCtrl in GameObject.FindObjectsOfType<PlayerCtrl>())
                        {

                            if (playerCtrl.GetComponent<PhotonView>().Owner.ActorNumber == ActorNumber)
                            {
                                playerCtrl.gun.ShootFromServer(position,rotation);
                                break;
                            }
                        }

                        break;
                    }

                case CodeKill:
                    {
                        byte info = (byte)photonEvent.CustomData;
                        if (info == player.GetComponent<PhotonView>().Owner.ActorNumber)
                        {
                            killCount++;
                            FindObjectOfType<UIManager>().RefreshKillCount(killCount);
                        }
                        break;
                    }
                case CodeChangeWeapon:
                    {

                        byte[] info = (byte[])photonEvent.CustomData;
                        foreach (var playerCtrl in GameObject.FindObjectsOfType<PlayerCtrl>())
                        {
                            if (playerCtrl.GetComponent<PhotonView>().Owner.ActorNumber == (int)info[0])
                            {
                                playerCtrl.GetGunByID((int)info[1]);
                                break;
                            }
                        }
                        break;

                    }

            }
        }


    }




}