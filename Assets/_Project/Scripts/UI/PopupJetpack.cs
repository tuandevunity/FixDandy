using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupJetpack : BaseUI
{
    // Start is called before the first frame update
    [SerializeField] Button laterButton;
    [SerializeField] Button getItButton;
    protected override void Awake()
    {
        base.Awake();
        laterButton.onClick.AddListener(OnClickLater);
        getItButton.onClick.AddListener(OnClickGetIt);
    }

    private void OnClickLater()
    {
        Hide();
        GameController.Instance.GameState = GameState.Playing;
    }

    private void OnClickGetIt() // TAT TRIGGER JETPACK
    {
        Hide();
        
        GameController.Instance.GameState = GameState.Playing;
        var player = GameController.Instance.PlayerMovement;
        player.Jetpack(2f);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
