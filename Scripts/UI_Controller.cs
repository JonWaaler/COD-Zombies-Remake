using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

public class UI_Controller : MonoBehaviour
{
    // Accessability vars
    private Transform player;
    private AudioManager audioManager;
    private PlayerSystems playerSystems;
    private Spawner spawner;
    public PostProcessProfile postProcess;
    
    public GameObject GameOver;

    [Header("Ammo")]
    public TextMeshProUGUI CurrentAmmo;
    public TextMeshProUGUI AmmoLeft;

    [Header("Player UI")]
    public Transform PlayerUI;
    public Animator HitMarkerAnim;
    public Slider PlayerHealth;

    [Header("Player Money")]
    public TextMeshProUGUI money;
    public GameObject MoneyReward_Prefab;

    // header Minimap Stuff
    //public GameObject SmallMinimap;
    //public GameObject LargeMinimap;
    //public List<Transform> playerMapIcons;

    [Header("Crosshair")]
    public List<RectTransform> crosshairs;
    private const float CrosshairMaxSpread = 60f; // The maximum position offset of the crosshair // Default pos is an offset of 25
    [SerializeField]
    private float t_CrosshairTimer = 0; // Increases with time
    private const float CrosshairSpreadTime = 1.0f; // always the same

    [Header("FPS")]
    public Text FPS;
    private float t_FPSupdate = 0;

    // All these scripts that are being found only have one instance
    private void Start()
    {
        playerSystems = FindObjectOfType<PlayerSystems>();
        player = FindObjectOfType<PlayerSystems>().gameObject.transform;
        audioManager = FindObjectOfType<AudioManager>();
        spawner = FindObjectOfType<Spawner>();
        vignette = postProcess.GetSetting<Vignette>();
    }

    private void Update()
    {
        // Update FPS
        t_FPSupdate += Time.deltaTime;

        if (t_FPSupdate >= .1f)
        {
            FPS.text = ((int)(1f / Time.unscaledDeltaTime)).ToString();
            t_FPSupdate = 0;
        }
        
        // Crosshair manipulation movement and with each shot
        SetCrosshairPosition(Mathf.Lerp(25f, 75f, Get_CrosshairSpreadPrecent()));
        UpdateMoneyText();
    }


    public float crosshairPrecent;
    // Gets set at start
    private Vignette vignette;
    public void SetVignetterFilter(float value)
    {
        vignette.intensity.value = value;
    }

    // When walking, running, or shooting an un weapon that isnt being aimed.
    // We will increase the spread otherwise it will attempt to decrease.
    float lerpPosition = 25; // this value dictates the crosshairSprea

    // Returns the crosshair spread precentage between 0.0 and 1.0
    public float Get_CrosshairSpreadPrecent()
    {
        t_CrosshairTimer = Mathf.Clamp(t_CrosshairTimer, 0, CrosshairSpreadTime);

        return (t_CrosshairTimer / CrosshairSpreadTime);
    }

    // This function increases/decreases the precentage of how spread the crosshairs are by a fixed amount
    // everything that should change the spread will call this value
    // Ex: value = .1f; therefore, the crosshair will increase by 10% of the 
    //      maxcrosshairspread everytime it's called.
    public void ModifyCrosshairSpread(float value)
    {
        // negative value = centering the crosshair
        // positive value = spread crosshair
        t_CrosshairTimer += value;
    }

    // When called the cross hairs will be set to the middle of the screen +/- offset.
    // Also accounts for diffent screen sizes.
    public void SetCrosshairPosition(float offsetFromMiddle)
    {
        // Y Axis
        crosshairs[0].position = new Vector3(Screen.width / 2f, Screen.height / 2f + offsetFromMiddle);
        crosshairs[1].position = new Vector3(Screen.width / 2f, Screen.height / 2f - offsetFromMiddle);
                                                                        
        // X Axis                                                       
        crosshairs[2].position = new Vector3(Screen.width / 2f + offsetFromMiddle, Screen.height / 2f);
        crosshairs[3].position = new Vector3(Screen.width / 2f - offsetFromMiddle, Screen.height / 2f);
    }

    public void UpdateAmmoUI(int curAmmo, int ammoLeft)
    {
        CurrentAmmo.text = curAmmo.ToString();
        AmmoLeft.text = ammoLeft.ToString();
    }

    public void EndGame()
    {
        GameOver.SetActive(true);
        GameOver.transform.GetChild(1).GetComponent<TextMeshPro>().text = "you survived " + spawner.RoundNumber + " rounds";
        Time.timeScale = 0;
    }

    // Instantiates an object that animate the player collecting money
    public void CollectMoneyAnimation(int moneyCollected)
    {
        //money.text = "$" + (playerSystems.GetMoney() + moneyCollected).ToString(); // should be no longer nessasary due to UpdateMoneyText()
        GameObject rewardInst = Instantiate(MoneyReward_Prefab, PlayerUI);
        rewardInst.GetComponent<TextMeshProUGUI>().text = "+$" + moneyCollected;
        rewardInst.GetComponent<RectTransform>().Translate(new Vector3(Random.Range(-50f, 50f), Random.Range(20f, -20f), 0));
        rewardInst.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, Random.Range(-7.5f,7.5f));
        Destroy(rewardInst, 5f);
    }

    public void UpdateMoneyText()
    {
        money.text = "$" + playerSystems.GetMoney().ToString();
    }

    public void UpdatePlayerHealth()
    {
        PlayerHealth.value = playerSystems.health;
    }

    // Hit marker visual effect
    public void CrosshairHitmarker()
    {
        // Player sound
        audioManager.PlayOneShot("HitMarker");

        // Visual hit marker
        HitMarkerAnim.SetTrigger("HitMarker");
    }
}
