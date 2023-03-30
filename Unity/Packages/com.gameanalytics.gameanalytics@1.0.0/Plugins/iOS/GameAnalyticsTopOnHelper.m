#if gameanalytics_topon_enabled
#import <AnyThinkSDK/AnyThinkSDK.h>

char* getTopOnSdkVersion() {
    NSString* version = [[ATAPI sharedInstance].version stringByReplacingOccurrencesOfString:@"UA_" withString:@""];
    const char* string = [version UTF8String];
    if (string == NULL)
        return NULL;

    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}
#endif
