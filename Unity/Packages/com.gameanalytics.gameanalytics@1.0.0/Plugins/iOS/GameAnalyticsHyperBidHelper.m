#if gameanalytics_hyperbid_enabled
#import <HyperBidSDK/HyperBidSDK.h>

char* getHyperBidSdkVersion() {
    NSString* version = [[HBAPI sharedInstance].version stringByReplacingOccurrencesOfString:@"HB_" withString:@""];
    const char* string = [version UTF8String];
    if (string == NULL)
        return NULL;

    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}
#endif
