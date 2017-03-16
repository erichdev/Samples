# Code Samples #

### Inventory Management ###

Inventory management app currently in development. Our inventory department does not have a software in place for inventory management, so I am creating a customized solution. The included files are for the Athlete Sizes feature, which will allow each sports team to upload their athletes' sizes for different clothing items. Included files are the front end HTML and JS (Angular), the API controller, and the data access service.

The CsvService transforms a CSV file that is uploaded from the browser into a DataTable that is further parsed and saved into different DB tables.

### Budget ###

My Money My Future is a financial planning web application currently in beta. It has a budgeting feature that uses the Atrium API to pull users' bank account and transaction data. The API endpoints mirror the Atrium API endpoints. (Note: Some of the DB transaction syncing code was adapted from code written by a developer hired through a "developer as a service" contract. The contract expired and the code still needs to be optimized.)

### Time Clock Manager ###

A system for HR and finance to manage employee time records. Currently the system is an Access database that I'm migrating to be an online web app with a SQL backend. This project uses Breeze, which allows for client caching and change tracking. Breeze uses a _single_ controller to access different tables and abstracts away much of the server side functions. 

ValidatePunches.vb (not written by me) is old VB code that  I translated into C# for the web app. 
