{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "{{SolutionName}}.API": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:{{ApiHttpPort}}",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
