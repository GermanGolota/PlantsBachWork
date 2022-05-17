module NavBar exposing (..)

import Assets exposing (treeIcon)
import Bootstrap.Dropdown as Dropdown
import Bootstrap.Utilities.Flex as Flex
import Color
import Html exposing (Html, a, div, i, text)
import Html.Attributes exposing (class, href, style)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), viewBase)
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


viewNav : ModelBase model -> Maybe Link -> (AuthResponse -> model -> Html msg) -> Html (MsgBase msg)
viewNav model link pageView =
    let
        navState =
            case model of
                Authorized _ _ navig ->
                    Just navig

                _ ->
                    Nothing

        viewP =
            viewMain navState link pageView
    in
    viewBase viewP model


viewMain : Maybe Dropdown.State -> Maybe Link -> (AuthResponse -> model -> Html msg) -> AuthResponse -> model -> Html (MsgBase msg)
viewMain dropState link pageView resp model =
    viewNavBase dropState resp.username resp.roles link (pageView resp model)


viewNavBase : Maybe Dropdown.State -> String -> List UserRole -> Maybe Link -> Html msg -> Html (MsgBase msg)
viewNavBase dropState username roles currentLink baseView =
    div fillScreen
        [ div ([ flex, Flex.row ] ++ fillParent)
            [ navBar dropState username roles currentLink
            , Html.map (\msgType -> Main msgType) (div [ style "flex" "3", style "margin-left" "25vw" ] [ baseView ])
            ]
        ]


navBar : Maybe Dropdown.State -> String -> List UserRole -> Maybe Link -> Html (MsgBase msg)
navBar dropState username roles currentLink =
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
            , div [ flex, Flex.row ] [ dropdownView dropState <| userView username ]
            ]
        ]


dropdownView state main =
    case state of
        Just val ->
            Dropdown.dropdown
                val
                { options = []
                , toggleMsg = NavChanged
                , toggleButton =
                    Dropdown.toggle [] [ main ]
                , items =
                    [ Dropdown.header [ text "Header" ]
                    , Dropdown.buttonItem [] [ text "Item 1" ]
                    , Dropdown.buttonItem [] [ text "Item 2" ]
                    , Dropdown.divider
                    , Dropdown.header [ text "Another heading" ]
                    , Dropdown.buttonItem [] [ text "Item 3" ]
                    , Dropdown.buttonItem [] [ text "Item 4" ]
                    ]
                }

        Nothing ->
            main


linksView : Maybe Link -> List Link -> Html msg
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


linkView : Bool -> Link -> Html msg
linkView isSelected link =
    let
        backColor =
            if isSelected then
                "bg-primary"

            else
                ""
    in
    a [ href link.url ]
        [ div [ class "nav-bar-item", flex, Flex.row, smallMargin, Flex.alignItemsCenter, largeFont, class ("btn " ++ backColor) ]
            [ i [ class link.icon, style "margin-right" "0.5em" ] []
            , text link.text
            ]
        ]


userView : String -> Html msg
userView username =
    div [ flex, Flex.row, smallMargin, style "border-top" "solid gray 1px", Flex.alignItemsCenter, largeFont ]
        [ i [ class "fa-solid fa-user", style "margin-right" "2em", smallMargin ] []
        , text username
        ]
