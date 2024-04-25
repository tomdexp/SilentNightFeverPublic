using _Project.Scripts.Runtime.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK;

public class WwEmiter3DNoPanning : MonoBehaviour
{
    public string EventName = "Default";
    public string StopEvent = "Default";
    private bool isInCollider = false;
    private int currentNumberPlayer = 0;

    [SerializeField] AkGameObj akGameObj;


    List<GameObject> players = new List<GameObject>();

    public GameObject Player1;
    public GameObject Player2;
    public GameObject Player3;
    public GameObject Player4;

 

    // Start is called before the first frame update
    void Start()
    {
        players.Add(Player1);
        players.Add(Player2);
       players.Add(Player3);
       players.Add(Player4);

        AkSoundEngine.RegisterGameObj(gameObject); //maintenant ce gameobject est reconnu dans Wwise
        
    }

    // Update is called once per frame
    void Update()
    {


       
        
            GameObject closestPlayer = null;
            float closestDistance = float.MaxValue;
    
            foreach (var player in players)
            {
                float distance = Vector3.SqrMagnitude(transform.position - player.transform.position);
                if (distance < closestDistance)
                {
                    closestPlayer = player;
                    closestDistance = distance;
                }
            }
            //En gros on active le plus proche et on desactive tout les autres, mais faudrait voir a pas le faire toutes les frames (et a pas utiliser getcomponentinchildren)
            //tom pardonne moi je t'en supplie
            foreach (var player in players)
            {
                if (player != closestPlayer)
                {
    
                    player.GetComponentInChildren<AkAudioListener>().StopListeningToEmitter(akGameObj);
    
                }
            }
    
            closestPlayer.GetComponentInChildren<AkAudioListener>().StartListeningToEmitter(akGameObj);
   

        if (!PlayerManager.HasInstance) return;
        
        if (!Player1)
        {
            var player = PlayerManager.Instance.GetNetworkPlayer(_Project.Scripts.Runtime.Player.PlayerIndexType.A);
            if (player)
            {
                Player1 = player.gameObject;
            }
        }
        if (!Player2)
        {
            var player = PlayerManager.Instance.GetNetworkPlayer(_Project.Scripts.Runtime.Player.PlayerIndexType.B);
            if (player)
            {
                Player2 = player.gameObject;
            }
        }
        if (!Player3)
        {
            var player = PlayerManager.Instance.GetNetworkPlayer(_Project.Scripts.Runtime.Player.PlayerIndexType.C);
            if (player)
            {
                Player3 = player.gameObject;
            }
        }
        if (!Player4)
        {
            var player = PlayerManager.Instance.GetNetworkPlayer(_Project.Scripts.Runtime.Player.PlayerIndexType.D);
            if (player)
            {
                Player4 = player.gameObject;
            }
        }

        if (!Player1) return;

    }

    private void OnTriggerEnter(Collider other)
    {
        
        currentNumberPlayer = currentNumberPlayer + 1;
        

        if (other.tag != "Player" || isInCollider)
        {
            return;
        }
        if (currentNumberPlayer != 0) {
            isInCollider = true;
            AkSoundEngine.PostEvent(EventName, gameObject);
            Debug.Log("sound is playing");
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        currentNumberPlayer = currentNumberPlayer - 1;
        Debug.Log(currentNumberPlayer);

        if (other.tag != "Player" || !isInCollider || currentNumberPlayer != 0)
        {
            return;
        }
        if(currentNumberPlayer == 0)
        {
            isInCollider = false;
            AkSoundEngine.PostEvent(StopEvent, gameObject);
        }
       
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player" || isInCollider)
        {
            return;
        }

        isInCollider = true;
        AkSoundEngine.PostEvent(EventName, gameObject);
        
    }
}
