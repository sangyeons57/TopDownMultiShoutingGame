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

    private int roomcount;

    private string playerColor;

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
        //1.사용 불가능한 닉네임을 사용한다
        if (!isableUseThisNickName())
        {
            PhotonNetwork.Disconnect();
            setStausText("NICKNAME is already used \n or NICKNAME is an imposible");
            return;
        }
        //2. 플레이어가 캐릭터을  안하거나
        else if (DisconnectPanel.transform.Find("PlayerSelecter").GetComponent<Playerselecter>()
            .selectedColor == "")
        {
            PhotonNetwork.Disconnect();
            setStausText("select COLOR");
            return;
        }
        // --중 하나라도 안하면 다시하게 한다
        //정상 접속시 텍스트 삭제
        else setStausText("");

        //플레이어 color 세팅
        playerColor = DisconnectPanel.transform.Find("PlayerSelecter").GetComponent<Playerselecter>().selectedColor;

        //페널세팅
        DisconnectPanel.SetActive(false);
        GamePlayPanel.SetActive(true);
        Spawn();
    }

    //사용가능한 닉네임인가
    private bool isableUseThisNickName()
    {
        //닉네임이 없는경우
        if (PhotonNetwork.NickName == "") return false;

        int counter = 0;
        foreach(var player in PhotonNetwork.PlayerList)
        {
            if( player.NickName == PhotonNetwork.NickName ) counter++;
        }
        //자신외에 같은 닉네임을 가진 플레이어가 있는 경우
        return (counter == 1 ? true : false);
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
        PhotonNetwork.Instantiate(playerColor, Vector3.zero, Quaternion.identity);
        RespawnPanel.SetActive(false);
    }

    private void setStausText(string text)
    {
        DisconnectPanel.transform.Find("status").GetComponent<TMP_Text>()
            .text = text; 

    }


    [ContextMenu("정보")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("현재 방 이름: " + PhotonNetwork.CurrentRoom.Name);
            Debug.Log("현재 방 인원수: " + PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.Log("현재 방 최대인원수: " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "방에 있는 플레이어 목록: ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            Debug.Log(playerStr);
        }
        else
        {
            Debug.Log("접속한 인원수: " + PhotonNetwork.CountOfPlayers);
            Debug.Log("방 개수: " + PhotonNetwork.CountOfRooms);
            Debug.Log("모든 방에 있는 인원 수: " + PhotonNetwork.CountOfPlayersInRooms);
            Debug.Log("로비에 있는지: " + PhotonNetwork.InLobby);
            Debug.Log("연결됐는지: " + PhotonNetwork.IsConnected);
        }
    }

}
