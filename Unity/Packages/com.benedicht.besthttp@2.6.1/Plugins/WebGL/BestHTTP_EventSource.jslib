var Lib_BEST_HTTP_WebGL_ES_Bridge =
{
	$es: {
		eventSourceInstances: {},
		nextInstanceId : 1,

		Set : function(event) {
			es.eventSourceInstances[es.nextInstanceId] = event;
			return es.nextInstanceId++;
		},

		Get : function(id) {
			return es.eventSourceInstances[id];
		},

		Remove: function(id) {
			delete es.eventSourceInstances[id];
		},

		_callOnError: function(errCallback, id, reason)
		{
			if (reason)
			{
				var length = lengthBytesUTF8(reason) + 1;
				var buffer = _malloc(length);

				stringToUTF8Array(reason, HEAPU8, buffer, length);

				Module['dynCall_vii'](errCallback, id, buffer);

				_free(buffer);
			}
			else
				Module['dynCall_vii'](errCallback, [id, 0]);
		},

        _GenericEventHandler: function(id, eventName, e, onMessage) {
          function AllocString(str) {
			  if (str != undefined)
			  {
				  var length = lengthBytesUTF8(str) + 1;
				  var buff = _malloc(length);

				  stringToUTF8Array(str, HEAPU8, buff, length);

				  return buff;
			  }

			  return 0;
		  }

		  var eventBuffer = AllocString(eventName);
		  var dataBuffer = AllocString(e.data);
		  var idBuffer = AllocString(e.id);

		  Module['dynCall_viiiii'](onMessage, id, eventBuffer, dataBuffer, idBuffer, e.retry);

		  if (eventBuffer != 0)
			  _free(eventBuffer);

		  if (dataBuffer != 0)
			  _free(dataBuffer);

		  if (idBuffer != 0)
			  _free(idBuffer);
       }
	},

    ES_IsSupported: function() {
      return typeof(EventSource) !== "undefined";
    },

	ES_Create: function(urlPtr, withCredentials, onOpen, onMessage, onError)
	{
		var url = new URL(UTF8ToString(urlPtr)); ///*encodeURI*/(UTF8ToString(urlPtr)).replace(/\+/g, '%2B').replace(/%252[fF]/ig, '%2F');

		var event = {
			onError: onError
		};

		var id = es.nextInstanceId;

		console.log(id + ' ES_Create(' + url + ', ' + withCredentials + ')');

		event.eventImpl = new EventSource(url, { withCredentials: withCredentials != 0 ? true : false } );
        event.onMessage = onMessage;

		event.eventImpl.onopen = function() {
			console.log(id + ' ES_Create - onOpen');

			Module['dynCall_vi'](onOpen, id);
		};

		event.eventImpl.onmessage = function(e) {
          console.log(id + ' on Generic Message');
          es._GenericEventHandler(id, undefined, e, onMessage);
		};

		event.eventImpl.onerror = function(e) {
			console.log(id + ' ES_Create - onError');

			es._callOnError(onError, id, "Unknown Error!");

            if (e.target.readyState === 0)
                event.eventImpl.close();
		};

		return es.Set(event);
	},

    ES_AddEventHandler: function(id, eventNamePtr) {
        var eventName = UTF8ToString(eventNamePtr);

        console.log(id + ' ES_AddEventHandler(' + eventName + ')');

		var event = es.Get(id);

		try
		{
			event.eventImpl.addEventListener(eventName, function(e) {
              console.log(id + ' onEvent('+ eventName + ')');

              es._GenericEventHandler(id, eventName, e, event.onMessage);
            });
		}
		catch(e) {
			es._callOnError(event.eventImpl.onError, id, ' ' + e.name + ': ' + e.message);
		}
    },

	ES_Close: function(id)
	{
		console.log(id + ' ES_Close');

		var event = es.Get(id);

		try
		{
			event.eventImpl.close();
		}
		catch(e) {
			es._callOnError(event.eventImpl.onError, id, ' ' + e.name + ': ' + e.message);
		}
	},

	ES_Release: function(id)
	{
		console.log(id + ' ES_Release');

		es.Remove(id);
	}
};

autoAddDeps(Lib_BEST_HTTP_WebGL_ES_Bridge, '$es');
mergeInto(LibraryManager.library, Lib_BEST_HTTP_WebGL_ES_Bridge);
