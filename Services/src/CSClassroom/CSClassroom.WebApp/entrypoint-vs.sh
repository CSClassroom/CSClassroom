echo $(/sbin/ip route|awk '/default/ { print $3 }') host.docker.internal >> /etc/hosts

tail -f /dev/null
