using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public Vector2 dir { get; set; } 

    protected int damage { get; set; } = 10;
    protected int speed { get; set; } = 7;

    protected GameObject player;

    protected int bulletNum;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        if (!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<PlayerScript>().Hit(damage, player);
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }


    [PunRPC]
    public void settingRPC(string nickName, int bulletNum)
    {
        if (pv.IsMine) dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
        this.player = GameObject.Find(nickName);
        this.bulletNum= bulletNum;

        pv.RPC("syncDir", RpcTarget.All, dir);
    }

    [PunRPC]
    protected void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    protected void syncDir(Vector2 dir)
    {
        if (!pv.IsMine) this.dir = dir;
    }

}
