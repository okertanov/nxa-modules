##
## Copyright (c) 2021 - Team11. All rights reserved.
##

all: build

build: restore
	dotnet publish -c Release -o ./dist

restore:
	dotnet restore

align-projects: src/Directory.Build.props
	dotnet remove $< reference Neo
	dotnet add $< reference ../neo/src/neo/neo.csproj

clean:
	-@dotnet clean 2>&1 > /dev/null
	-@rm -rf ./dist
	-@rm -rf ./src/NXABlockListener/bin
	-@rm -rf ./src/NXABlockListener/obj
	-@rm -rf ./src/NXAExtendedRpc/bin
	-@rm -rf ./src/NXAExtendedRpc/obj

.PHONY: all build restore align-project clean

.SILENT: clean
