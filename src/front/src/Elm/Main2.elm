module Main2 exposing (viewBase)

import Bootstrap.Button as Button
import Bootstrap.Modal as Modal
import Bootstrap.Utilities.Flex as Flex
import Html exposing (Html, a, div, text)
import Html.Attributes exposing (href)
import Html.Events exposing (onClick)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..))
import NavBar exposing (Link, viewNavBase)
import Utils exposing (Notification, flex, humanizePascalCase, viewLoading)


viewBase : ModelBase model -> Maybe Link -> (AuthResponse -> model -> Html (MsgBase msg)) -> Html (MsgBase msg)
viewBase model link pageView =
    case model of
        Unauthorized ->
            div [] [ text "You are not authorized to view this page!" ]

        NotLoggedIn ->
            div []
                [ text "You are not logged into your account!"
                , a [ href "/login" ] [ text "Go to login" ]
                ]

        Authorized resp authM ->
            div [] [ notificationsModal resp.notificationsModal resp.notifications, viewNavBase resp.username resp.roles link <| pageView resp authM ]


notificationsModal : Modal.Visibility -> List ( Notification, Bool ) -> Html (MsgBase msg)
notificationsModal modal notifications =
    Modal.config CloseNotificationsModal
        |> Modal.withAnimation AnimateNotificationsModal
        |> Modal.small
        |> Modal.h3 [] [ text "Notifications" ]
        |> Modal.body [] (viewNotifications notifications)
        |> Modal.footer []
            [ Button.button
                [ Button.outlinePrimary
                , Button.attrs [ onClick <| AnimateNotificationsModal Modal.hiddenAnimated ]
                ]
                [ text "Close" ]
            ]
        |> Modal.view modal


viewNotifications : List ( Notification, Bool ) -> List (Html msg)
viewNotifications notifications =
    List.map (\( n, l ) -> viewNotification n l) notifications


viewNotification : Notification -> Bool -> Html msg
viewNotification notification loaded =
    let
        lower =
            if loaded then
                text "Completed!"

            else
                viewLoading
    in
    div [ flex, Flex.col ]
        [ div [ Flex.row ] [ viewTitle notification ]
        , div [ Flex.row ] [ lower ]
        ]


viewTitle : Notification -> Html msg
viewTitle notification =
    text <| humanizePascalCase notification.command.aggregate.name ++ " - " ++ humanizePascalCase notification.command.commandName
