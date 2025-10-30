using _RacingGamePrototype.Scripts.Car;
using UnityEngine;
using UnityEngine.UI;

namespace _RacingGamePrototype.Scripts.UI
{
    public class BoostBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backImage;
        [SerializeField] private CarController carController;

        private void Update()
        {
            fillImage.fillAmount = carController.getBoostCooldownProgress();
            if (fillImage.fillAmount >= 1f)
            {
                backImage.color = Color.white;
            }
            else
            {
                backImage.color = Color.gray;
            }
        }
    }
    
    
}
