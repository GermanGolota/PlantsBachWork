module Main exposing (..)

import Browser
import Html exposing (Html, a, div, text)
import Html.Attributes exposing (href)
import Json.Decode as D
import Json.Decode.Pipeline exposing (required)
import Utils exposing (intersect)


type UserRole
    = Consumer
    | Producer
    | Manager


type alias AuthResponse =
    { token : String
    , roles : List UserRole
    , username : String
    }


type alias ApplicationConfig model msg =
    { init : Maybe AuthResponse -> D.Value -> ( model, Cmd msg )
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


init : (Maybe AuthResponse -> D.Value -> ( model, Cmd msg )) -> D.Value -> ( model, Cmd msg )
init initFunc flags =
    let
        authResp =
            case D.decodeValue decodeFlags flags of
                Ok res ->
                    Just res

                Err _ ->
                    Nothing
    in
    initFunc authResp flags


convertRoles roleIds =
    List.map convertRole roleIds


convertRole : String -> UserRole
convertRole roleId =
    case roleId of
        "Consumer" ->
            Consumer

        "Producer" ->
            Producer

        "Manager" ->
            Manager

        _ ->
            Consumer


decodeFlags : D.Decoder AuthResponse
decodeFlags =
    D.map3 AuthResponse
        (D.field "token" D.string)
        (D.field "roles" (D.list D.string) |> D.map convertRoles)
        (D.field "username" D.string)


type ModelBase model
    = Unauthorized
    | NotLoggedIn
    | Authorized AuthResponse model


initBase : List UserRole -> model -> (AuthResponse -> Cmd msg) -> Maybe AuthResponse -> ( ModelBase model, Cmd msg )
initBase requiredRoles initialModel initialCmd response =
    case response of
        Just resp ->
            if intersect requiredRoles resp.roles then
                ( Authorized resp initialModel, initialCmd resp )

            else
                ( Unauthorized, Cmd.none )

        Nothing ->
            ( NotLoggedIn, Cmd.none )


viewBase : (AuthResponse -> model -> Html msg) -> ModelBase model -> Html msg
viewBase authorizedView modelB =
    case modelB of
        Unauthorized ->
            div [] [ text "You are not authorized to view this page!" ]

        NotLoggedIn ->
            div []
                [ text "You are not logged into your account!"
                , a [ href "/login" ] [ text "Go to login" ]
                ]

        Authorized resp authM ->
            authorizedView resp authM
