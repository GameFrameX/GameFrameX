//
//  GameAnalyticsUnity.m
//  GA-SDK-IOS
//
//  Copyright (c) GameAnalytics. All rights reserved.
//

#import "GameAnalytics.h"

@interface GARemoteConfigsUnityDelegate : NSObject<GARemoteConfigsDelegate>
{
}

- (void) onRemoteConfigsUpdated;

@end

@implementation GARemoteConfigsUnityDelegate

- (void)onRemoteConfigsUpdated {
    UnitySendMessage("GameAnalytics", "OnRemoteConfigsUpdated","");
}

@end

GARemoteConfigsUnityDelegate* ga_rc_delegate = nil;

void configureAvailableCustomDimensions01(const char *list) {
    NSString *list_string = list != NULL ? [NSString stringWithUTF8String:list] : nil;
    NSArray *list_array = nil;
    if (list_string) {
        list_array = [NSJSONSerialization JSONObjectWithData:[list_string dataUsingEncoding:NSUTF8StringEncoding]
                                                     options:kNilOptions
                                                       error:nil];
    }
    [GameAnalytics configureAvailableCustomDimensions01:list_array];
}

void configureAvailableCustomDimensions02(const char *list) {
    NSString *list_string = list != NULL ? [NSString stringWithUTF8String:list] : nil;
    NSArray *list_array = nil;
    if (list_string) {
        list_array = [NSJSONSerialization JSONObjectWithData:[list_string dataUsingEncoding:NSUTF8StringEncoding]
                                                     options:kNilOptions
                                                       error:nil];
    }
    [GameAnalytics configureAvailableCustomDimensions02:list_array];
}

void configureAvailableCustomDimensions03(const char *list) {
    NSString *list_string = list != NULL ? [NSString stringWithUTF8String:list] : nil;
    NSArray *list_array = nil;
    if (list_string) {
        list_array = [NSJSONSerialization JSONObjectWithData:[list_string dataUsingEncoding:NSUTF8StringEncoding]
                                                     options:kNilOptions
                                                       error:nil];
    }
    [GameAnalytics configureAvailableCustomDimensions03:list_array];
}

void configureAvailableResourceCurrencies(const char *list) {
    NSString *list_string = list != NULL ? [NSString stringWithUTF8String:list] : nil;
    NSArray *list_array = nil;
    if (list_string) {
        list_array = [NSJSONSerialization JSONObjectWithData:[list_string dataUsingEncoding:NSUTF8StringEncoding]
                                                     options:kNilOptions
                                                       error:nil];
    }
    [GameAnalytics configureAvailableResourceCurrencies:list_array];
}

void configureAvailableResourceItemTypes(const char *list) {
    NSString *list_string = list != NULL ? [NSString stringWithUTF8String:list] : nil;
    NSArray *list_array = nil;
    if (list_string) {
        list_array = [NSJSONSerialization JSONObjectWithData:[list_string dataUsingEncoding:NSUTF8StringEncoding]
                                                     options:kNilOptions
                                                       error:nil];
    }
    [GameAnalytics configureAvailableResourceItemTypes:list_array];
}

void configureSdkGameEngineVersion(const char *gameEngineSdkVersion) {
    NSString *gameEngineSdkVersionString = gameEngineSdkVersion != NULL ? [NSString stringWithUTF8String:gameEngineSdkVersion] : nil;
    [GameAnalytics configureSdkVersion:gameEngineSdkVersionString];
}

void configureGameEngineVersion(const char *gameEngineVersion) {
    NSString *gameEngineVersionString = gameEngineVersion != NULL ? [NSString stringWithUTF8String:gameEngineVersion] : nil;
    [GameAnalytics configureEngineVersion:gameEngineVersionString];
}

void configureBuild(const char *build) {
    NSString *buildString = build != NULL ? [NSString stringWithUTF8String:build] : nil;
    [GameAnalytics configureBuild:buildString];
}

void configureUserId(const char *userId) {
    NSString *userIdString = userId != NULL ? [NSString stringWithUTF8String:userId] : nil;
    [GameAnalytics configureUserId:userIdString];
}

void configureAutoDetectAppVersion(BOOL flag) {
    [GameAnalytics configureAutoDetectAppVersion:flag];
}

