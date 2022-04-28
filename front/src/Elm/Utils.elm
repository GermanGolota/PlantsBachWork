module Utils exposing (AlignDirection(..), fillParent, filledBackground, flatten, flexCenter, itself, largeFont, mapStyles, rgba255, textAlign, textCenter, unique, viewLoading)

import Bootstrap.Spinner as Spinner
import Bootstrap.Text as Text
import Bootstrap.Utilities.Spacing as Spacing
import Color exposing (Color, rgba)
import Dict exposing (Dict)
import Html exposing (Attribute, Html, a, div)
import Html.Attributes exposing (attribute, style)


type AlignDirection
    = Left
    | Right
    | Center


largeFont : Attribute msg
largeFont =
    style "font-size" "2rem"


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
    [ style "width" "100vw", style "height" "100vh" ]


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
            , Text.light
            , Text.dark
            ]

        spiner color =
            Spinner.spinner [ Spinner.grow, Spinner.color color, Spinner.attrs [ Spacing.ml2 ] ] []

        spiners =
            List.map spiner colors
    in
    Html.div (fillParent ++ [ style "display" "flex" ] ++ flexCenter) spiners


itself : a -> a
itself item =
    item
