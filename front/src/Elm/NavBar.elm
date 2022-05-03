module NavBar exposing (..)

import Assets exposing (treeIcon)
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Bootstrap.Utilities.Flex as Flex
import Color
import Html exposing (Html, a, div, i, text)
import Html.Attributes exposing (class, href, style)
import Main exposing (AuthResponse, ModelBase, UserRole(..), viewBase)
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
    , Link "/plants" "Plants" "fa-solid fa-bucket" <| Just [ Manager, Producer ]
    , Link "/orders" "Orders" "fa-solid fa-bag-shopping" Nothing
    , Link "/instructions" "Instructions" "fa-solid fa-book" Nothing
    , statsLink
    , Link "/user" "Manage Users" "fa-solid fa-users" Nothing
    ]


statsLink =
    Link "/stats" "Statistics" "fa-solid fa-chart-pie" <| Just [ Manager ]


searchLink =
    Link "/search" "Find" "fa-solid fa-magnifying-glass" Nothing


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


viewNav : ModelBase model -> Maybe Link -> (AuthResponse -> model -> Html msg) -> Html msg
viewNav model link pageView =
    let
        viewP =
            viewMain link pageView
    in
    viewBase viewP model


viewMain : Maybe Link -> (AuthResponse -> model -> Html msg) -> AuthResponse -> model -> Html msg
viewMain link pageView resp model =
    viewNavBase resp.username resp.roles link (pageView resp model)


viewNavBase : String -> List UserRole -> Maybe Link -> Html msg -> Html msg
viewNavBase username roles currentLink baseView =
    div fillScreen
        [ div ([ flex, Flex.row ] ++ fillParent) [ navBar username roles currentLink, div [ style "flex" "3" ] [ baseView ] ]
        ]


navBar : String -> List UserRole -> Maybe Link -> Html msg
navBar username roles currentLink =
    div
        [ flex1
        , style "height" "100%"
        , style "margin-right" "0.5em"
        , class "bg-light"
        ]
        [ div ([ flex, Flex.col, style "justify-content" "space-between" ] ++ fillParent)
            [ div []
                [ div [ flex, Flex.row, Flex.justifyCenter ]
                    [ treeIcon (px 200) Color.black
                    ]
                , linksView currentLink <| getLinksFor roles
                ]
            , div [] [ userView username ]
            ]
        ]


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