void gaInitialize(const char *gameKey, const char *gameSecret, BOOL nativeErrorReporting) {
    NSString *gameKeyString = gameKey != NULL ? [NSString stringWithUTF8String:gameKey] : nil;
    NSString *gameSecretString = gameSecret != NULL ? [NSString stringWithUTF8String:gameSecret] : nil;

    ga_rc_delegate = [[GARemoteConfigsUnityDelegate alloc] init];
    [GameAnalytics setRemoteConfigsDelegate:ga_rc_delegate];

    [GameAnalytics setEnabledErrorReporting:nativeErrorReporting];
    [GameAnalytics initializeWithGameKey:gameKeyString gameSecret:gameSecretString];
}

void addBusinessEvent(const char *currency, int amount, const char *itemType, const char *itemId, const char *cartType, const char *receipt, const char *fields, BOOL mergeFields) {
    NSString *currencyString = currency != NULL ? [NSString stringWithUTF8String:currency] : nil;
    NSInteger amountInteger = (NSInteger)amount;
    NSString *itemTypeString = itemType != NULL ? [NSString stringWithUTF8String:itemType] : nil;
    NSString *itemIdString = itemId != NULL ? [NSString stringWithUTF8String:itemId] : nil;
    NSString *cartTypeString = cartType != NULL ? [NSString stringWithUTF8String:cartType] : nil;
    NSString *receiptString = receipt != NULL ? [NSString stringWithUTF8String:receipt] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addBusinessEventWithCurrency:currencyString
                                         amount:amountInteger
                                       itemType:itemTypeString
                                         itemId:itemIdString
                                       cartType:cartTypeString
                                        receipt:receiptString
                                   customFields:fields_dict
                                    mergeFields:mergeFields];
}

void addBusinessEventAndAutoFetchReceipt(const char *currency, int amount, const char *itemType, const char *itemId, const char *cartType, const char *fields, BOOL mergeFields) {
    NSString *currencyString = currency != NULL ? [NSString stringWithUTF8String:currency] : nil;
    NSInteger amountInteger = (NSInteger)amount;
    NSString *itemTypeString = itemType != NULL ? [NSString stringWithUTF8String:itemType] : nil;
    NSString *itemIdString = itemId != NULL ? [NSString stringWithUTF8String:itemId] : nil;
    NSString *cartTypeString = cartType != NULL ? [NSString stringWithUTF8String:cartType] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addBusinessEventWithCurrency:currencyString
                                         amount:amountInteger
                                       itemType:itemTypeString
                                         itemId:itemIdString
                                       cartType:cartTypeString
                               autoFetchReceipt:TRUE
                                   customFields:fields_dict
                                    mergeFields:mergeFields];
}

void addResourceEvent(int flowType, const char *currency, float amount, const char *itemType, const char *itemId, const char *fields, BOOL mergeFields) {
    NSString *currencyString = currency != NULL ? [NSString stringWithUTF8String:currency] : nil;
    NSNumber *amountNumber = [NSNumber numberWithFloat:amount];
    NSString *itemTypeString = itemType != NULL ? [NSString stringWithUTF8String:itemType] : nil;
    NSString *itemIdString = itemId != NULL ? [NSString stringWithUTF8String:itemId] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addResourceEventWithFlowType:flowType
                                       currency:currencyString
                                         amount:amountNumber
                                       itemType:itemTypeString
                                         itemId:itemIdString
                                   customFields:fields_dict
                                    mergeFields:mergeFields];
}

void addProgressionEvent(int progressionStatus, const char *progression01, const char *progression02, const char *progression03, const char *fields, BOOL mergeFields) {
    NSString *progression01String = progression01 != NULL ? [NSString stringWithUTF8String:progression01] : nil;
    NSString *progression02String = progression02 != NULL ? [NSString stringWithUTF8String:progression02] : nil;
    NSString *progression03String = progression03 != NULL ? [NSString stringWithUTF8String:progression03] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addProgressionEventWithProgressionStatus:progressionStatus
                                              progression01:progression01String
                                              progression02:progression02String
                                              progression03:progression03String
                                               customFields:fields_dict
                                                mergeFields:mergeFields];
}

