<p align="center">
	<img src="https://i.imgur.com/OibdkEz.png" />
	</br>
	<a href="https://discord.gg/ZJaVXK8">
		<img src="https://img.shields.io/badge/Discord-Support-%237289DA.svg?logo=discord&style=for-the-badge&logoWidth=20&labelColor=0d0d0d" />
	</a>
	<a href="https://ci.appveyor.com/project/Yucked/victoria">
		<img src="https://img.shields.io/appveyor/ci/gruntjs/grunt.svg?label=Appveyor&logo=appveyor&style=for-the-badge&logoWidth=20&labelColor=0d0d0d" />
	</a>
	<a href="https://www.nuget.org/packages/Victoria/">
		<img src="https://img.shields.io/nuget/dt/Victoria.svg?label=Downloads&logo=nuget&style=for-the-badge&logoWidth=20&labelColor=0d0d0d" />
	</a>
  	<a href="http://buymeacoff.ee/Yucked">
		<img src="https://img.shields.io/badge/Buy%20Me%20A-Coffee-%23FF813F.svg?logo=buy-me-a-coffee&style=for-the-badge&logoWidth=20&labelColor=0d0d0d" />
	</a>  
	<p align="center">
	     A Discord.NET wrapper for Lavalink and Frostbyte with add-ons and base library. 
  </p>
</p>

---

## 🌠`What is Victoria?`

In `v2`, `v3`, `v4`, Victoria was a Lavalink wrapper which was simple as Sharplink and code style was similar to DSharpPlus.Lavalink.
Now Victoria is divided in 4 projects and each packages offers something different which is explained below:

- ⚜ `Victoria.Addon`: Contains any extensions or helper classes for Lavalink & Frostbyte. All extension in `v4` are now in Addon package.
- 🧠 `Victoria.Common`: Provides the base classes for track/client/websocket/etc. Lavalink & Frostbyte package depend on Common.
- ❄ `Victoria.Frostbyte`: Serves as an example Client implementation for Frostbyte and also keeps up with any Frostbyte changes.
- 🌋 `Victoria.Lavalink`: Originally known as `Victoria` only. Library for interacting with Lavalink server.

On `Nuget` you will see 5 packages: `Victoria.XYZ` and `Victoria` which contains all the packages above. Each package has it's own `README.md` file provides example usage.
The main package `Victoria`'s version will keep incrementing whereas rest of the packages `Victoria.XYZ` will start from version `1.x.x`.
