module Main exposing (..)

import Browser
import Html exposing (Html)
import Json.Decode as D
import Json.Decode.Pipeline exposing (required)


type UserRole
    = Consumer
    | Producer
    | Manager


type alias AuthResponse =
    { token : String
    , roles : List UserRole
    }


type alias ApplicationConfig model msg =
    { init : Maybe AuthResponse -> ( model, Cmd msg )
    , view : model -> Html msg
    , update : msg -> model -> ( model, Cmd msg )
    , subscriptions : model -> Sub msg
    }


baseApplication : ApplicationConfig model msg -> Program D.Value model msg
baseApplication config =
    Browser.element
        { init = init config.init
        , view = config.view
        , update = config.update
        , subscriptions = config.subscriptions
        }


init : (Maybe AuthResponse -> ( model, Cmd msg )) -> D.Value -> ( model, Cmd msg )
init initFunc flags =
    let
        authResp =
            case D.decodeValue submitSuccessDecoder flags of
                Ok res ->
                    Just res

                Err _ ->
                    Nothing
    in
    initFunc authResp


convertRoles roleIds =
    List.map convertRole roleIds


convertRole : Int -> UserRole
convertRole roleId =
    case roleId of
        1 ->
            Consumer

        2 ->
            Producer

        3 ->
            Manager

        _ ->
            Consumer


submitSuccessDecoder : D.Decoder AuthResponse
submitSuccessDecoder =
    D.map2 AuthResponse
        (D.field "token" D.string)
        (D.field "roles" (D.list D.int) |> D.map convertRoles)