void addProgressionEventWithScore(int progressionStatus, const char *progression01, const char *progression02, const char *progression03, int score, const char *fields, BOOL mergeFields) {
    NSString *progression01String = progression01 != NULL ? [NSString stringWithUTF8String:progression01] : nil;
    NSString *progression02String = progression02 != NULL ? [NSString stringWithUTF8String:progression02] : nil;
    NSString *progression03String = progression03 != NULL ? [NSString stringWithUTF8String:progression03] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addProgressionEventWithProgressionStatus:progressionStatus
                                              progression01:progression01String
                                              progression02:progression02String
                                              progression03:progression03String
                                                      score:score
                                               customFields:fields_dict
                                                mergeFields:mergeFields];
}

void addDesignEvent(const char *eventId, const char *fields, BOOL mergeFields) {
    NSString *eventIdString = eventId != NULL ? [NSString stringWithUTF8String:eventId] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addDesignEventWithEventId:eventIdString value:nil customFields:fields_dict mergeFields:mergeFields];
}

void addDesignEventWithValue(const char *eventId, float value, const char *fields, BOOL mergeFields) {
    NSString *eventIdString = eventId != NULL ? [NSString stringWithUTF8String:eventId] : nil;
    NSNumber *valueNumber = [NSNumber numberWithFloat:value];
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addDesignEventWithEventId:eventIdString value:valueNumber customFields:fields_dict mergeFields:mergeFields];
}

void addErrorEvent(int severity, const char *message, const char *fields, BOOL mergeFields) {
    NSString *messageString = message != NULL ? [NSString stringWithUTF8String:message] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addErrorEventWithSeverity:severity message:messageString customFields:fields_dict mergeFields:mergeFields];
}

void addAdEventWithDuration(int adAction, int adType, const char *adSdkName, const char *adPlacement, long duration, const char *fields, BOOL mergeFields) {
    NSString *adSdkNameString = adSdkName != NULL ? [NSString stringWithUTF8String:adSdkName] : nil;
    NSString *adPlacementString = adPlacement != NULL ? [NSString stringWithUTF8String:adPlacement] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addAdEventWithAction:adAction
                                 adType:adType
                              adSdkName:adSdkNameString
                            adPlacement:adPlacementString
                               duration:duration
                           customFields:fields_dict
                            mergeFields:mergeFields];
}

void addAdEventWithReason(int adAction, int adType, const char *adSdkName, const char *adPlacement, int noAdReason, const char *fields, BOOL mergeFields) {
    NSString *adSdkNameString = adSdkName != NULL ? [NSString stringWithUTF8String:adSdkName] : nil;
    NSString *adPlacementString = adPlacement != NULL ? [NSString stringWithUTF8String:adPlacement] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addAdEventWithAction:adAction
                                 adType:adType
                              adSdkName:adSdkNameString
                            adPlacement:adPlacementString
                             noAdReason:noAdReason
                           customFields:fields_dict
                            mergeFields:mergeFields];
}

void addAdEvent(int adAction, int adType, const char *adSdkName, const char *adPlacement, const char *fields, BOOL mergeFields) {
    NSString *adSdkNameString = adSdkName != NULL ? [NSString stringWithUTF8String:adSdkName] : nil;
    NSString *adPlacementString = adPlacement != NULL ? [NSString stringWithUTF8String:adPlacement] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    [GameAnalytics addAdEventWithAction:adAction
                                 adType:adType
                              adSdkName:adSdkNameString
                            adPlacement:adPlacementString
                           customFields:fields_dict
                            mergeFields:mergeFields];
}

void addImpressionEvent(const char* adNetworkName, const char* adNetworkVersion, const char *json, const char *fields, BOOL mergeFields) {
    NSString *jsonString = json != NULL ? [NSString stringWithUTF8String:json] : nil;
    NSString *adNetworkNameString = adNetworkName != NULL ? [NSString stringWithUTF8String:adNetworkName] : nil;
    NSString *adNetworkVersionString = adNetworkVersion != NULL ? [NSString stringWithUTF8String:adNetworkVersion] : nil;
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }

    if(jsonString != nil && adNetworkNameString != nil && adNetworkVersionString != nil) {
        NSData *jsonData = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
        NSError *error;

        NSDictionary *impressionData = [NSJSONSerialization JSONObjectWithData:jsonData options:0 error:&error];
        if (error != nil) {
            return;
        }

        [GameAnalytics addImpressionEventWithAdNetworkName:adNetworkNameString adNetworkVersion:adNetworkVersionString impressionData:impressionData customFields:fields_dict mergeFields:mergeFields];
    }
}

