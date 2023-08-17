#import <Foundation/Foundation.h>

@interface BlankOperationClipboard : NSObject


#if defined (__cplusplus)
extern "C" {
#endif
    char * GetClipBoard();
    void SetClipBoard(char* text);
#if defined (__cplusplus)
}
#endif

@end
