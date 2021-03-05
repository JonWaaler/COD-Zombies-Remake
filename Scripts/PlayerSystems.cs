using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSystems : MonoBehaviour
{
    public GameObject Knife;
    public float health = 100f;
    [SerializeField]
    private int money = 5000;
    public int activeGunID;
    public float PlayerInvulnerability = 1.5f; // how much time the zombie has to wait before it can deal dmg to player
    public List<GameObject> GunInventory;
    public List<GameObject> AvailableGuns;

    private UI_Controller ui_controller;
    private AudioManager audioManager;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        ui_controller = FindObjectOfType<UI_Controller>();
        playerMovement = GetComponent<PlayerMovement>();
        // Add starter gun to inventory
        activeGunID = 0; // change active gun to starter gun id
        GunInventory.Add(AvailableGuns[0]);
        GunInventory[0].SetActive(true);
        t_Invulnerability = Time.time;

        InvokeRepeating("HealthRegeneration", 3f, 2f);
    }

    void HealthRegeneration()
    {
        if (health < 90)
            health += 10;
        else
            health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        // Handle player death and vignette (low health) vfx
        if(health <= 0)
        {
            StartCoroutine(GameOver());
        }
        else if (health < 30)
        {
            // Vignette Filter
            ui_controller.SetVignetterFilter(.5f);
        }
        else if(health < 60)
        {
            // Vignette Filter
            ui_controller.SetVignetterFilter(.4f);

        }
        else
        {
            // Health 60 and over
            ui_controller.SetVignetterFilter(0);

        }

        // Swapweapons
        if (Input.mouseScrollDelta.y != 0)
        {
            SwapWeapons();
        }

        // Melee attack
        if (Input.GetKeyDown(KeyCode.V))
        {
            // Make knife hands visible
            Knife.SetActive(true);

            // Make Active weapon invisible
            foreach (var gun in GunInventory)
            {
                // Active self ignores if its parent is making it inactive or not
                if (gun.activeSelf)
                {
                    gun.SetActive(false);
                }
            }
        }
        
        // Cheat codes :O
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddGun(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddGun(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddGun(3);
        }

        // Update PlayerSystems UI AMMO COunts
        ui_controller.UpdateAmmoUI(GetActiveGun().CurAmmo, GetActiveGun().AmmoLeft);        
    }

    private float t_Invulnerability = 0;
    public void HitPlayer(float dmg)
    {
        // If the last hit was less than a second from time.time, the player is invonerable
        if (Time.time - t_Invulnerability <= PlayerInvulnerability)
            return;

        // Save the time that we hit
        t_Invulnerability = Time.time;

        // Update player systems health
        health -= dmg;

        // Sound effect
        if (health <= 0)
            audioManager.Play("PlayerGrunt2");
        else
            audioManager.Play("PlayerGrunt1");

        // Update ui visuals health by matching playersystems health to ui_controller playerHealth
        ui_controller.UpdatePlayerHealth();
    }

    IEnumerator GameOver()
    {
        // Start Round end sound
        audioManager.Play("GameOverSound");
        ui_controller.EndGame();
        yield return new WaitForSeconds(5);

        SceneManager.LoadSceneAsync(0);
    }

    public int GetMoney()
    {
        return money;
    }

    // Adds money to the player wallet as well as calling
    // for collectMoneyAnimation()
    public void AddMoney(int value)
    {
        money += value;

        // Stack Overflow exception
        ui_controller.CollectMoneyAnimation(value);
    }

    // Used for any type of purchase, originally wall buys
    public void BuyWeapon(int value)
    {
        money -= value;

        audioManager.PlayOneShot("BuyGun");
    }

    public bool DoesPlayerHaveGun(int gunID)
    {
        for (int i = 0; i < GunInventory.Count; i++)
        {
            if (GunInventory[i].GetComponent<BasicGun>().GunId == gunID)
                return true;
        }

        return false;
    }

    public BasicGun GetActiveGun()
    {
        return AvailableGuns[activeGunID].GetComponent<BasicGun>();
    }

    // Changes guns in the inventory
    public void SwapWeapons()
    {
        // If only one gun in the inventor, cancel
        if (GunInventory.Count == 1)
            return;

        // Swap which guns are active && Change active gun ID
        // If slot 0 gun is active make [0] inactive and [1] active
        if (GunInventory[0].activeInHierarchy)
        {
            GunInventory[0].SetActive(false);
            GunInventory[1].SetActive(true);
            activeGunID = GunInventory[1].GetComponent<BasicGun>().GunId;
            playerMovement.HandAnimator = GunInventory[1].GetComponent<Animator>();
        }
        else
        {
            GunInventory[0].SetActive(true);
            GunInventory[1].SetActive(false);
            activeGunID = GunInventory[0].GetComponent<BasicGun>().GunId;
            playerMovement.HandAnimator = GunInventory[0].GetComponent<Animator>();
        }
    }

    // End Knife attack
    public void EndKnifeAttack()
    {
        // Make knife hands visible
        Knife.SetActive(false);

        // Make Active weapon invisible
        foreach (var gun in GunInventory)
        {
            // Active self ignores if its parent is making it inactive or not
            if (gun.GetComponent<BasicGun>().GunId == activeGunID)
            {
                gun.SetActive(true);
            }
        }
    }

    // Searches guns connected to the player for a gun with the same id and adds to inventory
    public void AddGun(int NewGunID)
    {
        // Check if gun being added is already in inventory
        foreach (var gun in GunInventory)
        {
            if (gun.GetComponent<BasicGun>().GunId == NewGunID)
            {
                Debug.LogWarning("Trying to add a gun thats already in the players inventory.");
                return;
            }
        }

        // If only one gun, then do basic gun add
        if (GunInventory.Count == 1)
        {
            // Look through available guns to find a matching id
            foreach (var gun in AvailableGuns)
            {
                // once we've found the gun, we shall perform a weapon swap
                if (gun.GetComponent<BasicGun>().GunId == NewGunID)
                {
                    GunInventory.Add(gun);

                    //GunInventory[0].SetActive(false);

                    activeGunID = NewGunID;
                }
            }
        }
        // if 2 guns then replace the active gun with the new
        else
        {
            foreach (var gun in GunInventory)
            {
                // find the active gun and diable it and remove it from our inventory
                if (gun.activeInHierarchy)
                {
                    //disable the active gun
                    gun.SetActive(false);
                    
                    // Remove from player inventory
                    GunInventory.Remove(gun);

                    // Add new gun
                    GunInventory.Add(AvailableGuns[NewGunID]);
                }
            }
        }

        // once the gun is added swap weapons (which also changes the active gun ID
        SwapWeapons();
    }
}
