module Endpoints exposing (Endpoint(..), endpointToUrl, getAuthed, getAuthedQuery, imageIdToUrl, imagesDecoder, instructioIdToCover, postAuthed, postAuthedQuery)

import Dict
import Http exposing (header, request)
import ImageList
import Json.Decode as D
import Main exposing (UserRole, roleToNumber)


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
    | NotPostedPlant Int
    | PreparedPlant Int
    | PostPlant Int Float
    | AddPlant
    | EditPlant Int
    | AllOrders Bool
    | SendOrder Int String
    | ReceivedOrder Int
    | SearchUsers
    | AddRole String UserRole
    | RemoveRole String UserRole
    | CreateUser
    | FindInstructions
    | CoverImage Int String
    | CreateInstruction
    | EditInstruction Int
    | GetInstruction Int


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

        PostPlant plantId price ->
            baseUrl ++ "plants/" ++ String.fromInt plantId ++ "/post?price=" ++ String.fromFloat price

        NotPostedPlant id ->
            baseUrl ++ "plants/notposted/" ++ String.fromInt id

        AddPlant ->
            baseUrl ++ "plants/add"

        EditPlant plantId ->
            baseUrl ++ "plants/" ++ String.fromInt plantId ++ "/edit"

        AllOrders onlyMine ->
            let
                mineStr =
                    if onlyMine then
                        "true"

                    else
                        "false"
            in
            baseUrl ++ "orders?onlyMine=" ++ mineStr

        SendOrder orderId ttn ->
            baseUrl ++ "orders/" ++ String.fromInt orderId ++ "/deliver?trackingNumber=" ++ ttn

        ReceivedOrder orderId ->
            baseUrl ++ "orders/" ++ String.fromInt orderId ++ "/delivered"

        SearchUsers ->
            baseUrl ++ "users"

        AddRole login role ->
            baseUrl ++ "users/" ++ login ++ "/add/" ++ (String.fromInt <| roleToNumber role)

        RemoveRole login role ->
            baseUrl ++ "users/" ++ login ++ "/remove/" ++ (String.fromInt <| roleToNumber role)

        CreateUser ->
            baseUrl ++ "users/create"

        FindInstructions ->
            baseUrl ++ "instructions/find"

        CoverImage id token ->
            baseUrl ++ "file/instruction/" ++ String.fromInt id ++ "?token=" ++ token

        CreateInstruction ->
            baseUrl ++ "instructions/create"

        GetInstruction id ->
            baseUrl ++ "instructions/" ++ String.fromInt id

        EditInstruction id ->
            baseUrl ++ "instructions/" ++ String.fromInt id ++ "/edit"


imageIdToUrl : String -> Int -> String
imageIdToUrl token id =
    endpointToUrl <| Image id token


instructioIdToCover : String -> Int -> String
instructioIdToCover token id =
    endpointToUrl <| CoverImage id token


imagesDecoder : String -> List String -> D.Decoder ImageList.Model
imagesDecoder token at =
    let
        baseDecoder =
            imageIdsToModel token
    in
    D.map baseDecoder (D.at at (D.list D.int))


imageIdsToModel : String -> List Int -> ImageList.Model
imageIdsToModel token ids =
    let
        baseList =
            List.map (\id -> ( id, imageIdToUrl token id )) ids
    in
    ImageList.fromDict <| Dict.fromList baseList


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
