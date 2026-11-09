using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip clickSound;
    private AudioClip grabSound;
    private AudioClip successSound;
    private AudioClip backgroundMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.3f;
        musicSource.playOnAwake = false;

        clickSound = GenerateTone(880f, 0.1f, 0.5f);
        grabSound = GenerateChord(new float[] { 440f, 554.37f, 659.25f }, 0.2f, 0.4f);
        successSound = GenerateSweep(600f, 1200f, 0.4f);

        backgroundMusic = Resources.Load<AudioClip>("bathing_in_the_sung_electronic");

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
        }
        else
        {
            Debug.LogError("[SoundManager] Не удалось найти файл музыки в папке Resources!");
        }
    }

    public void StartMusic()
    {
        if (backgroundMusic != null)
        {
            if (musicSource.isPlaying) return;
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            Debug.Log("[SoundManager] Музыка запущена.");
        }
    }

    public void PlayClick() => sfxSource.PlayOneShot(clickSound);
    public void PlayGrab() => sfxSource.PlayOneShot(grabSound);
    public void PlaySuccess() => sfxSource.PlayOneShot(successSound);

    // --- ГЕНЕРАТОРЫ ЗВУКА ---
    private AudioClip GenerateTone(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);
            samples[i] = (Mathf.Sin(2 * Mathf.PI * frequency * t) * 0.5f +
                          Mathf.Sin(2 * Mathf.PI * frequency * 2 * t) * 0.2f) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateChord(float[] frequencies, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);
            float sum = 0;
            foreach (var freq in frequencies)
                sum += Mathf.Sin(2 * Mathf.PI * freq * t);

            samples[i] = (sum / frequencies.Length) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("Chord", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateSweep(float startFreq, float endFreq, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float currentPhase = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(startFreq, endFreq, t / duration);
            currentPhase += 2 * Mathf.PI * freq / sampleRate;

            float envelope = 1f - (t / duration);
            samples[i] = Mathf.Sin(currentPhase) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("Sweep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
