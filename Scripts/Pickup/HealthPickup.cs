using UnityEngine;


public class HealthPickup : BasePickUp
{
    [SerializeField]
    float amount = 20;

    protected override void doPickUp(Collider other)
    {
        //Vältetään turhaa kallista serialisaatiota sillä että controllerissa on tämä metodi serverpuolella
        other.GetComponent<PlayerHpController>().ChangeHealthServerRpc(amount);
    }
    public override bool checkIfCanPickup(GameObject picker)
    {
        if (picker.GetComponent<PlayerHpController>().Health < picker.GetComponent<PlayerHpController>().MaxHP)
        {
            return true;
        }
        return false;
    }
}
