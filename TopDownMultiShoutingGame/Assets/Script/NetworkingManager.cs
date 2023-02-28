using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class NetworkingManager : MonoBehaviourPunCallbacks
{

    public TMP_InputField nickName;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public GameObject GamePlayPanel;

    public GameObject characterName1;

    private int roomcount;


    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        roomcount = 0;
    }

    public void JoinOrCreateRoom()
    {
        roomcount++;
        PhotonNetwork.JoinOrCreateRoom($"Room{roomcount}", new RoomOptions { MaxPlayers = 5 }, null);
    }

    // Update is called once per frame
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nickName.text;
        JoinOrCreateRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message) => JoinOrCreateRoom();

    public override void OnJoinedRoom()
    {
        roomcount = 0;
        //�г��� ��ġ�� ���
        if( !isableUseThisNickName() )
        {
            PhotonNetwork.Disconnect();

            DisconnectPanel.transform.Find("status").GetComponent<TMP_Text>()
                .text = "NICKNAME is already used";
            return;
        }
        //���� ���ӽ� �ؽ�Ʈ ����
        else DisconnectPanel.transform.Find("status").GetComponent<TMP_Text>().text = "";

        DisconnectPanel.SetActive(false);
        GamePlayPanel.SetActive(true);
        Spawn();
    }
    
    private bool isableUseThisNickName()
    {
        int counter = 0;
        foreach(var player in PhotonNetwork.PlayerList)
        {
            if( player.NickName == PhotonNetwork.NickName ) counter++;
        }
        return counter == 1 ? true : false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
        {
            GamePlayPanel.GetComponent<PhotonView>()
                .RPC("removePointDictEntry", RpcTarget.All, PhotonNetwork.NickName);
            Invoke("disconnect", 0.5f);
        }
    }
    private void disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
        GamePlayPanel.SetActive(false);
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate(characterName1.name, Vector3.zero, Quaternion.identity);
        RespawnPanel.SetActive(false);
    }


    [ContextMenu("����")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("���� �� �̸�: " + PhotonNetwork.CurrentRoom.Name);
            Debug.Log("���� �� �ο���: " + PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.Log("���� �� �ִ��ο���: " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "�濡 �ִ� �÷��̾� ���: ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            Debug.Log(playerStr);
        }
        else
        {
            Debug.Log("������ �ο���: " + PhotonNetwork.CountOfPlayers);
            Debug.Log("�� ����: " + PhotonNetwork.CountOfRooms);
            Debug.Log("��� �濡 �ִ� �ο� ��: " + PhotonNetwork.CountOfPlayersInRooms);
            Debug.Log("�κ� �ִ���: " + PhotonNetwork.InLobby);
            Debug.Log("����ƴ���: " + PhotonNetwork.IsConnected);
        }
    }

}
