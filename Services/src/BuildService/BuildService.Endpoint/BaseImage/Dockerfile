FROM microsoft/dotnet:2.2-aspnetcore-runtime
RUN curl -sSL https://get.docker.com/ | sh
RUN apt-get update
RUN apt-get install -y unzip && rm -rf /var/lib/apt/lists/*
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
RUN apt install ca-certificates
RUN sed 's|mozilla\/AddTrust_External_Root.crt|#mozilla\/AddTrust_External_Root.crt|g' -i /etc/ca-certificates.conf
RUN update-ca-certificates -f -v
