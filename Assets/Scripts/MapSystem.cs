using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSystem : MonoBehaviourPunCallbacks
{
    public static MapSystem instance;
    [SerializeField] GameObject[]            Character;
    [SerializeField] GameObject[]            Enemies;
    [SerializeField] GameObject              Boss;
    [SerializeField] GameObject[]            SpawnPoint;
    [SerializeField] GameObject[]            Fruit;
    [SerializeField] GameObject[]            SpawnPointFruit;

    private List<GameObject>                 Chickens;
    private List<GameObject>                 AngryPigs;
    private List<GameObject>                 Fruits;
    [SerializeField] private int             maxChickeninRoom;
    [SerializeField] private int             maxAngryPigInRoom;

    private int                              indexSelectedCharacter;

    private float                            TimeToArriedBoss = 60f;
    private bool                             isBossArried;

    private void Awake()
    {
        if(instance == null) instance = this;
    }
    private void Start()
    {
        Chickens = new List<GameObject>();
        AngryPigs = new List<GameObject>();
        Fruits = new List<GameObject>();

        if (PlayerPrefs.HasKey("SelectedCharacter")) indexSelectedCharacter = PlayerPrefs.GetInt("SelectedCharacter");
        else indexSelectedCharacter = 0;
        PhotonNetwork.JoinOrCreateRoom("Map", new RoomOptions() { MaxPlayers = 5 }, TypedLobby.Default);

        
    }

    private void Update()
    {
        TimeToArriedBoss -= Time.deltaTime;
        if(TimeToArriedBoss <= 0 && !isBossArried)
        {
            PhotonNetwork.Instantiate(Boss.name, SpawnPoint[2].transform.position, Quaternion.identity);
            TimeToArriedBoss = 75f;
            isBossArried = true;
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(Character[indexSelectedCharacter].name, Vector2.zero, Quaternion.identity);
        StartCoroutine(SpawnEnemies(Chickens, Enemies[0], SpawnPoint[0].transform.position, maxChickeninRoom, 9.9f));
        StartCoroutine(SpawnEnemies(AngryPigs, Enemies[1], SpawnPoint[1].transform.position, maxAngryPigInRoom, 17.9f));
        StartCoroutine(SpawnFruits(Fruits, Fruit, SpawnPointFruit, 20, 30f));
    }


    IEnumerator SpawnEnemies(List<GameObject> li_enemy,GameObject enemy, Vector3 pointspawn,int maxEnemy ,float WaitSeconds)
    {
        while (true)
        {
            for (int i = 0; i < maxEnemy; i++)
            {
                if(li_enemy.Count <= i)
                {
                    GameObject enemy_clone = PhotonNetwork.Instantiate(enemy.name, pointspawn, Quaternion.identity);
                    li_enemy.Add(enemy_clone);
                }
                if (li_enemy[i] == null) li_enemy[i] = PhotonNetwork.Instantiate(enemy.name, pointspawn, Quaternion.identity);
            }
            yield return new WaitForSeconds(WaitSeconds);
        }
    }

    IEnumerator SpawnFruits(List<GameObject> li_fruit, GameObject[] fruit, GameObject[] pointspawn, int maxFruits, float WaitSeconds)
    {
        while (true)
        {
            for (int i = 0; i < maxFruits; i++)
            {
                int indexPoint = Random.Range(0, pointspawn.Length-1);
                int indexFruit = Random.Range(0,fruit.Length -1);
                if (li_fruit.Count <= i)
                {
                    GameObject fruit_clone = PhotonNetwork.Instantiate(fruit[indexFruit].name, pointspawn[indexPoint].transform.position, Quaternion.identity);
                    li_fruit.Add(fruit_clone);
                }
                if (li_fruit[i] == null) li_fruit[i] = PhotonNetwork.Instantiate(fruit[indexFruit].name, pointspawn[indexPoint].transform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(WaitSeconds);
        }
    }

    public void BossIsDie()
    {
        isBossArried = false;
    }

    public void PlayerDied()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("MenuGame");
    }

}
