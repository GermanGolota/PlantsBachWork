module Endpoints exposing (Endpoint(..), endpointToUrl, getAuthed, getAuthedQuery, imageIdToUrl, postAuthed, postAuthedQuery)

import Http exposing (header, request)


baseUrl : String
baseUrl =
    "https://localhost:5001/"


type Endpoint
    = Login
    | StatsTotal
    | StatsFinancial
    | Search
    | Dicts
    | Image Int String
    | Post Int
    | OrderPost Int String Int --plantId, city, mailNumber
    | Addresses
    | NotPostedPlants
    | PreparedPlant Int


endpointToUrl : Endpoint -> String
endpointToUrl endpoint =
    case endpoint of
        Login ->
            baseUrl ++ "auth/login"

        StatsTotal ->
            baseUrl ++ "stats/total"

        StatsFinancial ->
            baseUrl ++ "stats/financial"

        Search ->
            baseUrl ++ "search"

        Dicts ->
            baseUrl ++ "info/dicts"

        Image id token ->
            baseUrl ++ "file/plant/" ++ String.fromInt id ++ "?token=" ++ token

        Post plantId ->
            baseUrl ++ "post/" ++ String.fromInt plantId

        OrderPost plantId city mailNumber ->
            baseUrl ++ "post/" ++ String.fromInt plantId ++ "/order" ++ "?city=" ++ city ++ "&mailNumber=" ++ String.fromInt mailNumber

        Addresses ->
            baseUrl ++ "info/addresses"

        NotPostedPlants ->
            baseUrl ++ "plants/notposted"

        PreparedPlant plantId ->
            baseUrl ++ "plants/prepared/" ++ String.fromInt plantId


imageIdToUrl : String -> Int -> String
imageIdToUrl token id =
    endpointToUrl <| Image id token


postAuthed : String -> Endpoint -> Http.Body -> Http.Expect msg -> Maybe Float -> Cmd msg
postAuthed token endpoint body expect timeout =
    baseRequest "POST" token (endpointToUrl endpoint) body expect timeout Nothing


getAuthed : String -> Endpoint -> Http.Expect msg -> Maybe Float -> Cmd msg
getAuthed token endpoint expect timeout =
    baseRequest "GET" token (endpointToUrl endpoint) Http.emptyBody expect timeout Nothing


getAuthedQuery : String -> String -> Endpoint -> Http.Expect msg -> Maybe Float -> Cmd msg
getAuthedQuery query token endpoint expect timeout =
    baseRequest "GET" token (endpointToUrl endpoint ++ query) Http.emptyBody expect timeout Nothing


postAuthedQuery : String -> String -> Endpoint -> Http.Expect msg -> Maybe Float -> Cmd msg
postAuthedQuery query token endpoint expect timeout =
    baseRequest "POST" token (endpointToUrl endpoint ++ query) Http.emptyBody expect timeout Nothing


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
