# Network Programming - Laboratory work nr. 1 - Dining Hall server
## The purpose of the laboratory work
The purpose of the laboratory work is to create two APIs that simulate a restaurant workflow. Dining Hall server communicates with Kitchen server and vice versa. 

Dining hall generates *orders* and gives these orders to the **Kitchen** which prepares them and returns prepared orders back to the Dining hall. 

## Link to Kitchen server
[Kitchen](https://github.com/flovik/PR_Kitchen)

## Run app with Docker
docker build -t dining-hall .

docker run -p 8080:8080 dining-hall
