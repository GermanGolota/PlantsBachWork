module Main2 exposing (..)

import Html exposing (Html, a, div, text)
import Html.Attributes exposing (href)
import Main exposing (AuthResponse, ModelBase(..), MsgBase)
import NavBar exposing (Link, viewNavBase)


viewBase2 : ModelBase model -> Maybe Link -> (AuthResponse -> model -> Html (MsgBase msg)) -> Html (MsgBase msg)
viewBase2 model link pageView =
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
                basePage =
                    pageView resp authM
            in
            viewNavBase resp.username resp.roles link basePage
