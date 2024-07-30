using System;
using UnityEngine;
using System.Collections.Generic;

namespace WalletConnectUnity.UI
{
    public class WCTabsController : MonoBehaviour, ITabsController
    {
        [SerializeField] protected List<WCTabPage> pages = new();
        [SerializeField] protected WCTabsBar tabsBar;

        public event EventHandler<WCTabPage> PageSelected;

        protected bool isInitialized;

        public virtual void Initialize()
        {
            if (isInitialized) return;

            tabsBar.Initialize(this);

            isInitialized = true;
        }

        public virtual void Enable(object parameters)
        {
            tabsBar.Enable(pages);
        }

        public virtual void Disable()
        {
            tabsBar.Disable();
        }

        public void SelectPage(WCTabPage page)
        {
            foreach (var p in pages)
            {
                p.PageTransform.gameObject.SetActive(p == page);
            }

            PageSelected?.Invoke(this, page);
        }
    }
}