version: '2'
services:
  buildservice:
    image: csclassroom/buildservice.endpoint
    restart: always
    hostname: {{ inventory_hostname }}
    ports:
      - "{{ hostvars[inventory_hostname].private_ipv4 }}:80:80"
    volumes:
      - ~/buildservice/appsettings.Environment.json:/app/appsettings.Environment.json
      - /var/run/docker.sock:/var/run/docker.sock
      - /tmp/CSClassroom:/tmp/CSClassroom
      - /var/log/csclassroom:/var/log/csclassroom
    environment:
      - RollingLogPath=/var/log/csclassroom/log.txt
    extra_hosts:
      - "buildci318.com:{{ hostvars[groups['linode_group=webapp'][0]].private_ipv4 }}"
