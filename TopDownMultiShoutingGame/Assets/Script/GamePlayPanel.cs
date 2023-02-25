using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class GamePlayPanel : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    [SerializeField]
    private TMP_Text[] scoreTextList;

    private void OnEnable()
    {
        InvokeRepeating("updateScore", 1.0f, 3.0f);
    }

    void updateScore()
    {
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");

        PlayerScript playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        scoreTextList[0].text = $"{1}. {playerScript.nicknameText.text} : {playerScript.playerPoint}";
        for (int i = 1; i <= enemys.Length; i++)
        {
            playerScript = enemys[i - 1].GetComponent<PlayerScript>();
            scoreTextList[i].text = $"{i+1}. {playerScript.nicknameText.text} : {playerScript.playerPoint}";
        }
    }

}
