
using Assets.Scripts.PlayerScript;
using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
[DefaultExecutionOrder(1000)]
public class WeaponController : NetworkBehaviour
{
    //Pitäisi olla oletuspistooli
    [SerializeField]
    BaseWeapon startWeapon;
    //AnimationRig
    [SerializeField]
    TwoBoneIKConstraint leftHandIK;
    [SerializeField]
    TwoBoneIKConstraint rightHandIK;
    [SerializeField]
    Animator anim;
    [SerializeField]
    RigBuilder builder;
    //Transform, jossa sijaitsee nykyisesti k�yt�ss� oleva ase.
    //T�m� tarvitaan, jotta animation rig constrainttien k�ytt� olisi v�h�n kivuttomampaa.
    //Sit� kyseist� transformia k�ytet��n rigin riglayer rakenteissa, siell� lis�� tietoa.
    [SerializeField]
    Transform weaponsParent;
    //Transformi kaikille muille aseille.
    [SerializeField]
    Transform inActiveWeaponsParent;

    List<IWeaponable> heldWeapons = new List<IWeaponable>();
    //allWeapons sisältää kaikki mahdollisesti omistettavat aseet, suoraan hahmon sisällä (ei tarvii instantiatee turhaan)
    [SerializeField]
    List<GameObject> allWeapons = new List<GameObject>();

    int currentWeaponIndex;
    float weaponSwitchDelayMinimum = 0.35f;
    float currentWeaponSwitchTimer = Mathf.Infinity;
    //PROPERTY
    public BaseWeapon currentWeapon { get; private set; }
    public bool WeaponsEnabled { get; private set; } = true;
    //DELEGATE
    public delegate void AmmoUpdate(IWeaponable weapon);
    public event AmmoUpdate onWeaponAmmoUpdate;
    public delegate void WeaponSwitch(IWeaponable weaponable);
    public event WeaponSwitch onWeaponSwitch;
    public delegate void ReloadInform();
    public event ReloadInform onReload;
    public delegate void ReloadEndInform();
    public event ReloadEndInform onReloadEnd;
    public delegate void NewWeaponAdded(IWeaponable weaponable);
    public static event NewWeaponAdded onWeaponAdded;


    //MONOBEHAVIOR
    void Start()
    {
        currentWeapon = startWeapon;

        SwitchWeaponLocally(currentWeapon);
        heldWeapons.Add(currentWeapon);
        currentWeaponIndex = heldWeapons.Count - 1;

    }
    private void OnEnable()
    {
        if (currentWeapon != null)
        {
            onWeaponAmmoUpdate?.Invoke(currentWeapon);
        }
    }
    void Update()
    {
        if (!IsOwner || currentWeapon == null) return;

        if (currentWeaponSwitchTimer > weaponSwitchDelayMinimum)
        {
            SwitchWeaponWheelOnMode();
            SwitchWeaponOnNumbers();
        }
        TrackWeaponSwitchDelay();
        if (WeaponsEnabled)
        {
            InteractWithWeapon();
        }

    }

    private void SwitchWeaponOnNumbers()
    {
        //Eli jos inputtia oli tällä framella...
        if (Input.inputString != "")
        {
            int numberInput;
            bool isNumberInput = Int32.TryParse(Input.inputString, out numberInput);
            if (isNumberInput && numberInput > 0)
            {
                if (numberInput <= heldWeapons.Count)
                {
                    currentWeaponIndex = numberInput - 1;
                    SwitchWeaponLocally(heldWeapons[currentWeaponIndex]);
                    SwitchWeaponServerRpc(currentWeaponIndex);
                }


            }
        }
    }

    private void TrackWeaponSwitchDelay()
    {
        currentWeaponSwitchTimer += Time.fixedDeltaTime;
    }


    //SERVER
    [ServerRpc] // Py�r�ytet��n ServerRpc:n kautta, ett� pelaajan uudet aseet n�kyy muillakin.
    public void GiveWeaponServerRpc(WeaponType weaponType)
    {
        GiveWeaponClientRpc(weaponType);
    }
    [ServerRpc]
    void SwitchWeaponServerRpc(int index)
    {
        SwitchWeaponClientRpc(index);
    }
    //CLIENT 
    [ClientRpc]
    void GiveWeaponClientRpc(WeaponType weaponType)
    {
        IWeaponable weapon = getWeaponFromAllWeapons(weaponType);
        SwitchWeaponLocally(weapon);
        heldWeapons.Add(weapon);
        currentWeaponIndex = heldWeapons.Count - 1;
        if (IsOwner) onWeaponAdded?.Invoke(weapon);

    }

    public void SetWeaponEnabledState(bool state) => WeaponsEnabled = state;

    [ClientRpc]
    void SwitchWeaponClientRpc(int wepIndex)
    {
        if (IsOwner) return;
        SwitchWeaponLocally(heldWeapons[wepIndex]);
    }

