# console-statistics-app-csharp
A console-based application that consume real-time statistics

## Prerequisits

This applications requires the following API Client libraries:

[Authentication Client C#](https://github.com/GenesysPureEngage/authentication-client-csharp)
[Statistics Client C#](https://github.com/GenesysPureEngage/statistics-client-csharp)

## Running

You can run the console sample by passing in command line arguments as shown in this example:<br>
`console-statistics-app-csharp.exe -ApiKey <key> -ClientId <clientId> -BaseUrl <url> -Username <tenant\\username> -Password <p> -AutoLogin <true||false>`

Edit the template.json file to define any statistics that you wish to subscribe to.

## Commands

| Command          | Aliases           | Arguments   | Description |
| -------------    |:-----------------:| ----------: |------------------------------ |
| initialize       | init, i           |             | initialize the API using the arguments provided at startup                      |
| destroy          | logout, l         |             | logout and cleanup                      |
| subscribe        | subscribe, s      |             | subscribe to statistic definitions found in template.json             |
| peek             | peek, p           |             | peek at statistic values for subscribed statistics                      |
| delete           | delete, d         |             | delete all statistics subscriptions                      |
| configuration    | config, conf      |             | print configuration returned by the server |
| exit             |exit, x            |             | logout if necessary then exit                      |
| help             |help, ?            |             | print the list of available commands                      |



<<<<<<< HEAD




=======
>>>>>>> e57ce9683decdf22ec1b45fdbbc4bc4d0c1430c5
