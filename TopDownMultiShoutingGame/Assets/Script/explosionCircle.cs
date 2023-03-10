using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class explosionCircle : MonoBehaviour
{
    private int damage;
    private GameObject player;
    private PhotonView pv;

    private void Start()
    {
        Destroy(gameObject, 0.2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<PlayerScript>().Hit(damage, player);
            Debug.Log("collision pv.isMine");
        }
        else if (pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            Debug.Log("collision pv.isMine");
        }
    }

    public void setting(int damgae, GameObject player, PhotonView pv)
    {
        this.damage= damgae;
        this.player= player;
        this.pv= pv;
    }
}
