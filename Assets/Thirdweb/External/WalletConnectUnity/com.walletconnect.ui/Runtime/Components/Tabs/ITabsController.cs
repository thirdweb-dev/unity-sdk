using System;

namespace WalletConnectUnity.UI
{
    public interface ITabsController
    {
        event EventHandler<WCTabPage> PageSelected;

        void SelectPage(WCTabPage page);
    }
}