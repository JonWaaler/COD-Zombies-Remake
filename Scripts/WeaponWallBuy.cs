using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponWallBuy : MonoBehaviour
{
    // TODO: when player enter trigger
    // 0. check if player has the weapon for the wall buy in inventory
    //      1. Show either buy weapon text or buy ammo text
    // 2. give player said product when input.F
    // 3. If they bought the weapon, make sure it changes to ammo products now
    public int GunID = 1;
    public int PriceOfGun = 1500;
    public int AmmoPrice = 900;
    public int Ammo = 240;

    // since the player has multiple colliders calling the purchase
    // we put a cool down on how often you can use the wall buy
    private float SaveTime = 0;

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Cooldown to prohibit multi-buys
            if (Time.time - SaveTime < .25f)
                return;

            PlayerSystems playerSystems = other.transform.root.GetComponent<PlayerSystems>();

            // Check if the player has the wall buy in the inventory
            if (playerSystems.DoesPlayerHaveGun(GunID))
            {
                // save the time at purchase
                SaveTime = Time.time;

                // If the player has the gun in the inventory, we will instead get ammo
                playerSystems.GetActiveGun().AmmoLeft += Ammo;

                // subtract money from player and play sound
                playerSystems.BuyWeapon(AmmoPrice);
            }
            else
            {
                // The player does not have the gun in the inventory so purchase the gun
                if (playerSystems.GetMoney() >= PriceOfGun)
                {
                    // save the time at purchase
                    SaveTime = Time.time;

                    // Adds the gun to the inventory and switchs weapons
                    playerSystems.AddGun(GunID);

                    // Handles sounds and players money for the transaction
                    playerSystems.BuyWeapon(PriceOfGun);
                }
            }
        }
    }
}
