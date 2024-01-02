using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player01Trigger : NetworkBehaviour
{
    public BoxCollider2D Col;

    //[SerializeField] public float Damage = 0.01f;
    [SerializeField] public int Damage = 1;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (Player01Move.Hits == false)
        {
            Col.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player1"))
        {
            if (collision.attachedRigidbody == null) return;

            if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
            {
                Player01Move.Hits = true;
                Debug.Log(Player01Move.Hits);
                //health.TakeDamage(Damage);
            }
            
        }
        
    }
}
