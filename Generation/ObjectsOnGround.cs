using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ObjectsOnGround : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] List<GameObject> someObjects = new List<GameObject>();
    [SerializeField] DiamondSquare mainGener;
     
    void Start()
    {
        Random.seed = mainGener.seed;
        int rndTriStart = Random.Range(0,mesh.triangles.Length/3)*3; 
        Vector2 rndNorm = new Vector2(Random.value, Random.value);
        if (rndNorm.x + rndNorm.y >= 1f)
        {
            rndNorm = new Vector2(1f - rndNorm.x, 1f - rndNorm.y);
        }
        Vector3 position = mesh.vertices[rndTriStart+0] + (rndNorm.x * mesh.vertices[rndTriStart+1]) + (rndNorm.y * mesh.vertices[rndTriStart+2]);
        Vector3 normal = mesh.normals[rndTriStart+0] + (rndNorm.x * mesh.normals[rndTriStart+1]) + (rndNorm.y * mesh.normals[rndTriStart+2]);
        //someNewObject.transform.position = position;
        //someNewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
        Instantiate (someObjects[Random.Range(0, someObjects.Count)], position, Quaternion.FromToRotation(Vector3.up, normal));
    }
}