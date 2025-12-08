# Vaccine
<p> cd .\Vaccine </p>

## DB Context Paketleri
<p> dotnet add package Microsoft.EntityFrameworkCore (For Migration.proj and Program.cs proj both) </p>
<p> dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL </p>
<p> dotnet add package Microsoft.EntityFrameworkCore.Design (For Migration DataLayer.proj and Program.cs proj both) </p>
<p> dotnet add package Npgsql </p>
<p> dotnet add package Microsoft.EntityFrameworkCore.Relational </p>
<p> dotnet add package Microsoft.EntityFrameworkCore.Tools (migration update-database DataLayer.proj) </p>
<p> dotnet tool install --global dotnet-ef </p>
<hr>

## Auto Mapper
<p> dotnet add package AutoMapper </p>

## API Swagger
<p> dotnet add package Swashbuckle.AspNetCore </p>
<strong>appSettings.Develeopment.json</strong>
<pre><code class="language-json">
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },"Jwt": {
    "SecretKey": "v@Cc1nE!Pr0jeKt2025_T0ken_S3cretKey_X",
    "Issuer": "BurcuSozayApp",
    "Audience": "BurcuSozay",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Database=vaccinedb;Username=postgres;Password=1234qqq"
  },"Redis": {
    "ConnectionString": "localhost:6379", // adjust for your Redis server info
    "InstanceName": "VaccineCache:"
  },"AllowedOrigins": "http://localhost:3000,https://localhost:44395"
}
</code></pre>

<em><p>Error Info: If you get an error like below. Please use longer SecretKey.</p></em>
<strong><p>IDX10720: Unable to create KeyedHashAlgorithm for algorithm 'HS256', the key size must be greater than: '256' bits, key has '232' bits.</p></strong> 

<strong>lunchSettings.json</strong>
<em><p>To Open WebAPI Swagger Automatically lunchSettings.json this code should be added like this below</p></em>

<pre><code class="language-json">
  "IIS Express": {
   "commandName": "IISExpress",
   "launchBrowser": true,
   "environmentVariables": {
     "ASPNETCORE_ENVIRONMENT": "Development"
   },
   "launchUrl": "swagger"
 }
</code></pre>
