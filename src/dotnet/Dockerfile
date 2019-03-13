FROM microsoft/dotnet:2.2-sdk-alpine as builder
WORKDIR /app

# Restore nuget packages
COPY ./DemoApi/*.csproj ./
RUN dotnet restore --runtime linux-musl-x64

# Build
COPY ./DemoApi ./

# If you ever get this error when doing the publish: 
# "error : NETSDK1061: The project was restored using Microsoft.NETCore.App version XYZ, but with current settings, version ABC would be used instead",
# make sure that you've set the TargetLatestRuntimePatch property to true
# in the csproj file. See https://docs.microsoft.com/en-us/dotnet/core/deploying/runtime-patch-selection
# and https://github.com/dotnet/docs/issues/9239 for the details.
RUN dotnet publish -c release --no-restore --runtime linux-musl-x64 -o /published

# Runtime image. The "runtime-deps" image includes the native deps
# required to run a .NET Core app but doesn't include the .NET runtime
# itself. That's what we need to run the self-contained app we just
# built without having to incur the cost of downloading the whole runtime.
FROM microsoft/dotnet:2.2-runtime-deps-alpine

WORKDIR /app
COPY --from=builder /published .
ENTRYPOINT ["./dotnetlogdemo"]