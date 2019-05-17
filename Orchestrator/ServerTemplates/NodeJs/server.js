const express = require("express");
const bodyParser = require("body-parser");
const programModule = require("./program");
const { program } = programModule;
const server = express();
const port = process.env.PORT;
server.use(bodyParser.json()); // for parsing application/json
server.use(function(req, res, next) {
  res.header("Access-Control-Allow-Origin", "*");
  res.header(
    "Access-Control-Allow-Headers",
    "Origin, X-Requested-With, Content-Type, Accept"
  );
  next();
});
server.get("/", (request, response) => {
  response.send(
    "Application is Online and feels good. To execute lambda-function, please send POST request to current address. Arguments of request should be sent as JSON body of request like array ['dead', 'alive', {name : 'steve', status : 'dead'}]"
  );
});
server.post("/", (request, response) => {
  response.send(program(request.body));
});
server.listen(port, err => {
  if (err) {
    return console.log("something bad happened", err);
  }
  console.log(`server is listening on ${port}`);
});
