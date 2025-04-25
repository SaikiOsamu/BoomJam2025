using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class InteractionPanelController : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI floatingText;
    private Action onConfirmCallback;

    public void Show(string message, Action onConfirm)
    {
        messageText.text = message;
        onConfirmCallback = onConfirm;

        gameObject.SetActive(true);
        floatingText.gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowFloatingText(string text)
    {
        floatingText.text = text;  
        Debug.Log("floatingtest show");
        floatingText.gameObject.SetActive(true);  

        StartCoroutine(HideFloatingTextAfterDelay(2f));  
    }
    private System.Collections.IEnumerator HideFloatingTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        floatingText.gameObject.SetActive(false);  
    }
    void Start()
    {   
        gameObject.SetActive(false);
        floatingText.gameObject.SetActive(false);
        confirmButton.onClick.AddListener(() =>
        {
            onConfirmCallback?.Invoke();
            Hide();
        });

        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }
}
