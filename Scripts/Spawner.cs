using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

// Also handles updating Round number
public class Spawner : MonoBehaviour
{
    public GameObject Zombie;
    public bool Debug_LimitSpawns = false;
    public List<GameObject> ZombiesLeft;
    public List<Transform> Spawners;
    public int RoundNumber = 0;
    public int killGoal = 12;
    //public Text RoundBanner;
    public TextMeshProUGUI RoundBanner;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        StartCoroutine(StartRound());
    }

    public int ZombieCount = 0; //Number decides which zombie recalculates path
    void Update()
    {
        // DEBUG - Kill zombies to test round stuff
        if (Input.GetKeyDown(KeyCode.K))
        {
            Destroy(ZombiesLeft[0]);
            ZombiesLeft.RemoveAt(0);
        }


        // Optimized Zombie AI Pathing. Only recalculate path for one zombie each frame
        //if (ZombiesLeft.Count>0)
        //{
        //    StartCoroutine(ZombiesLeft[ZombieCount].GetComponent<ZombieAI>().UpdateZombiePath());
        //    ZombieCount++;
        //    if (ZombieCount >= ZombiesLeft.Count)
        //        ZombieCount = 0;
        //}
    }

    IEnumerator StartRound()
    {
        // Delay new round text annoucement
        yield return new WaitForSecondsRealtime(2f);

        RoundNumber++;
        RoundBanner.text = RoundNumber.ToString();
        //RoundBanner.gameObject.SetActive(true); // disabled b/c we want to stay showing the round #
        // Playsound....
    
        // wait before hiding the round announcement
        yield return new WaitForSecondsRealtime(2f);
    
        //RoundBanner.gameObject.SetActive(false);
    
        // Decide next round kill goal    //\sqrt{x*5}+10
        killGoal = Mathf.RoundToInt(Mathf.Sqrt(RoundNumber * 5f) + 10f);
        
        // Start spawning zombies
        StartCoroutine(SpawnZombies_WithinSeconds(2f));
    }

    IEnumerator SpawnZombies_WithinSeconds(float RoundTime) // EXAMPLE: 120sec RoundTime to spawn 50 zombies
    {
        // Debug Spawn only 1 zombie in
        if(Debug_LimitSpawns)
            killGoal = 1;

        // Spawn All the zombies for the round
        for (int i = 0; i < killGoal; i++)
        {
            // Spawn Zombie and add to zombie left list
            GameObject instance = Instantiate(Zombie, Spawners[Random.Range(0, Spawners.Count)].position, Quaternion.identity);

            // Add the zombie to the list to track how many zombies are left
            ZombiesLeft.Add(instance);

            yield return new WaitForSecondsRealtime(RoundTime/killGoal);
        }

        if(Application.isEditor)
            print("Finished Spawning");
        
        // Continuously checks to see if the player has killed all zombies        
        StartCoroutine(CheckForRoundEnd(1f));
    }

    // Method to detect round end
    IEnumerator CheckForRoundEnd(float timeBetweenChecks)
    {
        if(ZombiesLeft.Count > 0)
        {
            // Means the player hasnt killed all the zombies
            yield return new WaitForSeconds(timeBetweenChecks);
            StartCoroutine(CheckForRoundEnd(timeBetweenChecks));
        }
        else
        {
            // Start the next round
            if (RoundNumber != 0)
                audioManager.PlayOneShot("NextRound");

            StartCoroutine(StartRound());
        }
    }

    public void RemoveZombie(GameObject zombie)
    {
        ZombiesLeft.Remove(zombie);
    }
}
