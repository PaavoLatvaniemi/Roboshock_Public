using UnityEngine;


public class EnergyPickUp : BasePickUp
{

    [SerializeField]
    float amount = 20;

    protected override void doPickUp(Collider other)
    {
        //Vältetään turhaa kallista serialisaatiota sillä että controllerissa on tämä metodi serverpuolella
        other.GetComponent<PlayerEnergyController>().ChangeEnergyServerRpc(amount);
    }
    public override bool checkIfCanPickup(GameObject picker)
    {
        if (picker.GetComponent<PlayerEnergyController>().EnergyAmount < picker.GetComponent<PlayerEnergyController>().MaxEnergyAmount)
        {
            return true;
        }
        return false;
    }

}
