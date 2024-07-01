using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject[]            Character;
    [SerializeField] GameObject[]            Enemies;
    [SerializeField] GameObject              Boss;

    private List<GameObject>                 Chickens;
    private List<GameObject>                 AngryPigs;
    [SerializeField] private int             maxChickeninRoom;
    [SerializeField] private int             maxAngryPigInRoom;

    private int                              indexSelectedCharacter;

    private bool                             isTimeToAttackBoss;

    private void Start()
    {
        Chickens = new List<GameObject>();
        AngryPigs = new List<GameObject>();

        if (PlayerPrefs.HasKey("SelectedCharacter")) indexSelectedCharacter = PlayerPrefs.GetInt("SelectedCharacter");
        else indexSelectedCharacter = 0;
        PhotonNetwork.JoinOrCreateRoom("Map", new RoomOptions() { MaxPlayers = 5 }, TypedLobby.Default);

        isTimeToAttackBoss = false;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(Character[indexSelectedCharacter].name, Vector2.zero, Quaternion.identity);
    }


    IEnumerator SpawnEnemies(List<GameObject> li_enemy,GameObject enemy,int maxEnemy ,float WaitSeconds)
    {
        while (true)
        {
            if (li_enemy.Count < maxEnemy)
            {
                for(int i = 0;i < maxEnemy; i++)
                {
                    GameObject enemy_clone = Instantiate(enemy);
                }
            }
            yield return new WaitForSeconds(WaitSeconds);
        }
    }

}
