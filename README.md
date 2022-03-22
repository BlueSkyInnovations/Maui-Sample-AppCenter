# Maui-Sample-AppCenter
Demonstrates the use of Microsofts AppCenter from within a Microsoft .NET MAUI app 

## Making AppSecrets a secret
*Microsoft AppCenter* needs an AppSecret string per platform.
Acquiring AppSecrets is as easy as going to the settings of your app configured in AppCenter, clicking on the three dots in the top right corner and selecting "Copy app secret".

Although Microsoft itself says that this information is not sensitive in the critical type of meaning, you may want to hide it from your source repository.

This is, when *Mobile.BuildTools* comes into play.

Find out more about Mobile.BuildTools here: https://mobilebuildtools.com/.

### Configuring Mobile.BuildTools

You can simply **Install-Package Mobile.BuildTools** into your .NET MAUI app which - amongst other stuff - adds a *buildtools.json* file to the root of your source folder (at solution level). 

Add a new section called appSettings to it like so:

```json
"appSettings": {
    "MauiSampleAppCenter": [
      {
        "accessibility": "Public",
        "className": "AppSettings",        
        "properties": [
          {
            "name": "AppCenteriOSSecret",
            "type": "String",
            "defaultValue": "null"
          },
          {
            "name": "AppCenterAndroidSecret",
            "type": "String",
            "defaultValue": "null"
          }
        ]
      }
    ]
  }
```
  
This configuration makes *Mobile.BuildTools* create a class called AppSettings, put two properties into it (AppCenteriOSSecret and AppCenterAndroidSecret) and include it in your project at runtime with the respective properties already set to your previously acquired AppSecrets. 

The generated source file is being created under obj\[Debug|Release]\[Target]\Helpers\AppSettings.g.cs which (of course) _should not be included in your repository_.

So where do these values come from now?

### Working with local AppSecrets

When working locally, you can add them as system environment variables by simply using the same names (AppCenterAndroidSecret and AppCenteriOSSecret) and setting these keys' values to their respective AppSecret strings.

*Mobile.BuildTools* will pull these values automagically while building and insert them into the generated class for you.

### Working with a Azure DevOps Build Pipeline

*Mobile.BuildTools* recognizes several build automation tools one of which is *Azure DevOps*.

In Azure DevOps you need to configure a variable group within your Library and name it AppSettings for example. 
Then you have to add two variables for each AppSecret - again: Use the same names (AppCenterAndroidSecret and AppCenteriOSSecret).
Add the previously acquired AppSecrets strings as values to these variables and you are almost done.

So that the *Azure DevOps Build Pipeline* can pipe through the correct value to the build process, you have to make it accessible via a specific prefix.
You can learn more about changing these prefixes to your needs in the Mobile.BuildTools documentation. 
Per default it's "DroidSecret_" and "iOSSecret_".

At the top of your pipeline file add this:

```yml
variables:
  - group: 'AppSettings'
  - name: DroidSecret_AppCenterAndroidSecret
    value: $(AppCenterAndroidSecret)
```

Or when configuring your iOS pipeline:

```yml
variables:
  - group: 'AppSettings'
  - name: iOSSecret_AppCenteriOSSecret
    value: $(AppCenteriOSSecret)
```

That's (almost) it. 
*Mobile.BuildTools* will do the magic for you.

### Known issues

As of writing this, there is a known issue in using *Mobile.BuildTools* in for example .NET MAUI. 
This has something to do with the order in which the system compiles and assembles everything together.
In other words: Your code tries to access the generated helper class before it is generated.

The workaround is adding the following line before the closing Project-tag in your .NET MAUIs' .csproj file:

```xml
<Target Name="MBTHack" BeforeTargets="Secrets" DependsOnTargets="MobileBuildToolsInit" />
```

This workaround is based on a comment by hte author of this package here: https://github.com/dansiegel/Mobile.BuildTools/issues/282#issuecomment-887798684

Also, people reported that using encrypted variables in an Azure DevOps Library might not work with Mobile.BuildTools.

## Initializing AppCenter

Now that you succcessfully hid the AppSecrets from your code, you can finally add AppCenter to your .NET MAUI app.

First, **Install-Package Microsoft.AppCenter.Crashes** to your App for tracking unhandled exceptions.
Then you need to simply call *AppCenter.Start* with a couple of parameters to initialize AppCenter.
Because I like to have all initialization and registration of services in one place, I usually provide this kind of code as extensions to *MauiAppBuilder* and then call into it from within MauiProgram.cs like this:

```csharp
public static partial class ConfigServicesExtensions
{
  public static MauiAppBuilder RegisterAppCenter(this MauiAppBuilder builder)
  {
    var appSecrets = string.Empty;

    if (Guid.TryParse(AppSettings.AppCenteriOSSecret, out Guid iOSSecret))
    {
      appSecrets += $"ios={iOSSecret};";
    }

    if (Guid.TryParse(AppSettings.AppCenterAndroidSecret, out Guid AndroidSecret))
    {
      appSecrets += $"android={AndroidSecret};";
    }

    AppCenter.Start(appSecrets, typeof(Crashes));

    return builder;
  }
}

public static MauiApp CreateMauiApp()
{
  var builder = MauiApp.CreateBuilder();
  builder
    .UseMauiApp<App>()
    .RegisterAppCenter()
    .ConfigureFonts(fonts =>
    {
      fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
    });

  return builder.Build();
}
```

You can read more about this way of organizing your startup process in Luis Matos' awesome blog article here: https://luismts.com/organize-your-net-maui-mauiprogram-startup-file/

## Final words

Now whenever an unhandled exception or crash occurs, AppCenter will get notified and you can see details about it on its diagnostics page.
Of course there is much more and I invite you to study the AppCenter documentation about Crashes and Analytics (which you can also integrate into your .NET MAUI app for analysing your app usage).

You might even want to go one step further and implement the Distribution API for offering in-app updates.




  

