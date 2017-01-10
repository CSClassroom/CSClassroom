FROM microsoft/aspnetcore:1.1.0
ARG CLRDBG_VERSION=VS2015U2
WORKDIR /clrdbg2
RUN apt-get update && apt-get install -y unzip && rm -rf /var/lib/apt/lists/*
RUN curl -SL https://raw.githubusercontent.com/Microsoft/MIEngine/getclrdbg-release/scripts/GetClrDbg.sh --output GetClrDbg.sh \
    && chmod 700 GetClrDbg.sh \
    && ./GetClrDbg.sh -v $CLRDBG_VERSION -l /clrdbg2 \
    && rm GetClrDbg.sh
ARG source=.
WORKDIR /app
EXPOSE 80
COPY $source .
ENTRYPOINT ["dotnet", "CSClassroom.WebApp.dll"]