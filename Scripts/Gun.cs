using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Raycast Data")]
    public Transform emitter;
    public Camera p_Camera;

    //VFX
    public ParticleSystem bullet_Particles;

    private AudioManager audioManager;
    private PlayerSystems playerSystems;

    private void Start()
    {
        playerSystems = FindObjectOfType<PlayerSystems>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int layerMask = 1 << 8;
            layerMask = ~layerMask;

            RaycastHit hit;
            if (Physics.Raycast(emitter.position, p_Camera.transform.forward, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Collide))
            {
                if (hit.transform)
                    if (hit.transform.root.CompareTag("Zombie"))
                    {
                        // reward points
                        playerSystems.AddMoney(15);

                        // Deal dmg to the zombie
                        hit.transform.root.GetComponent<ZombieAI>().DamageZombie(35);
                    }
            }

            PlayShootingParticles(hit);
            audioManager.PlayOneShot("KongsbergShot");
            Debug.DrawRay(emitter.position, p_Camera.transform.forward * hit.distance, Color.yellow, 3);
        }
    }

    void PlayShootingParticles(RaycastHit bulletImpact)
    {
        bullet_Particles.transform.LookAt(bulletImpact.point);
        bullet_Particles.Play();
    }
}
