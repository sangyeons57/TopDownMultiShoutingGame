using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;

public class BulletRed : Bullet
{
    private Vector2 detectedPos { get; set; }
    private GameObject target { get; set; }
    private Dictionary<GameObject, float> detect;


    private float detectDistance = 6f;
    private float rotateSpeed = 250f;

    private Rigidbody2D rb;

    private string playerNickname;

    // Start is called before the first frame update
    void Start()
    {
        detect = new Dictionary<GameObject, float>();
        rb = transform.GetComponent<Rigidbody2D>();

        Destroy(gameObject, 5f);

        base.damage= 15;
        base.speed= 5;

        transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90, Vector3.forward);

        if (pv.IsMine) pv.RPC("setPlayerNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName);

        InvokeRepeating("setDetect", 1.2f, 0.7f);
    }

    // Update is called once per frame
    void Update()
    {
        if ( target != null)
        {
            Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
            direction.Normalize();

            float rotateAmount = Vector3.Cross(direction, transform.up).z;
            rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        rb.velocity = transform.up * speed;
    }

    [PunRPC]
    private void setPlayerNickname(string nickname) => playerNickname = nickname;

    private void setDetect()
    {
        detect.Clear();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            if (obj.transform.name != playerNickname) detect.Add(obj, Vector2.Distance(transform.position, obj.transform.position));

        if (detect.Count == 0) return ;

        detect = (from pair in detect
                 orderby pair.Key descending
                 select pair)
                 .ToDictionary(pair => pair.Key, pair => pair.Value);

        setTarget(detectDistance);
    }


    private void setTarget(float distance)
    {
        foreach(var pair in detect)
        {
            if (pair.Value < distance)
            {
                target= pair.Key; detectedPos = target.transform.position;
                return;
            }
        }
        target= null;
    }


}
