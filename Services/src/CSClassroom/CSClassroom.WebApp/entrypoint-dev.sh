echo $(/sbin/ip route|awk '/default/ { print $3 }') dockerhost >> /etc/hosts
if [ "$1" = "Debug" ]; then
  dotnet --additionalprobingpath /root/.nuget/packages /app/bin/Debug/netcoreapp1.1/CSClassroom.WebApp.dll &
else
  dotnet CSClassroom.WebApp.dll &
fi

tail -f /dev/null