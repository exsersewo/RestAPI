# RestAPI
---
[![Build Status](https://dev.azure.com/exsersewo/RestAPI/_apis/build/status/exsersewo.RestAPI?branchName=master)](https://dev.azure.com/exsersewo/RestAPI/_build/latest?definitionId=3&branchName=master)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/e545f8f6d3d0497199348dc4553c4749)](https://www.codacy.com/gh/exsersewo/RestAPI/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=exsersewo/RestAPI&amp;utm_campaign=Badge_Grade)

A Restful GET-only database agnostic API provider built with ASP.Net & .Net 5.0;
Inspiration from: [project-open-data/db-to-api](https://github.com/project-open-data/db-to-api)

## Supports
---

#### Output
HTML, JSon, XML

#### Databases
###### Currently Supported
MariaDB, MySql, MongoDB, PostgreSQL

###### In Development
Firebird, SQLite, IBM, Informix, MS SQL Server, ODBC/DB2, Oracle, CUBRID, 4D

## Self Hosting
---

###### Prerequisites
* Visual Studio >= 2019
* .Net Version >= 5.0
* (If Docker; Docker & Docker Compose)
#### Development Builds
1. Clone the [dev](https://github.com/exsersewo/RestAPI/tree/dev/) branch
2. Open `/src/RestAPI.sln` within Visual Studio
3. Build the solution
4. Copy `/.env.default` into `output/.env`
3. Follow the steps for Consumer Releases from Step 3. onwards
#### Consumer Releases (non Docker)
1. Download the [latest release](https://github.com/exsersewo/RestAPI/releases/latest)
2. Copy `/.env.default` into `/.env`
3. Run `RestAPI.exe` or `dotnet RestAPI.dll`
4. 🎊 You now have a working instance of RestAPI 🎊
#### Consumer Releases (with Docker)
1. Download the [latest release](https://github.com/exsersewo/RestAPI/releases/latest)
2. Copy `.env.default` into `.env`
3. Configure the `.env` file
4. Run `docker-compose up`
5. 🎊 You now have a working instance of RestAPI 🎊