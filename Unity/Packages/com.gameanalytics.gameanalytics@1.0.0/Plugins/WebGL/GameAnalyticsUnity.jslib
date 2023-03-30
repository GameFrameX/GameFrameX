var GameAnalyticsUnity = {
    $listener: {
        onRemoteConfigsUpdated: function()
        {
            SendMessage("GameAnalytics", "OnRemoteConfigsUpdated");
        }
    },
    configureAvailableCustomDimensions01: function(list)
    {
        gameanalytics.GameAnalytics.configureAvailableCustomDimensions01(JSON.parse(Pointer_stringify(list)));
    },
    configureAvailableCustomDimensions02: function(list)
    {
        gameanalytics.GameAnalytics.configureAvailableCustomDimensions02(JSON.parse(Pointer_stringify(list)));
    },
    configureAvailableCustomDimensions03: function(list)
    {
        gameanalytics.GameAnalytics.configureAvailableCustomDimensions03(JSON.parse(Pointer_stringify(list)));
    },
    configureAvailableResourceCurrencies: function(list)
    {
        gameanalytics.GameAnalytics.configureAvailableResourceCurrencies(JSON.parse(Pointer_stringify(list)));
    },
    configureAvailableResourceItemTypes: function(list)
    {
        gameanalytics.GameAnalytics.configureAvailableResourceItemTypes(JSON.parse(Pointer_stringify(list)));
    },
    configureSdkGameEngineVersion: function(unitySdkVersion)
    {
        gameanalytics.GameAnalytics.configureSdkGameEngineVersion(Pointer_stringify(unitySdkVersion));
    },
    configureGameEngineVersion: function(unityEngineVersion)
    {
        gameanalytics.GameAnalytics.configureGameEngineVersion(Pointer_stringify(unityEngineVersion));
    },
    configureBuild: function(build)
    {
        gameanalytics.GameAnalytics.configureBuild(Pointer_stringify(build));
    },
    configureUserId: function(userId)
    {
        gameanalytics.GameAnalytics.configureUserId(Pointer_stringify(userId));
    },
    initialize: function(gamekey, gamesecret)
    {
        gameanalytics.GameAnalytics.addRemoteConfigsListener(listener);
        gameanalytics.GameAnalytics.initialize(Pointer_stringify(gamekey), Pointer_stringify(gamesecret));
    },
    setCustomDimension01: function(customDimension)
    {
        gameanalytics.GameAnalytics.setCustomDimension01(Pointer_stringify(customDimension));
    },
    setCustomDimension02: function(customDimension)
    {
        gameanalytics.GameAnalytics.setCustomDimension02(Pointer_stringify(customDimension));
    },
    setCustomDimension03: function(customDimension)
    {
        gameanalytics.GameAnalytics.setCustomDimension03(Pointer_stringify(customDimension));
    },
    setGlobalCustomEventFields: function(customFields)
    {
        gameanalytics.GameAnalytics.setGlobalCustomEventFields(JSON.parse(customFields));
    },
    addBusinessEvent: function(currency, amount, itemType, itemId, cartType, fields, mergeFields)
    {
        var fieldsString = Pointer_stringify(fields);
        fieldsString = fieldsString ? fieldsString : "{}";
        gameanalytics.GameAnalytics.addBusinessEvent(Pointer_stringify(currency), amount, Pointer_stringify(itemType), Pointer_stringify(itemId), Pointer_stringify(cartType), JSON.parse(fieldsString), mergeFields);
    },
    addResourceEvent: function(flowType, currency, amount, itemType, itemId, fields, mergeFields)
    {
        var fieldsString = Pointer_stringify(fields);
        fieldsString = fieldsString ? fieldsString : "{}";
        gameanalytics.GameAnalytics.addResourceEvent(flowType, Pointer_stringify(currency), amount, Pointer_stringify(itemType), Pointer_stringify(itemId), JSON.parse(fieldsString), mergeFields);
    },
    addProgressionEvent: function(progressionStatus, progression01, progression02, progression03, fields, mergeFields)
    {
        var fieldsString = Pointer_stringify(fields);
        fieldsString = fieldsString ? fieldsString : "{}";
        gameanalytics.GameAnalytics.addProgressionEvent(progressionStatus, Pointer_stringify(progression01), Pointer_stringify(progression02), Pointer_stringify(progression03), JSON.parse(fieldsString), mergeFields);
    },
    addProgressionEventWithScore: function(progressionStatus, progression01, progression02, progression03, score, fields, mergeFields)
    {
        var fieldsString = Pointer_stringify(fields);
        fieldsString = fieldsString ? fieldsString : "{}";
        gameanalytics.GameAnalytics.addProgressionEvent(progressionStatus, Pointer_stringify(progression01), Pointer_stringify(progression02), Pointer_stringify(progression03), score, JSON.parse(fieldsString), mergeFields);
    },
    addDesignEvent: function(eventId, fields, mergeFields)
    {
        var fieldsString = Pointer_stringify(fields);
        fieldsString = fieldsString ? fieldsString : "{}";
        gameanalytics.GameAnalytics.addDesignEvent(Pointer_stringify(eventId), JSON.parse(fieldsString), mergeFields);
    },
    addDesignEventWithValue: function(eventId, value, fields, mergeFields)
    {
        var fieldsString = Pointer_stringify(fields);
        fieldsString = fieldsString ? fieldsString : "{}";
        gameanalytics.GameAnalytics.addDesignEvent(Pointer_stringify(eventId), value, JSON.parse(fieldsString), mergeFields);
    },
    addErrorEvent: function(severity, message, fields, mergeFields)
    {
        var fieldsString = Pointer_stringify(fields);
        fieldsString = fieldsString ? fieldsString : "{}";
        gameanalytics.GameAnalytics.addErrorEvent(severity, Pointer_stringify(message), JSON.parse(fieldsString), mergeFields);
    },
    setEnabledInfoLog: function(enabled)
    {
        gameanalytics.GameAnalytics.setEnabledInfoLog(enabled);
    },
    setEnabledVerboseLog: function(enabled)
    {
        gameanalytics.GameAnalytics.setEnabledVerboseLog(enabled);
    },
    setManualSessionHandling: function(enabled)
    {
        gameanalytics.GameAnalytics.setEnabledManualSessionHandling(enabled);
    },
    setEventSubmission: function(enabled)
    {
        gameanalytics.GameAnalytics.setEnabledEventSubmission(enabled);
    },
    startSession: function()
    {
        gameanalytics.GameAnalytics.startSession();
    },
    endSession: function()
    {
        gameanalytics.GameAnalytics.endSession();
    },
    getRemoteConfigsValueAsString: function(key, defaultValue)
    {
        var returnStr = gameanalytics.GameAnalytics.getRemoteConfigsValueAsString(Pointer_stringify(key), Pointer_stringify(defaultValue));
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    },
    isRemoteConfigsReady: function()
    {
        return gameanalytics.GameAnalytics.isRemoteConfigsReady();
    },
    getRemoteConfigsContentAsString: function()
    {
        var returnStr = gameanalytics.GameAnalytics.getRemoteConfigsContentAsString();
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    },
    getABTestingId: function()
    {
        var returnStr = gameanalytics.GameAnalytics.getABTestingId();
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    },
    getABTestingVariantId: function()
    {
        var returnStr = gameanalytics.GameAnalytics.getABTestingVariantId();
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    }
};

autoAddDeps(GameAnalyticsUnity, '$listener');
mergeInto(LibraryManager.library, GameAnalyticsUnity);
