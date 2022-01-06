
namespace Assets.Scripts.UI
{
    public class HealthSliderBarBehaviour : BaseSliderBar
    {
        PlayerHpController hpController;

        private void OnEnable()
        {
            hpController = transform.root.GetComponent<PlayerHpController>();
            hpController.onHealthChange += changeBarValue;
        }
        private void OnDisable()
        {
            hpController.onHealthChange -= changeBarValue;
        }
    }
}