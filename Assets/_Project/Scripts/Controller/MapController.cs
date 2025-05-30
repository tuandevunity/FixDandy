using System;
using System.Collections;
using UnityEngine;

public class MapController : TickBehaviour
{
    public Checkpoint[] listCheckpoint;

    private Transform _checkpoint;
    public Transform Checkpoint
    {
        get
        {
            Transform checpointLoaded = LoadCheckpoint();
            if (checpointLoaded == null)
            {
                checpointLoaded = listCheckpoint[listCheckpoint.Length - 1].transform;
            }

            return checpointLoaded;
        }
        set => _checkpoint = value;
    }

    protected override void Awake()
    {
        LoadCheckpoint();
        Debug.Log("awake cua map");
    }

    Transform LoadCheckpoint()
    {
        Transform checkpoint = null;
        int IDCheckpointCurrent = DataManager.GamePlayUtility.Profile.CheckPointCurrent();
        foreach (var item in listCheckpoint)
        {
            if (item.ID == IDCheckpointCurrent)
            {

                checkpoint = item.transform;
            }
        }

        return checkpoint;
    }
}
