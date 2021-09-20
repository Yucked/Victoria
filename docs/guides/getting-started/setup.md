---
uid: Guides.GettingStarted.Setup
title: 🧰 Setup
---

# 🧰 Setup

## Lavalink Setup
Before we get to programming, we need to download the latest version of Lavalink and Application.yml.
# [GitHub](#tab/tabid-lavagit)
You can get an official release of Lavalink from their GitHub release section.
https://github.com/freyacodes/Lavalink/releases

# [CI Server](#tab/tabid-lavaci)
If you believe Lavalink is misbehaving: tracks not playing, etc then you can check CI releases. Chances are there is a fix out already.
https://ci.fredboat.com/viewLog.html?buildId=lastSuccessful&buildTypeId=Lavalink_Build&tab=artifacts&guest=1

# [Application.yml](#tab/tabid-lavaapp)
Once you've Lavalink downloaded it's time to create a new file `application.yml`. This file holds Lavalink configuration.
https://github.com/freyacodes/Lavalink/blob/master/LavalinkServer/application.yml.example

***

To start Lavalink, open a new shell window where Lavalink is located at and type in: `java -jar Lavalink.jar`.

> [!WARNING]
>  It is recommended to put Lavalink and application.yml in the same directory.  You'd be better off creating a new folder called Lavalink server and place everything in it.  

> [!CAUTION]
> The new version of Lavalink requires you have to JAVA 10+. Download the latest JAVA version that is 10+ at least.

---

## Victoria Setup
Once you've Lavalink up and running, you can then install Victoria from following sources:
# [Nuget](#tab/tabid-ngt)
Major/Minor releases are pushed to Nuget once enough changes are available. Versioning is as follow:
`Major`: Code redesign
`Minor`: Addition/Removal of a method or changing interface structure, etc.
`Patch`: Patch releases are usually quick bug fixes

### Installing VIA Dotnet CLI.
1. Open up a terminal/cmd in the root of your project.
2. Type in `dotnet add package Victoria`
3. `dotnet restore` to restore all packages.

# [MyGet Feed](#tab/tabid-mfr)
Small bug fixes and early preview versions are pushed to MyGet before landing on Nuget.

### nuget.config Alternative
In later .NET Core versions you can specify an `RestoreAdditionalProjectSources>` tag in your project file `(.csproj)`.
```xml
	<PropertyGroup>
		<RestoreAdditionalProjectSources>https://www.myget.org/F/yucked/api/v3/index.json</RestoreAdditionalProjectSources>
	</PropertyGroup>
```

### Adding To Rider Sources
1. Open up your project in JetBrains Rider
2. At the bottom, click on Nuget
3. In Nuget tab window, click on Sources
![Add Source](../../images/add-source.png)

4. In Feeds tab, click on the `+` to add source.
![Feed Dialoag](../../images/feed-dialog.png)

5. Click OK and access early previews and bug fixes.

# [GitHub](#tab/tabid-gthb)
Recently GitHub introduced the `Packages` feature. In `v6` packages will be pushed to GitHub as well. For now, you can either Fork/Clone to add Victoria as a reference in your project.
