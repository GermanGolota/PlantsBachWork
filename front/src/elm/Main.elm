module Main exposing (..)

import Browser
import Html exposing (Html)
import Json.Decode as D


type alias ApplicationConfig model msg =
    { init : String -> ( model, Cmd msg )
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


init : (String -> ( model, Cmd msg )) -> D.Value -> ( model, Cmd msg )
init initFunc flags =
    let
        authToken =
            case D.decodeValue flagDecoder flags of
                Ok token ->
                    token

                Err _ ->
                    ""
    in
    initFunc authToken


flagDecoder : D.Decoder String
flagDecoder =
    D.field "authToken" D.string
