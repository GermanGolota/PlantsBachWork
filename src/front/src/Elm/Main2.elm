module Main2 exposing (viewBase)

import Bootstrap.Accordion as Accordion
import Bootstrap.Button as Button
import Bootstrap.Card.Block as Block
import Bootstrap.Modal as Modal
import Bootstrap.Utilities.Flex as Flex
import Html exposing (Html, a, div, i, text)
import Html.Attributes exposing (class, href)
import Html.Events exposing (onClick)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..))
import NavBar exposing (Link, viewNavBase)
import Utils exposing (Notification, flex, humanizePascalCase, mediumCentered, viewLoading)


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
            let
                notificatonsCounts =
                    List.filter (\( _, loaded ) -> not loaded) resp.notifications |> List.length
            in
            div []
                [ notificationsModal resp.notificationsModal resp.notificationsAccordion resp.notifications
                , viewNavBase resp.username resp.roles link notificatonsCounts <| pageView resp authM
                ]


notificationsModal : Modal.Visibility -> Accordion.State -> List ( Notification, Bool ) -> Html (MsgBase msg)
notificationsModal modal accordion notifications =
    Modal.config CloseNotificationsModal
        |> Modal.withAnimation AnimateNotificationsModal
        |> Modal.small
        |> Modal.h3 [] [ text "Notifications" ]
        |> Modal.body [] [ Button.linkButton [ Button.outlineDanger, Button.onClick AllNotificationsDismissed ] [ text "Dismiss all" ], viewNotifications notifications accordion ]
        |> Modal.footer []
            [ Button.button
                [ Button.outlinePrimary
                , Button.attrs [ onClick <| AnimateNotificationsModal Modal.hiddenAnimated ]
                ]
                [ text "Close" ]
            ]
        |> Modal.view modal


viewNotifications : List ( Notification, Bool ) -> Accordion.State -> Html (MsgBase msg)
viewNotifications notifications accordion =
    Accordion.config NotificationsAccordion
        |> Accordion.withAnimation
        |> Accordion.cards
            (List.map (\( n, l ) -> viewNotification n l) notifications)
        |> Accordion.view accordion


viewNotification : Notification -> Bool -> Accordion.Card (MsgBase msg)
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
                case findResultLocation notification.command.name notification.command.aggregate.id of
                    Just location ->
                        [ Button.linkButton [ Button.onClick <| Navigate location, Button.info ]
                            [ text <| "See " ++ humanizePascalCase notification.command.aggregate.name ]
                        ]

                    Nothing ->
                        []

            else
                []

        cardColor =
            if loaded then
                if notification.success then
                    class "bg-info"

                else
                    class "bg-danger"

            else
                class "bg-warning"
    in
    Accordion.card
        { id = notification.command.id
        , options =
            []
        , header =
            Accordion.header [ cardColor ] <|
                Accordion.toggle []
                    [ div (mediumCentered ++ [ flex, Flex.row ])
                        [ text <| (humanizePascalCase notification.command.name ++ " at " ++ notification.command.startedTime)
                        , i [ class "fa-solid fa-x", onClick <| NotificationDismissed notification ] []
                        ]
                    ]
        , blocks =
            [ Accordion.block []
                [ Block.custom
                    (div ([ flex, Flex.col ] ++ mediumCentered)
                        [ div [ Flex.row ] [ viewTitle notification ]
                        , div [ Flex.row ] [ status ]
                        , div [ Flex.row ] result
                        ]
                    )
                ]
            ]
        }


viewTitle : Notification -> Html msg
viewTitle notification =
    text <| humanizePascalCase notification.command.aggregate.name ++ " - " ++ humanizePascalCase notification.command.name


findResultLocation : String -> String -> Maybe String
findResultLocation commandName aggregateId =
    let
        plantEditPage =
            "/notPosted/" ++ aggregateId ++ "/edit"

        instructionPage =
            "/instructions/" ++ aggregateId
    in
    case commandName of
        "AddToStock" ->
            Just plantEditPage

        "EditStockItem" ->
            Just plantEditPage

        "CreateInstruction" ->
            Just instructionPage

        "EditInstruction" ->
            Just instructionPage

        "PostStockItem" ->
            Just <| "/plant/" ++ aggregateId

        "OrderPost" ->
            Just <| "/orders"

        _ ->
            Nothing
