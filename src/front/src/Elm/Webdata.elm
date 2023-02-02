module Webdata exposing (WebData(..), viewWebdata)

import Html exposing (Html, div, text)
import Http exposing (Error(..))
import Utils exposing (viewLoading)


type WebData data
    = Loading
    | Loaded data
    | Error Http.Error


viewWebdata : WebData data -> (data -> Html msg) -> Html msg
viewWebdata data mainView =
    case data of
        Loading ->
            viewLoading

        Error err ->
            div [] [ text <| errorToString err ]

        Loaded fullModel ->
            mainView fullModel


errorToString : Http.Error -> String
errorToString error =
    case error of
        BadUrl url ->
            "The URL " ++ url ++ " was invalid"

        Timeout ->
            "Unable to reach the server, try again"

        NetworkError ->
            "Unable to reach the server, check your network connection"

        BadStatus 500 ->
            "The server had a problem, try again later"

        BadStatus 400 ->
            "Verify your information and try again"

        BadStatus _ ->
            "Something went wrong while loading data"

        BadBody errorMessage ->
            errorMessage
