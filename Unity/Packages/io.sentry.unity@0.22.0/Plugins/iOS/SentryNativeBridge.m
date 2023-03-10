#import <Sentry/PrivateSentrySDKOnly.h>
#import <Sentry/Sentry.h>

NS_ASSUME_NONNULL_BEGIN

// macOS only
int SentryNativeBridgeLoadLibrary() { return 0; }
void *SentryNativeBridgeOptionsNew() { return nil; }
void SentryNativeBridgeOptionsSetString(void *options, const char *name, const char *value) { }
void SentryNativeBridgeOptionsSetInt(void *options, const char *name, int32_t value) { }
void SentryNativeBridgeStartWithOptions(void *options) { }

int SentryNativeBridgeCrashedLastRun() { return [SentrySDK crashedLastRun] ? 1 : 0; }

void SentryNativeBridgeClose() { [SentrySDK close]; }

void SentryNativeBridgeAddBreadcrumb(
    const char *timestamp, const char *message, const char *type, const char *category, int level)
{
    if (timestamp == NULL && message == NULL && type == NULL && category == NULL) {
        return;
    }

    [SentrySDK configureScope:^(SentryScope *scope) {
        SentryBreadcrumb *breadcrumb = [[SentryBreadcrumb alloc]
            initWithLevel:level
                 category:(category ? [NSString stringWithUTF8String:category] : nil)];

        if (timestamp != NULL) {
            NSDateFormatter *dateFormatter = [[NSDateFormatter alloc] init];
            [dateFormatter setDateFormat:NSCalendarIdentifierISO8601];
            breadcrumb.timestamp =
                [dateFormatter dateFromString:[NSString stringWithUTF8String:timestamp]];
        }

        if (message != NULL) {
            breadcrumb.message = [NSString stringWithUTF8String:message];
        }

        if (type != NULL) {
            breadcrumb.type = [NSString stringWithUTF8String:type];
        }

        [scope addBreadcrumb:breadcrumb];
    }];
}

void SentryNativeBridgeSetExtra(const char *key, const char *value)
{
    if (key == NULL) {
        return;
    }

    [SentrySDK configureScope:^(SentryScope *scope) {
        if (value != NULL) {
            [scope setExtraValue:[NSString stringWithUTF8String:value]
                          forKey:[NSString stringWithUTF8String:key]];
        } else {
            [scope removeExtraForKey:[NSString stringWithUTF8String:key]];
        }
    }];
}

void SentryNativeBridgeSetTag(const char *key, const char *value)
{
    if (key == NULL) {
        return;
    }

    [SentrySDK configureScope:^(SentryScope *scope) {
        if (value != NULL) {
            [scope setTagValue:[NSString stringWithUTF8String:value]
                        forKey:[NSString stringWithUTF8String:key]];
        } else {
            [scope removeTagForKey:[NSString stringWithUTF8String:key]];
        }
    }];
}

void SentryNativeBridgeUnsetTag(const char *key)
{
    if (key == NULL) {
        return;
    }

    [SentrySDK configureScope:^(
        SentryScope *scope) { [scope removeTagForKey:[NSString stringWithUTF8String:key]]; }];
}

void SentryNativeBridgeSetUser(
    const char *email, const char *userId, const char *ipAddress, const char *username)
{
    if (email == NULL && userId == NULL && ipAddress == NULL && username == NULL) {
        return;
    }

    [SentrySDK configureScope:^(SentryScope *scope) {
        SentryUser *user = [[SentryUser alloc] init];

        if (email != NULL) {
            user.email = [NSString stringWithUTF8String:email];
        }

        if (userId != NULL) {
            user.userId = [NSString stringWithUTF8String:userId];
        }

        if (ipAddress != NULL) {
            user.ipAddress = [NSString stringWithUTF8String:ipAddress];
        }

        if (username != NULL) {
            user.username = [NSString stringWithUTF8String:username];
        }

        [scope setUser:user];
    }];
}

void SentryNativeBridgeUnsetUser()
{
    [SentrySDK configureScope:^(SentryScope *scope) { [scope setUser:nil]; }];
}

char *SentryNativeBridgeGetInstallationId()
{
    // Create a null terminated C string on the heap as expected by marshalling.
    // See Tips for iOS in https://docs.unity3d.com/Manual/PluginsForIOS.html
    const char *nsStringUtf8 = [[PrivateSentrySDKOnly installationID] UTF8String];
    size_t len = strlen(nsStringUtf8) + 1;
    char *cString = (char *)malloc(len);
    memcpy(cString, nsStringUtf8, len);
    return cString;
}

