mergeInto(LibraryManager.library, {
  openPopup: function (
    urlPtr,
    developerClientIdPtr,
    authOptionPtr,
    unityObjectNamePtr,
    unityCallbackMethodPtr
  ) {
    var initialUrl = UTF8ToString(urlPtr);
    var developerClientId = UTF8ToString(developerClientIdPtr);
    var authOption = UTF8ToString(authOptionPtr);
    var unityObjectName = UTF8ToString(unityObjectNamePtr);
    var unityCallbackMethod = UTF8ToString(unityCallbackMethodPtr);

    // Calculate dimensions and position for the popup
    var width = 350;
    var height = 500;
    var top = (window.innerHeight - height) / 2;
    var left = (window.innerWidth - width) / 2;

    // Open a new popup window
    var popupWindow = window.open(
      initialUrl,
      "Login",
      `width=${width},height=${height},top=${top},left=${left}`
    );

    if (!popupWindow) {
      console.error("Failed to open popup window.");
      return;
    }

    // Function to send message back to Unity
    function sendMessageToUnity(message) {
      SendMessage(unityObjectName, unityCallbackMethod, message);
    }

    // Listen for messages from the popup
    window.addEventListener("message", function (event) {
        var data = event.data;
        
        // console.log("Received message from popup: ", data);

        switch (data.eventType) {
          case "injectDeveloperClientId":
            // send message back with developer client id
            popupWindow.postMessage({
              eventType: "injectDeveloperClientIdResult",
              developerClientId: developerClientId,
              authOption: authOption,
            }, "*");
            break;
          case "userLoginSuccess":
            clearInterval(pollTimer);
            sendMessageToUnity(JSON.stringify(data));
            popupWindow.close();
            break;
          case "userLoginFailed":
            clearInterval(pollTimer);
            sendMessageToUnity(JSON.stringify(data));
            popupWindow.close();
            break;
          default:
            break;
        }
    });

    // Poll to check if the popup is closed
    var pollTimer = setInterval(function () {
      if (popupWindow.closed) {
        clearInterval(pollTimer);
        sendMessageToUnity("PopupClosedWithoutAction");
      }
    }, 500);

    // Close the popup when the main window is closed or refreshed
    window.addEventListener("beforeunload", function () {
      if (popupWindow) {
        popupWindow.close();
      }
    });
  }
});
