# FutureState Batch

## About

The FutureState Batch component library provides services to help developers quickly, and easily add ETL program logic to their applications.

The component is implemented in the classes in the FutureState.Batch library.

## Overview

This component is suitable for batch jobs that can complete in process and don't require map/reduce architectures like Apache Spark to process the work. 

It is design to process data from an incoming data store, such as a Csv file, and Xml document or other data store, validate the incoming data and map it to data structures appropriate for a target data store.

All objects read are expected to be mapped to a given DTO (data transfer object), which is validated against a set of rules and then loaded into a session state object to be batch loaded into a given target. The typical target would be a service that would bulk update a database, or another file.

Rules are defines as a set of Specifications using the FutureState Specifications library which largely relies on the rules that are declaratively defined on the dto being read from a given data store.

One of the limitations of this library is that the data source must be whole capable of loading valid data to a given datastore. In the event that multiple data sources must be read to load data then a custom data extractor should be created to aggregate and merge data. If a target to load data from is a

## Quick start

To be provided:

## Error report

Errors reports either parsing incoming data, mapping incoming dtos to target data structures or commit data to a given target are all eventually logged to file via NLog.

## Objectives and motivation

Several business applications perform bulk processing on a daily basis. The batch data loads are often handled platforms such as SSIS. SSIS, however, will never be as sophisticated as a platform as .NET to build and maintain simple batch processing logic and will not fit naturally within a .NET's technology stack. The code will have to be maintained differently, and often separatedly, from your application while will make development less agile.

SSIS, and other platform such as Ab Initio are an excessive solution to relatively straightfoward problems that are best defined and maintained in code.

## Road Map

- Offer data readers for sql server and odbc as well as Odata
- Affer framework to aggregate a dto from multiple data sources
- Extend unit and componet unit testing