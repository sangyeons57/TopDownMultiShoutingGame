using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayPanel : MonoBehaviourPunCallbacks
{

    public PhotonView pv;

    [SerializeField]
    private GameObject scoreInstance;

    private Dictionary<string, int> playerPointNote = new Dictionary<string, int>();

    private void OnEnable()
    {
        //들어올때 현제 방안에 있는 플레이어들 설정을 해준다
        foreach (var player in PhotonNetwork.PlayerList)
        {
            setPoint(player.NickName, 0);
        }
        pv.RPC("SYNC_All", RpcTarget.All);
        InvokeRepeating("updateScore", 1.0f, 3.0f);
    }

    void updateScore()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Score")) Destroy(obj);

        checkPlayerNumber();
        var sortingDict = from pair in playerPointNote
                    orderby pair.Value descending
                    select pair;
        int i = 0;
        foreach (KeyValuePair<string,int> pair in sortingDict)
        {
            GameObject obj = Instantiate(scoreInstance, gameObject.transform);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-75, -25 + (-20) * i);
            obj.GetComponent<TMP_Text>().text = $"{(int)i + 1}. {pair.Key} {pair.Value}";

            i++;
        }
    }

    [PunRPC]
    public void removePointDictEntry(string nickName)
    {
        Debug.Log("removePointDIctEntry");
        playerPointNote.Remove(nickName);
    }

    public void setPoint(string nickName, int point)
    {
        if (!playerPointNote.ContainsKey(nickName)) playerPointNote.Add(nickName, 0);
        playerPointNote[nickName] = point;
    }
    
    public int getPoint (string nickName)
    {
        if (!playerPointNote.ContainsKey(nickName)) playerPointNote.Add(nickName, 0);
        return playerPointNote[nickName];
    }

    public void checkPlayerNumber()
    {
        if (PhotonNetwork.PlayerList.Length != playerPointNote.Count)
        {
            List<string> checkingList = playerPointNote.Keys.ToList();
            //추가된 사람 데이터에 추가
            foreach (var player in PhotonNetwork.PlayerList)
            {
                checkingList.Remove(player.NickName);
                if (!playerPointNote.ContainsKey(player.NickName)) setPoint(player.NickName, 0);
            }
            //없어진사람 삭제
            foreach (string checkingPerson in checkingList)
            {
                removePointDictEntry(checkingPerson);
            }
        }
    }

    public void addPoint(string nickName, int point) => pv.RPC("SYNC_PlayerInfo", RpcTarget.All, nickName, playerPointNote[nickName] += point);

    [PunRPC]
    public void SYNC_PlayerInfo(string nickName, int point) 
    {
        setPoint(nickName, point);
    }

    [PunRPC]
    public void SYNC_All() => pv.RPC("SYNC_PlayerInfo", RpcTarget.All, PhotonNetwork.NickName, playerPointNote[PhotonNetwork.NickName]);
}
