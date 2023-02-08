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


viewNotifications : List ( Notification, Bool ) -> List (Html (MsgBase msg))
viewNotifications notifications =
    List.map (\( n, l ) -> viewNotification n l) notifications


viewNotification : Notification -> Bool -> Html (MsgBase msg)
viewNotification notification loaded =
    let
        status =
            if loaded then
                if notification.success then
                    text "Completed successfully!"

                else
                    text "Failed to process!"

            else
                viewLoading

        result =
            if loaded then
                case findResultLocation notification.command.commandName notification.command.aggregate.id of
                    Just location ->
                        [ Button.linkButton [ Button.onClick <| Navigate location, Button.info ]
                            [ text <| "See " ++ humanizePascalCase notification.command.aggregate.name ]
                        ]

                    Nothing ->
                        []

            else
                []
    in
    div [ flex, Flex.col ]
        [ div [ Flex.row ] [ viewTitle notification ]
        , div [ Flex.row ] [ status ]
        , div [ Flex.row ] result
        ]


viewTitle : Notification -> Html msg
viewTitle notification =
    text <| humanizePascalCase notification.command.aggregate.name ++ " - " ++ humanizePascalCase notification.command.commandName


findResultLocation : String -> String -> Maybe String
findResultLocation commandName aggregateId =
    case commandName of
        "AddToStock" ->
            Just <| "/notPosted/" ++ aggregateId ++ "/edit"

        _ ->
            Nothing
