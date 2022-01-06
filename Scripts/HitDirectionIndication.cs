using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDirectionIndication : NetworkBehaviour
{
    [SerializeField] GameObject clangObject;
    [ClientRpc]
    public void ShowDamageFromDirectionClientRpc(Vector3 attackDirection3D, Vector3 hitPoint, ClientRpcParams clientRpcSendParams)
    {
        Vector3 vectorDistance3D = transform.InverseTransformPoint(attackDirection3D).normalized;
        GameObject go = Instantiate(clangObject, hitPoint, Quaternion.identity);
        giveDistanceToUI(vectorDistance3D);
    }
    void giveDistanceToUI(Vector3 attackDirection3D)
    {
        HitIndicationController.Singleton.CreateIndicationInDirection(attackDirection3D);
    }
}
