
# Future State Enterprise Library

A collection of components to accelerate the development of robust, maintainable line of business application on a .NET technology stack.

##  Key components:
- Utility Libraries
- Data Access
- Validation
- ETL Batch
- Data Flow 
- Microservices

## Build Status

[![Build status](https://ci.appveyor.com/api/projects/status/aqh7hjoa5rlgw518?svg=true)](https://ci.appveyor.com/project/ArisNikolaou/futurestate)
  
## Building

This solution relies on [Cake](https://cakebuild.net/) to build, test and publish components. Assembly versioning can be assigned through the command line argument using the following command line parameters.

    ./build.ps1 -Target Publish-Packages -ScriptArgs "-BUILD_VERSION=0.3.0"
  
## [Technical Docs](http://futurestate.readthedocs.io/en/latest/index/)
