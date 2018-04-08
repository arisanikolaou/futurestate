# FutureState

This project's primary goal is to provide a reference architecture for building N-Tier line of business application on .NET technology stack. It provides re-useable components to manage common application challenges involving data processing, data access, validation, service bus integration as well as standing up microservices to drive downstream applications.

The primary design influences have been Domain Driven Design, JHispter, Apache Nifi and Spring. The underlying design philosophy favours simplicity, obviousness in design as well as convention over configuration whenever possible. 

The primary 3rd party libraries used are:

- Autofac
- Dapper and complimentary classes
- X Unit TestStack (BDD)
- Open API (Swagger)
- Kafka Client (Comming Soon)

The role and purpose of the projects in the solution are described below:

## Common Library

The FutureState.Common library exposes several utility style classes to validate object state and define the base contracts to form data access patterns,  manage security, interrogate the meta data of types in an assembly as well as provides several extension classes to make working with strings and numeric data types simpler. This is a library of reuse for the project's main components.

## Specifications Library

This FutureState.Specifications library is an implementation of Domain Driven Design's specification pattern. Object state can be validated by a given specification provider that defines the rules to test the validity of a given object. These 'specification provider' class of objects can be chained and composed to enrich the library of validation services that can be provided by an application. These rules are expected to be used to validate data exchanged through service classes.

## Data Access and Sql Setup

The data access library, FutureState.Data, defines the contracts for a 'unit of work' as well as 'repository' classes. The FutureState.Data.Sql adapts these patterns against the popular Dapper Micro-Orm. 

The role of this component is to provide abstractions over data access that would be useful in optimizing the performance of unit tests and swapping data access from MS-Sql to other relational databases. There are plans to include data access abstraction components over document oriented databases such as MongoDb, AWS DynamoDb as well as Azure Cosmos Db within this package

The FutureState.Data.Setup library provides services to help stand up a MS-Sql Server database using DacPac to help either install an application or run unit integration or scenario tests.

These components serve as an alternative to EntityFramework which is viewed as an excellent library to prototype applications but is not recommended beyond basic usage for business critical apps.

## ETL (TBP)

The ETL library provides basic batch processing services for extracting, loading and transforming data from simple csv and xml files into a target data store. This component exposes classes to extract tabular or document oriented data from a given CSV and/or XML file, transform it via a mapping function and load the validated results into a target data store.

The component would be well suited to embed basic ETL transform tasks into an application where the use of SSIS, Ab Initio or Pentaho would be excessive and would fracture the application architecture. This module would be useful in loading data sets less than a gig in a given batch.

## ETL Flow

The FutureState.Flow library is based on the FutureState.ETL library and aims to allow developers to chain ETL flows in a pipeline and allow these data flows to be configured through simple files. It is inspired by Apache Nifi and expected, at some point, to work along side of it.

The FutureState Flow components provides a basic file based data bus, for all intents and purposes, that is simple to maintain, favours auditability of data flows over performance and can be customized and extended on .NET.

## Microservices (TBP)

The FutureState.Services.Web library implements Web Api, Swagger (Open Api) and provides a framework to control a user's authorization to web services.