void setEnabledInfoLog(BOOL flag) {
    [GameAnalytics setEnabledInfoLog:flag];
}

void setEnabledVerboseLog(BOOL flag) {
    [GameAnalytics setEnabledVerboseLog:flag];
}

void setEnabledWarningLog(BOOL flag) {
    [GameAnalytics setEnabledWarningLog:flag];
}

void setManualSessionHandling(BOOL flag) {
    [GameAnalytics setEnabledManualSessionHandling:flag];
}

void setEventSubmission(BOOL flag) {
    [GameAnalytics setEnabledEventSubmission:flag];
}

void gameAnalyticsStartSession() {
    [GameAnalytics startSession];
}

void gameAnalyticsEndSession() {
    [GameAnalytics endSession];
}

void setCustomDimension01(const char *customDimension) {
    NSString *customDimensionString = customDimension != NULL ? [NSString stringWithUTF8String:customDimension] : nil;
    [GameAnalytics setCustomDimension01:customDimensionString];
}

void setCustomDimension02(const char *customDimension) {
    NSString *customDimensionString = customDimension != NULL ? [NSString stringWithUTF8String:customDimension] : nil;
    [GameAnalytics setCustomDimension02:customDimensionString];
}

void setCustomDimension03(const char *customDimension) {
    NSString *customDimensionString = customDimension != NULL ? [NSString stringWithUTF8String:customDimension] : nil;
    [GameAnalytics setCustomDimension03:customDimensionString];
}

void setGlobalCustomEventFields(const char *fields) {
    NSString *fieldsString = fields != NULL ? [NSString stringWithUTF8String:fields] : nil;
    NSDictionary *fields_dict = nil;
    if (fieldsString) {
        fields_dict = [NSJSONSerialization JSONObjectWithData:[fieldsString dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }
    [GameAnalytics setGlobalCustomEventFields:fields_dict];
}

char* cStringCopy(const char* string)
{
    if (string == NULL)
        return NULL;

    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);

    return res;
}

char* getRemoteConfigsValueAsString(const char *key, const char *defaultValue) {
    NSString *keyString = key != NULL ? [NSString stringWithUTF8String:key] : nil;
    NSString *defaultValueString = defaultValue != NULL ? [NSString stringWithUTF8String:defaultValue] : nil;
    NSString *result = [GameAnalytics getRemoteConfigsValueAsString:keyString defaultValue:defaultValueString];

    return cStringCopy([result UTF8String]);
}

BOOL isRemoteConfigsReady() {
    return [GameAnalytics isRemoteConfigsReady];
}

char* getRemoteConfigsContentAsString() {
    NSString *result = [GameAnalytics getRemoteConfigsContentAsString];
    return cStringCopy([result UTF8String]);
}

char* getABTestingId() {
    NSString *result = [GameAnalytics getABTestingId];
    return cStringCopy([result UTF8String]);
}

char* getABTestingVariantId() {
    NSString *result = [GameAnalytics getABTestingVariantId];
    return cStringCopy([result UTF8String]);
}

void startTimer(const char *key) {
    NSString *keyString = key != NULL ? [NSString stringWithUTF8String:key] : nil;
    [GameAnalytics startTimer:keyString];
}

void pauseTimer(const char *key) {
    NSString *keyString = key != NULL ? [NSString stringWithUTF8String:key] : nil;
    [GameAnalytics pauseTimer:keyString];
}

void resumeTimer(const char *key) {
    NSString *keyString = key != NULL ? [NSString stringWithUTF8String:key] : nil;
    [GameAnalytics resumeTimer:keyString];
}

long stopTimer(const char *key) {
    NSString *keyString = key != NULL ? [NSString stringWithUTF8String:key] : nil;
    return [GameAnalytics stopTimer:keyString];
}
