# FutureState Web

FutureState Web is a reference project to build micro services with Asp.Net core running on a Kestrel. 

The project integrates swagger, odata, elmah as well as NLog as well as a number of other popular frameworks. Controllers
have dependencies injected by DI containers built by autofac.

Services from this project are expected to data drive web apps as well as reporting consumers.

## Handling exceptions

Unhandled exceptions from web controllers are always logged to file via Nlog

## Key objects

FsControllerBase is a based class exposing web services that can read/write data to generic FutureState
service. The controller exposes an add,remove and getbyid method natively.

## Road map

- Implement generic caching and cache aside pattern
- Implement odata
- Implement dot net new template engine
- Integrate elmah for error logging and reporting