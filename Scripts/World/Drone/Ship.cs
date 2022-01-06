using System.Collections;
using UnityEngine;
using PathCreation;
using MLAPI;
using MLAPI.Messaging;

public class Ship : NetworkBehaviour
{
    [SerializeField] PathCreator pathCreator;
    [SerializeField] float speed;
    [SerializeField] float minWaitTime;
    [SerializeField] float maxWaitTime;
    [SerializeField] AudioSource engineAudioSource;

    float distanceTravelled;
    bool move = false;

    private void OnEnable()
    {
        GameModeManager.onGameModeStartServer += StartWaitCycle;
    }
    private void OnDisable()
    {
        GameModeManager.onGameModeStartServer -= StartWaitCycle;
    }

    void Update()
    {
        if (pathCreator == null || !IsServer) return;
        if (move)
        {
            distanceTravelled += speed * Time.deltaTime;
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);
            if (pathCreator.path.length - 0.1f <= distanceTravelled)
            {
                StartWaitCycle();
            }
        }
    }

    void StartWaitCycle()
    {
        StartCoroutine(WaitCycle());
    }

    IEnumerator WaitCycle()
    {
        float timeToWait = Random.Range(minWaitTime, maxWaitTime);

        move = false;
        distanceTravelled = 0;
        HideShipClientRpc();

        yield return new WaitForSeconds(timeToWait);

        move = true;
        ShowShipClientRpc();
    }

    [ClientRpc] void HideShipClientRpc()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        engineAudioSource.Stop();
    }
    [ClientRpc] void ShowShipClientRpc()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        engineAudioSource.Play();
    }
}
