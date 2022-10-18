
![MODUS](https://img.shields.io/badge/MODUS-ASSUMPTION-orange?style=for-the-badge) ![MongoDB](https://img.shields.io/badge/MongoDB-%234ea94b.svg?style=for-the-badge&logo=mongodb&logoColor=white) ![dotnet build status](https://img.shields.io/github/workflow/status/modus-1/order-api/.NET%20Core%20CI?label=.NET%20build&logo=.NET&logoColor=white&style=for-the-badge) ![sonarcloud status](https://img.shields.io/sonar/quality_gate/Modus-1_order-api/main?logo=sonarcloud&server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge)<br /> [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Modus-1_order-api&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Modus-1_order-api) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Modus-1_order-api&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Modus-1_order-api) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Modus-1_order-api&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Modus-1_order-api) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Modus-1_order-api&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Modus-1_order-api) [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Modus-1_order-api&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Modus-1_order-api)

![Modus Logo](Docs/Modus.png)
# Modus Ordering API Service

This microservice is responsible for handling orders for the Modus Assumption™ solution.

## Runtime requirements
 - Docker installation (preferably on a Linux environment)

 - Working MongoDB database for storing complete orders. If no database is specified, orders will not be logged.

## Technical overview
The API service leverages the cross-platform ASP.NET Core vesion 6.0 framework.