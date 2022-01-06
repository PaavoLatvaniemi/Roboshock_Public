
namespace Assets.Scripts.UI
{
    public class EnergySliderBarBehaviour : BaseSliderBar
    {
        PlayerEnergyController controller;
        private void OnEnable()
        {
            controller = transform.root.gameObject.GetComponent<PlayerEnergyController>();
            controller.onEnergyChange += changeBarValue;
        }
        private void OnDisable()
        {
            controller.onEnergyChange -= changeBarValue;
        }
    }
}