# Network Programming - Laboratory work nr. 1 - Dining Hall server
## The purpose of the laboratory work
The purpose of the laboratory work is to create two APIs that simulate a restaurant workflow. Dining Hall server communicates with Kitchen server and vice versa. 

Dining hall generates *orders* and gives these orders to the **Kitchen** which prepares them and returns prepared orders back to the Dining hall. 

## Link to Kitchen server
[Kitchen](https://github.com/flovik/PR_Kitchen)

## Run app with Docker
To run both servers properly, run the *docker-compose.yml* file where both folders of DiningHall and Kitchen stay. Take care at the context folder selection, context has
to be where the Dockerfile is located. 

> docker-compose build

> docker-compose up


```
version: "3.9"

services:
 dining-hall:
  image: dining-hall
  build:
   context: ./PR_Dining-Hall/PR_Dining-Hall/DiningHall
   dockerfile: Dockerfile
  ports:
   - "8090:80"
 kitchen:
  image: kitchen
  build:
   context: ./PR_Kitchen/PR_Kitchen/Kitchen
   dockerfile: Dockerfile
  ports:
   - "8091:80"
```
