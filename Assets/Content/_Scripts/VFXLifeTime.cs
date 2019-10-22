using UnityEngine;

public class VFXLifeTime : MonoBehaviour
{
    // Start is called before the first frame update
    ParticleSystem particleSystem;
    // Update is called once per frame
    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }
    void Update()
    {
        if (!particleSystem.isPlaying) Destroy(this.gameObject);
    }
}
