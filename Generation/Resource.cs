using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField] GameObject connectedObject;
    [SerializeField] int necessaryStrength; 
    [SerializeField] int prochnost;
    [SerializeField] int maxProchnost;

    void Start()
    {
        prochnost = maxProchnost;
    }
    public void Mine(int _strength)
    {
        prochnost -= _strength;
        if (_strength >= necessaryStrength && prochnost <= 0)
        {
            Instantiate(connectedObject, transform.position, transform.rotation);
            Destroy(this);
        }
        else if (_strength < necessaryStrength && prochnost <= 0)
        {
            Destroy(this);
        }
    }
}
