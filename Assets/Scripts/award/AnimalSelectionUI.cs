using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalSelectionUI : MonoBehaviour
{
    public delegate void SelectAnimalPartnerDelegate(Character prefab);

    public GameObject panel;
    public List<Character> allAnimals;


    public Button[] animalButtons;
    public Image[] animalIcons;
    public TextMeshProUGUI[] animalNames;
    public TextMeshProUGUI descriptionBox;
    public SelectAnimalPartnerDelegate selectAnimalPartnerDelegate;

    private Character[] selectedOptions = new Character[2];

    private void Awake()
    {
        panel.SetActive(false); 
    }
    public void Show()
    {   
        Debug.Log("AnimalSelectionUI: Show() 被调用了！");
        panel.SetActive(true);
        Time.timeScale = 0;
        var chosen = allAnimals.OrderBy(x => Random.value).Take(2).ToArray();

        bool hasSelected = false; // 用于防止重复选择

        for (int i = 0; i < 2; i++)
        {   
            selectedOptions[i] = chosen[i];
            animalIcons[i].sprite = chosen[i].icon;
            animalNames[i].text = chosen[i].entityName;

            int index = i;

            animalButtons[i].gameObject.SetActive(true);
            animalButtons[i].interactable = true;
            animalButtons[i].onClick.RemoveAllListeners();
            animalButtons[i].onClick.AddListener(() =>
            {
                if (hasSelected) return; // 已选过则不再执行
                hasSelected = true;

                selectAnimalPartnerDelegate.Invoke(selectedOptions[index]);

                // 禁用所有按钮，避免双击
                foreach (var btn in animalButtons)
                btn.interactable = false;


                // 动画关闭 or 直接关闭面板
                panel.SetActive(false);
                Time.timeScale = 1; 
            });
        }

    }
}
