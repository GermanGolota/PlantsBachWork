module Utils exposing (..)

import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Bootstrap.Spinner as Spinner
import Bootstrap.Text as Text
import Bootstrap.Utilities.Spacing as Spacing
import Color exposing (Color, rgba)
import Dict exposing (Dict)
import FormatNumber exposing (format)
import FormatNumber.Locales exposing (usLocale)
import Html exposing (Attribute, Html, a, div)
import Html.Attributes exposing (attribute, style)
import Html.Parser
import Html.Parser.Util
import Json.Decode as D


type AlignDirection
    = Left
    | Right
    | Center


largeCentered =
    [ largeFont, textCenter ]


mediumCentered =
    [ mediumFont, textCenter ]


largeFont : Attribute msg
largeFont =
    style "font-size" "2rem"


flex1 : Attribute msg
flex1 =
    style "flex" "1"


mediumFont : Attribute msg
mediumFont =
    style "font-size" "1.5rem"


textFromDirection : AlignDirection -> String
textFromDirection dir =
    case dir of
        Left ->
            "left"

        Right ->
            "right"

        Center ->
            "center"


textAlign : AlignDirection -> Attribute msg
textAlign dir =
    style "text-align" (textFromDirection dir)


textCenter : Attribute msg
textCenter =
    textAlign Center


mapStyles : Dict String String -> List (Attribute msg)
mapStyles styles =
    let
        keys =
            Dict.keys styles

        values =
            Dict.values styles
    in
    List.map2 style keys values


filledBackground : List (Attribute msg) -> Html msg
filledBackground additionalAttributes =
    div ((mapStyles <| Dict.fromList [ ( "position", "absolute" ), ( "width", "100vw" ), ( "height", "100vh" ), ( "left", "0px" ), ( "top", "0px" ), ( "z-index", "-1" ) ]) ++ additionalAttributes) []


scaleFrom255 : Int -> Float
scaleFrom255 c =
    toFloat c / 255


rgba255 : Int -> Int -> Int -> Float -> Color
rgba255 r g b a =
    rgba (scaleFrom255 r) (scaleFrom255 g) (scaleFrom255 b) a


fillScreen : List (Attribute msg)
fillScreen =
    [ style "width" "100vw", style "height" "100vh", style "max-width" "100%", style "max-height" "100vh", style "overflow-x" "hidden" ]


fillParent : List (Attribute msg)
fillParent =
    [ style "width" "100%", style "height" "100%" ]


flexCenter =
    [ style "align-items" "center", style "justify-content" "center" ]


unique : List a -> List a
unique l =
    let
        incUnique : a -> List a -> List a
        incUnique elem lst =
            case List.member elem lst of
                True ->
                    lst

                False ->
                    elem :: lst
    in
    List.foldr incUnique [] l


flatten : List (List a) -> List a
flatten plane =
    plane |> List.foldr (++) []


viewLoading : Html msg
viewLoading =
    let
        colors =
            [ Text.primary
            , Text.secondary
            , Text.success
            , Text.danger
            , Text.warning
            , Text.info
            , Text.dark
            ]

        spiner color =
            Spinner.spinner [ Spinner.grow, Spinner.color color, Spinner.attrs [ Spacing.ml2 ] ] []

        spiners =
            List.map spiner colors
    in
    Html.div (fillParent ++ [ flex ] ++ flexCenter) spiners


itself : a -> a
itself item =
    item


intersect : List a -> List a -> Bool
intersect first second =
    let
        inFirst member =
            List.member member first
    in
    List.any inFirst second


flex : Html.Attribute msg
flex =
    style "display" "flex"


smallMargin : Html.Attribute msg
smallMargin =
    style "margin" "0.5em"


mediumMargin : Html.Attribute msg
mediumMargin =
    style "margin" "1em"


chunk : Int -> List a -> List (List a)
chunk chunkSize initial =
    let
        indexed =
            List.indexedMap Tuple.pair initial

        paged =
            List.map (\x -> ( Tuple.first x // chunkSize, Tuple.second x )) indexed

        pages =
            unique <| List.map Tuple.first paged

        itemsInPage page =
            List.filter (\item -> Tuple.first item == page) paged
    in
    List.map (\page -> List.map Tuple.second <| itemsInPage page) pages


chunkedView : Int -> (a -> Html msg) -> List a -> Html msg
chunkedView size viewFunc items =
    let
        chunks =
            chunk size items

        remainder =
            case modBy size <| List.length items of
                0 ->
                    0

                val ->
                    size - val

        emptyCol =
            Grid.col [ Col.attrs [ style "flex" "1", smallMargin ] ] []

        toCol item =
            Grid.col [ Col.attrs [ style "flex" "1", smallMargin ] ] [ viewFunc item ]

        addRemainder index =
            if index == List.length chunks - 1 then
                List.map (\i -> emptyCol) (List.repeat remainder 0)

            else
                List.map (\i -> emptyCol) []

        buildRow index rowItems =
            Grid.row []
                (List.map toCol rowItems
                    ++ addRemainder index
                )
    in
    Grid.container []
        (List.indexedMap
            buildRow
            chunks
        )


formatPrice : Float -> String
formatPrice price =
    format usLocale price ++ " ₴"


type SubmittedResult
    = SubmittedSuccess String
    | SubmittedFail String


submittedDecoder : D.Decoder Bool -> D.Decoder String -> D.Decoder SubmittedResult
submittedDecoder successDecoder msgDecoder =
    successDecoder |> D.andThen (submittedMsgDecoder msgDecoder)


submittedMsgDecoder : D.Decoder String -> Bool -> D.Decoder SubmittedResult
submittedMsgDecoder messageField success =
    if success then
        D.map SubmittedSuccess messageField

    else
        D.map SubmittedFail messageField


existsDecoder : D.Decoder a -> D.Decoder (Maybe a)
existsDecoder dec =
    D.field "exists" D.bool |> D.andThen (maybeDecoder dec)


maybeDecoder : D.Decoder a -> Bool -> D.Decoder (Maybe a)
maybeDecoder dec exists =
    if exists then
        D.map Just dec

    else
        D.succeed Nothing


createdDecoder : D.Decoder String
createdDecoder =
    let
        combine date humanDate =
            date ++ " (" ++ humanDate ++ ")"
    in
    D.map2
        combine
        (D.at [ "item", "createdDate" ] D.string)
        (D.at [ "item", "createdHumanDate" ] D.string)


buildQuery : List ( String, String ) -> String
buildQuery items =
    List.foldl addQuery "?" items


addQuery : ( String, String ) -> String -> String
addQuery ( key, value ) result =
    result ++ key ++ "=" ++ value ++ "&"


bgTeal : Html.Attribute msg
bgTeal =
    style "background-color" "var(--bs-teal)"


textHtml : String -> List (Html.Html msg)
textHtml t =
    case Html.Parser.run t of
        Ok nodes ->
            Html.Parser.Util.toVirtualDom nodes

        Err _ ->
            []


decodeId : D.Decoder String
decodeId =
    D.oneOf [ D.string, D.map String.fromInt D.int ]
