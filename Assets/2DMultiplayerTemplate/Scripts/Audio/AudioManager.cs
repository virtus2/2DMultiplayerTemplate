using UnityEngine;
using UnityEngine.Pool;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Component References")]
    [SerializeField] private AudioSource bgmPlayer;

    [Header("Background Musics")]
    [SerializeField] private float bgmVolume;

    [Header("Sound Effects")]
    [SerializeField] private float sfxVolume;
    [SerializeField] private bool sfxMute;
    [SerializeField] private int sfxPlayerdefaultCount = 10;
    [SerializeField] private int sfxPlayerMaxCount = 32;

    IObjectPool<AudioSource> sfxPlayerPool;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxPlayerPool = new ObjectPool<AudioSource>(
            createFunc: CreateSFXPlayerPooledItem,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPooledItem,
            defaultCapacity: sfxPlayerdefaultCount,
            maxSize: sfxPlayerMaxCount
        );
    }

    private AudioSource CreateSFXPlayerPooledItem()
    {
        GameObject go = new GameObject("Pooled Audio Source");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = sfxVolume;
        audioSource.mute = sfxMute;
        audioSource.clip = null;
        // TODO: adjust AudioSource values
        // audioSource.maxDistance 

        return audioSource;
    }

    private void OnGetFromPool(AudioSource source)
    {
        source.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(AudioSource source)
    {
        source.gameObject.SetActive(false);
    }

    private void OnDestroyPooledItem(AudioSource source)
    {
        Destroy(source.gameObject);
    }

    public void PlayBgm(AudioClip clip)
    {
        bgmPlayer.clip = clip;
        bgmPlayer.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        AudioSource audioSource = sfxPlayerPool.Get();
        audioSource.clip = clip;
        audioSource.volume = sfxVolume;
        audioSource.Play();
    }
}