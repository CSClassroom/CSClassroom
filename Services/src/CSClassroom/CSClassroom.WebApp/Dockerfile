FROM microsoft/dotnet:1.0.0-core
RUN curl -sSL https://get.docker.com/ | sh
WORKDIR /app
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80
ENTRYPOINT ["dotnet", "CSClassroom.WebApp.dll"]
COPY . /app
