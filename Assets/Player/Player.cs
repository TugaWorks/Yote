using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Photon.Realtime.Player photonPlayer;
    public List<GameObject> PlayerPieces = new List<GameObject>();

    public List<GameObject> PlayerPiecesInside = new List<GameObject>();
    public List<GameObject> PlayerPiecesOutside = new List<GameObject>();

    public bool isportugal = false;
    public Material playerMaterial;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Initialize(Photon.Realtime.Player player)
    {
        photonPlayer = player;
    }
    // Update is called once per frame
    void Update()
    {
       
    }
}
