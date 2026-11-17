using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Movement playerMovement;

    private void Update()
    {
        if (staminaBar == null || playerMovement == null)
        {
            return;
        }

        staminaBar.value = playerMovement.CurrentStamina
                         / playerMovement.MaxStamina;
    }
}