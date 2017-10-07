FROM microsoft/aspnetcore:2.0
RUN curl -sSL https://get.docker.com/ | sh
RUN apt-get install -y unzip && rm -rf /var/lib/apt/lists/*
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
