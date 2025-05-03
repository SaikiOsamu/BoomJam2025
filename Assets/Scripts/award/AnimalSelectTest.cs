using UnityEngine;

public class AnimalSelectTest : MonoBehaviour
{
   
    public AnimalSelectionUI animalUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            animalUI.Show();
        }
    }
    
}
