using Assets.Scripts.PlayerScript;
using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Yaw = Kulma oikealle ja vasemmalle
    float yaw;
    //Pitch = Kulma ylös ja alas
    float pitch;
    public float mouseSens { get => PlayerGlobals.MouseSensitivity; }
    [SerializeField] Transform player;
    [SerializeField] Transform cameraPivotPoint;
    [SerializeField] Canvas cameraCanvas;
    [SerializeField] Vector3 offset;
    //Kamera klippaa turhan hölmösti jos se on keskellä headin bonea, joten offsetataan sitä hieman
    float baseFieldOfView;
    float normalZoomFOV = 25;
    bool zoomedIn = false;
    Camera cam;
    NetworkObject identity;
    [SerializeField] GameObject[] hideFromCameraClientside;
    [SerializeField] LayerMask proceduralHideLayer;

    public bool ZoomedIn { get => zoomedIn; set => zoomedIn = value; }
    public Camera Cam { get => cam; set => cam = value; }

    [ClientRpc]
    void setupCameraLayersClientRpc()
    {
        for (int i = 0; i < hideFromCameraClientside.Length; i++)
        {
            hideFromCameraClientside[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        GetComponentInChildren<Camera>().cullingMask -= proceduralHideLayer;
    }
    void Start()
    {
        identity = GetComponentInParent<NetworkObject>();
        if (identity != null)
        {

            if (!identity.IsLocalPlayer) return;
            activateCanvasClientRpc();
            setupCameraLayersClientRpc();
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cam = GetComponentInChildren<Camera>();
        cam.gameObject.AddComponent<InterpolationObjectController>();
        baseFieldOfView = Cam.fieldOfView;

    }
    [ClientRpc]
    void activateCanvasClientRpc()
    {
        cameraCanvas.gameObject.SetActive(true);
    }
    private void Update()
    {
        if (identity != null)
            if (!identity.IsLocalPlayer) return;
        GetCameraInput();
    }

    private void GetCameraInput()
    {
        if (PlayerGlobals.isNotInMenu)
        {
            yaw += Input.GetAxis("Mouse X") * (mouseSens / 2f);
            pitch += -Input.GetAxis("Mouse Y") * (mouseSens / 2f);
        }

    }

    void FixedUpdate()
    {
        if (identity != null)
            if (!identity.IsLocalPlayer) return;

        MoveCamera();
        PositionCamera();
        CursorLock();
    }

    private void PositionCamera()
    {

        transform.position = cameraPivotPoint.position + offset;
    }

    private void CursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void MoveCamera()
    {



        //Matikkafunktion avulla clampataan, eli selkokielellä rajoitetaan arvo a, arvojen b ja c välille.
        pitch = Mathf.Clamp(pitch, -65, 65);

        player.transform.rotation = Quaternion.Euler(new Vector3(player.transform.rotation.eulerAngles.x,
                                                                 yaw,
                                                                 player.transform.rotation.eulerAngles.z));

        this.transform.rotation = Quaternion.Euler(new Vector3(pitch,
                                                               transform.rotation.eulerAngles.y,
                                                               transform.rotation.eulerAngles.z));
    }
    public void ToggleZoom()
    {
        bool doZoom = (ZoomedIn == false) ? true : false;
        float refA = 0;
        float refB = 0;
        if (doZoom)
        {
            refA = baseFieldOfView;
            refB = normalZoomFOV;
            ZoomedIn = true;
        }
        else
        {
            refA = normalZoomFOV;
            refB = baseFieldOfView;
            ZoomedIn = false;
        }

        StartCoroutine(lerpCameraZoomAmount(refA, refB));

    }
    IEnumerator lerpCameraZoomAmount(float referenceA, float ReferenceB)
    {
        float timeElapsed = 0;
        while (timeElapsed < 0.15f)
        {
            Cam.fieldOfView = Mathf.Lerp(referenceA, ReferenceB, timeElapsed / 0.15f);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

    }
}
