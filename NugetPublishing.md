
# Install Nuget.exe Cli Tool

- Downalod Nuget.exe
- Move to Directory like C:/Tools/
- Add Directory to System Environment Path

# Setup Nuget Account and Api Key

- Create an account on nuget.org
- Create an api key
- Set api key for nuget cli
	- `nuget setapikey oy2da3214c...`

# To Publish Package

- Open command line and cd to .csproj directory

- Create a nuget spec for the project
	- `nuget spec`

- Package the nuget project along with dependencies
	- `nuget pack PROJECT.csproj -IncludeReferencedProjects -Build -Properties Configuration=Release`

- Publish the package
	- `nuget push PROJECT.1.0.0.nupkg -Source https://api.nuget.org/v3/index.json`

---

# Using dotnet.exe (For dotnet standard)

Use Visual Studio for dotnet standard projects because the nuget.exe tool doesn't work right. However, then when using nuget.exe with a referencing project, it doesn't work either.

- Configure the settings VisualStudio/Project > Properties/Package

- Edit the .csproj for the license type
	- `<PackageLicenseExpression>MIT</PackageLicenseExpression>`

- Package the nuget project along with dependencies
	- `dotnet pack -c Release -p:TargetFrameworks=netstandard2.0`

- Publish the package
	- cd to bin/Release folder
	- `nuget push PROJECT.1.0.0.nupkg -Source https://api.nuget.org/v3/index.json`

# Including a Project Reference dotnet standard as a nuget package reference

- Create a nuget spec for the project (or copy from related project) into the dotnet standard project
	- `nuget spec`

- Copy the settings from the .csproj

- This duplicates the information, but it is required to tell nuget that this project should be included as a package dependency, not an internal .dll