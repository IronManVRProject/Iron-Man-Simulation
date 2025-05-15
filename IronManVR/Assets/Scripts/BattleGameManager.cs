using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGameManager : MonoBehaviour
{
    public GameObject thanosPrefab;
    public Transform[] spawnPoints;
    public float checkTime = 1f;

    public AudioClip[] spawnSounds;
    public AudioClip desertAmbience;

    private AudioSource audioSource;
    private float timer;
    private bool thanosAlive;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        var ambienceObj = GetComponentsInChildren<AudioSource>()[^1];
        
        ambienceObj.clip = desertAmbience;
        ambienceObj.loop = true;
        ambienceObj.Play();
        
        timer = 0f;
    }
    
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkTime)
        {
            FindThanos();
            timer = 0f;
        }
    }

    void FindThanos()
    {
        thanosAlive = GameObject.Find("Chasing Thanos Variant(Clone)") != null;
        
        if (thanosAlive) return;
        
        var randomIndex = Random.Range(0, spawnPoints.Length);
        var spawnPoint = spawnPoints[randomIndex];

        Instantiate(thanosPrefab, spawnPoint.position, Quaternion.identity);

        int randomPitchChance = 40;

        if (Random.Range(0, 100) < randomPitchChance)
        {
            audioSource.pitch = 1.75f;
        }
        
        audioSource.PlayOneShot(spawnSounds[Random.Range(0, spawnSounds.Length)]);
        audioSource.pitch = 1f;
    }
}
