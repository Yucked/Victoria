---
uid: Guides.GettingStarted.Setup
title: 🧰 Setup
---

# 🧰 Setup

## Lavalink Setup
Before we get to programming, we need to download the latest version of Lavalink and Application.yml.
# [GitHub](#tab/tabid-lavagit)
You can get an official release of Lavalink from their GitHub release section.
https://github.com/Frederikam/Lavalink/releases

# [CI Server](#tab/tabid-lavaci)
If you believe Lavalink is misbehaving: tracks not playing, etc then you can check CI releases. Chances are there is a fix out already.
https://ci.fredboat.com/viewLog.html?buildId=lastSuccessful&buildTypeId=Lavalink_Build&tab=artifacts&guest=1

# [Application.yml](#tab/tabid-lavaapp)
Once you've Lavalink downloaded it's time to create a new file `application.yml`. This file holds Lavalink configuration.
https://github.com/Frederikam/Lavalink/blob/master/LavalinkServer/application.yml.example

***

To start Lavalink, open a new shell window where Lavalink is located at and type in: `java -jar Lavalink.jar`. \
If you happen to have multiple JAVA versions installed, please make sure the PATH is pointing to the latest version of JAVA.

> [!WARNING]
>  It is recommended to put Lavalink and application.yml in the same directory.  You'd be better off creating a new folder called Lavalink server and place everything in it.  

> [!CAUTION]
> The new version of Lavalink requires you have to JAVA 10+. Download the latest JAVA version that is 10+ at least.

## Victoria Setup
Once you've Lavalink up and running, you can then install Victoria from following sources:
# [Nuget](#tab/tabid-ngt)

# [MyGet Feed Csproj](#tab/tabid-mfcsp)
In later .NET Core versions you can specify an `RestoreAdditionalProjectSources>` tag in your project file `(.csproj)`.
```xml
	<PropertyGroup>
		<RestoreAdditionalProjectSources>https://www.myget.org/F/yucked/api/v3/index.json</RestoreAdditionalProjectSources>
	</PropertyGroup>
```

By no means this shows other packages in nuget package browser but it is used for restoring packages.

# [MyGet Feed VS](#tab/tabid-mfvs)

# [MyGet Feed Rider](#tab/tabid-mfr)
You can add MyGet feed as a source to browse and restore packages.
![Add Source](../../images/add-source.png)
![Feed Dialoag](../../images/feed-dialog.png)

# [GitHub](#tab/tabid-gthb)
Tab content-2-1.