{
  "ProjectRunner": {
    "GitHubOAuthToken": "{{ github_oauth_token }}",
    "ProjectJobResultHost": "https://{{ domain }}"
  },
  "ConnectionStrings": {
    "PostgresDefaultConnection": "User ID=postgres;Password={{ postgres_password }};Host={{ hostvars[groups['linode_group=webapp'][0]].private_ipv4 }};Port=5432;Database=csclassroom;Pooling=true;"
  },
  "ApplicationInsights": {
    "InstrumentationKey": "{{ appinsights_instrumentation_key }}"
  }
}
