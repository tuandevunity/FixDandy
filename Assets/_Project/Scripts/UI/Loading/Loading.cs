using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : TickBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        UIManager.ShowPanel<PanelLoading>(0);
    }
}
