module Main exposing (..)

import Browser
import Html exposing (Html, a, div, text)
import Html.Attributes exposing (href)
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
            case D.decodeValue decodeFlags flags of
                Ok res ->
                    Just res

                Err _ ->
                    Nothing
    in
    initFunc authResp


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
    D.map2 AuthResponse
        (D.field "token" D.string)
        (D.field "roles" (D.list D.string) |> D.map convertRoles)


type ModelBase model
    = Unauthorized
    | NotLoggedIn
    | Authorized model


initBase : List UserRole -> ( model, Cmd msg ) -> Maybe AuthResponse -> ( ModelBase model, Cmd msg )
initBase requiredRoles initialModel response =
    let
        roleInRequired role =
            List.member role requiredRoles
    in
    case response of
        Just resp ->
            if List.any roleInRequired resp.roles then
                ( Authorized <| Tuple.first initialModel, Tuple.second initialModel )

            else
                ( Unauthorized, Cmd.none )

        Nothing ->
            ( NotLoggedIn, Cmd.none )


viewBase : (model -> Html msg) -> ModelBase model -> Html msg
viewBase authorizedView modelB =
    case modelB of
        Unauthorized ->
            div [] [ text "You are not authorized to view this page!" ]

        NotLoggedIn ->
            div []
                [ text "You are not logged into your account!"
                , a [ href "/login" ] [ text "Go to login" ]
                ]

        Authorized authM ->
            authorizedView authM
