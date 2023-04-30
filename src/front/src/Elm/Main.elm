port module Main exposing (..)

import Bootstrap.Accordion as Accordion
import Bootstrap.Modal as Modal
import Browser
import Html exposing (Html)
import Json.Decode as D
import Json.Decode.Pipeline exposing (hardcoded, required)
import Task
import Utils exposing (Notification, SubmittedResult(..), decodeNotificationPair, intersect)


notifyCmd : SubmittedResult -> Cmd (MsgBase msg)
notifyCmd result =
    case result of
        SubmittedSuccess _ command ->
            Task.perform (\_ -> NotificationStarted <| Notification command True) (Task.succeed True)

        SubmittedFail _ ->
            Cmd.none


type UserRole
    = Consumer
    | Producer
    | Manager


allRoles =
    [ Producer, Consumer, Manager ]


isAdmin auth =
    List.any (\r -> r == Manager) auth.roles


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
    , userId : String
    , notifications : List ( Notification, Bool )
    , notificationsModal : Modal.Visibility
    , notificationsAccordion : Accordion.State
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


decodeFlags : D.Decoder AuthResponse
decodeFlags =
    let
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

        convertRolesStr roleIds =
            List.map convertRoleStr roleIds
    in
    D.succeed AuthResponse
        |> required "token" D.string
        |> required "roles" (D.list D.string |> D.map convertRolesStr)
        |> required "username" D.string
        |> required "userId" D.string
        |> required "notifications" (D.list decodeNotificationPair)
        |> hardcoded Modal.hidden
        |> hardcoded Accordion.initialState


roleToStr : UserRole -> String
roleToStr role =
    case role of
        Consumer ->
            "Consumer"

        Producer ->
            "Producer"

        Manager ->
            "Manager"


type ModelBase model
    = Unauthorized
    | NotLoggedIn
    | Authorized AuthResponse model


port navigate : String -> Cmd msg


port goBack : () -> Cmd msg


port notificationReceived : (Notification -> msg) -> Sub msg


port dismissNotification : Notification -> Cmd msg


port resizeAccordions : () -> Cmd msg


type MsgBase msg
    = Navigate String
    | GoBack
    | Main msg
    | NotificationStarted Notification
    | NotificationReceived Notification
    | NotificationDismissed Notification
    | AllNotificationsDismissed
    | CloseNotificationsModal
    | ShowNotificationsModal
    | AnimateNotificationsModal Modal.Visibility
    | NotificationsAccordion Accordion.State


subscriptionBase : ModelBase model -> Sub (MsgBase msg) -> Sub (MsgBase msg)
subscriptionBase mod baseSub =
    let
        notifications =
            case mod of
                Authorized auth _ ->
                    [ Modal.subscriptions auth.notificationsModal AnimateNotificationsModal
                    , Accordion.subscriptions auth.notificationsAccordion NotificationsAccordion
                    ]

                _ ->
                    []
    in
    ([ notificationReceived NotificationReceived, baseSub ] ++ notifications) |> Sub.batch


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


updateBase : (msg -> ModelBase model -> ( ModelBase model, Cmd (MsgBase msg) )) -> MsgBase msg -> ModelBase model -> ( ModelBase model, Cmd (MsgBase msg) )
updateBase updateFunc message model =
    case message of
        Navigate location ->
            ( model, navigate location )

        GoBack ->
            ( model, goBack () )

        Main main ->
            updateFunc main model

        NotificationStarted notification ->
            case model of
                Authorized auth page ->
                    let
                        updateNotifications =
                            if List.any (\( n, _ ) -> n.command.id == notification.command.id) auth.notifications then
                                auth.notifications

                            else
                                ( notification, False ) :: auth.notifications
                    in
                    ( Authorized { auth | notifications = updateNotifications } page, resizeAccordions () )

                _ ->
                    ( model, Cmd.none )

        NotificationDismissed notification ->
            case model of
                Authorized auth page ->
                    ( Authorized { auth | notifications = List.filter (\( n, _ ) -> n.command.id /= notification.command.id) auth.notifications } page, [ resizeAccordions (), dismissNotification notification ] |> Cmd.batch )

                _ ->
                    ( model, Cmd.none )

        AllNotificationsDismissed ->
            case model of
                Authorized auth page ->
                    ( Authorized
                        { auth
                            | notifications = []
                        }
                        page
                    , List.map (\( n, _ ) -> dismissNotification n) auth.notifications |> Cmd.batch
                    )

                _ ->
                    ( model, Cmd.none )

        NotificationReceived notification ->
            case model of
                Authorized auth page ->
                    let
                        mapNotification not succ =
                            if not.command.id == notification.command.id then
                                ( not, True )

                            else
                                ( not, succ )
                    in
                    ( Authorized { auth | notifications = List.map (\( n, s ) -> mapNotification n s) auth.notifications } page, resizeAccordions () )

                _ ->
                    ( model, Cmd.none )

        CloseNotificationsModal ->
            case model of
                Authorized auth page ->
                    ( Authorized { auth | notificationsModal = Modal.hidden } page, Cmd.none )

                _ ->
                    ( model, Cmd.none )

        ShowNotificationsModal ->
            case model of
                Authorized auth page ->
                    ( Authorized { auth | notificationsModal = Modal.shown } page, Cmd.none )

                _ ->
                    ( model, Cmd.none )

        AnimateNotificationsModal visibility ->
            case model of
                Authorized auth page ->
                    ( Authorized { auth | notificationsModal = visibility } page, Cmd.none )

                _ ->
                    ( model, Cmd.none )

        NotificationsAccordion state ->
            case model of
                Authorized auth page ->
                    ( Authorized { auth | notificationsAccordion = state } page, Cmd.none )

                _ ->
                    ( model, Cmd.none )


mapCmd : Cmd a -> Cmd (MsgBase a)
mapCmd msg =
    Cmd.map Main msg


mapSub : Sub a -> Sub (MsgBase a)
mapSub msg =
    Sub.map Main msg
