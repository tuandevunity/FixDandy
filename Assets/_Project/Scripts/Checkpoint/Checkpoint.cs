using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public int ID;
    private MapController mapController;
    void Start()
    {
        mapController = FindFirstObjectByType<MapController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        mapController.Checkpoint = transform;
        DataManager.GamePlayUtility.Profile.OnChangedCheckPointInMap(ID, () => Debug.Log("update checkpoint !"));
    }
}
