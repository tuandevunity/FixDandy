using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerJetpack : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector]
    public BoxCollider boxCollider;
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("trigger jetpack");
            GameController.Instance.GameState = GameState.Pause;
            UIManager.ShowPanel<PopupJetpack>(0.2f);
            boxCollider.enabled = false;  // ko show popup khi het jetpack tai vi tri bay
        }
    }
}
