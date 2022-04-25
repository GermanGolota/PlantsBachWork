module Utils exposing (AlignDirection(..), largeFont, textAlign, textCenter)

import Html exposing (Attribute)
import Html.Attributes exposing (style)


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
