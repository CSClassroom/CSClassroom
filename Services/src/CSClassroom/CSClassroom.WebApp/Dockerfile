FROM csclassroom/csclassroom.webapp.base:2.2
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .
RUN echo ${source:-obj/Docker/publish}
ENTRYPOINT ["dotnet", "CSClassroom.WebApp.dll"]
