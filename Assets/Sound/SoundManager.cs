using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource soundPlacePiece;
    [SerializeField] private AudioSource soundSelect;
    [SerializeField] private AudioSource soundMusic1;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        soundPlacePiece = Instantiate(soundPlacePiece);
        soundSelect = Instantiate(soundSelect);
        soundMusic1 = Instantiate(soundMusic1);
        PlaySoundMusic1();
    }

    public void PlaySoundPiece()
    {
        soundPlacePiece.Play();
    }
    public void PlaySoundSelect()
    {
        soundSelect.Play();
    }
    public void PlaySoundMusic1()
    {
        soundMusic1.Play();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
