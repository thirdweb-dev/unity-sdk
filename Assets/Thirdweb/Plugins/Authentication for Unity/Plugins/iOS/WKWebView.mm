#import <UIKit/UIKit.h>
#import <WebKit/WebKit.h>

#include "Common.h"

extern UIViewController* UnityGetGLViewController();

typedef NS_ENUM(NSUInteger, WKWebViewAuthenticationSessionErrorCode)
{
    WKWebViewAuthenticationSessionErrorCodeNone = 0,
    WKWebViewAuthenticationSessionErrorCodeCancelled = 1,
    WKWebViewAuthenticationSessionErrorCodeOther = 2
};

typedef void (*WKWebViewAuthenticationSessionCompletionCallback)(void* sessionPtr, const char* callbackUrl, int errorCode, const char* errorMessage);

@interface WKWebViewAuthenticationSession : UIViewController<WKNavigationDelegate>
@end

@implementation WKWebViewAuthenticationSession

WKWebView* _webView;
NSURLRequest* _request;
NSString* _callbackUrlScheme;
BOOL _isCompletedSuccessfully;
BOOL _isDisposed;
BOOL _isViewDisappeared;
WKWebViewAuthenticationSessionCompletionCallback _completionCallback;

- (instancetype)initWithURL:(NSURLRequest *)request callbackURLScheme:(nullable NSString *)callbackURLScheme completionCallback:(WKWebViewAuthenticationSessionCompletionCallback)completionCallback
{
    self = [super init];
    if(!self) return nil;
        
    _webView = nil;
    _isCompletedSuccessfully = false;
    _isDisposed = false;
    _isViewDisappeared = false;
    _request = request;
    _callbackUrlScheme = callbackURLScheme;
    _completionCallback = completionCallback;
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    // Non persistent data store - private browsing.
    WKWebViewConfiguration *configuration = [[WKWebViewConfiguration alloc] init];
    configuration.websiteDataStore = [WKWebsiteDataStore nonPersistentDataStore];
    
    _webView = [[WKWebView alloc] initWithFrame:self.view.frame configuration:configuration];
    _webView.allowsLinkPreview = false;
    _webView.navigationDelegate = self;
    _webView.translatesAutoresizingMaskIntoConstraints = false;
    
    [self.view addSubview:_webView];
    
    [NSLayoutConstraint activateConstraints:@[
        [self.view.leadingAnchor constraintEqualToAnchor:_webView.leadingAnchor],
        [self.view.trailingAnchor constraintEqualToAnchor:_webView.trailingAnchor],
        [self.view.topAnchor constraintEqualToAnchor:_webView.topAnchor],
        [self.view.bottomAnchor constraintEqualToAnchor:_webView.bottomAnchor]
    ]];
    
    [_webView loadRequest:_request];
}

- (void)viewDidDisappear:(BOOL)animated
{
    [super viewDidDisappear:animated];
    
    _isViewDisappeared = true;
    
    [self cancel];
}

- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler
{
    NSURL* url = [[navigationAction request] URL];
    if (url != nil)
    {
        //NSLog(@"[WKWebViewAuthenticationSession] decidePolicyForNavigationAction:%@", [url absoluteString]);
        
        NSURLComponents* urlComponents = [NSURLComponents componentsWithString:[url absoluteString]];
        
        if (urlComponents != nil)
        {            
            // If this is the OAuth callback, intercept it and feed it into the completion handler.
            if ([self matchesCallbackUrl:urlComponents])
            {
                NSLog(@"[WKWebViewAuthenticationSession] matchesCallbackUrl:YES");
                
                _isCompletedSuccessfully = true;
                decisionHandler(WKNavigationActionPolicyCancel);
                
                if (_completionCallback != nil)
                {
                    _completionCallback((__bridge void*)self, toString([url absoluteString]), (int)WKWebViewAuthenticationSessionErrorCodeNone, nil);
                }
                
                [self dispose];
                return;
            }
        }
    }
    
    decisionHandler(WKNavigationActionPolicyAllow);
}

- (void)webView:(WKWebView *)webView didFailNavigation:(WKNavigation *)navigation withError:(NSError *)error
{
    [self completeWithError:WKWebViewAuthenticationSessionErrorCodeOther withError:error];
}

- (void)webView:(WKWebView *)webView didFailProvisionalNavigation:(WKNavigation *)navigation withError:(NSError *)error
{
    [self completeWithError:WKWebViewAuthenticationSessionErrorCodeOther withError:error];
}

