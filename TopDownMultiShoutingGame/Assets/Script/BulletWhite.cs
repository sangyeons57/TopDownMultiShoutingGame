using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWhite : Bullet
{

    private void Awake()
    {
        base.damage = 10;
        base.speed= 7;
    }

    // Start is called before the first frame update
    void Start() => Destroy(gameObject, 3f);

    // Update is called once per frame
    void Update()
    {
        transform.Translate(base.dir * Time.deltaTime * base.speed);
    }
}
