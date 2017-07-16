echo $(/sbin/ip route|awk '/default/ { print $3 }') dockerhost >> /etc/hosts

tail -f /dev/null
