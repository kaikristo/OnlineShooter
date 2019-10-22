using Photon.Pun;
using UnityEngine;

namespace KaiKristo.Shooter
{
    public class PlayerCtrl : MonoBehaviourPun, IPunObservable
    {

        [SerializeField]
        private int hitPoints = 100;
        [SerializeField]
        private float speed = 10f;
        [SerializeField]
        private float rotationSpeed = 10f;
        [SerializeField]
        private float autoAimingRange = 5f;
        [SerializeField]
        private float respawnCooldown = 3f;
        [SerializeField]
        private GameObject GunSlot;


        public bool IsMine { get => isMine; }
        public int HitPoints { get => hitPoints; }
        public bool IsDead { get => isDead; }

        [Header("Визуализация")]
        [SerializeField]
        SkinnedMeshRenderer playerRenderer;
        [SerializeField]
        Animator animator;




        private bool isHiding = false; //мы прячемся в кустах
        private bool wasSpotted = false;//нас обнаружили  
        private bool isAiming = false;// мы целимся вторым стиком
        private bool isDead = false;
        private bool isMine = false;
        private bool isWalking = false;

        private float currentDeathTime = 0f;//таймер смерти  
        private float buttonDeltaTime;//нужна для управления мышью в едиторе
        private float delayAfterAim = 0.5f;
        private float timeAfterAim = 0f;

        private Joystick moveJoystick;
        private Joystick aimJoystick;



        public Gun gun;
        private SkinnedMeshRenderer gunRenderer;
        private CameraManager cameraManager;
        private PhotonView playerPV = null;
        private Rigidbody playerRB = null;



        #region string constants
        const string moveJoystickTag = "MoveJoystick";
        const string aimJoystickTag = "AimJoystick";
        const string playerMaterialDefault = "PlayerMaterialDefault";
        const string playerMaterialTransparent = "PlayerMaterialTransparent";
        const string otherMaterialDefault = "OtherMaterial";
        #endregion



