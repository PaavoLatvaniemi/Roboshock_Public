using System.Collections;
using UnityEngine;


public class RaycastAutomaticWeapon : RayCastWeapon
{
    public override void TryShootWithInputMethod()
    {

            if (Input.GetKey(KeyCode.Mouse0))
            {
                TryShootMainFire();

            }

    }

}