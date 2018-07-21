# CJ Fan Server Bot Test Plan

This is the main test plan for the CJ Fan Server Bot.

## Structure
Folders will be created for each test suite. A test suite will contain a test strategy (named `TestStrategy.md`) and associated test cases (named `TestCaseN.md`).

## In Scope
- *Modules* - Individual methods and class state.
- *Commands* - Classes inheriting ICmd.
- *Chat Processes* - Features triggered by chat interaction.
- *Database Functions* - SQL functions.
- *Server Flow Features* - Features that directly effect users (does not require direct interaction).

## Out of Scope
- *Dependencies* - External libraries.
- *Discord* - Interactions with Discord.

## Tools
Visual Studio's *.Net Test project*, Windows *Event Log*, and Windows *Services* will be used during testing. Additional tools may be used in conjunction.

## Test Results
Failed tests will be considered bugs. A bug may either be fixed or recorded for later reference as an issue.
