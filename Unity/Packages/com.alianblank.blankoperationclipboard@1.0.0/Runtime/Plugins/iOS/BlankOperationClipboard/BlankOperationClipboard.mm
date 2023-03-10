#import "BlankOperationClipboard.h"

@implementation BlankOperationClipboard

#if defined (__cplusplus)
extern "C" {
#endif
    char * GetClipBoard(){
        UIPasteboard * pasteboard = [UIPasteboard generalPasteboard];		
		const char * a = [pasteboard.string UTF8String];
		char * b = (char*)malloc(strlen(a)+1);
		strcpy(b, a);
		return b;
    }
    void SetClipBoard(char * text){
        UIPasteboard * pasteboard = [UIPasteboard generalPasteboard];
        pasteboard.string = [NSString stringWithUTF8String:text];
    }
#if defined (__cplusplus)
}
#endif

@end