-(BOOL)matchesCallbackUrl:(NSURLComponents*)urlComponents
{
    if (_callbackUrlScheme != nil and urlComponents != nil)
    {
        return [[urlComponents scheme] isEqualToString:_callbackUrlScheme];
    }
    
    return false;
}

-(void)cancelButtonClicked:(UIButton*)sender
{
    NSLog(@"[WKWebViewAuthenticationSession] Cancelled login by pressing the 'Cancel' button.");
    
    [self cancel];
}

-(BOOL)start
{
    if (_isDisposed)
    {
        NSLog(@"[WKWebViewAuthenticationSession] It was already disposed!");
        return false;
    }
        
    if (_webView != nil)
    {
        NSLog(@"[WKWebViewAuthenticationSession] It was already started!");
        return false;
    }
    
    self.modalPresentationStyle = UIModalPresentationPageSheet;
    
    UIViewController *unityViewController = UnityGetGLViewController();
    UINavigationController *navigationController = [[UINavigationController alloc] initWithRootViewController:self];
    UIBarButtonItem* cancelButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemCancel target:self action:@selector(cancelButtonClicked:)];
    self.navigationItem.rightBarButtonItem = cancelButton;
    [unityViewController presentViewController:navigationController animated:YES completion:nil];
    
    return true;
}

-(void)cancel
{
    NSError* error = [NSError errorWithDomain:@"User cancelled" code:(int)WKWebViewAuthenticationSessionErrorCodeCancelled userInfo:nil];
    
    [self completeWithError:WKWebViewAuthenticationSessionErrorCodeCancelled withError:error];
}

- (void)dispose
{
    if (_isDisposed)
        return;
    
    _isDisposed = true;
    
    UIViewController *unityViewController = UnityGetGLViewController();
    if (unityViewController != nil && !_isViewDisappeared)
    {
        [unityViewController dismissViewControllerAnimated:YES completion:^(){ [self destroyWebView]; }];
    }
    else
    {
        [self destroyWebView];
    }
}

- (void)destroyWebView
{
    WKWebView* webViewTemp = _webView;
    _webView = nil;
    
    if (webViewTemp != nil)
    {
        webViewTemp.UIDelegate = nil;
        webViewTemp.navigationDelegate = nil;
        
        [webViewTemp stopLoading];
        [webViewTemp removeFromSuperview];
    }
}

- (void)completeWithError:(WKWebViewAuthenticationSessionErrorCode)errorCode withError:(NSError*)error
{
    if (!_isCompletedSuccessfully && !_isDisposed)
    {
        if (_completionCallback != nil)
        {
            _completionCallback((__bridge void*)self, nil, (int)errorCode, toString(error.localizedDescription));
        }
        
        [self dispose];
    }
}

@end

extern "C"
{
    WKWebViewAuthenticationSession* Cdm_Auth_WKWebViewAuthenticationSession_Init(const char* urlStr, const char* urlScheme,
        WKWebViewAuthenticationSessionCompletionCallback completionCallback)
    {
        //NSLog(@"[WKWebViewAuthenticationSession:Init] [url:%s] [scheme:%s]", urlStr, urlScheme);
        
        NSURLRequest *request = [NSURLRequest requestWithURL:[NSURL URLWithString: toString(urlStr)]];
        
        WKWebViewAuthenticationSession* session = 
            [[WKWebViewAuthenticationSession alloc] initWithURL:request callbackURLScheme:toString(urlScheme) completionCallback:completionCallback];
        
        return session;
    }
        
    int Cdm_Auth_WKWebViewAuthenticationSession_Start(void* sessionPtr)
    {
        //NSLog(@"[WKWebViewAuthenticationSession:Start]");
        
        if (sessionPtr == NULL)
            return toBool(false);
        
        WKWebViewAuthenticationSession* session = (__bridge WKWebViewAuthenticationSession*) sessionPtr;
        return toBool([session start]);
    }
    
    void Cdm_Auth_WKWebViewAuthenticationSession_Cancel(void* sessionPtr)
    {
        //NSLog(@"[WKWebViewAuthenticationSession:Cancel]");
        
        if (sessionPtr == NULL)
            return;
            
        WKWebViewAuthenticationSession *session = (__bridge WKWebViewAuthenticationSession *)sessionPtr;
        [session cancel];
    }
    
    void Cdm_Auth_WKWebViewAuthenticationSession_Dispose(void* sessionPtr)
    {
        //NSLog(@"[WKWebViewAuthenticationSession:Dispose]");
        
        if (sessionPtr == NULL)
            return;
            
        WKWebViewAuthenticationSession *session = (__bridge WKWebViewAuthenticationSession *)sessionPtr;
        [session dispose];
    }
}
