using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.Audio
{
    public enum SFX
    {
        // Tower sounds
        LaserFire,
        PlasmaFire,
        RocketFire,
        RocketExplode,
        FreezeFire,
        ShockFire,
        SniperFire,
        FlameFire,
        TeslaFire,
        BuffActivate,

        // Enemy sounds
        EnemyHit,
        EnemyDie,
        EnemyBossRoar,
        EnemyReachEnd,

        // UI sounds
        UITap,
        UIConfirm,
        UICancel,
        TowerPlace,
        TowerUpgrade,
        TowerSell,
        WaveStart,
        VictoryFanfare,
        DefeatSound,
        CreditGain,
    }

    public enum MusicTrack
    {
        MainMenu,
        Battle_Low,
        Battle_Mid,
        Battle_High,
        Victory,
        Defeat,
    }

    [System.Serializable]
    public class SFXClip
    {
        public SFX id;
        public AudioClip[] clips; // Random variation support
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.9f, 1.1f)] public float pitchMin = 0.95f;
        [Range(0.9f, 1.1f)] public float pitchMax = 1.05f;
    }

    [System.Serializable]
    public class MusicClip
    {
        public MusicTrack id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.6f;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX")]
        [SerializeField] private SFXClip[] sfxClips;
        [SerializeField] private int sfxPoolSize = 12;

        [Header("Music")]
        [SerializeField] private MusicClip[] musicTracks;
        [SerializeField] private float musicFadeDuration = 1.5f;

        [Header("Volume (Runtime — overridden by SaveManager)")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;

        // Internal
        private AudioSource musicSource;
        private AudioSource musicSourceB;      // For crossfade
        private bool musicSourceAActive = true;
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private Dictionary<SFX, SFXClip> sfxMap = new Dictionary<SFX, SFXClip>();
        private Dictionary<MusicTrack, MusicClip> musicMap = new Dictionary<MusicTrack, MusicClip>();
        private MusicTrack currentTrack;
        private Coroutine fadeCoroutine;

        // Properties
        public float MasterVolume
        {
            get => masterVolume;
            set { masterVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }
        public float SFXVolume
        {
            get => sfxVolume;
            set { sfxVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }
        public float MusicVolume
        {
            get => musicVolume;
            set { musicVolume = Mathf.Clamp01(value); ApplyMusicVolume(); }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildMaps();
            CreateMusicSources();
            CreateSFXPool();
        }

        private void BuildMaps()
        {
            foreach (var s in sfxClips)
                if (!sfxMap.ContainsKey(s.id)) sfxMap[s.id] = s;

            foreach (var m in musicTracks)
                if (!musicMap.ContainsKey(m.id)) musicMap[m.id] = m;
        }

        private void CreateMusicSources()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume * masterVolume;

            musicSourceB = gameObject.AddComponent<AudioSource>();
            musicSourceB.loop = true;
            musicSourceB.playOnAwake = false;
            musicSourceB.volume = 0f;
        }

        private void CreateSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                sfxPool.Add(src);
            }
        }

        // ── SFX ─────────────────────────────────────────────────────────────

        public void PlaySFX(SFX id, float volumeScale = 1f)
        {
            if (!sfxMap.TryGetValue(id, out SFXClip sfxClip)) return;
            if (sfxClip.clips == null || sfxClip.clips.Length == 0) return;

            AudioSource src = GetFreeSFXSource();
            if (src == null) return;

            src.clip = sfxClip.clips[Random.Range(0, sfxClip.clips.Length)];
            src.volume = sfxClip.volume * volumeScale * sfxVolume * masterVolume;
            src.pitch = Random.Range(sfxClip.pitchMin, sfxClip.pitchMax);
            src.Play();
        }

        public void PlaySFXAt(SFX id, Vector3 worldPos, float volumeScale = 1f)
        {
            if (!sfxMap.TryGetValue(id, out SFXClip sfxClip)) return;
            if (sfxClip.clips == null || sfxClip.clips.Length == 0) return;

            AudioClip clip = sfxClip.clips[Random.Range(0, sfxClip.clips.Length)];
            float vol = sfxClip.volume * volumeScale * sfxVolume * masterVolume;
            AudioSource.PlayClipAtPoint(clip, worldPos, vol);
        }

        private AudioSource GetFreeSFXSource()
        {
            foreach (var src in sfxPool)
                if (!src.isPlaying) return src;

            // All busy — steal oldest (wrap around)
            return sfxPool[0];
        }

        // ── Music ────────────────────────────────────────────────────────────

        public void PlayMusic(MusicTrack track, bool crossfade = true)
        {
            if (currentTrack == track && GetActiveMusic().isPlaying) return;
            if (!musicMap.TryGetValue(track, out MusicClip mc)) return;

            currentTrack = track;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            if (crossfade && GetActiveMusic().isPlaying)
            {
                fadeCoroutine = StartCoroutine(CrossfadeMusic(mc));
            }
            else
            {
                GetActiveMusic().Stop();
                GetActiveMusic().clip = mc.clip;
                GetActiveMusic().volume = mc.volume * musicVolume * masterVolume;
                GetActiveMusic().Play();
            }
        }

        public void StopMusic(bool fade = true)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            if (fade)
                fadeCoroutine = StartCoroutine(FadeOutMusic());
            else
                GetActiveMusic().Stop();
        }

        public void SetMusicIntensity(float waveProgress)
        {
            // Switch between battle tracks based on progress (0–1)
            if (waveProgress < 0.33f)
                PlayMusic(MusicTrack.Battle_Low);
            else if (waveProgress < 0.66f)
                PlayMusic(MusicTrack.Battle_Mid);
            else
                PlayMusic(MusicTrack.Battle_High);
        }

        private IEnumerator CrossfadeMusic(MusicClip newTrack)
        {
            AudioSource outSrc = GetActiveMusic();
            AudioSource inSrc = GetInactiveMusic();

            inSrc.clip = newTrack.clip;
            inSrc.volume = 0f;
            inSrc.Play();

            float elapsed = 0f;
            float startVol = outSrc.volume;
            float targetVol = newTrack.volume * musicVolume * masterVolume;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / musicFadeDuration;
                outSrc.volume = Mathf.Lerp(startVol, 0f, t);
                inSrc.volume = Mathf.Lerp(0f, targetVol, t);
                yield return null;
            }

            outSrc.Stop();
            musicSourceAActive = !musicSourceAActive;
        }

        private IEnumerator FadeOutMusic()
        {
            AudioSource active = GetActiveMusic();
            float startVol = active.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                active.volume = Mathf.Lerp(startVol, 0f, elapsed / musicFadeDuration);
                yield return null;
            }

            active.Stop();
        }

        private AudioSource GetActiveMusic() => musicSourceAActive ? musicSource : musicSourceB;
        private AudioSource GetInactiveMusic() => musicSourceAActive ? musicSourceB : musicSource;

        // ── Volume ───────────────────────────────────────────────────────────

        private void ApplyVolumes()
        {
            ApplyMusicVolume();
        }

        private void ApplyMusicVolume()
        {
            if (musicMap.TryGetValue(currentTrack, out MusicClip mc))
            {
                GetActiveMusic().volume = mc.volume * musicVolume * masterVolume;
            }
        }
    }
}
