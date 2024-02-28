var LibraryMetaMaskDeeplink = {

OpenMetaMaskDeeplink: function (url) {
  var urlStr = UTF8ToString(url);

  // JS lib should handle this, do nothing and just log
  //window.open(urlStr, "_blank");
  console.log("MetaMask deeplink URL mobile: " + urlStr);
},

WebGLIsMobile: function () {
  return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent)
},

LSExists: function (key) {
  return localStorage.getItem(UTF8ToString(key)) !== null
},
  
LSWrite: function(key, data) {
  localStorage.setItem(UTF8ToString(key), UTF8ToString(data))
},

LSRead: function(key) {
  var data = localStorage.getItem(UTF8ToString(key))
  
  // required to return string data
  var bufferSize = lengthBytesUTF8(data) + 1;
  var buffer = _malloc(bufferSize)
  stringToUTF8(data, buffer, bufferSize);
  return buffer;
},

LSDelete: function(key) {
  localStorage.removeItem(UTF8ToString(key))
}, 
  
_SendRequestFetch: function(idUtf8, objectNameUtf8, methodUtf8, urlUtf8, parUtf8, isGet, authHeaderKeyUtf8, authHeaderValueUtf8) {
  const id = UTF8ToString(idUtf8)
  const objectName = UTF8ToString(objectNameUtf8)
  const method = UTF8ToString(methodUtf8)
  const url = UTF8ToString(urlUtf8)
  const par = UTF8ToString(parUtf8)
  const authHeaderKey = UTF8ToString(authHeaderKeyUtf8)
  const authHeaderValue = UTF8ToString(authHeaderValueUtf8)
  
  let headers = {}
  if (authHeaderKey) {
    headers = {
      [authHeaderKey]: authHeaderValue
    }
  }
  
  const data = {
    method,
    body: par,
    headers
  }
  
  fetch(url, data).then(function (resp) {
    return resp.json()
  }).then(function (result) {
    const resultData = {
      responseJson: JSON.stringify(result),
      errorMessage: null,
      id,
    }
    
    window.unityInstance.SendMessage(objectName, "OnFetchResponseCallback", JSON.stringify(resultData))
  }).catch(function(e) {
    const resultData = {
      responseJson: null,
      errorMessage: e.toString(),
      id,
    }
    
    window.unityInstance.SendMessage(objectName, "OnFetchResponseCallback", JSON.stringify(resultData))
  })
},
  
_InitMetaMaskJS: function(dappName, dappUrl, dappIcon, infuraAPIKey, readonlyRPCMapJson, walletConnectedCallback, providerReadyCallback, providerErrorCallback, providerEventCallback, doJsConnect, isDebug) {
  var walletConnected = UTF8ToString(walletConnectedCallback)
  var providerReady = UTF8ToString(providerReadyCallback)
  var errorCallback = UTF8ToString(providerErrorCallback)
  var eventCallback = UTF8ToString(providerEventCallback)
  var readonlyRPCMap = JSON.parse(UTF8ToString(readonlyRPCMapJson))
  
  if (!window.MMSDK) {
    window.MMSDK = new MetaMaskSDK.MetaMaskSDK({
      dappMetadata: {
        name: UTF8ToString(dappName),
        url: UTF8ToString(dappUrl),
        base64Icon: UTF8ToString(dappIcon),
      },
      infuraAPIKey: UTF8ToString(infuraAPIKey),
      readonlyRPCMap,
      logging: {
        developerMode: Boolean(isDebug),
      }
    })
  }

  var DoUnityCallback = (data, parmData) => {
    var parms = data.split(":")
    var objName = parms[0];
    var functionName = parms[1];
    var guid = parms[2];
    var allParms = [guid]
    if (parmData) {
      var json = JSON.stringify(parmData);
      allParms.push(json)
    }
    
    console.log("Doing Unity callback " + functionName + " with arguments " + JSON.stringify(allParms))
    window.unityInstance.SendMessage(objName, functionName, JSON.stringify(allParms));
  }
  
  var notReadyDelay = 1
  var PostSDKInit = () => {
    try {
      var ethereum = window.MMSDK.getProvider()
      ethereum.on("accountsChanged", (accounts) => {
        if (typeof accounts === "string") {
          accounts = [accounts]
        }
        
        var accountChangeEvent = {
          method: "metamask_accountsChanged",
          params: accounts
        }
        DoUnityCallback(eventCallback, accountChangeEvent)
      })

      ethereum.on("chainChanged", (chainId) => {
        if (typeof chainId === "string") {
          chainId = {
            chainId,
            networkVersion: null,
          }
        }
        var chainChangeEvent = {
          method: "metamask_chainChanged",
          params: chainId,
        }
        DoUnityCallback(eventCallback, chainChangeEvent)
      })

      DoUnityCallback(walletConnected)
    } catch (e) {
      console.warn("MetaMask SDK provider not yet ready..")
      notReadyDelay *= 2
      setTimeout(PostSDKInit, notReadyDelay)
    }
  };
  
  var DoSDKInit = undefined;
  DoSDKInit = () => {
    if (!window.MMSDK.isInitialized) {
      setTimeout(OnSDKInit, 0);
      return;
    }

    if (doJsConnect) {
      window.MMSDK.connect().then(function () {
        PostSDKInit()
      }).catch(function (e) {
        DoUnityCallback(errorCallback)
      })
    } else {
      PostSDKInit()
    }

    DoUnityCallback(providerReady)
  };
  
  setTimeout(DoSDKInit, 0)
},
  
_SendMetaMaskJS: function(idData, methodData, jsonData, responseCallback, errorCallback) {
  var ethereum = window.MMSDK.getProvider()
  if (!ethereum) {
    ethereum = window.ethereum
  }

  var DoUnityCallback = (data, parmData) => {
    var parms = data.split(":")
    var objName = parms[0];
    var functionName = parms[1];
    var guid = parms[2];
    var allParms = [guid]
    if (parmData) {
      var json = JSON.stringify(parmData);
      allParms.push(json)
    }
    window.unityInstance.SendMessage(objName, functionName, JSON.stringify(allParms));
  }
  
  const onResponse = UTF8ToString(responseCallback)
  const onError = UTF8ToString(errorCallback)
  const request = UTF8ToString(jsonData)
  const m = UTF8ToString(methodData)
  const i = UTF8ToString(idData)

  ethereum.request(JSON.parse(request))
      .then((result) => DoUnityCallback(onResponse, { 
        method: m,
        id: i,
        jsonrpc: "2.0",
        result,
      }))
      .catch((e) => DoUnityCallback(onError, e))
},
  
_TerminateMetaMaskJS: function() {
  if (window.MMSDK) {
    console.log("Running MMSDK.terminate()")
    window.MMSDK.terminate()
  } else {
    console.log("Did not run MMSDK.terminate()")
  }
}, 
  
_DisconnectMetaMaskJS: function() {
  if (window.MMSDK) {
    console.log("Checking if we can disconnect")
    var connection = window.MMSDK._getRemoteConnection()
    if (connection) {
      console.log("Running window.MMSDK._getRemoteConnection().disconnect()")
      connection.disconnect()
    } else {
      console.log("No remote connection to disconnect")
    }
  } else {
    console.log("Did not disconnect MMSDK")
  }
},
  
_HasMetaMaskJSSession: function() {
  return window.MMSDK && window.MMSDK.isAuthorized();
}
};

mergeInto(LibraryManager.library, LibraryMetaMaskDeeplink);
