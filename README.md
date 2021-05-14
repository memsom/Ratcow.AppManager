# Ratcow.AppManager
A Xamarin.Android Library for installing and uninstalling Android APK's from Android.

Using this library, you will be able to create code to install and uninstall an APK file.

Caveat: This version currently relies on the legacy file system access. I will clean this up and remove the requirement for direct filesystem access.

## Usage

### Set-up
To initialise the library, add this line to the `MainActivity.OnCreate(..)` Method:

`AppManagerAndroid.Init(this);`

For example:

```csharp

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            RatCow.AppManager.AppManagerAndroid.Init(this);

            LoadApplication(new App());
        }

```

You will also need to add an 'xml' directory under Resources, and create this file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<paths>
  <root-path name="root" path="" />
  <external-path name="external_storage_root" path="." />
  <external-path name="external_storage_download" path="Download" />
</paths>
```

This should be saved as "file_paths.xml'.

In your Android.Manifest.xml, add the following `android:requestLegacyExternalStorage="true"` and a `<provider />` sections to your `<application />`.

```xml
<application android:label="My App" android:theme="@style/MainTheme"
             android:requestLegacyExternalStorage="true" >
   <provider
      android:name="androidx.core.content.FileProvider"
      android:authorities="${applicationId}.provider"
      android:exported="false"
      android:grantUriPermissions="true">
      <meta-data
         android:name="android.support.FILE_PROVIDER_PATHS"
         android:resource="@xml/file_paths"/>
   </provider>
</application>
```

If you want to use plain Http, you will also need to add the following attributes to the `<application />`:

`android:usesCleartextTraffic="true" android:networkSecurityConfig="@xml/network_security_config"`

And also add the following file called 'network_security_config.xml' to the resources/xml directory:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<network-security-config>
  <base-config cleartextTrafficPermitted="true" />
</network-security-config>
```

(To be honest, I have a feeling this file duplicates the `android:usesCleartextTraffic="true"` attribute, but will clear that up when I do my next set of changes..)

### Getting a list of installed apps:

Simply use the following call:

`var listOfInstalledApps = AppManager.Instance.GetIntalledApps();`

This currently returns a `List<InstalledAppData>`. This gived you `AppName`, `PackageName`, `VersionCode` and `VersionName`. If you pass a string, it will do a partial match against that string.

### Installing an APK
There are two methods available for you to use. 

Assuming you have the .APK file on your local file system (and assuming you have the correct file permissions set)

```csharp
var path = "/my/file/temp/location/app.apk";
AppManager.Instance.Install(path);
```

To initialise a download from a URL, you can try the following:

```csharp
var ip = "http://my.url.com/file.apk";
var path = "/my/file/temp/location";
await AppManager.Instance.InstallRemoteFile(ip, path);
```

### Uninstalling an app
This is a lot easier.

```csharp
var packagename = "name.of.a.package";
AppManager.Instance.Uninstall(packagename);
```

The package name is the same value as is found in `InstalledAppData` as `PackageName`.

## License

This is MIT licensed and is open for any usage.

## Code provinence
This code is based on examples I found online. But this specific implementation is my own. The code examples I found were in Kotlin and Java - the C# examples I found were mainly outdated and mostly didn't work.

The code has been tested under Android 10. Your mileage will vary. The next set of changes will clean a lot of the code up and will make a Nuget package that is releasable on Nuget.org.

