using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletGreen : Bullet
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 2.0f);

        base.damage = 10;
        base.speed = 17;

        InvokeRepeating("damageUpdate",0f, 0.1f);
    }
        

    // Update is called once per frame
    void Update()
    {
        transform.Translate(base.dir * Time.deltaTime * base.speed);
    }

    void damageUpdate()
    {
        base.damage += 15;
    }
}
