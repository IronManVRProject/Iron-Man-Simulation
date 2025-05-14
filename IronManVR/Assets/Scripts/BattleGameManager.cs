using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGameManager : MonoBehaviour
{
    public GameObject thanosPrefab;
    public Transform[] spawnPoints;
    public float checkTime = 1f;
    
    
    
    private float timer;
    private bool thanosAlive;
    
    void Start()
    {
        // Instantiate(thanosPrefab, Vector3.zero, Quaternion.identity);
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
    }
}
