# Unity: keep names on select sentry-java classes & their methods - we use string-based JNI lookup in our integration.
-keep class io.sentry.Sentry { *; }
-keep class io.sentry.SentryLevel { *; }
-keep class io.sentry.SentryOptions { *; }
-keep class io.sentry.Hub { *; }
-keep class io.sentry.Breadcrumb { *; }
-keep class io.sentry.Scope { *; }
-keep class io.sentry.ScopeCallback { *; }
-keep class io.sentry.protocol.** { *; }
