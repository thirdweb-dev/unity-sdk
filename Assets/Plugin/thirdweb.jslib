mergeInto(LibraryManager.library, {
  ThirdwebInvoke: function (route, payload, cb) {
    window.bridge
      .invoke(UTF8ToString(route), UTF8ToString(payload))
      .then((returnStr) => {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        dynCall_vi(cb, buffer);
      });
  },
});
