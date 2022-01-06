using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpAbility : MonoBehaviour
{
    PlayerController playerController;
    PlayerEnergyController playerEnergyController;
    PlayerAudioController playerAudioController;

    float _warpTime;
    [SerializeField] float _warpCooldown = 2.5f;
    [SerializeField] float _power = 2f;
    float currentCoolDown = 0;
    public float WarpTime { get => _warpTime; }
    public float WarpCooldown { get => _warpCooldown; set => _warpCooldown = value; }

    public delegate void Warp();
    public event Warp onWarp;
    public event Warp onWarpEnd;
    public delegate void WarpCooldownProgess(float progress);
    public static event WarpCooldownProgess onWarpCDProgress;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerEnergyController = GetComponent<PlayerEnergyController>();
        playerAudioController = GetComponent<PlayerAudioController>();
    }


    void LateUpdate()
    {
        if (currentCoolDown <= 0)
        {
            //Warppaa pelaajaa eteenpäin Z-akselilla
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!playerEnergyController.hasEnoughEnergy(20f))
                {
                    playerAudioController.PlayAudio(PlayerAudioType.NoEnergy);
                    return;
                }

                if (playerController.IsMovingWithInput())
                {
                    playerEnergyController.ChangeEnergyServerRpc(-20);
                    Vector3 warpDir = playerController.getInputMovementVector() * _power;
                    StartCoroutine(warpExponential(transform.TransformDirection(warpDir)));
                    currentCoolDown = WarpCooldown;
                    playerAudioController.PlayAudio(PlayerAudioType.Warp);
                }
            }
        }
        //Super ei väliä onko negatiivinen antaa mennä, nousee positiviiselle kun ability käytetään.
        currentCoolDown -= Time.fixedDeltaTime;
        if (currentCoolDown >= 0)
        {
            onWarpCDProgress?.Invoke(currentCoolDown);
        }

    }
    IEnumerator warpExponential(Vector3 force)
    {
        Vector3 referenceForce = force;
        Vector3 oldExternalForce = playerController.getExternalForce();
        float timer = 0;

        onWarp?.Invoke();
        while (timer < 1)
        {
            //Eksponentiaalinen kasvu, joten warp on alussa hidas, lopussa nopea
            //T ei mene yli sen rajan (interpolaatio 0...1) joten ei haittaa vaikka kerrottaisiin liikaa (arvo 1.1 on interpolaatiossa silti 1, ei ekstrapolaatiota)
            playerController.setExternalForce(Vector3.Lerp(referenceForce + oldExternalForce, Vector3.zero, (timer) * timer));
            timer += Time.fixedDeltaTime;
            _warpTime = timer;
            yield return null;
        }


    }
}
