# ASP.NET 9 Sample Project

This is an ASP.NET 9 sample project.

## Features

It uses:
- Identity
- Entity Framework Core
- JWT Authorization
- Logging
- OpenWeatherMap API

> **WARNING:** The JWT and OpenWeatherMap API keys are visible and public, so they will be deactivated after **01.05.2025**. After that point, you will need to provide your own API key locally!

## Requirements

- [.Net SDK 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Visual Studio 2022 or higher](https://visualstudio.microsoft.com/downloads/)
- [MS SQL DataBase](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [A smile :) ](https://youtu.be/V1bFr2SWP1I?si=d70I6aEJDIJHk1R8)

## Getting Started

1. **Open the project:** Use the `IdentityJwtWeather.sln` file.
2. **Update the database:** Run `dotnet ef database update` in the Package Manager Console.  
   The connection string is located in `appsettings.json` under `ConnectionStrings:DevelopmentConnection` if you need to change it.
3. **Run the project:** Use the "https" setting (default). A console window should open and allow you to communicate with the server via a browser.
4. **Navigate to the API:** Open your browser and go to [https://localhost:7199/scalar/v1](https://localhost:7199/scalar/v1).
5. **Register a new user:**  
   Go to `User/Register` and create a new user with: "{ "email": "test123@gmail.com", "password": "Test.123" }"
6. **Login:**  
   Navigate to `User/Login`, log in as the user, and save the token you receive.
7. **Seed data:**  
     - Use the token by adding the header:  
   `Authorization: Bearer {token}`
     - Call the endpoint at `SeedData/SeedData` to create 5 power plants and populate their production in 15-minute increments for the last 5 days.
8. **API usage instructions:**  
     - To create a Solar Power Plant, provide a `dateOfInstallation` in the format:  
       `2025-03-31T09:15:52.247Z`
     - To use the `SolarPlantProduction/GetProductionData` endpoint, provide a `timeSpan` in the format:  
       `1.00:00:00` (1 day)
       - `"type": 0` → Actual  
       - `"type": 1` → Forecast  
       - `"granularity": 0` → QuarterHourly  
       - `"granularity": 1` → Hourly
