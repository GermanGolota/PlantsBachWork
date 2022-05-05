module Pages.NotPosted exposing (..)

import Html exposing (Html, div)
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import NavBar exposing (plantsLink, viewNav)



--model


type alias Model =
    ModelBase View


type alias View =
    {}



--update


type Msg
    = NoOp


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    case m of
        Authorized auth model ->
            ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )



--commands
--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    div [] []


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    initBase [ Producer, Consumer, Manager ] {} (\res -> Cmd.none) resp


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
