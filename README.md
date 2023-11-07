# Archiver

This is an application that its primary purpose is to crawl data, archive files and audit within the sharepoint directory. This is exclusive for Surestone but can be used in other Sharepoint directory.

## Features

- Hangfire service (to run the process in the background and track them easily)
- Swagger UI (convenience in calling APIs rather than doing a manual request such as POSTMAN)
- Crawler (read through the entire Sharepoint based from the given root directory)
- Crawler's Microsoft File Date Corrector (Microsoft files has a record of their own file creation date rather than the date that it was uploaded in Sharepoint)
- Archiver (responsible for moving all crawled files to a given root directory with retained folder heirarchy)
- Audit (checker for the moved files, making sure that they are moved in the right place)

## Technologies

Project is created with:
- .NET version: 7.0
- C# version: 11.0
- MySQL version: 8.0.35
- Hangfire core version: 1.7.36
- Hangire jobs logger version: 0.2.1
- Z entity framework: 7.100.0.5 (you can download the latest package on their site for free trial)

## Getting Started

When you received the copy, I probably going to give it to you as a compressed file. The best approach is to store it in a repository (github / bitbucket) for better version management.

To run the project, you just need to open "/code/SureStone.Archiver.sln" file in Visual Studio and do the following:

1. Set "SureStone.API" as the startup project
2. Update SureStone.API/appsettings.json's connection string (as for development, it is better to just create appsettings.Development.json and mark it as ignored files)
3. Open Tools > NuGet Package Manager > Package Manager Console and type "Update-Database"
4. Build the project to automatically download the required packages
5. Run the system to automatically create hangfire tables in the schema


URL guide:
- Go to "/swagger" to see the available APIs
- Go to "/hangfire" to see the running process


### Prerequisites

List of things you need to use the software and how to install them.
- Visual Studio 2022
- .NET version: 7.0 (https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- MySQL version: 8.0.35 (https://dev.mysql.com/downloads/mysql)

## Deployment

If you wish to publish the code in production, make sure that you installed "ASP.NET Core Runtime 7.0.13" on the server.

Open Tools > NuGet Package Manager > Package Manager Console and type "dotnet publish -c Release -o ./publish" will rebuild your system and make a released directory under "/code".

## Support

If you have any further questions to the system, you can email me at ericjohnadamos@gmail.com.
