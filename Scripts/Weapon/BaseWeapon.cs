
using Assets.Scripts.PaerreMath;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BaseWeapon : NetworkBehaviour, IWeaponable
{

    //UI
    [SerializeField] Sprite weaponSprite;
    //Käytetään valitsemaan killfeedissa oikea kuva tapon aikana.
    [SerializeField] int weaponIndexForSprite;
    //Energy ja special
    PlayerEnergyController playerEnergy;
    protected float specialFireCost = 10;
    protected NetworkVariableBool firingSpecial = new NetworkVariableBool(false);
    protected bool FiringSpecial => firingSpecial.Value;
    [SerializeField] protected int neededAmmoForSpecialFire = 1;
    //Animation rig Transformit
    float LastFireTime;
    [SerializeField] Transform leftHandIK;
    [SerializeField] Transform rightHandIK;

    //Raycast
    //Kameran avulla hankitaan ruudun keskipiste suhteessa kameraan, jotta voidaan ampua täysin keskelle ruutua.
    protected Camera playerCamera;
    protected Vector3 screenFiringPosition
    {
        get
        {
            return playerCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, playerCamera.nearClipPlane));
        }
    }
    //Pelaaja ei voi ampua toisen pelaajan liikkumista ja painovoimaa hallitsevaan collideriin, se tallennetaan tässsä.
    [SerializeField] protected LayerMask onShootIgnoreLayers;
    [SerializeField] protected LayerMask onShootHitLayers;


    //Origin sijainti mm. luotien luomiseen kohdasta origin.

    [Tooltip("Sijainti, josta luoti tai projektiili lähtee.")]
    [SerializeField] protected Transform barrelOrigin;
    //FX

    [SerializeField] protected GameObject bulletImpactEffect;
    [SerializeField] protected GameObject bulletTrail;
    //Visual FX rendataan käyttämällä hyödyksi GPUta, particlet rendataan käyttämällä hyödyksi CPUta, joten 
    //Spawnataan hienot physics riippumattomat efektit FXllä ja sitten valoja perään particle efekteillä
    [SerializeField] protected VisualEffect barrelVisualEffects;
    [SerializeField] protected ParticleSystem barrelVisualEffectParticleLights;

    //Recoil
    float baseWeaponRecoil = 0f;
    float weaponVerticalRecoil = 0f;
    protected float horizontalRecoil;
    [SerializeField] float recoilDiminishAmount;
    [SerializeField] protected float recoilIncreaseAmount;
    [SerializeField] protected float verticalRecoilIncreaseAmount;
    protected int shotNumber = 0;
    // Hit Force
    protected float hitForce = 2000f;



    protected Vector3 recoilVector => createAndGetRecoilVector();

    private Vector3 createAndGetRecoilVector()
    {
        if (WeaponVerticalRecoil <= 0 && horizontalRecoil <= 0)
        {
            return Vector3.zero;
        }
        (float, float) randomPointsOnEllipsis = ((float, float))PaerreMath.getRandomVertexOnEllipseShape(horizontalRecoil, WeaponVerticalRecoil);
        return new Vector3(randomPointsOnEllipsis.Item1, randomPointsOnEllipsis.Item2, 0);
    }


    [SerializeField] WeaponType weaponType;
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float headShotDamage;
    //AMMO  
    [SerializeField]
    bool hasInfiniteAmmo = false;
    //Trackaa hetken x nykyisen lippaan ammustilaa
    protected int currentAmmo = 5;
    //Trackaa muita kun nykyisen lippaan ammustilaa
    int currentSurplusAmmo = 10;
    [SerializeField] int defaultSurplusAmmo;
    [SerializeField] protected int maxMagazineAmmo = 5;
    [SerializeField] int maxSurplusAmmo = 10;

    //Reload ja aika
    bool reloading;
    float reloadTime = 0.55f;
    [SerializeField] protected float firingSpeed = 0.225f;
    [SerializeField] protected float altFireSpeed = 0f;
    protected float previousShotTime = 0f;
    protected ClientRpcParams ownerParams;
    //PROPERTY
    public int CurrentAmmo
    {
        get
        {
            return currentAmmo;
        }

        set
        {
            currentAmmo = value;
            onAmmoChange?.Invoke();
        }
    }
    public bool Reloading { get => reloading; set => reloading = value; }
    public int CurrentSurplusAmmo
    {
        get
        {
            return currentSurplusAmmo;
        }
        set
        {
            currentSurplusAmmo = value;
            onAmmoChange?.Invoke();
        }
    }

    public float WeaponVerticalRecoil
    {
        get => weaponVerticalRecoil; set
        {
            weaponVerticalRecoil = value;
        }
    }

    public PlayerEnergyController PlayerEnergy
    {
        //Haetaan siis koko playerihierarkian rootista se hahmon playerenergycontroller
        get
        {
            return transform.root.GetComponent<PlayerEnergyController>();
        }
    }
    public PlayerAudioController PlayerAudio
    {
        //Haetaan siis koko playerihierarkian rootista se hahmon playerenergycontroller
        get
        {
            return transform.root.GetComponent<PlayerAudioController>();
        }
    }
    public Transform LeftHandIK { get => leftHandIK; set => leftHandIK = value; }
    public Transform RightHandIK { get => rightHandIK; set => rightHandIK = value; }
    ConnectPlayer _owner;
    public ConnectPlayer Owner
    {
        get
        {
            if (_owner == null)
            {
                _owner = transform.root.GetComponent<ConnectPlayer>();
            }
            return _owner;
        }
    }
    //DELEGATE
    public delegate void AmmoChange();
    public event AmmoChange onAmmoChange;
    public delegate void Fire();
    public event Fire onFire;

    public delegate void SpecialFire();
    public event SpecialFire onSpecialFire;

    public delegate void Reload();
    public event Reload onReload;

    public delegate void ReloadEnd();
    public event ReloadEnd onReloadEnd;

    public delegate void Equip();
    public event Equip onEquip;




    //Subclassit eivät suoranaisesti pysty invokeemaan inheritatun classin eventtejä ellei niitä wrappaa näin.
    protected virtual void OnSpecialFire()
    {
        onSpecialFire?.Invoke();
    }
    //MONOBEHAVIOUR
    private void Awake()
    {
        ownerParams = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new ulong[] { transform.root.GetComponent<NetworkObject>().OwnerClientId }
            }
        };
        LastFireTime = 0;
        currentAmmo = maxMagazineAmmo;
        currentSurplusAmmo = defaultSurplusAmmo;

    }
    protected virtual void OnEnable()
    {
        //Voi mennä vähän rikki jos asetta vaihtaa kesken reloadin...
        Reloading = false;
        onEquip?.Invoke();
        if (barrelVisualEffects != null)
        {
            barrelVisualEffects.Stop();
            barrelVisualEffectParticleLights.Stop();
        }
        //Tarvitaan jotta voidaan laskea ruudun keskipiste ampumiselle.
        playerCamera = transform.root.GetComponentInChildren<CameraController>().Cam;
    }

    void FixedUpdate()
    {

        LastFireTime += Time.fixedDeltaTime;
        if (WeaponVerticalRecoil > 0f)
        {
            WeaponVerticalRecoil -= recoilDiminishAmount * LastFireTime;

        }
        if (horizontalRecoil > 0f)
        {
            horizontalRecoil -= recoilDiminishAmount * LastFireTime;
        }
        if (WeaponVerticalRecoil < 0 && horizontalRecoil < 0)
        {
            WeaponVerticalRecoil = 0;
            shotNumber = 0;
            horizontalRecoil = 0;
        }
    }

    protected virtual void TryShootMainFire()
    {
        var timeOfInput = Time.time;
        if (CurrentAmmo <= 0 && Reloading == false)
        {
            ReloadWeapon();
        }
        else if (reloading == false && (previousShotTime + firingSpeed <= timeOfInput))
        {
            FireWeapon();
            previousShotTime = Time.time;

        }
    }

    protected virtual void TryShootAltFire()
    {
        var timeOfInput = Time.time;
        if (CurrentAmmo >= neededAmmoForSpecialFire)
        {
            if (Reloading == false)
            {
                if (previousShotTime + altFireSpeed <= timeOfInput)
                {
                    if (PlayerEnergy.EnergyAmount >= specialFireCost)
                    {
                        SpecialFireWeapon();
                        previousShotTime = Time.time;
                    }
                    else
                        PlayerAudio.PlayAudio(PlayerAudioType.NoEnergy);
                }
            }

        }
    }

    //Ensimmäinen piste kun saavutaan ampumiseen
    public virtual void TryShootWithInputMethod()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Reloading == false && currentAmmo != maxMagazineAmmo)
            {
                ReloadWeapon();
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {

            TryShootMainFire();

        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            TryShootAltFire();
        }
    }



    public virtual void FireWeapon()
    {

        CurrentAmmo--;
        shotNumber++;
        LastFireTime = 0;
        CreateWeaponClassFX(screenFiringPosition+playerCamera.transform.TransformDirection(Vector3.forward*1.5f), playerCamera.transform.TransformDirection(Vector3.forward
            + recoilVector + Vector3.forward));
        CreateWeaponClassDamageServerRpc(Owner.NetworkObjectId, Owner.ClientSideInterpolationWatcher.getWrappedPlayersAndInterpolations(), Owner.GetNetworkTimeWithInputDelay(),
                                         screenFiringPosition,
                                         playerCamera.transform.TransformDirection(Vector3.forward + recoilVector)); ;


        onFire?.Invoke();

    }


    protected virtual void CreateWeaponClassFX(Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {
        //Täytä asetyypille omassa classissa.
        //Visuaalinen client ja serverside aseen ampuminen.
    }
    public virtual void ReloadWeapon()
    {
        if (CurrentSurplusAmmo > 0 || hasInfiniteAmmo)
        {
            reloading = true;
            StartCoroutine(completeReloadInThread());
            onReload?.Invoke();
        }
    }

    //Reloadi vaatii periaatteessa toisen threadin laskemaan aikaa kunnes ase on ladattu, joten toteutetaan se IEnumeratorilla
    //IEnumeratorissa on pääsy WaitForSecondsiin, jolla voidaan oottaa threadissä tietyn ajan verran.
    IEnumerator completeReloadInThread()
    {
        yield return new WaitForSeconds(reloadTime);

        if (CurrentSurplusAmmo > maxMagazineAmmo)
        {
            CurrentSurplusAmmo = currentSurplusAmmo - maxMagazineAmmo + CurrentAmmo;
            CurrentAmmo = maxMagazineAmmo;

        }
        else
        {
            if (hasInfiniteAmmo)
            {
                CurrentAmmo = maxMagazineAmmo;
            }
            else
            {
                CurrentAmmo = CurrentSurplusAmmo;
                CurrentSurplusAmmo = 0;
            }

        }
        reloading = false;
        onReloadEnd?.Invoke();
    }
    public void addAmmo(int ammoAmount)
    {
        CurrentSurplusAmmo += ammoAmount;
    }
    public int getCurrentAmmo()
    {
        return currentAmmo;
    }

    public int getMaxMagazineAmmo()
    {
        return maxMagazineAmmo;
    }

    public int getSurplusAmmo()
    {
        return CurrentSurplusAmmo;
    }

    public WeaponType getWeaponType()
    {
        return weaponType;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public int getMaxDefaultSurplusAndCurrentMagazineAmmo()
    {
        return maxMagazineAmmo + defaultSurplusAmmo;
    }

    bool IWeaponable.hasInfiniteAmmo()
    {
        return hasInfiniteAmmo;
    }
    protected virtual void SpecialFireWeapon()
    {

    }
    //RPC RAKENTEET
    [ServerRpc] // Damage Raycast tehdään palvelimella.
    protected virtual void CreateWeaponClassDamageServerRpc(ulong firingPlayerNetworkId, InterpolationStateInfo[] intStates, int frameID, Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {
        //Täytä inherittaavassa rakenteessa
        //Damagea tekevä serverside raycast tai muu
    }
    [ServerRpc]
    protected virtual void DoClassSpecificSpecialFireWeaponServerRpc(ulong firingPlayerNetworkId)
    {

    }
    [ServerRpc]
    protected virtual void CreateDecalsServerRpc(Vector3 hitPoint, Vector3 hitNormal)
    {
        //Täytä inherittaavassa rakenteessa
    }
    protected virtual void CreateWeaponFXLocal()
    {
        //Täytä inherittaavassa rakenteessa
    }
    [ServerRpc]
    protected virtual void CreateWeaponFXServerRpc(Vector3 position)
    {
        //Täytä inherittaavassa rakenteessa
    }
    [ClientRpc]
    protected virtual void CreateWeaponFXClientRpc(Vector3 position)
    {
        //Täytä inherittaavassa rakenteessa
    }
    [ServerRpc]
    protected void setFiringSpecialServerRpc(bool state)
    {
        firingSpecial.Value = state;
        firingSpecial.SetDirty(true);
    }
    public (float, float) getRecoilRadii()
    {
        return (horizontalRecoil, WeaponVerticalRecoil);
    }
    public Vector3 getRecoilVector()
    {
        return recoilVector;
    }

    public Sprite getWeaponSprite()
    {
        return weaponSprite;
    }

    public void ResetWeapon()
    {
        currentAmmo = maxMagazineAmmo;
        currentSurplusAmmo = defaultSurplusAmmo;
    }

    public int getWeaponSpriteWithIndex()
    {
        return weaponIndexForSprite;
    }
}