using System;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [Serializable]
    public class MenuPage
    {
        [SerializeField] Transform menuTransform;
        MenuPage previousMenu;

        public MenuPage GoBackToPreviousMenu()
        {
            previousMenu.OpenMenu(this, false);
            CloseThisMenu();
            return previousMenu;
        }
        public void OpenMenu(MenuPage Opener, bool updatePrevious = true)
        {
            menuTransform.gameObject.SetActive(true);
            previousMenu = Opener;
        }
        public void CloseThisMenu()
        {
            menuTransform.gameObject.SetActive(false);
        }

    }
}
