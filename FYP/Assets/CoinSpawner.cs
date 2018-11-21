using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour {

    public GameObject coin;

	// Use this for initialization
	public void StartCoinSpawning () {
        InvokeRepeating("SpawnCoin", Random.Range(0f, 1f), Random.Range(0.3f, 1f));
    }

    void SpawnCoin()
    {
        GameObject.Instantiate(coin, this.transform);
    }
}