        void Awake()
        {
            playerPV = GetComponent<PhotonView>();
            isMine = playerPV.IsMine;
        }
        private void Start()
        {
            timeAfterAim = delayAfterAim;
            playerRB = GetComponent<Rigidbody>();

            if (isMine)
            {
                AttachCameraManager();
                moveJoystick = GameObject.FindWithTag(moveJoystickTag).GetComponent<Joystick>();
                aimJoystick = GameObject.FindWithTag(aimJoystickTag).GetComponent<Joystick>();

            }
            else
            {
                //предотвращает рывки при синхронизации
                playerRB.constraints = RigidbodyConstraints.FreezeAll;
                //скин плеера
                playerRenderer.material = Resources.Load<Material>(@"Materials\" + otherMaterialDefault);
            }
            GetRandomGun();
            SetVisible();

        }

        public void GetRandomGun()
        {
            if (!isMine) return;
            if (gun != null)
            {

                ClearGunSlot();
            }
            int index = 0;
            gun = gameObject.AddComponent<Gun>();
            gun.SetGun(GameObject.FindObjectOfType<GunManager>().GiveMeGun(out index));
            gunRenderer = gun.Init(GunSlot.transform).GetComponentInChildren<SkinnedMeshRenderer>();
            if (isMine)
            {
                GameCtrl.instance.SendChangeWeaponCommand(this.gameObject, index);
            }
        }

        public void GetGunByID(int id)
        {
            if (gun != null)
            {
                ClearGunSlot();
            }
            gun = gameObject.AddComponent<Gun>();
            gun.SetGun(GameObject.FindObjectOfType<GunManager>().GiveMeGun(id));
            gunRenderer = gun.Init(GunSlot.transform).GetComponentInChildren<SkinnedMeshRenderer>();
        }

        private void ClearGunSlot()
        {
            foreach (var subObject in GunSlot.GetComponentsInChildren<Transform>())
            {
                if (subObject.gameObject != GunSlot)
                    Destroy(subObject.gameObject);
            }
            Destroy(gun);
        }

        void FixedUpdate()
        {
            if (IsDead)
                if (currentDeathTime > respawnCooldown)
                    Respawn();
                else
                {
                    currentDeathTime += Time.deltaTime;
                    return;
                }

            if (IsMine)
            {


#if UNITY_EDITOR || UNITY_STANDALONE

                if (!Input.GetMouseButton(0))
                {
                    if (timeAfterAim < delayAfterAim)
                        timeAfterAim += Time.fixedDeltaTime;
                }
                if (Input.GetMouseButton(0) && Input.mousePosition.x > Screen.width / 2)
                {
                    buttonDeltaTime += Time.deltaTime;
                    if (buttonDeltaTime > 0.2f)
                        isAiming = true;
                }
                Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                Move(input);
                if (isAiming)
                {
                    Aim();
                }
                if (Input.GetMouseButtonUp(0) && Input.mousePosition.x > Screen.width / 2)
                {
                    if (isAiming)
                    {
                        Shoot();
                    }
                    else
                    {
                        GameObject nearestEnemy = GetNearestPlayer(autoAimingRange);
                        if (nearestEnemy != null)
                            transform.LookAt(nearestEnemy.transform.position);
                        Shoot();
                        timeAfterAim = 0;
                    }
                    buttonDeltaTime = 0f;
                    isAiming = false;
                }






#elif UNITY_ANDROID

                Vector2 mobileInput = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
                if (mobileInput.magnitude == 0)
                {
                    if (timeAfterAim < delayAfterAim)
                        timeAfterAim += Time.fixedDeltaTime;
                }
                Move(mobileInput);
                if (Input.touchCount > 0)
                {
                    foreach (var touch in Input.touches)
                    {
                        if (touch.position.x > Screen.width / 2)
                        {
                            if (buttonDeltaTime > 0.2f) isAiming = true;
                            buttonDeltaTime += Time.fixedDeltaTime;
                            if (touch.phase == TouchPhase.Ended)
                            {
                                if (isAiming)
                                {
                                    Shoot();
                                }
                                else
                                {
                                    GameObject nearestEnemy = GetNearestPlayer(autoAimingRange);
                                    if (nearestEnemy != null)
                                    {
                                        transform.LookAt(nearestEnemy.transform.position);
                                        Shoot();
                                        timeAfterAim = 0;
                                    }
                                }
                                buttonDeltaTime = 0;
                                isAiming = false;
                            }


                        }
                    }


                    if (isAiming) Aim();
                }



#endif

            }

            animator.SetBool("IsWalking", isWalking);

            SetVisible();
        }




        private void AttachCameraManager()
        {
            cameraManager = gameObject.AddComponent<CameraManager>();
            cameraManager.offset = new Vector3(0, 7f, -7f);
            cameraManager.smoothing = 5f;
            cameraManager.target = transform;
        }







        private void SetVisible()
        {


            if (!isMine)
            {
                if (isHiding && !wasSpotted)
                {
                    HideOtherPlayer();
                }
                else
                {
                    ShowOtherPlayer();
                }
            }
            else
            {
                if (isHiding)
                {

                    HideMinePlayer();
                }
                else
                {
                    ShowMinePlayer();
                }
            }
        }

        private void ShowOtherPlayer()
        {
            foreach (var item in GetComponentsInChildren<Transform>())
            {
                if (item.tag == "Rendered")
                {
                    item.GetComponent<SkinnedMeshRenderer>().enabled = true;

                }
            }
        }

        private void HideOtherPlayer()
        {

            foreach (var item in GetComponentsInChildren<Transform>())
            {
                if (item.tag == "Rendered")
                {
                    item.GetComponent<SkinnedMeshRenderer>().enabled = false;


                }
            }
        }

        private void ShowMinePlayer()
        {
            playerRenderer.GetComponent<SkinnedMeshRenderer>().material = Resources.Load<Material>(@"Materials\" + playerMaterialDefault);
            gunRenderer.GetComponent<SkinnedMeshRenderer>().material = Resources.Load<Material>(@"Materials\" + gunRenderer.gameObject.name + "Default");


        }

        private void HideMinePlayer()
        {
            playerRenderer.GetComponent<SkinnedMeshRenderer>().material = Resources.Load<Material>(@"Materials\" + playerMaterialTransparent);
            gunRenderer.GetComponent<SkinnedMeshRenderer>().material = Resources.Load<Material>(@"Materials\" + gunRenderer.gameObject.name + "Transparent");
        }



        public void Shoot()
        {
            if (playerPV.IsMine)
            {
                gun.Shoot(playerPV.Owner.ActorNumber);
                //GameCtrl.instance.SendShootCommand(playerPV.Owner.ActorNumber);
            }


        }

        private void Aim()
        {
            Vector3 aim = new Vector3(aimJoystick.Horizontal, 0f, aimJoystick.Vertical);
            Vector3 target = (transform.position) + aim;
            if (aim.magnitude > 0)
            {

                SmoothlyLookAt(target);
            }
            timeAfterAim = 0;
        }

        private void Move(Vector2 inputVector)
        {



            Vector2 inputDirection = inputVector.normalized;


            if (inputVector != Vector2.zero)
                isWalking = true;
            else
                isWalking = false;
            animator.SetBool("IsWalking", isWalking);
            float pushForce = speed * inputDirection.magnitude * Time.deltaTime;
            Vector3 moveVector = new Vector3
                (
                inputDirection.x,
                0,
                inputDirection.y
                );



            playerRB.velocity = (moveVector * pushForce);

            Vector3 target;
            if (!isAiming)
            {
                if (timeAfterAim >= delayAfterAim)
                {
                    if (inputVector.magnitude > 0)
                    {
                        target = (transform.position) + new Vector3(inputVector.x, 0f, inputVector.y);
                        SmoothlyLookAt(target);
                    }
                }
            }



        }

        /// <summary>
        /// Плавно повернуться к цели
        /// </summary>
        /// <param name="target">позиция цели</param>
        private void SmoothlyLookAt(Vector3 target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        void Respawn()
        {
            GameCtrl.instance.RespawnAtRandomPosition(this);
            GetRandomGun();
            hitPoints = 100;
            FindObjectOfType<UIManager>().RefreshHP(HitPoints);
            isDead = false;
            currentDeathTime = 0;
            animator.Play("Idle");

        }
        public void Hit(int damage)
        {
            if (IsDead) return;

            if (isMine)
            {
                hitPoints -= damage;
                FindObjectOfType<UIManager>().RefreshHP(HitPoints);
                if (hitPoints <= 0)
                {
                    isDead = true;
                    Die();
                }

            }
        }

        private void Die()
        {
            animator.SetTrigger("Die");
        }

        private GameObject GetNearestPlayer(float autoAimingRange)
        {
            float nearestDistance = autoAimingRange + 1;
            GameObject result = null;
            foreach (var player in GameObject.FindObjectsOfType<PlayerCtrl>())
            {
                if (player == this || player.IsDead) continue;
                float curDistance = Vector3.Distance(transform.position, player.transform.position);
                if (curDistance < autoAimingRange && curDistance < nearestDistance)
                {
                    result = player.gameObject;
                    nearestDistance = curDistance;
                }
            }
            return result;
        }



        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Grass")
            {

                isHiding = true;

            }
            else
            if (other.tag == "Spotting")
            {
                wasSpotted = true;

            }

        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Grass")
            {
                isHiding = false;

            }
            else
            if (other.tag == "Spotting")
            {
                wasSpotted = false;

            }
        }




        //синхронизация
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

            if (stream.IsWriting)
            {
                object[] data = new object[] { isWalking, HitPoints, isDead, gun.Id };
                stream.SendNext(data);
            }
            else
              if (stream.IsReading)
            {
                object[] data = (object[])stream.ReceiveNext();

                isWalking = (bool)data[0];
                hitPoints = (int)data[1];
                bool deathState = (bool)data[2];
                int id = (int)data[3];
                if (gun == null)
                {
                    GetGunByID(id);
                    Debug.Log("getGun");
                }
                else
                if (id != gun.Id)
                {
                    Debug.Log("getGunbyID");
                    GetGunByID(id);
                }
                if (IsDead && !deathState)
                    animator.Play("Idle");
                else
                    if (deathState && !IsDead)
                {
                    isDead = deathState;
                    Die();
                }
            }
        }
    }

}