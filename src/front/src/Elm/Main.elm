port module Main exposing (..)

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


allRoles =
    [ Producer, Consumer, Manager ]


rolesDecoder : D.Decoder (List Int) -> D.Decoder (List UserRole)
rolesDecoder idsDecoder =
    D.map convertRoles idsDecoder


convertRoles : List Int -> List UserRole
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


roleToNumber : UserRole -> Int
roleToNumber role =
    case role of
        Consumer ->
            1

        Producer ->
            2

        Manager ->
            3


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
        { init = mainInit config.init
        , view = config.view
        , update = config.update
        , subscriptions = config.subscriptions
        }


mainInit : (Maybe AuthResponse -> D.Value -> ( model, Cmd msg )) -> D.Value -> ( model, Cmd msg )
mainInit initFunc flags =
    let
        authResp =
            case D.decodeValue decodeFlags flags of
                Ok res ->
                    Just res

                Err _ ->
                    Nothing
    in
    initFunc authResp flags


convertRolesStr roleIds =
    List.map convertRoleStr roleIds


roleToStr : UserRole -> String
roleToStr role =
    case role of
        Consumer ->
            "Consumer"

        Producer ->
            "Producer"

        Manager ->
            "Manager"


convertRoleStr : String -> UserRole
convertRoleStr roleId =
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
        (D.field "roles" (D.list D.string) |> D.map convertRolesStr)
        (D.field "username" D.string)


type ModelBase model
    = Unauthorized
    | NotLoggedIn
    | Authorized AuthResponse model


port navigate : String -> Cmd msg


type MsgBase msg
    = Navigate String
    | Main msg


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


updateBase : (msg -> model -> ( model, Cmd (MsgBase msg) )) -> MsgBase msg -> model -> ( model, Cmd (MsgBase msg) )
updateBase updateFunc message model =
    case message of
        Navigate location ->
            ( model, navigate location )

        Main main ->
            updateFunc main model


mapCmd : Cmd a -> Cmd (MsgBase a)
mapCmd msg =
    Cmd.map Main msg


mapSub : Sub a -> Sub (MsgBase a)
mapSub msg =
    Sub.map Main msg


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
