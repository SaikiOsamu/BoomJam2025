using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelection : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Button>().onClick.AddListener(BuildTower);
    }

    void BuildTower()
    {
        gameObject.transform.parent.parent.gameObject.GetComponent<GameManager>().Upgrade();
    }
}
