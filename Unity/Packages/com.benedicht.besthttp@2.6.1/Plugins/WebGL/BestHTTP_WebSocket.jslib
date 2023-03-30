var Lib_BEST_HTTP_WebGL_WS_Bridge =
{
	$ws: {
		webSocketInstances: {},
		nextInstanceId : 1,
        /*UTF8Decoder: new TextDecoder('utf8'),*/

		Set : function(socket) {
			ws.webSocketInstances[ws.nextInstanceId] = socket;
			return ws.nextInstanceId++;
		},

		Get : function(id) {
			return ws.webSocketInstances[id];
		},

		Remove: function(id) {
			delete ws.webSocketInstances[id];
		},

		_callOnClose: function(onClose, id, code, reason)
		{
			var length = lengthBytesUTF8(reason) + 1;
			var buffer = _malloc(length);

			stringToUTF8Array(reason, HEAPU8, buffer, length);

			Module['dynCall_viii'](onClose, id, code, buffer);

			_free(buffer);
		},

		_callOnError: function(errCallback, id, reason)
		{
			var length = lengthBytesUTF8(reason) + 1;
			var buffer = _malloc(length);

			stringToUTF8Array(reason, HEAPU8, buffer, length);

			Module['dynCall_vii'](errCallback, id, buffer);

			_free(buffer);
		}
	},

	WS_Create: function(url, protocol, onOpen, onText, onBinary, onError, onClose)
	{
		var urlStr = new URL(UTF8ToString(url)); ///*encodeURI*/(UTF8ToString(url)).replace(/\+/g, '%2B').replace(/%252[fF]/ig, '%2F');
		var proto = UTF8ToString(protocol);

		console.log('WS_Create(' + urlStr + ', "' + proto + '")');

		var socket = {
			onError: onError,
			onClose: onClose
		};

		if (proto == '')
			socket.socketImpl = new WebSocket(urlStr);
		else
			socket.socketImpl = new WebSocket(urlStr, [proto]);

		var id = ws.nextInstanceId;
		socket.socketImpl.binaryType = "arraybuffer";

		socket.socketImpl.onopen = function(e) {
			console.log(id + ' WS_Create - onOpen');

			Module['dynCall_vi'](onOpen, id);
		};

		socket.socketImpl.onmessage = function (e)
		{
			// Binary?
			if (e.data instanceof ArrayBuffer)
			{
				var byteArray = new Uint8Array(e.data);
				var buffer = _malloc(byteArray.length);
				HEAPU8.set(byteArray, buffer);

				Module['dynCall_viii'](onBinary, id, buffer, byteArray.length);

				_free(buffer);
			}
			else // Text
			{
				var length = lengthBytesUTF8(e.data) + 1;
				var buffer = _malloc(length);

				stringToUTF8Array(e.data, HEAPU8, buffer, length);

				Module['dynCall_vii'](onText, id, buffer);

				_free(buffer);
			}
		};

		socket.socketImpl.onerror = function (e)
		{
			console.log(id + ' WS_Create - onError');

			// Do not call this, onClose will be called with an apropriate error code and reason
			//ws._callOnError(onError, id, "Unknown error.");
		};

		socket.socketImpl.onclose = function (e) {
			console.log(id + ' WS_Create - onClose ' + e.code + ' ' + e.reason);

			//if (e.code != 1000)
			//{
			//	if (e.reason != null && e.reason.length > 0)
			//		ws._callOnError(onError, id, e.reason);
			//	else
			//	{
			//		switch (e.code)
			//		{
			//			case 1001: ws._callOnError(onError, id, "Endpoint going away.");
			//				break;
			//			case 1002: ws._callOnError(onError, id, "Protocol error.");
			//				break;
			//			case 1003: ws._callOnError(onError, id, "Unsupported message.");
			//				break;
			//			case 1005: ws._callOnError(onError, id, "No status.");
			//				break;
			//			case 1006: ws._callOnError(onError, id, "Abnormal disconnection.");
			//				break;
			//			case 1009: ws._callOnError(onError, id, "Data frame too large.");
			//				break;
			//			default: ws._callOnError(onError, id, "Error " + e.code);
			//		}
			//	}
			//}
			//else
				ws._callOnClose(onClose, id, e.code, e.reason);
		};

		return ws.Set(socket);
	},

	WS_GetState: function (id)
	{
		var socket = ws.Get(id);

		if (typeof socket === 'undefined' ||
			socket == null ||
			typeof socket.socketImpl === 'undefined' ||
			socket.socketImpl == null)
			return 3; // closed

		return socket.socketImpl.readyState;
	},

    WS_GetBufferedAmount: function (id)
	{
		var socket = ws.Get(id);
		return socket.socketImpl.bufferedAmount;
	},

	WS_Send_String: function (id, ptr, pos, length)
	{
		var socket = ws.Get(id);

        var startPtr = ptr + pos;
        var endPtr = startPtr + length;

        var UTF8Decoder = new TextDecoder('utf8');
        var str = UTF8Decoder.decode(HEAPU8.subarray ? HEAPU8.subarray(startPtr, endPtr) : new Uint8Array(HEAPU8.slice(startPtr, endPtr)));

		try
		{
			socket.socketImpl.send(str);
		}
		catch(e) {
			ws._callOnError(socket.onError, id, ' ' + e.name + ': ' + e.message);
		}

		return socket.socketImpl.bufferedAmount;
	},

	WS_Send_Binary: function(id, ptr, pos, length)
	{
		var socket = ws.Get(id);

		try
		{
			var buff = HEAPU8.subarray(ptr + pos, ptr + pos + length);
			socket.socketImpl.send(buff /*HEAPU8.buffer.slice(ptr + pos, ptr + pos + length)*/);
		}
		catch(e) {
			ws._callOnError(socket.onError, id, ' ' + e.name + ': ' + e.message);
		}

		return socket.socketImpl.bufferedAmount;
	},

	WS_Close: function (id, code, reason)
	{
		var socket = ws.Get(id);
		var reasonStr = UTF8ToString(reason);

		console.log(id + ' WS_Close(' + code + ', ' + reasonStr + ')');

		socket.socketImpl.close(/*ulong*/code, reasonStr);
	},

	WS_Release: function(id)
	{
		console.log(id + ' WS_Release');

		ws.Remove(id);
	}
};

autoAddDeps(Lib_BEST_HTTP_WebGL_WS_Bridge, '$ws');
mergeInto(LibraryManager.library, Lib_BEST_HTTP_WebGL_WS_Bridge);
