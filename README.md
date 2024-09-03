# Archiver

The Archiver application serves to crawl, archive files, and perform audits within SharePoint directories. Initially designed for Insurance, it is versatile enough to be adapted for use with other SharePoint instances.

## Features

- **Hangfire Service**: Facilitates background processing and easy tracking of tasks.
- **Swagger UI**: Provides a convenient interface for API interaction, removing the need for tools like Postman.
- **Crawler**: Traverses SharePoint directories starting from a specified root directory.
- **Microsoft File Date Corrector**: Corrects file creation dates for Microsoft files, which typically display the date they were uploaded to SharePoint rather than their actual creation date.
- **Archiver**: Moves files to a specified root directory, preserving the original folder hierarchy.
- **Audit**: Verifies that files are moved correctly and placed in the intended location.

## Technologies

This project is built with:
- .NET 7.0
- C# 11.0
- MySQL 8.0.35
- Hangfire Core 1.7.36
- Hangfire Jobs Logger 0.2.1
- Z Entity Framework 7.100.0.5 (Latest packages available for a free trial on their [website](https://entityframework-extensions.net/))

## Getting Started

To set up the project:

1. Extract the provided compressed file to your local machine.
2. Initialize a new repository on GitHub or Bitbucket to enable version control.
3. Open the solution file `/code/Insurance.Archiver.sln` in Visual Studio.
4. Set `Insurance.API` as the startup project.
5. Update the connection string in `Insurance.API/appsettings.json`. For development, consider creating `appsettings.Development.json` and exclude it from version control.
6. Use Package Manager Console to run `Update-Database`, setting up the initial database schema.
7. Build the project, which will also download any necessary NuGet packages.
8. Run the application to automatically generate Hangfire tables in your database schema.

URL guide:
- Go to `/swagger` to see the available APIs
- Go to `/hangfire` to see the running process

### Prerequisites

- Visual Studio 2022
- [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [MySQL 8.0.35](https://dev.mysql.com/downloads/mysql/)

## Deployment

For production deployment:

1. Ensure that "ASP.NET Core Runtime 7.0.13" is installed on the server.
2. Execute `dotnet publish -c Release -o ./publish` in the Package Manager Console to prepare a release build.
3. The output will be in the `/code/publish` directory, ready for deployment.

## Support

For further inquiries or support, contact me at [ericjohnadamos@gmail.com](mailto:ericjohnadamos@gmail.com).