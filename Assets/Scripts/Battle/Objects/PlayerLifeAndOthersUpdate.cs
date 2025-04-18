using TMPro;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerLifeAndOthersUpdate : MonoBehaviour
{
    [SerializeField]
    private GameObject root;
    [SerializeField]
    private TextMeshProUGUI textMeshProUGUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var player = root.GetComponentInParent<LevelManager>().player;
        string text = "Life: " + player.life + " / " + player.lifeMax;
        if (player.shield > 0)
        {
            text += "\nShield: " + player.shield + " / " + player.shieldMax;
        }
        text += "\nGod Power: " + player.godPower + " / " + player.godPowerMax;
        textMeshProUGUI.text = text;
    }
}
