module Pages.Login exposing (main)

import Assets exposing (treeIcon)
import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Color exposing (Color, rgb255)
import Html exposing (Html, div, text)
import Html.Attributes exposing (attribute, style)
import Json.Decode as D
import Main exposing (baseApplication)
import Svg exposing (Svg, image, svg)
import Svg.Attributes exposing (height, width)
import TypedSvg.Types exposing (px)
import Utils exposing (textCenter)


type alias Model =
    { username : String
    , password : String
    }


type Msg
    = UsernameUpdated String
    | PasswordUpdate String
    | Submitted


init : String -> ( Model, Cmd Msg )
init key =
    ( Model "" "", Cmd.none )


view : Model -> Html Msg
view model =
    Card.config [ Card.attrs [ style "width" "20rem" ] ]
        |> Card.header [ textCenter ]
            [ treeIcon (px 200) (rgb255 36 158 71)
            ]
        |> Card.block []
            [ Block.titleH4 [] [ text "Card title" ]
            , Block.text [] [ text "Some quick example text to build on the card title and make up the bulk of the card's content." ]
            , Block.custom <|
                Button.button [ Button.primary ] [ text "Go somewhere" ]
            ]
        |> Card.view


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        UsernameUpdated login ->
            ( { model | username = login }, Cmd.none )

        PasswordUpdate pass ->
            ( { model | password = pass }, Cmd.none )

        Submitted ->
            ( model, Cmd.none )


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
