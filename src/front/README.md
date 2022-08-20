## Application description
This application is the frontend application for Plants backend.
It is a React application that embeds Elm within itself. This is done to simplify persistent state managment and to gain access to wider js package-ecosystem.

## Starting the application

# Dev mode:
1) Install dependencies
```bash
yarn install
```
2) Start the application
```bash
yarn start
```
App is running on http://localhost:1234
# Prod mode:
1) Run script
```bash
run_prod.cmd
```
App is running on http://localhost:1235

## Creating typescipts file for Elm page
1) Run:
```bash
yarn ts-interop
```
