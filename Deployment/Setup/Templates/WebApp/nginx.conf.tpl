events
{
  worker_connections  4096;  ## Default: 1024
}

http
{
  server
  {
    listen 443 ssl;

    server_name {{ domain }};
    ssl_certificate /etc/nginx/ssl/nginx.crt;
    ssl_certificate_key /etc/nginx/ssl/nginx.key;
    large_client_header_buffers 8 64k;
    client_max_body_size 20M;

    if ($host = www.$server_name) {
      rewrite ^(.*) https://$server_name$request_uri? permanent;
    }

    location /
    {
      proxy_pass http://webapp;
      proxy_set_header Host $host;
      proxy_set_header X-Real-IP $remote_addr;
      proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
      proxy_set_header X-Forwarded-Proto $scheme;
      proxy_buffer_size 128k;
      proxy_buffers 4 256k;
      proxy_busy_buffers_size 256k;
      proxy_connect_timeout 900;
      proxy_send_timeout 900;
      proxy_read_timeout 900;
      send_timeout 900;
    }
  }

  server {
    listen 80;
    server_name {{ domain }};
    return 301 https://$server_name$request_uri;
  }

  upstream csclassroom-buildservice
  {
    least_conn;
{% for host in groups['linode_group=buildservice'] %}
    server {{ hostvars[host].private_ipv4 }};
{% endfor %}
  }

  server
  {
    listen 81;
    location /
    {
      proxy_pass http://csclassroom-buildservice;
      add_header X-Upstream $upstream_addr always;
    }
  }
}