    IWeaponable getWeaponFromAllWeapons(WeaponType weaponType)
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            if (allWeapons[i].GetComponent<IWeaponable>().getWeaponType() == weaponType)
            {
                return allWeapons[i].GetComponent<IWeaponable>();
            }
        }
        //Ei pit�is saavuttaa, jos kaikki aseet on m��ritelty 
        return null;
    }
    public IWeaponable getWeaponFromPlayer(WeaponType weaponType)
    {
        for (int i = 0; i < heldWeapons.Count; i++)
        {
            if (heldWeapons[i].getWeaponType() == weaponType)
            {
                return heldWeapons[i];
            }

        }
        //Jos asetta ei l�ydy, return null
        return null;
    }

    // Palauttaa aseet oletustilaan
    public void ResetWeapons()
    {
        resetWeaponVariables();
        heldWeapons.Clear();
        GiveWeaponServerRpc(startWeapon.getWeaponType());
    }

    private void resetWeaponVariables()
    {
        for (int i = 0; i < heldWeapons.Count; i++)
        {
            heldWeapons[i].ResetWeapon();
        }
    }

    private void SwitchWeaponWheelOnMode()
    {
        if (PlayerGlobals.isNotInMenu)
        {
            //Mouse ScrollWheel input on akseli v�li� -1 - 1. Voidaan k�ytt�� t�t� informaatiota p��ttelem��n scrollauksen suunnan.
            float mWheel = Input.GetAxis("Mouse ScrollWheel");
            if (mWheel > 0 && heldWeapons.Count > 1)
            {
                //Eli jos mousewheeli� vedet��n k�ytt�j�st� ulosp�in.
                if ((heldWeapons.Count - 1) > currentWeaponIndex)
                {
                    //Tarkistetaan, ett� onko seuraava indeksi aselistassa olemassa, jos on, vaihda siihen
                    currentWeaponIndex++;
                }
                else
                {
                    //Tilanne saavutetaan, jos aseita on vain 1 tai sitten viimeinen indeksi on saavutettu, looppaa ymp�ri ensimm�iseen aseeseen
                    currentWeaponIndex = 0;
                }
                SwitchWeaponLocally(heldWeapons[currentWeaponIndex]);
                SwitchWeaponServerRpc(currentWeaponIndex);
            }
            if (mWheel < 0 && heldWeapons.Count > 1)
            {
                //Sis��np�in. Sama asia, mutta vastaikkaisissa skenaarioissa.
                if (currentWeaponIndex <= 0)
                {
                    currentWeaponIndex = heldWeapons.Count - 1;
                }
                else
                {
                    currentWeaponIndex--;
                }
                SwitchWeaponLocally(heldWeapons[currentWeaponIndex]);
                SwitchWeaponServerRpc(currentWeaponIndex);

            }
        }


    }


    private void InteractWithWeapon()
    {
        //Pelaaja todennäköisesti liikkuu, joten bufferataan tämä inputti fixedUpdaten loppuun, jotta pelaaja
        //Ei voi ampua itseään.

        if (PlayerGlobals.isNotInMenu)
        {
            currentWeapon.TryShootWithInputMethod();
        }


    }


    /* VAIHTAA ASETTA VAIN PAIKALLISESTI.
     * Kutsu my�s lis�ksi SwitchWeaponServerRpc metodia, jos haluat vaihtaa asetta*/
    void SwitchWeaponLocally(IWeaponable newWeapon)
    {
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
            currentWeapon.onAmmoChange -= updateAmmo;
            currentWeapon.onFire -= animateShooting;
            currentWeapon.onReload -= informReload;
            currentWeapon.onReloadEnd -= informReloadEnd;
            currentWeapon.transform.SetParent(inActiveWeaponsParent);
        }

        currentWeapon = (BaseWeapon)newWeapon;

        SetCurrentWeaponParentAndResetPosition();
        UpdateWeaponIKDataSources();
        if (IsOwner)
        {
            currentWeapon.onAmmoChange += updateAmmo;
            currentWeapon.onFire += animateShooting;
            currentWeapon.onReload += informReload;
            currentWeapon.onReloadEnd += informReloadEnd;
            currentWeaponSwitchTimer = 0;
            onWeaponSwitch?.Invoke(currentWeapon);
            onWeaponAmmoUpdate?.Invoke(currentWeapon);
            //Jos keskellä reloadia, reload ui ei jää pyörimään.
            onReloadEnd?.Invoke();

        }


    }

    private void informReloadEnd()
    {
        onReloadEnd?.Invoke();
    }

    private void informReload()
    {
        onReload?.Invoke();
    }

    private void animateShooting()
    {
        anim.SetTrigger("Shoot");
    }

    //Kaikki positiot ym. pit�� resetoida, jotta aseen meshi asettuu oikein rigiin.
    private void SetCurrentWeaponParentAndResetPosition()
    {
        currentWeapon.transform.SetParent(weaponsParent);
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.gameObject.transform.localPosition = Vector3.zero;
        currentWeapon.gameObject.transform.localRotation = Quaternion.identity;
    }
    private void OnDisable()
    {
        currentWeapon.onAmmoChange -= updateAmmo;
        currentWeapon.onFire -= animateShooting;
        currentWeapon.onReload -= informReload;
        currentWeapon.onReloadEnd -= informReloadEnd;
    }
    //Animaatiorigin IKt vaatii p�ivityksen, jotta k�det osaa tr�ck�t� asetta oikein.
    private void UpdateWeaponIKDataSources()
    {
        leftHandIK.data.target = currentWeapon.LeftHandIK;
        rightHandIK.data.target = currentWeapon.RightHandIK;
        builder.Build();
    }
    //Päivittää ammotilanteen mm. kanvaksen tietoon.
    private void updateAmmo()
    {
        onWeaponAmmoUpdate?.Invoke(currentWeapon);
    }
    public void addAmmo(int amount, WeaponType weaponType)
    {
        getWeaponFromPlayer(weaponType).addAmmo(amount);
    }
    public (float, float) getWeaponRecoilAsFloats()
    {
        if (currentWeapon == null)
        {
            return (0f, 0f);
        }
        return currentWeapon.getRecoilRadii();
    }


}
