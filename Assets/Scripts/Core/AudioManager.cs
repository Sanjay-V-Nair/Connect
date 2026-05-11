using UnityEngine;

namespace Connect.Core {
    public class AudioManager : MonoBehaviour {
        
        public static AudioManager Instance;
        
        [SerializeField] private AudioSource gameAudioSource;
        [SerializeField] private AudioSource genericAudioSource;
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] private AudioClip levelMusic;
        [SerializeField] private AudioClip buttonClickMusic;
        [SerializeField] private AudioClip winMusic;
        [SerializeField] private AudioClip edgeProgressAudio;
        [SerializeField] private AudioClip connectAudio;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayGameAudio(bool isEnable) {
            gameAudioSource.Stop();
            gameAudioSource.clip = gameMusic;
            if (isEnable) {
                gameAudioSource.Play();
            } else {
                gameAudioSource.Stop();
            }
        }
        
        public void PlayLevelAudio(bool isEnable) {
            gameAudioSource.clip = levelMusic;
            if (isEnable) {
                gameAudioSource.Play();
            } else {
                gameAudioSource.Stop();
            }
        }

        public void PlayButtonClickAudio() {
            genericAudioSource.PlayOneShot(buttonClickMusic);
        }
        
        public void PlayWinAudio() {
            genericAudioSource.PlayOneShot(winMusic);
        }
        
        public void PlayEdgeProgressAudio() {
            genericAudioSource.PlayOneShot(edgeProgressAudio);
        }

        public void PlayConnectAudio() {
            genericAudioSource.PlayOneShot(connectAudio);
        }

    }
}