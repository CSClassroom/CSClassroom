FROM csclassroom/buildservice.endpoint.base:1.0
ARG source=.
WORKDIR /app
EXPOSE 80
COPY $source .
ENTRYPOINT ["dotnet", "BuildService.Endpoint.dll"]