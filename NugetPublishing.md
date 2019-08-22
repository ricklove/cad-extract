
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
	- `nuget pack PROJECT.csproj -IncludeReferencedProjects`

- Publish the package
	- `nuget push PROJECT.1.0.0.nupkg -Source https://api.nuget.org/v3/index.json`
