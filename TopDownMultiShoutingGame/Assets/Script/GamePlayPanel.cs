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
        //SYN_All을 해서 기존 플레이어에 점수를 전부 싱크시킨다.
        pv.RPC("SYNC_All", RpcTarget.All);
        //플레이어에게 score의 업데이트 시간을3.0f초로 한다.
        InvokeRepeating("updateScore", 1.0f, 3.0f);
    }

    //플레이어에게 보여주는 score창 업데이트
    void updateScore()
    {
        //score구 데이터 삭제
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Score")) Destroy(obj);

        //플레이어수가 server에 수와 현제 표현하고 있는 수가 같은지 확인하고
        //아닐경우 수정함
        checkPlayerNumber();
        //사전 정렬
        var sortingDict = from pair in playerPointNote
                    orderby pair.Value descending
                    select pair;

        //정렬된거가지고 표시
        int counter = 0;
        foreach (KeyValuePair<string,int> pair in sortingDict)
        {
            GameObject obj = Instantiate(scoreInstance, gameObject.transform);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-75, -25 + (-20) * counter);
            obj.GetComponent<TMP_Text>().text = $"{(int)counter + 1}. {pair.Key} {pair.Value}";

            counter++;
        }
    }

    //제거밑 discconet될때 전체 제거에 사용
    [PunRPC]
    public void removePointDictEntry(string nickName)
    {
        Debug.Log("removePointDIctEntry");
        playerPointNote.Remove(nickName);
    }

    public void setPoint(string nickName, int point)
    {
        //플레이가 존제하는지 확인 없는겨우 playerPoinNote에 추가
        if (!playerPointNote.ContainsKey(nickName)) playerPointNote.Add(nickName, 0);
        playerPointNote[nickName] = point;
    }
    
    public int getPoint (string nickName)
    {
        //플레이가 존제하는지 확인 없는겨우 playerPoinNote에 추가
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
                //확인되 플레이어는list에서 하나씩 빠짐
                checkingList.Remove(player.NickName);
                //있는데 없는경우 추가로 보정
                if (!playerPointNote.ContainsKey(player.NickName)) setPoint(player.NickName, 0);
            }
            //없어진사람 삭제
            foreach (string checkingPerson in checkingList)
            {
                //없는데 있는경우 제거로 보정
                removePointDictEntry(checkingPerson);
            }
        }
    }

    //포인트 추가하면 싱크
    public void addPoint(string nickName, int point) => pv.RPC("SYNC_PlayerInfo", RpcTarget.All, nickName, playerPointNote[nickName] += point);

    //플레이어 포인트 싱크
    [PunRPC]
    public void SYNC_PlayerInfo(string nickName, int point) => setPoint(nickName, point);

    //전부싱크할때
    [PunRPC]
    public void SYNC_All() => pv.RPC("SYNC_PlayerInfo", RpcTarget.All, PhotonNetwork.NickName, playerPointNote[PhotonNetwork.NickName]);
}
