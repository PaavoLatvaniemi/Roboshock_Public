using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using UnityEngine;

/// <summary>
/// Energiankäyttöön liittyvä yksittäisen pelaajan energiakomponentti
/// </summary>
public class PlayerEnergyController : NetworkBehaviour
{
    NetworkVariableFloat energyAmount = new NetworkVariableFloat(100f);
    public float EnergyAmount => energyAmount.Value;

    public float MaxEnergyAmount { get => maxEnergyAmount; set => maxEnergyAmount = value; }

    float maxDamageWithReduction = 0.8f;
    float maxEnergyAmount = 100;
    public delegate void EnergyChange(float amount);
    public event EnergyChange onEnergyChange;
    Coroutine drainRoutine;

    public void ChangeEnergy(float amount, bool isToSet = false)
    {
        if (!IsServer) return;
        if (isToSet)
        {
            energyAmount.Value = amount;
        }
        else
        {
            energyAmount.Value += amount;
        }
        energyAmount.SetDirty(true);
        if (energyAmount.Value >= 100)
        {
            energyAmount.Value = 100;
        }
        if (energyAmount.Value < 0)
        {
            energyAmount.Value = 0;
        }

    }
    [ServerRpc]
    public void ChangeEnergyServerRpc(float amount)
    {
        ChangeEnergy(amount);
    }

    public void ResetEnergy() // Vain palvelin suorittaa.
    {
        if (!IsServer) return;
        ChangeEnergy(100f, true);
    }
    public bool hasEnoughEnergy(float comparisonValue)
    {
        return (energyAmount.Value >= comparisonValue) ? true : false;
    }
    IEnumerator drainEnergyThread(float amount, float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            if (energyAmount.Value <= 0)
            {
                stopDrainingEnergyServerRpc();
            }
            ChangeEnergy(-amount);
        }


    }
    [ServerRpc]
    public void drainEnergyServerRpc(float amount, float interval)
    {
        drainRoutine = StartCoroutine(drainEnergyThread(amount, interval));
    }
    [ServerRpc]
    public void stopDrainingEnergyServerRpc()
    {
        StopCoroutine(drainRoutine);
    }
    public float returnEnergyDamageReductionDamage(float damageAmount)
    {
        float newDamage = damageAmount + (Mathf.Abs(damageAmount) * ((1 - maxDamageWithReduction) * getCurrentDamageReduction()));
        if (newDamage > 0)
        {
            newDamage = 0;
        }
        return newDamage;
    }
    float getCurrentDamageReduction()
    {
        return EnergyAmount / (MaxEnergyAmount);
    }
    private void OnEnable()
    {
        energyAmount.OnValueChanged += OnEnergyChanged;
    }

    private void OnEnergyChanged(float previousValue, float newValue)
    {
        if (IsOwner && IsClient)
        {
            onEnergyChange?.Invoke(newValue);
        }
    }
}
