using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSwitch : MonoBehaviour
{
    public SpriteRenderer backgroundSprite;
    public Sprite winterBackground;
    public Sprite springBackground;

    public StationaryPrefabSpawner[] spawners;
    public Season currentSeason = Season.Spring;

    public void ToggleSeason()
    {
        currentSeason = currentSeason == Season.Winter ? Season.Spring : Season.Winter;
        StartCoroutine(SwitchSeason());
    }

    IEnumerator SwitchSeason()
    {
        Debug.Log($"[SeasonSwitcher] Switching to {currentSeason}");

        // Switch background sprite
        backgroundSprite.sprite = currentSeason == Season.Spring ? springBackground : winterBackground;

        yield return new WaitForSeconds(0.1f);

        foreach (var spawner in spawners)
        {
            spawner.SetSeason(currentSeason);
        }

        yield return null;
    }
}
