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
        //���ö� ���� ��ȿ� �ִ� �÷��̾�� ������ ���ش�
        foreach (var player in PhotonNetwork.PlayerList)
        {
            setPoint(player.NickName, 0);
        }
        //SYN_All�� �ؼ� ���� �÷��̾ ������ ���� ��ũ��Ų��.
        pv.RPC("SYNC_All", RpcTarget.All);
        //�÷��̾�� score�� ������Ʈ �ð���3.0f�ʷ� �Ѵ�.
        InvokeRepeating("updateScore", 1.0f, 3.0f);
    }

    //�÷��̾�� �����ִ� scoreâ ������Ʈ
    void updateScore()
    {
        //score�� ������ ����
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Score")) Destroy(obj);

        //�÷��̾���� server�� ���� ���� ǥ���ϰ� �ִ� ���� ������ Ȯ���ϰ�
        //�ƴҰ�� ������
        checkPlayerNumber();
        //���� ����
        var sortingDict = from pair in playerPointNote
                    orderby pair.Value descending
                    select pair;

        //���ĵȰŰ����� ǥ��
        int counter = 0;
        foreach (KeyValuePair<string,int> pair in sortingDict)
        {
            GameObject obj = Instantiate(scoreInstance, gameObject.transform);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-75, -25 + (-20) * counter);
            obj.GetComponent<TMP_Text>().text = $"{(int)counter + 1}. {pair.Key} {pair.Value}";

            counter++;
        }
    }

    //���Ź� discconet�ɶ� ��ü ���ſ� ���
    [PunRPC]
    public void removePointDictEntry(string nickName)
    {
        Debug.Log("removePointDIctEntry");
        playerPointNote.Remove(nickName);
    }

    public void setPoint(string nickName, int point)
    {
        //�÷��̰� �����ϴ��� Ȯ�� ���°ܿ� playerPoinNote�� �߰�
        if (!playerPointNote.ContainsKey(nickName)) playerPointNote.Add(nickName, 0);
        playerPointNote[nickName] = point;
    }
    
    public int getPoint (string nickName)
    {
        //�÷��̰� �����ϴ��� Ȯ�� ���°ܿ� playerPoinNote�� �߰�
        if (!playerPointNote.ContainsKey(nickName)) playerPointNote.Add(nickName, 0);
        return playerPointNote[nickName];
    }

    public void checkPlayerNumber()
    {
        if (PhotonNetwork.PlayerList.Length != playerPointNote.Count)
        {
            List<string> checkingList = playerPointNote.Keys.ToList();
            //�߰��� ��� �����Ϳ� �߰�
            foreach (var player in PhotonNetwork.PlayerList)
            {
                //Ȯ�ε� �÷��̾��list���� �ϳ��� ����
                checkingList.Remove(player.NickName);
                //�ִµ� ���°�� �߰��� ����
                if (!playerPointNote.ContainsKey(player.NickName)) setPoint(player.NickName, 0);
            }
            //��������� ����
            foreach (string checkingPerson in checkingList)
            {
                //���µ� �ִ°�� ���ŷ� ����
                removePointDictEntry(checkingPerson);
            }
        }
    }

    //����Ʈ �߰��ϸ� ��ũ
    public void addPoint(string nickName, int point) => pv.RPC("SYNC_PlayerInfo", RpcTarget.All, nickName, playerPointNote[nickName] += point);

    //�÷��̾� ����Ʈ ��ũ
    [PunRPC]
    public void SYNC_PlayerInfo(string nickName, int point) => setPoint(nickName, point);

    //���ν�ũ�Ҷ�
    [PunRPC]
    public void SYNC_All() => pv.RPC("SYNC_PlayerInfo", RpcTarget.All, PhotonNetwork.NickName, playerPointNote[PhotonNetwork.NickName]);
}
