using UnityEngine;

namespace KaiKristo.Shooter
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Объект для отслеживания")]
        public Transform target;
        [Header("Плавность следования камеры")]
        public float smoothing = 5f;

        [Header("Отступ от объекта")]
        public Vector3 offset;

        private Camera cam;

        public void Start()
        {
            cam = Camera.main;
        }

        void FixedUpdate()
        {

            Vector3 targetCamPos = target.position + offset;
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetCamPos, smoothing * Time.deltaTime);

        }
    }
}