inline NSString *_NSStringOrNil(const char *value)
{
    return value ? [NSString stringWithUTF8String:value] : nil;
}

inline NSString *_NSNumberOrNil(int32_t value) { return value == 0 ? nil : @(value); }

inline NSNumber *_NSBoolOrNil(int8_t value)
{
    if (value == 0) {
        return @NO;
    }
    if (value == 1) {
        return @YES;
    }
    return nil;
}

void SentryNativeBridgeWriteScope( // clang-format off
    // // const char *AppStartTime,
    // const char *AppBuildType,
    // // const char *OperatingSystemRawDescription,
    // int DeviceProcessorCount,
    // const char *DeviceCpuDescription,
    // const char *DeviceTimezone,
    // int8_t DeviceSupportsVibration,
    // const char *DeviceName,
    // int8_t DeviceSimulator,
    // const char *DeviceDeviceUniqueIdentifier,
    // const char *DeviceDeviceType,
    // // const char *DeviceModel,
    // // long DeviceMemorySize,
    int32_t GpuId,
    const char *GpuName,
    const char *GpuVendorName,
    int32_t GpuMemorySize,
    const char *GpuNpotSupport,
    const char *GpuVersion,
    const char *GpuApiType,
    int32_t GpuMaxTextureSize,
    int8_t GpuSupportsDrawCallInstancing,
    int8_t GpuSupportsRayTracing,
    int8_t GpuSupportsComputeShaders,
    int8_t GpuSupportsGeometryShaders,
    const char *GpuVendorId,
    int8_t GpuMultiThreadedRendering,
    const char *GpuGraphicsShaderLevel,
    const char *UnityInstallMode,
    const char *UnityTargetFrameRate,
    const char *UnityCopyTextureSupport,
    const char *UnityRenderingThreadingMode
) // clang-format on
{
    // Note: we're using a NSMutableDictionary because it will skip fields with nil values.
    [SentrySDK configureScope:^(SentryScope *scope) {
        NSMutableDictionary *gpu = [[NSMutableDictionary alloc] init];
        gpu[@"id"] = _NSNumberOrNil(GpuId);
        gpu[@"name"] = _NSStringOrNil(GpuName);
        gpu[@"vendor_name"] = _NSStringOrNil(GpuVendorName);
        gpu[@"memory_size"] = _NSNumberOrNil(GpuMemorySize);
        gpu[@"npot_support"] = _NSStringOrNil(GpuNpotSupport);
        gpu[@"version"] = _NSStringOrNil(GpuVersion);
        gpu[@"api_type"] = _NSStringOrNil(GpuApiType);
        gpu[@"max_texture_size"] = _NSNumberOrNil(GpuMaxTextureSize);
        gpu[@"supports_draw_call_instancing"] = _NSBoolOrNil(GpuSupportsDrawCallInstancing);
        gpu[@"supports_ray_tracing"] = _NSBoolOrNil(GpuSupportsRayTracing);
        gpu[@"supports_compute_shaders"] = _NSBoolOrNil(GpuSupportsComputeShaders);
        gpu[@"supports_geometry_shaders"] = _NSBoolOrNil(GpuSupportsGeometryShaders);
        gpu[@"vendor_id"] = _NSStringOrNil(GpuVendorId);
        gpu[@"multi_threaded_rendering"] = _NSBoolOrNil(GpuMultiThreadedRendering);
        gpu[@"graphics_shader_level"] = _NSStringOrNil(GpuGraphicsShaderLevel);
        [scope performSelector:@selector(setContextValue:forKey:) withObject:gpu withObject:@"gpu"];

        NSMutableDictionary *unity = [[NSMutableDictionary alloc] init];
        unity[@"install_mode"] = _NSStringOrNil(UnityInstallMode);
        unity[@"target_frame_rate"] = _NSStringOrNil(UnityTargetFrameRate);
        unity[@"copy_texture_support"] = _NSStringOrNil(UnityCopyTextureSupport);
        unity[@"rendering_threading_mode"] = _NSStringOrNil(UnityRenderingThreadingMode);
        [scope performSelector:@selector(setContextValue:forKey:)
                    withObject:unity
                    withObject:@"unity"];
    }];
}

NS_ASSUME_NONNULL_END
