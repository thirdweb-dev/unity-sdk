#import <UIKit/UIKit.h>
#import <SafariServices/SafariServices.h>
#import "UnityAppController.h"

@interface iOSBrowserPlugin : NSObject <SFSafariViewControllerDelegate> {
    SFSafariViewController *safariVC;
}
@end

@implementation iOSBrowserPlugin

- (void)openURL:(NSString *)urlString {
    NSURL *url = [NSURL URLWithString:urlString];
    safariVC = [[SFSafariViewController alloc] initWithURL:url];
    safariVC.delegate = self;

    UIViewController *rootVC = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
    [rootVC presentViewController:safariVC animated:YES completion:nil];
}

- (void)safariViewControllerDidFinish:(SFSafariViewController *)controller {
    [controller dismissViewControllerAnimated:YES completion:nil];
    safariVC = nil;
}

@end

@implementation UnityAppController (URLHandling)

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey, id> *)options {
    UnitySendMessage("EmbeddedWalletUI", "HandleURL", [[url absoluteString] UTF8String]);
    return YES;
}

@end

extern "C" {
    void _OpenURL(const char *url) {
        NSString *urlString = [NSString stringWithUTF8String:url];
        [[[iOSBrowserPlugin alloc] init] openURL:urlString];
    }
}
