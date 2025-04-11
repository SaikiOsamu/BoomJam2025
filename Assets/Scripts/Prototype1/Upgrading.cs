using UnityEngine;

public class Upgrading : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponentInParent<GameManager>().upgrading)
        {
            GetComponent<CanvasGroup>().alpha = 1.0f;
            GetComponent<CanvasGroup>().interactable = true;
        }
        else
        {
            GetComponent<CanvasGroup>().alpha = 0.0f;
            GetComponent<CanvasGroup>().interactable = false;
        }
    }
}
