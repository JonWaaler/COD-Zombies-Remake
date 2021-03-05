using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public List<GameObject> MeleeMeshes;
    private AudioManager audioManager;
    private PlayerSystems playerSystems;
    public float MeleeDamage = 100;

    private void Start()
    {
        playerSystems = FindObjectOfType<PlayerSystems>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    // AttackHitCheck called when the player has melee'd
    // Handles deal dmg to zombie
    public void AttackHitCheck()
    {
        RaycastHit hit = Create_Raycast(transform.parent.position, transform.parent.forward, 1.5f);
        if (hit.transform)
        {
            if (hit.transform.CompareTag("Zombie"))
            {
                print("Player knifed Zombie");
                hit.transform.root.GetComponent<ZombieAI>().DamageZombie(200f);
                audioManager.Play("KnifeHitZombie");
                playerSystems.AddMoney(100);
            }
            else
            {
                audioManager.Play("KnifeHitMiss");
            }
        }
        else
        {
            audioManager.Play("KnifeHitMiss");

        }
    }

    // Function puts away the knife and takes out the previously active gun
    public void MeleeEnd()
    {
        transform.parent.parent.GetComponent<PlayerSystems>().EndKnifeAttack();
    }

    // Raycast for shooting
    // Returns raycast data
    public RaycastHit Create_Raycast(Vector3 startPos, Vector3 direction, float distance)
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(startPos, direction, out hit, distance, layerMask, QueryTriggerInteraction.Collide))
        {
            Debug.DrawRay(startPos, direction * hit.distance, Color.red);
        }

        return hit;
    }
}
