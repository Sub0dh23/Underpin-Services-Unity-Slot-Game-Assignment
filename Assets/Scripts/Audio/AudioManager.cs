using System.Collections.Generic;
using UnityEngine;

namespace Underpin.SlotGame.Audio
{
    /// <summary>
    /// Audio manager providing high-fidelity slot sound effects, background music, and procedural fallback synth.
    /// Works out-of-the-box even without external audio files.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Audio Clips (Optional - Uses Procedural Synth if Empty)")]
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip betChangeClip;
        [SerializeField] private AudioClip spinStartClip;
        [SerializeField] private AudioClip reelTickClip;
        [SerializeField] private AudioClip reelStopClip;
        [SerializeField] private AudioClip winSmallClip;
        [SerializeField] private AudioClip winBigClip;
        [SerializeField] private AudioClip freeSpinsClip;
        [SerializeField] private AudioClip coinsCollectClip;

        private readonly Dictionary<SoundType, AudioClip> _synthClips = new Dictionary<SoundType, AudioClip>();
        private bool _isMuted = false;

        public bool IsMuted => _isMuted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            GenerateProceduralAudioFallbacks();
        }

        public void PlaySound(SoundType type, float volume = 1f)
        {
            if (_isMuted || sfxSource == null) return;

            AudioClip clip = GetClip(type);
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void ToggleMute()
        {
            _isMuted = !_isMuted;
            if (sfxSource != null) sfxSource.mute = _isMuted;
            if (musicSource != null) musicSource.mute = _isMuted;
        }

        private AudioClip GetClip(SoundType type)
        {
            switch (type)
            {
                case SoundType.ButtonClick:
                    return buttonClickClip != null ? buttonClickClip : _synthClips.GetValueOrDefault(type);
                case SoundType.BetChange:
                    return betChangeClip != null ? betChangeClip : _synthClips.GetValueOrDefault(type);
                case SoundType.SpinStart:
                    return spinStartClip != null ? spinStartClip : _synthClips.GetValueOrDefault(type);
                case SoundType.ReelTick:
                    return reelTickClip != null ? reelTickClip : _synthClips.GetValueOrDefault(type);
                case SoundType.ReelStop:
                    return reelStopClip != null ? reelStopClip : _synthClips.GetValueOrDefault(type);
                case SoundType.WinSmall:
                    return winSmallClip != null ? winSmallClip : _synthClips.GetValueOrDefault(type);
                case SoundType.WinBig:
                    return winBigClip != null ? winBigClip : _synthClips.GetValueOrDefault(type);
                case SoundType.FreeSpinsTrigger:
                    return freeSpinsClip != null ? freeSpinsClip : _synthClips.GetValueOrDefault(type);
                case SoundType.CoinsCollect:
                    return coinsCollectClip != null ? coinsCollectClip : _synthClips.GetValueOrDefault(type);
                default:
                    return null;
            }
        }

        private void GenerateProceduralAudioFallbacks()
        {
            _synthClips[SoundType.ButtonClick] = CreateToneClip("Click", 880f, 0.04f, WaveType.Square);
            _synthClips[SoundType.BetChange] = CreateToneClip("BetChange", 523f, 0.06f, WaveType.Triangle);
            _synthClips[SoundType.SpinStart] = CreateSlideClip("SpinStart", 220f, 660f, 0.22f);
            _synthClips[SoundType.ReelTick] = CreateToneClip("Tick", 350f, 0.025f, WaveType.Noise);
            _synthClips[SoundType.ReelStop] = CreateSlideClip("ReelStop", 280f, 180f, 0.08f);
            _synthClips[SoundType.WinSmall] = CreateArpeggioClip("WinSmall", new float[] { 523f, 659f, 784f, 1046f }, 0.08f);
            _synthClips[SoundType.WinBig] = CreateArpeggioClip("WinBig", new float[] { 523f, 659f, 784f, 1046f, 1318f, 1568f }, 0.12f);
            _synthClips[SoundType.FreeSpinsTrigger] = CreateArpeggioClip("FreeSpins", new float[] { 440f, 554f, 659f, 880f, 1108f }, 0.15f);
            _synthClips[SoundType.CoinsCollect] = CreateToneClip("Coins", 1200f, 0.05f, WaveType.Sine);
        }

        private enum WaveType { Sine, Square, Triangle, Noise }

        private AudioClip CreateToneClip(string name, float frequency, float duration, WaveType wave)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / totalSamples); // Decay envelope
                float sample = 0f;

                switch (wave)
                {
                    case WaveType.Sine:
                        sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                        break;
                    case WaveType.Square:
                        sample = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frequency * t));
                        break;
                    case WaveType.Triangle:
                        sample = Mathf.PingPong(t * frequency * 4f, 2f) - 1f;
                        break;
                    case WaveType.Noise:
                        sample = (Random.value * 2f - 1f) * Mathf.Exp(-t * 80f);
                        break;
                }

                samples[i] = sample * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateSlideClip(string name, float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];
            float phase = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float progress = (float)i / totalSamples;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, progress);
                float envelope = 1f - (progress * 0.5f);

                phase += 2f * Mathf.PI * currentFreq / sampleRate;
                samples[i] = Mathf.Sin(phase) * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateArpeggioClip(string name, float[] notes, float noteDuration)
        {
            int sampleRate = 44100;
            int noteSamples = Mathf.FloorToInt(sampleRate * noteDuration);
            int totalSamples = noteSamples * notes.Length;
            float[] samples = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                float freq = notes[n];
                int startIdx = n * noteSamples;

                for (int i = 0; i < noteSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = 1f - ((float)i / noteSamples);
                    float sample = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);
                    samples[startIdx + i] = sample * envelope * 0.35f;
                }
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
