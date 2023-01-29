module Endpoints exposing (Endpoint(..), endpointToUrl, getAuthed, getAuthedQuery, imageIdToUrl, imagesDecoder, instructioIdToCover, postAuthed, postAuthedQuery, historyUrl)

import Dict
import Http exposing (header, request)
import ImageList
import Json.Decode as D
import Main exposing (UserRole, roleToNumber)
import Utils exposing (decodeId)


baseUrl : String
baseUrl =
    "https://localhost:5001/v2/"


type Endpoint
    = Login
    | StatsTotal
    | StatsFinancial
    | Search
    | Dicts
    | Image String String --id, token
    | Post String
    | OrderPost String String Int --plantId, city, mailNumber
    | Addresses
    | NotPostedPlants
    | NotPostedPlant String
    | PreparedPlant String
    | PostPlant String Float
    | AddPlant
    | EditPlant String
    | AllOrders Bool
    | SendOrder String String
    | ReceivedOrder String
    | SearchUsers
    | AddRole String UserRole
    | RemoveRole String UserRole
    | CreateUser
    | FindInstructions
    | CoverImage String String
    | CreateInstruction
    | EditInstruction String
    | GetInstruction String
    | DeletePost String
    | RejectOrder String
    | ChangePassword
    | History


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
            baseUrl ++ "file/plant/" ++ id ++ "?token=" ++ token

        Post plantId ->
            baseUrl ++ "post/" ++ plantId

        OrderPost plantId city mailNumber ->
            baseUrl ++ "post/" ++ plantId ++ "/order" ++ "?city=" ++ city ++ "&mailNumber=" ++ String.fromInt mailNumber

        Addresses ->
            baseUrl ++ "info/addresses"

        NotPostedPlants ->
            baseUrl ++ "plants/notposted"

        PreparedPlant plantId ->
            baseUrl ++ "plants/prepared/" ++ plantId

        PostPlant plantId price ->
            baseUrl ++ "plants/" ++ plantId ++ "/post?price=" ++ String.fromFloat price

        NotPostedPlant id ->
            baseUrl ++ "plants/notposted/" ++ id

        AddPlant ->
            baseUrl ++ "plants/add"

        EditPlant plantId ->
            baseUrl ++ "plants/" ++ plantId ++ "/edit"

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
            baseUrl ++ "orders/" ++ orderId ++ "/deliver?trackingNumber=" ++ ttn

        ReceivedOrder orderId ->
            baseUrl ++ "orders/" ++ orderId ++ "/delivered"

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
            baseUrl ++ "file/instruction/" ++ id ++ "?token=" ++ token

        CreateInstruction ->
            baseUrl ++ "instructions/create"

        GetInstruction id ->
            baseUrl ++ "instructions/" ++ id

        EditInstruction id ->
            baseUrl ++ "instructions/" ++ id ++ "/edit"

        DeletePost id ->
            baseUrl ++ "post/" ++ id ++ "/delete"

        RejectOrder orderId ->
            baseUrl ++ "orders/" ++ orderId ++ "/reject"

        ChangePassword ->
            baseUrl ++ "users/changePass"

        History ->
            baseUrl ++ "history"


imageIdToUrl : String -> String -> String
imageIdToUrl token id =
    endpointToUrl <| Image id token


instructioIdToCover : String -> String -> String
instructioIdToCover token id =
    endpointToUrl <| CoverImage id token


imagesDecoder : String -> List String -> D.Decoder ImageList.Model
imagesDecoder token at =
    let
        baseDecoder =
            imageIdsToModel token
    in
    D.map baseDecoder (D.at at (D.list decodeId))


imageIdsToModel : String -> List String -> ImageList.Model
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


historyUrl name id =
    "/history/" ++ name ++ "/" ++ id
