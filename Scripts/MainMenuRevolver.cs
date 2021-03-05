using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuRevolver : MonoBehaviour
{
    private AudioManager audioManager;
    private Animator revolver_anim;
    public ParticleSystem RevolverParticles;
    public Slider ProgressBar_SceneLoading;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        revolver_anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PlayGame();
            RevolverShot();
        }
    }

    public void PlayGame()
    {
        revolver_anim.SetBool("IsShooting", true);
    }

    public void RevolverShot()
    {
        // Find a way to load the game scene before switching to it
        RevolverParticles.Play();
        audioManager.Play("RevolverShot");
        revolver_anim.SetBool("IsShooting", false);
        ProgressBar_SceneLoading.gameObject.SetActive(true);
        StartCoroutine(WaitForSound(1.5f));
    }

    IEnumerator WaitForSound(float delay)
    {
        // Load Overview scene in background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (!asyncLoad.isDone)
        {
            ProgressBar_SceneLoading.value = asyncLoad.progress;
            yield return new WaitForSeconds(.1f);
            yield return null;
        }
        yield return new WaitForSeconds(.5f);
    }
}
