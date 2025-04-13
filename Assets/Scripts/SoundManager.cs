using UnityEngine;

// Ensures an AudioSource component is attached to the same GameObject
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip paddleBounceSound; // Assign paddle bounce SFX in Inspector
    [SerializeField] private AudioClip wallBounceSound;   // Assign wall bounce SFX in Inspector
    [SerializeField] private AudioClip scoreSound;        // Assign score point SFX in Inspector
    // Add more clips here if needed (e.g., background music)

    // --- Private Variables ---
    private AudioSource audioSource; // The component that plays the sounds

    // --- Singleton Pattern ---
    // Makes the SoundManager easily accessible from anywhere using SoundManager.Instance
    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Optional: Keep the SoundManager across scene changes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another instance already exists, destroy this one
            Destroy(gameObject);
            return; // Exit Awake early
        }

        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("SoundManager requires an AudioSource component.", this);
            enabled = false; // Disable script if no AudioSource
        }
        else
        {
            // Optional: Configure AudioSource defaults if needed
            // audioSource.playOnAwake = false;
        }
    }
    // --- End Singleton ---


    // --- Public Methods to Play Sounds ---

    // Plays the paddle bounce sound effect
    public void PlayPaddleBounceSound()
    {
        PlaySound(paddleBounceSound);
    }

    // Plays the wall bounce sound effect
    public void PlayWallBounceSound()
    {
        PlaySound(wallBounceSound);
    }

    // Plays the scoring sound effect
    public void PlayScoreSound()
    {
        PlaySound(scoreSound);
    }

    // --- Helper Method ---

    // Generic method to play a one-shot sound effect
    private void PlaySound(AudioClip clip)
    {
        // Check if the AudioSource and the clip are valid
        if (audioSource != null && clip != null)
        {
            // Play the clip once, overlapping previous sounds if necessary
            audioSource.PlayOneShot(clip);
        }
        else
        {
            if (clip == null) Debug.LogWarning("SoundManager tried to play a null AudioClip.");
        }
    }

    // Example for background music (requires a separate AudioSource or different handling)
    /*
    [SerializeField] private AudioSource musicSource; // Assign a second AudioSource for music
    [SerializeField] private AudioClip backgroundMusic;

    void Start() {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic() {
        if (musicSource != null && backgroundMusic != null) {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    */
}
