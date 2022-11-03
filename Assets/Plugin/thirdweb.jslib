mergeInto(LibraryManager.library, {
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
        dynCall_vii(cb, idPtr, buffer);
      });
  },
  ThirdwebInitialize: function (chain) {
    window.bridge.initialize(UTF8ToString(chain));
  },
});
