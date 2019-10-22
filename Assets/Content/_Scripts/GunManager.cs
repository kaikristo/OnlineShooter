
using UnityEngine;

namespace KaiKristo.Shooter
{
    public class GunManager : MonoBehaviour
    {
        [SerializeField]
        Gun[] Armory;

        public Gun GiveMeGun(out int id)
        {
            id = Random.Range(0, Armory.Length);
            return Armory[id];
        }
        public Gun GiveMeGun(int id)
        {
            if (id > Armory.Length - 1)
            {
                Debug.Log("index out of bounds");
                return null;
            }

            return Armory[id];
        }
    }
}