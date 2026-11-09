using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip clickSound;
    private AudioClip grabSound;
    private AudioClip successSound;
    private AudioClip musicLoop;

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
        musicSource.volume = 0.4f; // Увеличили громкость музыки (было 0.15)
        musicSource.playOnAwake = false;

        // Генерируем звуки
        clickSound = GenerateTone(880f, 0.1f, 0.5f);
        grabSound = GenerateChord(new float[] { 440f, 554.37f, 659.25f }, 0.2f, 0.4f);
        successSound = GenerateSweep(600f, 1200f, 0.4f);
        musicLoop = GenerateAmbientMusic(8.0f); // Увеличили длительность до 8 сек

        // Запускаем фоновую музыку
        musicSource.clip = musicLoop;
        musicSource.Play();
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

    private AudioClip GenerateAmbientMusic(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Аккорды: Am -> F -> C -> G (уютная последовательность)
        float[][] chords = new float[][] {
            new float[] { 220f, 261.63f, 329.63f },
            new float[] { 174.61f, 220f, 261.63f },
            new float[] { 261.63f, 329.63f, 392f },
            new float[] { 196f, 246.94f, 293.66f }
        };

        int samplesPerChord = sampleCount / 4;

        for (int c = 0; c < 4; c++)
        {
            for (int i = 0; i < samplesPerChord; i++)
            {
                int sampleIndex = c * samplesPerChord + i;
                if (sampleIndex >= sampleCount) break;

                float t = (float)i / sampleRate;

                // Плавное нарастание и спад для каждого аккорда (без резких затуханий)
                float envelope = Mathf.Sin(Mathf.PI * (float)i / samplesPerChord);
                float sum = 0;

                foreach (var freq in chords[c])
                {
                    // Складываем чистые синусоиды
                    sum += Mathf.Sin(2 * Mathf.PI * freq * t);
                }

                // Добавляем мягкую вибрацию (LFO) для эффекта "космоса"
                float lfo = 1.0f + 0.1f * Mathf.Sin(2 * Mathf.PI * 0.5f * t);

                samples[sampleIndex] = (sum / 3f) * envelope * 0.3f * lfo;
            }
        }

        // Плавный переход в конце, чтобы не было щелчка при зацикливании
        int fadeSamples = sampleRate / 2;
        for (int i = 0; i < fadeSamples; i++)
        {
            samples[sampleCount - 1 - i] *= (float)i / fadeSamples;
        }

        AudioClip clip = AudioClip.Create("AmbientMusic", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
