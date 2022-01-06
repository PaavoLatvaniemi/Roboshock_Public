using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using MLAPI;

public class DroneMoveAlongPath : NetworkBehaviour
{
    PathCreator pathCreator;
    [SerializeField] EndOfPathInstruction end;
    [SerializeField] float speed;

    [SerializeField] float distanceTravelled;
    public bool move = false;

    public void StartPath(PathCreator _dronePath)
    {
        if (!IsServer) return;

        pathCreator = _dronePath;
        move = true;
    }

    public void StopPath()
    {
        move = false;
        pathCreator.GetComponent<DronePath>().RemoveDroneFromPath();
    }

    void Update()
    {
        if (pathCreator == null || !IsServer) return;
        if (move)
        {
            distanceTravelled += speed * Time.deltaTime;
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, end);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, end);
        }
    }
}
