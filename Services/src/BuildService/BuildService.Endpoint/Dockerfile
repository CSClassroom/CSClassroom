FROM csclassroom/buildservice.endpoint.base:2.2
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "BuildService.Endpoint.dll"]