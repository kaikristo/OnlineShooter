using Photon.Pun;
using UnityEngine;

namespace KaiKristo.Shooter
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField]
        private int maxRange;
        [SerializeField]
        private int damage;
        [SerializeField]
        private float speed;

        [SerializeField]
        GameObject hitPlayerVFX;
        [SerializeField]
        GameObject hitObstacleVFX;


        private GameObject owner;
        private Vector3 startPoint;
        private float distanceTravelled;
        private bool isMasterClient;

        public GameObject Owner { get => owner; set => owner = value; }

        // Start is called before the first frame update

        void Start()
        {
            Rigidbody m_Rigidbody = GetComponent<Rigidbody>();
            startPoint = transform.position;
            isMasterClient = PhotonNetwork.IsMasterClient;
            //игнорируем воду и невидимые границы.
            Physics.IgnoreLayerCollision(9, 10);
            Physics.IgnoreLayerCollision(9, 9);
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (Vector3.Distance(startPoint, transform.position) > maxRange)
                Destroy(this.gameObject);

            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return;
            switch (other.gameObject.tag)
            {
                
                case "Player":
                    Instantiate(hitPlayerVFX, transform.position, transform.rotation);
                    Destroy(this.gameObject);
                    if (PhotonNetwork.IsMasterClient)
                        GameCtrl.instance.SendHitCommand(other.gameObject, owner, damage);

                    break;
                case "Obstacle":
                    Instantiate(hitObstacleVFX, transform.position, transform.rotation);
                    Destroy(this.gameObject);
                    break;
            }
        }


    }
}