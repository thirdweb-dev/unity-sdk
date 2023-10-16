#import <Foundation/Foundation.h>
#import <SafariServices/SafariServices.h>
#import <UIKit/UIKit.h>

@interface iOSBrowser : NSObject <SFSafariViewControllerDelegate>
@end

@implementation iOSBrowser

static UIViewController* GetCurrentViewController() {
    UIWindow *window = [[UIApplication sharedApplication] keyWindow];
    UIViewController *rootViewController = window.rootViewController;

    UIViewController *currentController = rootViewController;
    while (currentController.presentedViewController) {
        currentController = currentController.presentedViewController;
    }
    return currentController;
}

void _OpenURL(const char* url) {
    NSString *urlString = [NSString stringWithUTF8String:url];
    NSURL *nsURL = [NSURL URLWithString:urlString];

    SFSafariViewController *safariViewController = [[SFSafariViewController alloc] initWithURL:nsURL];
    UIViewController *currentViewController = GetCurrentViewController();
    [currentViewController presentViewController:safariViewController animated:YES completion:nil];
}

@end
