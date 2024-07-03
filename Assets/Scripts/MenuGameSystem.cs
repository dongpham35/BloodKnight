using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class MenuGameSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] private Sprite[]       imgsCharacter;

    [SerializeField] private GameObject     panelLoading;
    [SerializeField] private GameObject     panelMenuGame;
    [SerializeField] private GameObject     panelSetting;
    [SerializeField] private Slider         Volume;
    [SerializeField] private Image          Character;
    [SerializeField] private TMP_Text       txtBlood;
    [SerializeField] private TMP_Text       txtDamage;
    [SerializeField] private TMP_Text       txtAmor;
    [SerializeField] private TMP_Text       txtSpeed;
    [SerializeField] private TMP_InputField ipfUsername;

    private int                              indexSelected;
    private string                           defaultName;

    float[] BloodChar = new float[2]{45.5f,50.5f};
    float[] DamageChar = new float[2]{5.5f,5.3f};
    float[] AmorChar = new float[2]{3.2f,3.5f};
    float[] SpeedChar = new float[2]{6.5f,7.5f};

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        panelLoading.SetActive(true);
        panelMenuGame.SetActive(true);
        panelSetting.SetActive(false);
        indexSelected = 0;

        Character.sprite = imgsCharacter[indexSelected];
        txtBlood.text = "Blood: " + BloodChar[indexSelected].ToString();
        txtDamage.text = "Damage: " + DamageChar[indexSelected].ToString();
        txtAmor.text = "Amor: " + AmorChar[indexSelected].ToString();
        txtSpeed.text = "Speed: " + SpeedChar[indexSelected].ToString();

        defaultName = "Dongpham";
    }

    public override void OnConnectedToMaster()
    {
        panelLoading.SetActive(false);
        if (PlayerPrefs.HasKey("Volume"))
        {
            Volume.value = PlayerPrefs.GetFloat("Volume");
            AudioListener.volume = Volume.value;
        }
        else
        {
            PlayerPrefs.SetFloat("Volume", 1f);
            Volume.value = 1f;
            AudioListener.volume = Volume.value;
        }
    }
    
    public void btnMusic()
    {
        if (AudioListener.volume > 0f) AudioListener.volume = 0f;
        else AudioListener.volume = Volume.value;
    }

    public void TurnOnSettingInMenuGame()
    {
        panelSetting.SetActive(true);
    }

    public void TurnOffSettingInMenuGame()
    {
        panelSetting.SetActive(false);
    }

    public void SetVolume()
    {
        AudioListener.volume = Volume.value;
        PlayerPrefs.SetFloat("Volume", Volume.value);
    }

    public void OnClickbtnNext()
    {
        indexSelected++;
        if (indexSelected >= imgsCharacter.Length) indexSelected = 0;
        Character.sprite = imgsCharacter[indexSelected];
        txtBlood.text = "Blood: " + BloodChar[indexSelected].ToString();
        txtDamage.text = "Damage: " + DamageChar[indexSelected].ToString();
        txtAmor.text = "Amor: " + AmorChar[indexSelected].ToString();
        txtSpeed.text = "Speed: " +  SpeedChar[indexSelected].ToString();
    }

    public void OnClockbtnBack()
    {
        indexSelected--;
        if(indexSelected < 0) indexSelected = imgsCharacter.Length - 1;
        Character.sprite = imgsCharacter[indexSelected];
        txtBlood.text = "Blood: " + BloodChar[indexSelected].ToString();
        txtDamage.text = "Damage: " + DamageChar[indexSelected].ToString();
        txtAmor.text = "Amor: " + AmorChar[indexSelected].ToString();
        txtSpeed.text = "Speed: " + SpeedChar[indexSelected].ToString();
    }

    public void OnClickbtnPlay()
    {
        PlayerPrefs.SetInt("SelectedCharacter", indexSelected);
        if (string.IsNullOrEmpty(ipfUsername.text)) PlayerPrefs.SetString("Username", defaultName);
        else PlayerPrefs.SetString("Username", ipfUsername.text);

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.LoadLevel("Map");
    }
}
