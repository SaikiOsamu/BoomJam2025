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
        var levelManager = root.GetComponentInParent<LevelManager>();
        var player = levelManager.player;
        string text = "Life: " + player.life + " / " + player.lifeMax;
        if (player.shield > 0)
        {
            text += "\nShield: " + player.shield + " / " + player.shieldMax;
        }
        text += "\nGod Power: " + player.godPower + " / " + player.godPowerMax;
        text += "\nArea: " + levelManager.area;
        text += "\nCleanse: " + levelManager.cleanse + " / " + levelManager.cleanseThreshold;
        if (levelManager.boss != null)
        {
            text += "\nBoss: " + levelManager.boss.life + " / " + levelManager.boss.lifeMax;
        }
        if (levelManager.levelStage == LevelStage.LEVEL_STAGE_WINNER)
        {
            text += "\nYOU WIN!";
        }
        if (levelManager.levelStage == LevelStage.LEVEL_STAGE_LOST)
        {
            text += "\nYou lose..";
        }
        textMeshProUGUI.text = text;
    }
}
