version: '2'
services:
  webapp:
    image: csclassroom/csclassroom.webapp
    restart: always
    mem_limit: {{ webapp_memory_limit }}
    hostname: {{ inventory_hostname }}
    volumes:
      - ~/webapp/appsettings.Environment.json:/app/appsettings.Environment.json
      - /var/log/csclassroom:/var/log/csclassroom
    environment:
      - RollingLogPath=/var/log/csclassroom/log.txt
  nginx:
    image: nginx:1.11.3
    restart: always
    ports:
      - "80:80"
      - "443:443"
      - "127.0.0.1:81:81"
    volumes:
      - ~/webapp/nginx.crt:/etc/nginx/ssl/nginx.crt
      - ~/webapp/nginx.key:/etc/nginx/ssl/nginx.key
      - ~/webapp/nginx.conf:/etc/nginx/nginx.conf
  postgres:
    image: postgres:9.5.4
    restart: always
    ports:
      - "{{ hostvars[inventory_hostname].private_ipv4  }}:5432:5432"
      - "127.0.0.1:5432:5432"
    volumes:
      - /var/lib/postgresql/data:/var/lib/postgresql/data
    environment:
      - POSTGRES_PASSWORD={{ postgres_password  }}

