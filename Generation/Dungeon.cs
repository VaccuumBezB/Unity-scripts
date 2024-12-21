using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    [SerializeField] Transform dungeon_spawner;
    [SerializeField] GameObject dungeon;
    [SerializeField] List<GameObject> rooms = new List<GameObject>();
    
    [SerializeField] private int minRooms = 5;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private float minRoomDistance = 10f;
    [SerializeField] private float maxRoomDistance = 20f;
    
    public int world_size;
    float room_distance;
    
    private List<Vector3> usedPositions = new List<Vector3>();
    
    void Start()
    {
        room_distance = Random.Range(minRoomDistance, maxRoomDistance);
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        Vector3 centralPos = new Vector3(Random.Range(0, world_size), 11.74f, Random.Range(0, world_size));
        dungeon_spawner.position = centralPos;
        GameObject mainRoom = Instantiate(dungeon, dungeon_spawner);
        usedPositions.Add(centralPos);

        int roomCount = Random.Range(minRooms, maxRooms);
        
        for(int i = 0; i < roomCount; i++)
        {
            TryGenerateRoom(mainRoom.transform.position);
        }
    }

    private bool TryGenerateRoom(Vector3 centerPoint)
    {
        for(int attempts = 0; attempts < 10; attempts++)
        {
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(minRoomDistance, maxRoomDistance);
            
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;
            Vector3 newPos = centerPoint + offset;
            newPos.y = 11.74f;

            if (!IsPositionValid(newPos)) continue;

            dungeon_spawner.position = newPos;
            GameObject newRoom = Instantiate(rooms[Random.Range(0, rooms.Count)], dungeon_spawner);
            newRoom.transform.LookAt(new Vector3(centerPoint.x, newRoom.transform.position.y, centerPoint.z));
            
            usedPositions.Add(newPos);
            return true;
        }
        return false;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach(Vector3 usedPos in usedPositions)
        {
            if(Vector3.Distance(position, usedPos) < minRoomDistance)
                return false;
        }
        return true;
    }
}
