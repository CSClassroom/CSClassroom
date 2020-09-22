{
  "Authentication": {
    "AzureAd": {
      "AADInstance": "https://login.microsoftonline.com/",
      "CallbackPath": "/signin-oidc",
      "ClientId": "{{ aad_client_id }}"
    }
  },
  "CSClassroom": {
    "ActivationToken": "{{ activation_token }}",
    "HostName": "https://{{ domain }}",
    "EmailAddress": "no-reply@{{ domain }}"
  },
  "GitHub": {
    "OAuthToken": "{{ github_oauth_token }}",
    "WebhookSecret": "{{ github_webhook_secret }}"
  },
  "Postmark": {
    "ApiKey": "{{ postmark_apikey }}"
  },
  "ConnectionStrings": {
    "PostgresDefaultConnection": "User ID=postgres;Password={{ postgres_password }};Host=postgres;Port=5432;Database=csclassroom;Pooling=true;"
  },
  "BuildService": {
    "Host": "http://nginx:81"
  },
  "ApplicationInsights": {
    "InstrumentationKey": "{{ appinsights_instrumentation_key }}"
  }
}

