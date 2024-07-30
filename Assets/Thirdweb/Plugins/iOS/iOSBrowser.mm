#import <AuthenticationServices/AuthenticationServices.h>

#include "Common.h"

extern UIViewController* UnityGetGLViewController();

typedef void (*ASWebAuthenticationSessionCompletionCallback)(void* sessionPtr, const char* callbackUrl, int errorCode, const char* errorMessage);

@interface Thirdweb_ASWebAuthenticationSession : NSObject<ASWebAuthenticationPresentationContextProviding>

@property (readonly, nonatomic)ASWebAuthenticationSession* session;

@end

@implementation Thirdweb_ASWebAuthenticationSession

- (instancetype)initWithURL:(NSURL *)URL callbackURLScheme:(nullable NSString *)callbackURLScheme completionCallback:(ASWebAuthenticationSessionCompletionCallback)completionCallback
{
    _session = [[ASWebAuthenticationSession alloc] initWithURL:URL
                                            callbackURLScheme: callbackURLScheme
                                             completionHandler:^(NSURL * _Nullable callbackURL, NSError * _Nullable error)
    {
        if (error != nil)
        {
            NSLog(@"[ASWebAuthenticationSession:CompletionHandler] %@", error.description);
        }
        else
        {
            //NSLog(@"[ASWebAuthenticationSession:CompletionHandler] Callback URL: %@", callbackURL);
        }

        completionCallback((__bridge void*)self, toString(callbackURL.absoluteString), (int)error.code, toString(error.localizedDescription));
    }];
    
    [_session setPresentationContextProvider:self];
    return self;
}

- (nonnull ASPresentationAnchor)presentationAnchorForWebAuthenticationSession:(nonnull ASWebAuthenticationSession *)session
{
    #if __IPHONE_OS_VERSION_MAX_ALLOWED >= 130000 || __TV_OS_VERSION_MAX_ALLOWED >= 130000
        return [[[UIApplication sharedApplication] delegate] window];
    #elif __MAC_OS_X_VERSION_MAX_ALLOWED >= 101500
        return [[NSApplication sharedApplication] mainWindow];
    #else
        return nil;
    #endif
}

@end

extern "C"
{
    Thirdweb_ASWebAuthenticationSession* Thirdweb_ASWebAuthenticationSession_InitWithURL(
        const char* urlStr, const char* urlSchemeStr, ASWebAuthenticationSessionCompletionCallback completionCallback)
    {
        //NSLog(@"[ASWebAuthenticationSession:InitWithURL] initWithURL: %s callbackURLScheme:%s", urlStr, urlSchemeStr);
        
        NSURL* url = [NSURL URLWithString: toString(urlStr)];
        NSString* urlScheme = toString(urlSchemeStr);
        
        Thirdweb_ASWebAuthenticationSession* session = [[Thirdweb_ASWebAuthenticationSession alloc] initWithURL:url
                                                                            callbackURLScheme: urlScheme
                                                                            completionCallback:completionCallback];
        return session;
    }
        
    int Thirdweb_ASWebAuthenticationSession_Start(void* sessionPtr)
    {
        Thirdweb_ASWebAuthenticationSession* session = (__bridge Thirdweb_ASWebAuthenticationSession*) sessionPtr;
        BOOL started = [[session session] start];
        
        //NSLog(@"[ASWebAuthenticationSession:Start]: %s", (started ? "YES" : "NO"));
        
        return toBool(started);
    }
    
    void Thirdweb_ASWebAuthenticationSession_Cancel(void* sessionPtr)
    {
        //NSLog(@"[ASWebAuthenticationSession:Cancel]");
        
        Thirdweb_ASWebAuthenticationSession* session = (__bridge Thirdweb_ASWebAuthenticationSession*) sessionPtr;
        [[session session] cancel];
    }
    
    int Thirdweb_ASWebAuthenticationSession_GetPrefersEphemeralWebBrowserSession(void* sessionPtr)
    {
        Thirdweb_ASWebAuthenticationSession* session = (__bridge Thirdweb_ASWebAuthenticationSession*) sessionPtr;
        return toBool([[session session] prefersEphemeralWebBrowserSession]);
    }
    
    void Thirdweb_ASWebAuthenticationSession_SetPrefersEphemeralWebBrowserSession(void* sessionPtr, int enable)
    {
        Thirdweb_ASWebAuthenticationSession* session = (__bridge Thirdweb_ASWebAuthenticationSession*) sessionPtr;
        [[session session] setPrefersEphemeralWebBrowserSession:toBool(enable)];
    }
}
