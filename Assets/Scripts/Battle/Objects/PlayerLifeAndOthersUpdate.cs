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
        textMeshProUGUI.text = "Life: " + player.life + " / " + player.lifeMax
            + "\nGod Power: " + player.godPower + " / " + player.godPowerMax;
    }
}
