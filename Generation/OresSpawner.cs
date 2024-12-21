using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OresSpawner : MonoBehaviour
{
    public List<GameObject> spawnableObjects = new List<GameObject>();
    [SerializeField] DiamondSquare mapGen;

    void Start()
    {
        Random.seed = mapGen.seed;
        Vector3 spawnPos = new Vector3(Random.Range(-1436, -1201), Random.Range(-80, -57), Random.Range(-1236, 1352));
        Instantiate(spawnableObjects[Random.Range(0, spawnableObjects.Count)], spawnPos, new Quaternion(0, Random.Range(0, 360), 0, 0));
    }
}
