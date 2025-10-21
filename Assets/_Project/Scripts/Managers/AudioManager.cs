using UnityEngine;

namespace PongQuest.Managers
{
    /// <summary>
    /// Handles all audio playback (music and SFX).
    /// Will be expanded in later phases.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.7f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            // Create AudioSources if they don't exist
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }

            Debug.Log("[AudioManager] Audio sources configured");
        }

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }

        /// <summary>
        /// Play background music
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;

            musicSource.clip = clip;
            musicSource.Play();
        }

        /// <summary>
        /// Stop current music
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}