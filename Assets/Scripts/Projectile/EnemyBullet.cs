using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyBullet : Bullet
{

    private void Start()
    {
        Destroy(gameObject.transform.parent.gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Health>() != null
            && collision.gameObject.layer == LayerMask.NameToLayer("Player") && !collision.gameObject.GetComponent<Player>().IsInvulnerable)
        {
            Health health = collision.gameObject.GetComponent<Health>();
            health.Damage(Damage);
            Debug.Log("Damage Dealt");
            Destroy(gameObject.transform.parent.gameObject);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject.transform.parent.gameObject);
        }
        
            
    }
}
