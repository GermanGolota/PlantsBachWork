module Pages.Login exposing (main)

import Assets exposing (treeIcon)
import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Form as Form
import Bootstrap.Form.Input as Input
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Color exposing (Color, rgba)
import Dict
import Html exposing (Html, div, text)
import Html.Attributes exposing (attribute, for, style)
import Json.Decode as D
import Main exposing (baseApplication)
import Svg exposing (Svg, image, svg)
import Svg.Attributes exposing (height, width)
import TypedSvg.Types exposing (px)
import Utils exposing (fillParent, filledBackground, flexCenter, mapStyles, rgba255, textCenter)


type alias Model =
    { username : String
    , password : String
    }


type Msg
    = UsernameUpdated String
    | PasswordUpdate String
    | Submitted


init : String -> ( Model, Cmd Msg )
init _ =
    ( Model "" "", Cmd.none )


greenColor : Float -> Color
greenColor opacity =
    rgba255 36 158 71 opacity


view : Model -> Html Msg
view model =
    Grid.containerFluid [ style "height" "100vh" ]
        [ Grid.row [ Row.attrs (fillParent ++ flexCenter) ]
            [ Grid.col [] []
            , Grid.col [] []
            , Grid.col [ Col.middleXs ]
                [ viewForm model
                , viewBackground
                ]
            , Grid.col [] []
            , Grid.col [] []
            ]
        ]


viewBackground =
    filledBackground <|
        mapStyles <|
            Dict.fromList
                [ ( "background"
                  , "linear-gradient(180deg, #C4C4C4 0%, #159A42 0.01%, rgba(0, 128, 0, 0.53) 53.65%, #006400 100%)"
                  )
                , ( "box-shadow"
                  , "0px 4px 4px rgba(0, 0, 0, 0.25)"
                  )
                ]


viewForm : Model -> Html Msg
viewForm model =
    Card.config [ Card.attrs [ style "width" "100%", style "opacity" "0.66" ] ]
        |> Card.header [ textCenter ]
            [ treeIcon (px 200) (greenColor 1)
            ]
        |> Card.block []
            [ Block.custom <| viewFormMain model
            ]
        |> Card.view


viewFormMain : Model -> Html Msg
viewFormMain model =
    let
        updatePass pass =
            PasswordUpdate pass

        updateLogin login =
            UsernameUpdated login
    in
    div []
        [ Form.form
            []
            [ Form.group []
                [ Form.label [ for "login" ] [ text "Login" ]
                , Input.text
                    [ Input.id "login"
                    , Input.onInput updateLogin
                    ]
                ]
            , Form.group []
                [ Form.label [ for "password" ] [ text "Password" ]
                , Input.password
                    [ Input.id "password"
                    , Input.onInput updatePass
                    ]
                ]
            ]
        , Form.group []
            [ Button.button
                [ Button.primary
                , Button.onClick Submitted
                , Button.attrs
                    [ style "margin" "10px auto"
                    , style "width" "100%"
                    ]
                ]
                [ text "Login" ]
            ]
        ]


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
