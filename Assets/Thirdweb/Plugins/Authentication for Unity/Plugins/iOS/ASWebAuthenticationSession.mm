#import <AuthenticationServices/AuthenticationServices.h>

#include "Common.h"

extern UIViewController* UnityGetGLViewController();

typedef void (*ASWebAuthenticationSessionCompletionCallback)(void* sessionPtr, const char* callbackUrl, int errorCode, const char* errorMessage);

@interface Cdm_ASWebAuthenticationSession : NSObject<ASWebAuthenticationPresentationContextProviding>

@property (readonly, nonatomic)ASWebAuthenticationSession* session;

@end

@implementation Cdm_ASWebAuthenticationSession

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
    Cdm_ASWebAuthenticationSession* Cdm_Auth_ASWebAuthenticationSession_InitWithURL(
        const char* urlStr, const char* urlSchemeStr, ASWebAuthenticationSessionCompletionCallback completionCallback)
    {
        //NSLog(@"[ASWebAuthenticationSession:InitWithURL] initWithURL: %s callbackURLScheme:%s", urlStr, urlSchemeStr);
        
        NSURL* url = [NSURL URLWithString: toString(urlStr)];
        NSString* urlScheme = toString(urlSchemeStr);
        
        Cdm_ASWebAuthenticationSession* session = [[Cdm_ASWebAuthenticationSession alloc] initWithURL:url
                                                                            callbackURLScheme: urlScheme
                                                                            completionCallback:completionCallback];
        return session;
    }
        
    // Starts a web authentication session.
    // https://developer.apple.com/documentation/authenticationservices/aswebauthenticationsession/2990953-start?language=objc
    int Cdm_Auth_ASWebAuthenticationSession_Start(void* sessionPtr)
    {
        Cdm_ASWebAuthenticationSession* session = (__bridge Cdm_ASWebAuthenticationSession*) sessionPtr;
        BOOL started = [[session session] start];
        
        //NSLog(@"[ASWebAuthenticationSession:Start]: %s", (started ? "YES" : "NO"));
        
        return toBool(started);
    }
    
    // Cancels a web authentication session.
    // https://developer.apple.com/documentation/authenticationservices/aswebauthenticationsession/2990951-cancel?language=objc
    void Cdm_Auth_ASWebAuthenticationSession_Cancel(void* sessionPtr)
    {
        //NSLog(@"[ASWebAuthenticationSession:Cancel]");
        
        Cdm_ASWebAuthenticationSession* session = (__bridge Cdm_ASWebAuthenticationSession*) sessionPtr;
        [[session session] cancel];
    }
    
    int Cdm_Auth_ASWebAuthenticationSession_GetPrefersEphemeralWebBrowserSession(void* sessionPtr)
    {
        Cdm_ASWebAuthenticationSession* session = (__bridge Cdm_ASWebAuthenticationSession*) sessionPtr;
        return toBool([[session session] prefersEphemeralWebBrowserSession]);
    }
    
    void Cdm_Auth_ASWebAuthenticationSession_SetPrefersEphemeralWebBrowserSession(void* sessionPtr, int enable)
    {
        Cdm_ASWebAuthenticationSession* session = (__bridge Cdm_ASWebAuthenticationSession*) sessionPtr;
        [[session session] setPrefersEphemeralWebBrowserSession:toBool(enable)];
    }
}
