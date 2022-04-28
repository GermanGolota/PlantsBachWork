module Endpoints exposing (Endpoint(..), endpointToUrl, getAuthed, postAuthed)

import Http exposing (header, request)


baseUrl : String
baseUrl =
    "https://localhost:5001/"


type Endpoint
    = Login


endpointToUrl : Endpoint -> String
endpointToUrl endpoint =
    case endpoint of
        Login ->
            baseUrl ++ "auth/login"


postAuthed : String -> Endpoint -> Http.Body -> Http.Expect msg -> Maybe Float -> Cmd msg
postAuthed token endpoint body expect timeout =
    baseRequest "POST" token (endpointToUrl endpoint) body expect timeout Nothing


getAuthed : String -> Endpoint -> Http.Expect msg -> Maybe Float -> Cmd msg
getAuthed token endpoint expect timeout =
    baseRequest "GET" token (endpointToUrl endpoint) Http.emptyBody expect timeout Nothing


baseRequest : String -> String -> String -> Http.Body -> Http.Expect msg -> Maybe Float -> Maybe String -> Cmd msg
baseRequest method token url body expect timeout tracker =
    request
        { method = method
        , headers = [ header "Authorization" <| "Bearer " ++ token ]
        , url = url
        , body = body
        , expect = expect
        , timeout = timeout
        , tracker = tracker
        }
