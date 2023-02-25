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
        DisconnectPanel.SetActive(false);
        roomcount = 0;
        Spawn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)&& PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate(characterName1.name, Vector3.zero, Quaternion.identity);
        RespawnPanel.SetActive(false);
        GamePlayPanel.SetActive(true);
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
