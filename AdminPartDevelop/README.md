# My Visual Studio Project

This is a .NET CORE project developed in Visual Studio.

## Prerequisites

- [.NET SDK 8](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- file AdminPartDevelop\appsettings.json from administrator containing sensitive informations
- DOCKER DESKTOP to run postgresql containers 

## How to Run

1. Clone the repository:
   ```bash
   git clone https://gitlab.fit.cvut.cz/pavlifi3/bachelor-thesis/-/tree/main/AdminPartDevelop
   cd AdminPartDevelop

2. Download create and init scripts 
   ```bash
   Move or download file from source folder adminCreate.sql to home or Desktop(you need to change docker run)
   Move or download file from source folder refereeCreate.sql to home or Desktop(you need to change docker run)
   Move or download file from source folder initCompetitions.sql to home or Desktop(you need to change docker run)

3. Set docker containers with docker desktop or command prompt
   ```bash
   docker run -d --name database-referee-part \
   -v /var/lib/docker/volumes/referee:/var/lib/postgresql/data \
   -e POSTGRES_USER=RefereePart\
   -e POSTGRES_PASSWORD=refereeTest123\
   -e POSTGRES_DB=mydb \
   -v /home/refereeCreate.sql:/docker-entrypoint-initdb.d/refereeCreate.sql\
   postgres

   docker run -d --name database-admin-part \
      -v /var/lib/docker/volumes/admin:/var/lib/postgresql/data \
      -v /home/adminCreate.sql:/docker-entrypoint-initdb.d/adminCreate.sql\
      -v /home/initCompetitions.sql:/docker-entrypoint-initdb.d/02-initCompetitions.sql \

   -e POSTGRES_USER=AdminPart\
   -e POSTGRES_PASSWORD=adminTest123\
   -e POSTGRES_DB=mydb \
   postgres