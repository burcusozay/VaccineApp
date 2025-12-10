# Vaccine
<p> .NET CLI (VisualStudio > View > Terminal > cd .\{ProjectName}) </p>
<p>  cd .\Vaccine </p>

## DB Context Paketleri
<p> dotnet add package Microsoft.EntityFrameworkCore (For DataLayer.proj and VaccineApp.API.proj proj both) </p>
<p> dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL </p>
<p> dotnet add package Microsoft.EntityFrameworkCore.Design (For Migration DataLayer.proj and VaccineApp.API.proj both) </p>
<p> dotnet add package Npgsql </p>
<p> dotnet add package Microsoft.EntityFrameworkCore.Relational </p>
<p> dotnet add package Microsoft.EntityFrameworkCore.Tools (migration update-database DataLayer.proj) </p>
<p> dotnet add package Microsoft.Extensions.Configuration.Json </p>
<p> dotnet add package Microsoft.EntityFrameworkCore.Design </p>
<p> dotnet tool install --global dotnet-ef</p>
<p> $env:ASPNETCORE_ENVIRONMENT = "Development"   (for development settings json. arrangement of development enviroment variable)</p>
<hr>

## Auto Mapper
<p> dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection </p>

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

<em><p>Error Info: If you get an error like below, Please use longer SecretKey.</p></em>
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
## Dependency Injection  
<p>dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions</p>

## Fluent Validation
<p>dotnet add package FluentValidation.AspNetCore (ModelView.proj and WebAPI.proj)</p>

## React 
<p>Cmd console > npx create-react-app examapp (If does not work instructions should be done below)</p>
<p>1. npm cache clean –force </p>
<p>2. npm install -g npm@latest </p>
<p>3. npm install -g create-react-app </p>
<p>npm install react-router-dom </p>
<p>npm install react-bootstrap </p>
<p>npm i @material-ui/core --legacy-peer-deps </p>
<p>npm config set legacy-peer-deps true (eğer dependency hatası verirse)  </p>
<p>npm install react-router-dom </p>
<p>npm install axios  </p>
<p>npm install jwt-decode </p>
<p>npm install ajv ajv-keywords (burada hangi paketse)  </p>
<p>npm install @mui/material @emotion/react @emotion/styled </p>
<p>npm install </p>
<p>npm start </p> 
<p>npm ls {package-name} (burası dependency listesini verir)</p>
<p>Eğer uygulama oluşturulduktan sonra dependency hatası alınırsa şu komutlar yapılacak 
<p> - rm node_modules -r -fo </p>
<p> - rm package-lock.json -fo </p>
<p> - npm install react-scripts@latest </p>

## Migration NpgSQL Code First

<strong><p> Eğer migrationdan önce bir hata alınırsa mutlaka bunu çalıştır. </p> </strong>
<p>	dotnet tool install --global dotnet-ef  </p>

<strong><p>Aşağıdaki 2 işlem .snl root dizininde yapılacak </p></strong>
<p>dotnet ef migrations add InitialCodeFirstMigration --context AppDbContext --project VaccineApp.Data/VaccineApp.Data.csproj --startup-project VaccineApp.WebAPI/VaccineApp.WebAPI.csproj </p>

<p>dotnet ef database update --project VaccineApp.Data/VaccineApp.Data.csproj --startup-project VaccineApp.WebAPI/VaccineApp.WebAPI.csproj  </p>

<strong><p>	Eğer Migration silinmesi gerekirse bu işlem yine .snl root dizininde yapılacak. Sonra yukarıdaki işlemleri tekrar yap.  </p></strong>
<p>dotnet ef migrations remove --project VaccineApp.Data/VaccineApp.Data.csproj --startup-project VaccineApp.WebAPI/VaccineApp.WebAPI.csproj  </p>

<strong><p>Eğer aynı csproj içinde migration yapılacaksa bunları yap  </p></strong>
<p>dotnet ef migrations add InitialCodeFirstMigration --context AppDbContext  </p>
<p>dotnet ef database update --context AppDbContext  </p>

<p>Enable-Migrations</p>
<p>add-migration EmptyMigration</p>
<p>update-database</p>

<p> Daha sonra seed işlemi için uygulama ayağa kaldırılır.   </p>

## Migration NpgSQL DB First
<p> Eğer hazırda bir db varsa ve buradan entity modeller yaratılacaksa bu komut kullanılır.  Eğer public adında bir şema yok derse db export import yapmak lazım.  </p>
<p> dotnet tool install --global dotnet-ef 	 </p>
<p> dotnet ef dbcontext scaffold "Host=localhost;Database=vaccinedb;Username=postgres;Password=1234qqq" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Context --context AppDbContext --no-onconfiguring --force –verbose --use-database-names  </p>

