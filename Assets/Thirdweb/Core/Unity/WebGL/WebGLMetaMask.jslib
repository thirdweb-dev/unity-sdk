mergeInto(LibraryManager.library, {
  EnableEthereum: async function (gameObjectName, callback, fallback) {
    const parsedObjectName = UTF8ToString(gameObjectName);
    const parsedCallback = UTF8ToString(callback);
    const parsedFallback = UTF8ToString(fallback);

    try {
      const accounts = await ethereum.request({
        method: "eth_requestAccounts",
      });
      ethereum.autoRefreshOnNetworkChange = false;

      SendMessage(parsedObjectName, parsedCallback, accounts[0]);
    } catch (error) {
      SendMessage(parsedObjectName, parsedFallback, error.message);
    }
  },
  EthereumInit: function (gameObjectName, callBackAccountChange, callBackChainChange) {
    const parsedObjectName = UTF8ToString(gameObjectName);
    const parsedCallbackAccountChange = UTF8ToString(callBackAccountChange);
    const parsedCallbackChainChange = UTF8ToString(callBackChainChange);

    ethereum.on("accountsChanged", function (accounts) {
      let account = accounts[0] !== undefined ? accounts[0] : "";
      SendMessage(parsedObjectName, parsedCallbackAccountChange, account);
    });
    ethereum.on("chainChanged", function (chainId) {
      SendMessage(parsedObjectName, parsedCallbackChainChange, chainId.toString());
    });
  },
  GetChainId: async function (gameObjectName, callback, fallback) {
    const parsedObjectName = UTF8ToString(gameObjectName);
    const parsedCallback = UTF8ToString(callback);
    const parsedFallback = UTF8ToString(fallback);
    try {
      const chainId = await ethereum.request({ method: "eth_chainId" });
      SendMessage(parsedObjectName, parsedCallback, chainId.toString());
    } catch (error) {
      SendMessage(parsedObjectName, parsedFallback, error.message);
    }
  },
  IsMetamaskAvailable: function () {
    return window.ethereum ? true : false;
  },
  GetSelectedAddress: function () {
    var returnValue = ethereum.selectedAddress;
    if (returnValue !== null) {
      var bufferSize = lengthBytesUTF8(returnValue) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(returnValue, buffer, bufferSize);
      return buffer;
    }
  },
  Request: async function (message, gameObjectName, callback, fallback) {
    const parsedMessageStr = UTF8ToString(message);
    const parsedObjectName = UTF8ToString(gameObjectName);
    const parsedCallback = UTF8ToString(callback);
    const parsedFallback = UTF8ToString(fallback);
    let parsedMessage = JSON.parse(parsedMessageStr);
    try {
      const response = await ethereum.request(parsedMessage);
      let rpcResponse = {
        jsonrpc: "2.0",
        result: response,
        id: parsedMessage.id,
        error: null,
      };

      var json = JSON.stringify(rpcResponse);
      SendMessage(parsedObjectName, parsedCallback, json);
    } catch (e) {
      let rpcResonseError = {
        jsonrpc: "2.0",
        id: parsedMessage.id,
        error: {
          message: e.message,
        },
      };
      var json = JSON.stringify(rpcResonseError);
      SendMessage(parsedObjectName, parsedFallback, json);
    }
  }
});
