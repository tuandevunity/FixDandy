using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSetting : BaseUI
{
    // Start is called before the first frame update
    [SerializeField] private Button resetButton;
    [SerializeField] private Button skipCheckpoint;

    protected override void Awake()
    {
        base.Awake();
        resetButton.onClick.AddListener(GameController.Instance.ResetGame);
        skipCheckpoint.onClick.AddListener(GameController.Instance.SkipCheckpoint);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
