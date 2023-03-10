using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMint : Bullet
{
    public GameObject explosionRadius;
    private int explosionDamage;
    // Start is called before the first frame update
    void Start()
    {
        explosionDamage= 90;

        base.damage = 10;
        base.speed= 6;
        
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(dir * base.speed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        Instantiate(explosionRadius, transform.position, Quaternion.identity).GetComponent<explosionCircle>()
            .setting(explosionDamage, base.player, base.pv);
    }
}
