using TMPro;
using UnityEngine;

public class LifeUpdate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = "Life: " + gameObject.GetComponentInParent<GameManager>().life.ToString()
            + "\nExp: " + gameObject.GetComponentInParent<GameManager>().exp.ToString()
            + " / " + gameObject.GetComponentInParent<GameManager>().exp_to_upgrade.ToString()
            + "\nLevel: " + gameObject.GetComponentInParent<GameManager>().level.ToString();
    }
}
