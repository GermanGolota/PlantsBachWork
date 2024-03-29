module NavBar exposing (..)

import Assets exposing (treeIcon)
import Bootstrap.Badge as Badge
import Bootstrap.Utilities.Flex as Flex
import Color
import Html exposing (Html, a, div, i, text)
import Html.Attributes exposing (class, style)
import Html.Events exposing (onClick)
import Main exposing (MsgBase(..), UserRole(..))
import TypedSvg.Types exposing (px)
import Utils exposing (fillParent, fillScreen, flex, flex1, intersect, largeFont, smallMargin)


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


viewNavBase : String -> List UserRole -> Maybe Link -> Int -> Html (MsgBase msg) -> Html (MsgBase msg)
viewNavBase username roles currentLink notifications baseView =
    div fillScreen
        [ div ([ flex, Flex.row ] ++ fillParent)
            [ navBar username roles currentLink notifications
            , div [ style "flex" "3", style "margin-left" "25vw", style "max-width" "75vw" ] [ baseView ]
            ]
        ]


navBar : String -> List UserRole -> Maybe Link -> Int -> Html (MsgBase msg)
navBar username roles currentLink notifications =
    div
        [ flex1
        , style "height" "100%"
        , style "width" "25vw"
        , style "margin-right" "0.5em"
        , class "bg-light"
        , style "position" "fixed"
        ]
        [ div ([ flex, Flex.col, style "justify-content" "space-between", style "background-color" "#8FBC8F" ] ++ fillParent)
            [ div [] [ userView username notifications ]
            , div [ flex1 ]
                [ linksView currentLink <| getLinksFor roles
                ]
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


userView : String -> Int -> Html (MsgBase msg)
userView username notifications =
    let
        notificationsView =
            if notifications == 0 then
                [ div [] [] ]

            else
                [ Badge.badgeWarning badgeInfo [ text <| String.fromInt notifications ] ]
    in
    div [ flex, Flex.row, smallMargin, style "border-top" "solid gray 1px", Flex.alignItemsCenter, largeFont ]
        [ i [ class "fa-solid fa-user", style "margin-right" "2em", smallMargin ] []
        , a [ onClick <| Navigate "/profile" ] [ text username ]
        , i [ class "fa-solid fa-bell", style "margin-right" "2em", smallMargin, onClick <| ShowNotificationsModal ]
            notificationsView
        ]


badgeInfo : List (Html.Attribute msg)
badgeInfo =
    [ style "color" "#fff", style "background-color" "#17a2b8" ]
