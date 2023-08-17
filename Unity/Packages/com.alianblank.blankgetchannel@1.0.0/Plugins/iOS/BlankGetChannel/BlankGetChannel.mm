#if defined (__cplusplus)
extern "C" {
#endif
    char * getChannelName(char * channel_name);
#if defined (__cplusplus)
}
#endif
static char *StringToCharArray(NSString *channel_value) {
    const char *charstr = [channel_value UTF8String];
        // alloc
    char *resultChar = (char*)malloc(strlen(charstr)+1);
        // copy
    strcpy(resultChar, charstr);
    return resultChar;
}

char * getChannelName(char * channel_name) {
    
    NSString* channel_name_String=  [NSString stringWithUTF8String:channel_name];
    NSString* File = [[NSBundle mainBundle] pathForResource:@"Info" ofType:@"plist"];
    NSMutableDictionary* dict = [[NSMutableDictionary alloc] initWithContentsOfFile:File];
    NSString * channel_value =  [dict objectForKey:channel_name_String];
    if (channel_value != nil) {
        return StringToCharArray(channel_value);
    }
    channel_value = @"unknown";
    return StringToCharArray(channel_value);
}
