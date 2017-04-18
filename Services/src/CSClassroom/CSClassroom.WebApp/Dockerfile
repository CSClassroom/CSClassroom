FROM csclassroom/csclassroom.webapp.base:1.0
ARG source=.
WORKDIR /app
EXPOSE 80
COPY $source .
ENTRYPOINT ["dotnet", "CSClassroom.WebApp.dll"]
