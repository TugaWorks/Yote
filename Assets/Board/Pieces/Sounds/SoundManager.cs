using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource soundPlacePiece;
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
    }

    // Start is called before the first frame update
    void Start()
    {
        soundPlacePiece = Instantiate(soundPlacePiece);
    }

    public void PlaySoundPiece()
    {
        soundPlacePiece.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
