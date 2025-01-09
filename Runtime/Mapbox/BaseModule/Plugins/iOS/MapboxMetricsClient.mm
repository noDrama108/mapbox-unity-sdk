#import "tokenSetter.h"
#import <MapboxCommon/MBXMapboxOptions.h>
#import <MapboxCommon/MBXTelemetryService_Internal.h>
#import <MapboxCommon/MBXEventsService_Internal.h>
#import <MapboxCommon/MBXTurnstileEvent_Internal.h>
#import <MapboxCommon/MBXUserSKUIdentifier_Internal.h>
#import <MapboxCommon/MBXTelemetryUtils_Internal.h>
#import <MapboxCommon/MBXSdkInformation_Internal.h>
#import <MapboxCommon/MBXEventsServerOptions_Internal.h>

void setAccessTokenForToken(const char* token) {
  [MBXMapboxOptions setAccessTokenForToken: [[NSString alloc] initWithCString: token encoding:NSUTF8StringEncoding]];
}

char* getAccessToken() {
    
    const char *nsStringUtf8 = [[MBXMapboxOptions getAccessToken] UTF8String];
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);
    return cString;
}

MBXTelemetryService* getOrCreateTelemetryService() {
    MBXTelemetryService* telemService = [MBXTelemetryService getOrCreate];
    return telemService;
}

void setEventsCollectionStateForEnableCollection(bool state)
{
    [MBXTelemetryUtils setEventsCollectionStateForEnableCollection:state callback:NULL];
}


void sendTurnstileEvent()
{
    MBXSdkInformation *information = [[MBXSdkInformation alloc] initWithName:@"Test" version:@"1.0.0" packageName:@"com.mapbox.test"];
    MBXEventsServerOptions *options = [[MBXEventsServerOptions alloc] initWithSdkInformation:information
                                                                  deferredDeliveryServiceOptions:nil];
    MBXEventsService *service = [MBXEventsService getOrCreateForOptions:options];
    
    MBXTurnstileEvent *turnstile = [[MBXTurnstileEvent alloc] initWithSkuId:MBXUserSKUIdentifierMapsMAUS];
    [service sendTurnstileEventForTurnstileEvent:turnstile callback:^(MBXExpected<NSNull *, MBXEventsServiceError *> * _Nonnull result) {
                // place to check and log result if needed
            }];
}
