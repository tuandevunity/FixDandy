using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelGamePlay : BaseUI
{
    [Header("Event Trigger")]
    [SerializeField] EventTrigger eventTriggerJump;
    EventTrigger.Entry entry = new EventTrigger.Entry
    {
        eventID = EventTriggerType.PointerDown
    };

    [Header("Other")]
    public FloatingJoystick FloatingJoystick;
    public FixedTouchField FixedTouchField;
    protected override void Awake()
    {
        base.Awake();
        entry.callback.AddListener((data) => { OnClickTriggerJump(); });
        eventTriggerJump.triggers.Add(entry);
    }
    private void Start()
    {
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    public override void SetInfo()
    {
        base.SetInfo();
    }

    #region EventTriggerJump
    void OnClickTriggerJump()
    {
        GameController.Instance.PlayerMovement.Jump();
    }
    #endregion
}


