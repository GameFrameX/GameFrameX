#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef void (GameAnalyticsATTListenerNotDetermined)(void);
typedef void (GameAnalyticsATTListenerRestricted)(void);
typedef void (GameAnalyticsATTListenerDenied)(void);
typedef void (GameAnalyticsATTListenerAuthorized)(void);

FOUNDATION_EXPORT void RequestConsentInfoUpdate(
    GameAnalyticsATTListenerNotDetermined gameAnalyticsATTListenerNotDetermined,
    GameAnalyticsATTListenerRestricted gameAnalyticsATTListenerRestricted,
    GameAnalyticsATTListenerDenied gameAnalyticsATTListenerDenied,
    GameAnalyticsATTListenerAuthorized gameAnalyticsATTListenerAuthorized);

NS_ASSUME_NONNULL_END
