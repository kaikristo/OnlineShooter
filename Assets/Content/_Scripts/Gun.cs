
using UnityEngine;

namespace KaiKristo.Shooter
{
    public class Gun : MonoBehaviour
    {

        [SerializeField]
        int id;
        [SerializeField]
        private GameObject GunSkin;

        [Header("Пуль за каждый выстрел")]
        [SerializeField]
        private int bulletsPerShoot;
        [Header("Выстрелов в минуту")]
        [SerializeField]
        private float fireRate;

        [Header("Разброс")]
        [SerializeField]
        private float recoil;

        [SerializeField]
        private GameObject bulletPrefab;

        [SerializeField]
        private AudioClip[] gunShotSounds;
        [SerializeField]
        private GameObject gunFlashVFX;

        private GameObject gunBarrelEnd;
        public GameObject BulletPrefab { get => bulletPrefab; }
        public GameObject GunBarrelEnd { get => gunBarrelEnd; }
        public int BulletCount { get => bulletsPerShoot; }
        public int Id { get => id; set => id = value; }

        private AudioSource audioSource;

        private float timeElapsed = 99;

        public void SetGun(int id, GameObject gunSkin, int bulletsPerShoot, float fireRate, float recoil, GameObject bulletPrefab, AudioClip[] gunShotSounds, GameObject gunFlashVFX, GameObject gunBarrelEnd, AudioSource audioSource, float timeElapsed)
        {
            this.id = id;
            GunSkin = gunSkin;
            this.bulletsPerShoot = bulletsPerShoot;
            this.fireRate = fireRate;
            this.recoil = recoil;
            this.bulletPrefab = bulletPrefab;
            this.gunShotSounds = gunShotSounds;
            this.gunFlashVFX = gunFlashVFX;
            this.gunBarrelEnd = gunBarrelEnd;
            this.audioSource = audioSource;
            this.timeElapsed = timeElapsed;
        }

        public void SetGun(Gun selectedGun)
        {
            this.id = selectedGun.Id;
            GunSkin = selectedGun.GunSkin;
            this.bulletsPerShoot = selectedGun.bulletsPerShoot;
            this.fireRate = selectedGun.fireRate;
            this.recoil = selectedGun.recoil;
            this.bulletPrefab = selectedGun.bulletPrefab;
            this.gunShotSounds = selectedGun.gunShotSounds;
            this.gunFlashVFX = selectedGun.gunFlashVFX;
            this.gunBarrelEnd = selectedGun.gunBarrelEnd;
            this.audioSource = selectedGun.audioSource;
            this.timeElapsed = selectedGun.timeElapsed;
        }
        private void Awake()
        {

        }


        public GameObject Init(Transform parent = null)
        {
            audioSource = GetComponent<AudioSource>();
            GameObject gun = Instantiate(GunSkin, parent);
            foreach (var i in gun.GetComponentsInChildren<Transform>())
                if (i.name == "GunBarrelEnd")
                {
                    gunBarrelEnd = i.gameObject;
                    break;
                }
            return gun;
        }

        public bool Shoot(int ActorNumber)
        {
            if (timeElapsed < (60 / fireRate))
                return false;
            for (int i = 0; i < BulletCount; i++)
            {
                Vector3 eulerRecoil = transform.eulerAngles + new Vector3(0f, Random.RandomRange(-recoil / 2, recoil / 2), 0f);
                Quaternion curRecoil = Quaternion.Euler(eulerRecoil);
                //Quaternion curRecoil = transform.rotation + Quaternion.Euler(0, 0, 0);
                Bullet curBullet = Instantiate(BulletPrefab, GunBarrelEnd.transform.position, curRecoil).GetComponent<Bullet>();
                curBullet.Owner = this.gameObject;
                GameCtrl.instance.SendShootCommand(ActorNumber, curBullet.transform.position, curBullet.transform.rotation);
            }

            if (gunShotSounds.Length > 0)
            {
                audioSource.PlayOneShot(GetRandomClip());
            }

            Instantiate(gunFlashVFX, gunBarrelEnd.transform.position, transform.rotation);
            timeElapsed = 0;
            return true;
        }
        public void ShootFromServer(Vector3 startPosition, Quaternion rotation)
        {
            Bullet curBullet = Instantiate(BulletPrefab, startPosition, rotation).GetComponent<Bullet>();
            curBullet.Owner = this.gameObject;
            Instantiate(gunFlashVFX, gunBarrelEnd.transform.position, transform.rotation);
            if (gunShotSounds.Length > 0)
            {
                audioSource.PlayOneShot(GetRandomClip());
            }

        }

        private void FixedUpdate()
        {
            if (timeElapsed < (60 / fireRate))
                timeElapsed += Time.fixedDeltaTime;
        }


        private AudioClip GetRandomClip()
        {
            return gunShotSounds[UnityEngine.Random.Range(0, gunShotSounds.Length)];
        }
    }
}
