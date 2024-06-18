mergeInto(LibraryManager.library, {
  openPopup: function (
    urlPtr,
    redirectUrlPtr,
    unityObjectNamePtr,
    unityCallbackMethodPtr
  ) {
    var url = UTF8ToString(urlPtr);
    var redirectUrl = UTF8ToString(redirectUrlPtr);
    var unityObjectName = UTF8ToString(unityObjectNamePtr);
    var unityCallbackMethod = UTF8ToString(unityCallbackMethodPtr);

    // Open the URL in a popup window
    var popupWindow = window.open(url, "_blank", "width=600,height=600");

    // Function to send message back to Unity
    function sendMessageToUnity(url) {
      SendMessage(unityObjectName, unityCallbackMethod, url);
    }

    // Poll the popup window to see if it sends a message
    window.addEventListener("message", function (event) {
      if (event.origin !== new URL(redirectUrl).origin) {
        return;
      }
      var data = event.data;
      if (data && data.type === "redirect") {
        sendMessageToUnity(data.url);
        popupWindow.close();
      }
    });

    // Check if the popup window is closed
    var interval = setInterval(function () {
      if (popupWindow.closed) {
        clearInterval(interval);
        sendMessageToUnity(redirectUrl);
      }
    }, 500);
  },
});
