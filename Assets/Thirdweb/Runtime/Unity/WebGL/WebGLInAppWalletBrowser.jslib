mergeInto(LibraryManager.library, {
  openPopup: function (urlPtr, unityObjectNamePtr, unityCallbackMethodPtr) {
    var initialUrl = UTF8ToString(urlPtr);
    var unityObjectName = UTF8ToString(unityObjectNamePtr);
    var unityCallbackMethod = UTF8ToString(unityCallbackMethodPtr);

    // Calculate dimensions and position for the popup
    var width = 600;
    var height = 750;
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

    // Detect when the popup is closed
    var pollTimer = window.setInterval(function () {
      if (popupWindow.closed) {
        clearInterval(pollTimer);
        window.removeEventListener("message", messageListener);
        sendMessageToUnity(
          JSON.stringify({
            eventType: "PopupClosedWithoutAction",
          })
        );
      }
    }, 1000);

    // Listen for messages from the popup window
    function messageListener(event) {
      // Ensure the message is from the expected origin
      if (event.origin !== new URL(initialUrl).origin) {
        return;
      }

      // Ensure the event data is an object
      if (typeof event.data !== "object") {
        return;
      }

      switch (event.data.eventType) {
        case "oauthSuccessResult":
        case "oauthFailureResult":
          window.removeEventListener("message", messageListener);
          clearInterval(pollTimer);
          popupWindow.close();
          sendMessageToUnity(JSON.stringify(event.data));
          break;
        default:
          // no-op, do not throw here
          break;
      }
    }

    window.addEventListener("message", messageListener);

    // Close the popup when the main window is closed or refreshed
    window.addEventListener("beforeunload", function () {
      if (popupWindow) {
        popupWindow.close();
      }
    });
  },
});
