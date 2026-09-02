using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HitSoundPlayer : MonoBehaviour
{
    [Tooltip("Drag in any object whose script implements IReadable.")]
    [SerializeField] private MonoBehaviour targetObject;
    [SerializeField] private AudioClip hitSound;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private IReadable readable;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        readable = targetObject as IReadable;

        if (readable == null)
            Debug.LogWarning($"{name}: Target Object doesn't implement IReadable.");
    }

    private void OnEnable()
    {
        if (readable != null)
            readable.OnDataChanged += PlayHitSound;
    }

    private void OnDisable()
    {
        if (readable != null)
            readable.OnDataChanged -= PlayHitSound;
    }

    private void PlayHitSound()
    {
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound, volume);
    }
}