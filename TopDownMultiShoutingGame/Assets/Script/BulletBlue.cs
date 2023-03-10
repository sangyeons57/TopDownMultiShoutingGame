using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBlue : Bullet
{
    float[] shotgunDir = new float[]{-18, -15, -12, -9, -6, -3, 0, 3, 6, 9, 12, 15, 18};
    float angle;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 0.9f);
        base.damage = 8;
        base.speed = 7;

        angle = Mathf.Atan2(base.dir.y, base.dir.x) * Mathf.Rad2Deg + shotgunDir[base.bulletNum];
        angle *= Mathf.Deg2Rad;

        base.dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        pv.RPC("syncDir", RpcTarget.AllBuffered , base.dir);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(base.dir * base.speed * Time.deltaTime);
    }
}
