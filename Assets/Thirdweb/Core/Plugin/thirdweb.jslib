var plugin = {
  ThirdwebInvoke: function (taskId, route, payload, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .invoke(UTF8ToString(route), UTF8ToString(payload))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        // callback into unity
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        console.error("ThirdwebSDK invoke error", err);
        var msg = err.reason || err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebInvokeListener: function (taskId, route, payload, action) {
    // execute bridge call
    window.bridge.invokeListener(
      UTF8ToString(taskId),
      UTF8ToString(route),
      UTF8ToString(payload),
      action,
      (jsAction, jsTaskId, jsResult) => {
        // Handle Task ID
        var jsId = jsTaskId;
        var jsIdSize = lengthBytesUTF8(jsId) + 1;
        var jsIdBuffer = _malloc(jsIdSize);
        stringToUTF8(jsId, jsIdBuffer, jsIdSize);

        // Handle Result
        var jsRes = jsResult;
        var jsResSize = lengthBytesUTF8(jsRes) + 1;
        var jsResBuffer = _malloc(jsResSize);
        stringToUTF8(jsRes, jsResBuffer, jsResSize);

        dynCall_vii(jsAction, jsIdBuffer, jsResBuffer);
      }
    );
  },
  ThirdwebInitialize: function (chain, options) {
    window.bridge.initialize(UTF8ToString(chain), UTF8ToString(options));
  },
  ThirdwebConnect: function (
    taskId,
    wallet,
    chainId,
    password,
    email,
    phoneNumber,
    personalWallet,
    authOptions,
    smartWalletAccountOverride,
    cb
  ) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .connect(
        UTF8ToString(wallet),
        UTF8ToString(chainId),
        UTF8ToString(password),
        UTF8ToString(email),
        UTF8ToString(phoneNumber),
        UTF8ToString(personalWallet),
        UTF8ToString(authOptions),
        UTF8ToString(smartWalletAccountOverride)
      )
      .then((address) => {
        if (address) {
          var bufferSize = lengthBytesUTF8(address) + 1;
          var buffer = _malloc(bufferSize);
          stringToUTF8(address, buffer, bufferSize);
          dynCall_viii(cb, idPtr, buffer, null);
        } else {
          dynCall_viii(cb, idPtr, null, null);
        }
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebSwitchNetwork: async function (taskId, chainId, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .switchNetwork(UTF8ToString(chainId))
      .then(() => {
        dynCall_viii(cb, idPtr, null, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebDisconnect: async function (taskId, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .disconnect()
      .then(() => {
        dynCall_viii(cb, idPtr, idPtr, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebFundWallet: async function (taskId, payload, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .fundWallet(UTF8ToString(payload))
      .then(() => {
        dynCall_viii(cb, idPtr, idPtr, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebExportWallet: async function (taskId, password, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .exportWallet(UTF8ToString(password))
      .then((encryptedJson) => {
        if (encryptedJson) {
          var bufferSize = lengthBytesUTF8(encryptedJson) + 1;
          var buffer = _malloc(bufferSize);
          stringToUTF8(encryptedJson, buffer, bufferSize);
          dynCall_viii(cb, idPtr, buffer, null);
        } else {
          dynCall_viii(cb, idPtr, null, null);
        }
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebSmartWalletAddAdmin: async function (taskId, admin, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .smartWalletAddAdmin(UTF8ToString(admin))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebSmartWalletRemoveAdmin: async function (taskId, admin, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .smartWalletRemoveAdmin(UTF8ToString(admin))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebSmartWalletCreateSessionKey: async function (taskId, options, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .smartWalletCreateSessionKey(UTF8ToString(options))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebSmartWalletRevokeSessionKey: async function (taskId, signer, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .smartWalletRevokeSessionKey(UTF8ToString(signer))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebSmartWalletGetAllActiveSigners: async function (taskId, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .smartWalletGetAllActiveSigners()
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebWaitForTransactionResult: async function (taskId, txHash, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .waitForTransactionResult(UTF8ToString(txHash))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebGetLatestBlockNumber: async function (taskId, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .getLatestBlockNumber()
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebGetBlock: async function (taskId, blockNumber, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .getBlock(UTF8ToString(blockNumber))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebGetBlockWithTransactions: async function (taskId, blockNumber, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .getBlockWithTransactions(UTF8ToString(blockNumber))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebGetEmail: async function (taskId, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .getEmail()
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebGetSignerAddress: async function (taskId, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .getSignerAddress()
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebSmartWalletIsDeployed: async function (taskId, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .smartWalletIsDeployed()
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebResolveENSFromAddress: async function (taskId, address, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .resolveENSFromAddress(UTF8ToString(address))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebResolveAddressFromENS: async function (taskId, ens, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .resolveAddressFromENS(UTF8ToString(ens))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_viii(cb, idPtr, buffer, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
  ThirdwebCopyBuffer: async function (taskId, text, cb) {
    // convert taskId from pointer to str and allocate it to keep in memory
    var id = UTF8ToString(taskId);
    var idSize = lengthBytesUTF8(id) + 1;
    var idPtr = _malloc(idSize);
    stringToUTF8(id, idPtr, idSize);
    // execute bridge call
    window.bridge
      .copyBuffer(UTF8ToString(text))
      .then(() => {
        dynCall_viii(cb, idPtr, idPtr, null);
      })
      .catch((err) => {
        var msg = err.message;
        var bufferSize = lengthBytesUTF8(msg) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(msg, buffer, bufferSize);
        dynCall_viii(cb, idPtr, null, buffer);
      });
  },
};

mergeInto(LibraryManager.library, plugin);
