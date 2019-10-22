using UnityEngine;
namespace KaiKristo.Shooter
{

    public class HitCloud : MonoBehaviour
    {
        [SerializeField]
        AudioClip[] hitObstacleSounds;

        private AudioSource audioSource;
        private ParticleSystem particleSystem;
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            particleSystem = GetComponent<ParticleSystem>();
        }
        private void Start()
        {
            audioSource.PlayOneShot(GetRandomAudioClip(hitObstacleSounds));
        }

        private void Update()
        {
            if (!audioSource.isPlaying && !particleSystem.isPlaying)
                Destroy(this.gameObject);
        }
        private AudioClip GetRandomAudioClip(AudioClip[] audioClips)
        {
            return audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
        }
    }
}
