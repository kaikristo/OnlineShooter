
using UnityEngine;

namespace KaiKristo.Shooter
{
    public class FootSteps : MonoBehaviour
    {
        [SerializeField]
        AudioClip[] stepsSounds;

        private AudioSource audioSource;
        private bool isMine;

        private void Awake()
        {
            isMine = GetComponentInParent<PlayerCtrl>().IsMine;
            audioSource = GetComponent<AudioSource>();
        }

        private void Step()
        {
            if (!isMine) return;

            if (stepsSounds.Length > 0)
                audioSource.PlayOneShot(GetRandomClip());
        }

        private AudioClip GetRandomClip()
        {
            return stepsSounds[UnityEngine.Random.Range(0, stepsSounds.Length)];
        }
    }
}