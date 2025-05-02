using TMPro;
using UnityEngine;

public class ShowHintForPur : MonoBehaviour
{
    [SerializeField]
    private LevelManager levelManager;
    [SerializeField]
    private TextMeshProUGUI textMeshPro;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.enabled = levelManager?.nearPurificationPoint ?? false;
    }
}
