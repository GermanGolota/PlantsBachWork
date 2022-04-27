module Endpoints exposing (loginUrl)


baseUrl : String
baseUrl =
    "https://localhost:5001/"


loginUrl : String
loginUrl =
    baseUrl ++ "auth/login"
