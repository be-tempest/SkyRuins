using UnityEngine;

public enum SEType
{
    Footstep,
    Attack,
    Flame,
    Ice,
    Item,
    Slide,
    Select,
    Decide,
    Cancel
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip titleBgm;
    [SerializeField] private AudioClip gameBgm;
    [SerializeField] private AudioClip gameOver;

    [Header("SE Clips")]
    [SerializeField] private AudioClip footstep;
    [SerializeField] private AudioClip attack;
    [SerializeField] private AudioClip flame;
    [SerializeField] private AudioClip ice;
    [SerializeField] private AudioClip slide;
    [SerializeField] private AudioClip item;
    [SerializeField] private AudioClip select;
    [SerializeField] private AudioClip decide;
    [SerializeField] private AudioClip cancel;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayTitleBGM()
    {
        bgmSource.clip = titleBgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayGameBGM()
    {
        bgmSource.clip = gameBgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayGameOverBGM()
    {
        bgmSource.clip = gameOver;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySE(SEType type)
    {
        switch (type)
        {
            case SEType.Footstep:
                seSource.PlayOneShot(footstep);
                break;

            case SEType.Attack:
                seSource.PlayOneShot(attack);
                break;

            case SEType.Flame:
                seSource.PlayOneShot(flame);
                break;

            case SEType.Ice:
                seSource.PlayOneShot(ice);
                break;

            case SEType.Item:
                seSource.PlayOneShot(item);
                break;

            case SEType.Slide:
                seSource.PlayOneShot(slide);
                break;

            case SEType.Select:
                seSource.PlayOneShot(select);
                break;

            case SEType.Decide:
                seSource.PlayOneShot(decide);
                break;

            case SEType.Cancel:
                seSource.PlayOneShot(cancel);
                break;
        }
    }

    public void StopSE()
    {
        seSource.Stop();
    }   
}
