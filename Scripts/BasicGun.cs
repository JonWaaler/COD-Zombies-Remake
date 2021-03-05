using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGun : MonoBehaviour
{
    public int GunId;
    public AudioManager audioManager;
    public Animator cameraAnimator;
    public Animator HandAnimator;
    private float startTimer = 0;
    private PlayerMovement playerMovement;

    [Header("Gun Specs")]
    public float Damage;
    public float RunningBulletSpread;
    public float WalkingBulletSpread;
    public float IdleBulletSpread;
    public float reloadTime = .5f; // How long it takes for the current ammo to be updated when reloading (try and match animations)

    [Header("Ammo")]
    public int CurAmmo;
    public int AmmoLeft;
    [SerializeField]
    private int ClipSize;

    [Header("Emitter")]
    public Transform emitter; //uses camera for easy accuracy
    public GameObject muzzleFlash;

    [HideInInspector]
    public bool isAiming;
    public Camera playerCamera;
    private PlayerSystems playerSystems;

    void Start()
    {
        // Set clip size for the gun to cur ammo
        ClipSize = CurAmmo;
        // Setters
        playerSystems = FindObjectOfType<PlayerSystems>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        HandAnimator = GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
        cameraAnimator = playerCamera.GetComponent<Animator>();
        Debug.ClearDeveloperConsole();
    }


    void FixedUpdate()
    {
        if (startTimer < 1.5f)
        {
            startTimer += Time.deltaTime;
            return;
        }
        muzzleFlash.SetActive(false);

        // Aim
        if (Input.GetMouseButton(1))
        {
            // Play aim animation
            HandAnimator.SetBool("IsAiming", true);
            cameraAnimator.SetBool("IsAiming", true);
            

            // Then shoot
            if (Input.GetMouseButtonDown(0))
            {
                if (CurAmmo > 0)
                    HandAnimator.SetBool("IsShooting", true);
                else
                {
                    NoAmmoSound();
                }

            }
        }
        else
        {
            HandAnimator.SetBool("IsShooting", false);
            HandAnimator.SetBool("IsAiming", false);
            cameraAnimator.SetBool("IsAiming", false);

        }

        // Shoot Without Aiming
        if (Input.GetMouseButton(0))
        {
            if(CurAmmo > 0)
                HandAnimator.SetBool("IsShooting", true);
            else
            {
                NoAmmoSound();
            }

        }
        else
        {
            HandAnimator.SetBool("IsShooting", false);
        }

        // If we arent reloading, allow reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            // If there is no ammo to reload with play no ammo sound
            // and exit
            if(AmmoLeft <= 0)
            {
                NoAmmoSound();
                return;
            }

            isReloading = true;
            HandAnimator.SetTrigger("Reload");
        }
    }

    //  When a weapon is out of ammo or out of ammo to reload the gun
    public void NoAmmoSound()
    {
        audioManager.Play("NoAmmo");
    }


    // Movement Sounds
    public void StepSound()
    {
        if(playerMovement.characterController.isGrounded)
            audioManager.PlayOneShot("Step");
    }

    // All gun sounds
    public void GetGun()
    {
        audioManager.PlayOneShot("GetGun");
    }
    
    //------------ GUN ANIMATIONS EVENTS ----------------//
    // Kongsberg Commands
    public void ShootKongsberg()
    {
        if (Input.GetMouseButton(1))
        {
            // if we are aiming down sights
            CheckRaycastHit(.001f); // Shooting the gun
        }
        else
        {
            // If we arent aiming down sight
            CheckRaycastHit(Mathf.Lerp(0.05f, 0.10f, playerMovement.ui_controller.Get_CrosshairSpreadPrecent())); // Shooting the gun
        }
        playerMovement.ui_controller.ModifyCrosshairSpread(.5f); // Crosshair visuals
        audioManager.PlayOneShot("KongsbergShot"); // Audio
    }
    public void ReloadKongsberg()
    {
        audioManager.PlayOneShot("KongsbergReload");

        StartCoroutine(DelayReload(reloadTime));
    }

    // MAC-10
    public void ShootMAC10()
    {
        if (Input.GetMouseButton(1))
        {
            // if we are aiming down sights increase accuracy
            CheckRaycastHit(.001f);

        }
        else
        {
            // If we arent aiming down sight
            CheckRaycastHit(Mathf.Lerp(0.05f, 0.18f, playerMovement.ui_controller.Get_CrosshairSpreadPrecent()));
        }
        playerMovement.ui_controller.ModifyCrosshairSpread(.5f);
        audioManager.PlayOneShot("KongsbergShot");
    }
    public void ReloadMAC10()
    {
        audioManager.PlayOneShot("KongsbergReload");

        StartCoroutine(DelayReload(reloadTime));
    }

    // Walther
    public void ShootWalther()
    {
        if (Input.GetMouseButton(1))
        {
            // if we are aiming down sights increase accuracy
            CheckRaycastHit(.001f);

        }
        else
        {
            // If we arent aiming down sight
            CheckRaycastHit(Mathf.Lerp(0.05f, 0.15f, playerMovement.ui_controller.Get_CrosshairSpreadPrecent()));
        }
        playerMovement.ui_controller.ModifyCrosshairSpread(.5f);
        audioManager.PlayOneShot("KongsbergShot");
    }
    public void ReloadWalther()
    {
        audioManager.PlayOneShot("KongsbergReload");

        StartCoroutine(DelayReload(reloadTime));
    }

    // Revolver
    public void ShootRevolver()
    {
        if (Input.GetMouseButton(1))
        {
            // if we are aiming down sights increase accuracy
            CheckRaycastHit(0f);

        }
        else
        {
            // If we arent aiming down sight
            CheckRaycastHit(Mathf.Lerp(0.1f, 0.25f, playerMovement.ui_controller.Get_CrosshairSpreadPrecent()));
        }
        playerMovement.ui_controller.ModifyCrosshairSpread(.75f);
        audioManager.PlayOneShot("KongsbergShot");
    }
    public void ReloadRevolver()
    {
        audioManager.PlayOneShot("KongsbergReload");

        StartCoroutine(DelayReload(reloadTime));
    }



    [SerializeField]
    bool isReloading = false;
    // We dont want to refresh the ammo right away incase they cancel the reload
    IEnumerator DelayReload(float delay)
    {
        //print("Reloading..");
        yield return new WaitForSecondsRealtime(delay);

        // Calculate how much ammo we want to add to curammo
        int ammoToRefill = ClipSize - CurAmmo;
        // Check to see if we have enough "ammo left" to do a full clipSize refill
        if (AmmoLeft - ammoToRefill < 0)// This means theres no enough ammo to do a full refill
        {
            // If no ammo left
            if (ammoToRefill <= 0)
            {
                // this discision should be made before getting into animation
                // Will end up playing a sound
            }
            else // We have 1 or more bullets to add to the clip
            {
                // Give gun the rest of the ammo
                CurAmmo += AmmoLeft;
                AmmoLeft = 0;
            }
        }
        else // This means we have enough to do a full refill
        {
            AmmoLeft -= ammoToRefill;
            CurAmmo += ammoToRefill;
        }

        isReloading = false;
    }

    // Check raycastHit
    public void CheckRaycastHit(float shotAccuracy)
    {
        if (CurAmmo <= 0)
            return;

        // Raycast | Shoot gun
        RaycastHit raycastHit;
        raycastHit = Create_Raycast(emitter.position, transform.parent.forward, new Vector3(Random.Range(-shotAccuracy, shotAccuracy), Random.Range(-shotAccuracy, shotAccuracy), 0));
        //Debug.DrawRay(raycastHit.point, -raycastHit.normal * raycastHit.distance, Color.yellow);
        // Check if hitobject is a zombie
        if (raycastHit.transform)
            if (raycastHit.transform.CompareTag("Zombie"))
            {
                print("Hit Zombie");
                // Reward player for better aim
                if (raycastHit.transform.GetComponent<DamageModifier>())
                {
                    switch (raycastHit.transform.GetComponent<DamageModifier>().areaModifier)
                    {
                        case DamageModifier.AreaModifier.Head: // Rewards more money and damage to zombie
                print("HEAD");
                            // reward points
                            playerSystems.AddMoney(50);

                            // Deal dmg to the zombie
                            raycastHit.transform.root.GetComponent<ZombieAI>().DamageZombie(Damage * 2f);
                            break;
                        case DamageModifier.AreaModifier.Body:
                            // reward points
                            playerSystems.AddMoney(15);

                            // Deal dmg to the zombie
                            raycastHit.transform.root.GetComponent<ZombieAI>().DamageZombie(Damage);
                            break;
                        case DamageModifier.AreaModifier.Leg: // Rewards least money and damage to zombies
                                                              // reward points
                            playerSystems.AddMoney(10);

                            // Deal dmg to the zombie
                            raycastHit.transform.root.GetComponent<ZombieAI>().DamageZombie(Damage * .65f);
                            break;
                        default:
                            break;
                    }

                }
                else
                    Debug.Log("Hit Zombie Col with no DmgModifier", raycastHit.transform);
            }
    }

    // Raycast for shooting
    // Returns raycast data
    // Change ammo counts & applys accuracy value to the raycast
    public RaycastHit Create_Raycast(Vector3 startPos, Vector3 direction, Vector3 Accuracy)
    {
        // Muzzle flash VFX when shooting
        muzzleFlash.SetActive(true);
        
        // Ammo change
        CurAmmo -= 1;

        // Raycast to every layer but layer 8
        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        RaycastHit hit;

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(startPos, direction + Accuracy, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Collide))
        {
            //if (hit.transform.GetComponent<BoxCollider>().isTrigger)
            //{
            //    print("istrigger = true: " + hit.transform.gameObject.name);
            //}
            //else
            //    print("istrigger = false: " + hit.transform.gameObject.name);

        }
        Debug.DrawRay(startPos, (direction + Accuracy) * hit.distance, Color.red);

        return hit;
    }

}
