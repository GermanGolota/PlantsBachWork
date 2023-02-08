module NavBar exposing (..)

import Assets exposing (treeIcon)
import Bootstrap.Button as Button
import Bootstrap.Popover as Popover
import Bootstrap.Utilities.Flex as Flex
import Color
import Html exposing (Html, a, div, i, text)
import Html.Attributes exposing (class, style)
import Html.Events exposing (onClick)
import Main exposing (MsgBase(..), UserRole(..))
import TypedSvg.Types exposing (px)
import Utils exposing (Notification, fillParent, fillScreen, flex, flex1, humanizePascalCase, intersect, largeFont, smallMargin, viewLoading)


type alias Link =
    { url : String
    , text : String
    , icon : String
    , access : Maybe (List UserRole)
    }


allLinks : List Link
allLinks =
    [ searchLink
    , plantsLink
    , ordersLink
    , instructionsLink
    , statsLink
    , usersLink
    ]


instructionsLink =
    Link "/instructions" "Instructions" "fa-solid fa-book" Nothing


usersLink =
    Link "/user" "Manage Users" "fa-solid fa-users" <| Just [ Producer, Manager ]


ordersLink =
    Link "/orders" "Orders" "fa-solid fa-bag-shopping" Nothing


statsLink =
    Link "/stats" "Statistics" "fa-solid fa-chart-pie" <| Just [ Manager ]


searchLink =
    Link "/search" "Find" "fa-solid fa-magnifying-glass" Nothing


plantsLink =
    Link "/notPosted" "Plants" "fa-solid fa-bucket" <| Just [ Manager, Producer ]


getLinksFor : List UserRole -> List Link
getLinksFor roles =
    let
        roleIntersect link =
            case link.access of
                Nothing ->
                    True

                Just items ->
                    intersect roles items
    in
    List.filter roleIntersect allLinks


viewNavBase : String -> List ( Notification, Bool ) -> Popover.State -> List UserRole -> Maybe Link -> Html (MsgBase msg) -> Html (MsgBase msg)
viewNavBase username notifications notificationsState roles currentLink baseView =
    div fillScreen
        [ div ([ flex, Flex.row ] ++ fillParent)
            [ navBar username notificationsState notifications roles currentLink
            , div [ style "flex" "3", style "margin-left" "25vw", style "max-width" "75vw" ] [ baseView ]
            ]
        ]


navBar : String -> Popover.State -> List ( Notification, Bool ) -> List UserRole -> Maybe Link -> Html (MsgBase msg)
navBar username popover notifications roles currentLink =
    div
        [ flex1
        , style "height" "100%"
        , style "width" "25vw"
        , style "margin-right" "0.5em"
        , class "bg-light"
        , style "position" "fixed"
        ]
        [ div ([ flex, Flex.col, style "justify-content" "space-between" ] ++ fillParent)
            [ div []
                [ div [ flex, Flex.row, Flex.justifyCenter ]
                    [ treeIcon (px 200) Color.black
                    ]
                , linksView currentLink <| getLinksFor roles
                ]
            , div [] [ userView username popover notifications ]
            ]
        ]


linksView : Maybe Link -> List Link -> Html (MsgBase msg)
linksView selected links =
    let
        isSelected link =
            case selected of
                Just selectedLink ->
                    link.url == selectedLink.url

                Nothing ->
                    False
    in
    div [ flex, Flex.col, style "border-top" "solid gray 1px", smallMargin ] (List.map (\link -> linkView (isSelected link) link) links)


linkView : Bool -> Link -> Html (MsgBase msg)
linkView isSelected link =
    let
        backColor =
            if isSelected then
                "bg-primary"

            else
                ""
    in
    a [ onClick <| Navigate link.url ]
        [ div [ class "nav-bar-item", flex, Flex.row, smallMargin, Flex.alignItemsCenter, largeFont, class ("btn " ++ backColor) ]
            [ i [ class link.icon, style "margin-right" "0.5em" ] []
            , text link.text
            ]
        ]


userView : String -> Popover.State -> List ( Notification, Bool ) -> Html (MsgBase msg)
userView username popover notifications =
    div [ flex, Flex.row, smallMargin, style "border-top" "solid gray 1px", Flex.alignItemsCenter, largeFont ]
        [ i [ class "fa-solid fa-user", style "margin-right" "2em", smallMargin ] []
        , a [ onClick <| Navigate "/profile" ] [ text username ]
        , Popover.config
            (Button.button
                [ Button.small
                , Button.primary
                , Button.attrs <|
                    Popover.onClick popover NotificationsPopover
                ]
                [ i [ class "fa fa-regular fa-bell" ]
                    []
                ]
            )
            |> Popover.top
            |> Popover.titleH4 [] [ text "Notifications" ]
            |> Popover.content []
                (viewNotifications notifications)
            |> Popover.view popover
        ]


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
