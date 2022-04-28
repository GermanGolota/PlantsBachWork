module Webdata exposing (WebData(..), viewWebdata)

import Html exposing (Html, div, text)
import Utils exposing (viewLoading)


type WebData data
    = Loading
    | Loaded data
    | Error


viewWebdata : WebData data -> (data -> Html msg) -> Html msg
viewWebdata data mainView =
    case data of
        Loading ->
            viewLoading

        Error ->
            div [] [ text "Something went wrong while loading data" ]

        Loaded fullModel ->
            mainView fullModel
